using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class QuicksandPit : MonoBehaviour
{
    [Header("גודל ומראה הבוץ")]
    public float width = 10f;
    public float height = 1.2f;
    public Color mudColor = new Color(0.32f, 0.20f, 0.09f, 1f);
    public Color surfaceColor = new Color(0.45f, 0.30f, 0.14f, 1f);

    [Header("כוונון גובה רגליים (0 = צמוד בדיוק לאבן)")]
    public float footAdjustment = 0f;

    [Header("חבל מתנדנד")]
    public float ropeHeightOffset = 5.6f;
    public float ropeLength = 3.2f;
    public float ropeSwingSpeed = 2.3f;
    public float ropeSwingAngle = 38f;
    public float playerHangOffsetY = -0.85f;

    [Header("החזרת שחקן (אם אין צ'ק פוינט)")]
    public Transform fallbackRespawnPoint;

    private Vector3 defaultStartPos;
    private float baseFloorY;
    private float pivotToFeetDistance = 0f;
    private TemplePlayer playerScript;
    private Transform playerTransform;
    private List<Rect> platforms = new List<Rect>();

    // משתני החבל המנוהלים ישירות מכאן (ללא סקריפט נוסף)
    private Transform ropePivotTransform;
    private Transform ropeGrabPoint;
    private bool isPlayerOnRope = false;
    private float ropeCooldown = 0f;

    private float lastWidth;
    private float lastHeight;
    private float lastRopeLength;
    private float lastRopeHeightOffset;
    private Vector3 lastPos;

    void OnEnable()
    {
        RebuildAll();
    }

    void Start()
    {
        RebuildAll();

        if (!Application.isPlaying) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            defaultStartPos = player.transform.position;
            playerScript = player.GetComponent<TemplePlayer>();
            baseFloorY = player.transform.position.y;

            Collider2D col = player.GetComponent<Collider2D>();
            if (col != null)
            {
                pivotToFeetDistance = Mathf.Max(0f, player.transform.position.y - col.bounds.min.y);
            }
            else
            {
                SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    pivotToFeetDistance = Mathf.Max(0f, player.transform.position.y - sr.bounds.min.y);
            }
        }
    }

    public void RebuildAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        platforms.Clear();
        ropePivotTransform = null;
        ropeGrabPoint = null;

        BuildMudVisuals();
        BuildObstacleCourse();

        lastWidth = width;
        lastHeight = height;
        lastRopeLength = ropeLength;
        lastRopeHeightOffset = ropeHeightOffset;
        lastPos = transform.position;
    }

    void BuildMudVisuals()
    {
        int texW = 256;
        int texH = 64;
        Texture2D tex = new Texture2D(texW, texH);
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < texW; x++)
        {
            float wave = Mathf.Sin((x / (float)texW) * Mathf.PI * 8f) * 3f;
            int topEdge = texH - 6 + Mathf.RoundToInt(wave);

            for (int y = 0; y < texH; y++)
            {
                if (y > topEdge) tex.SetPixel(x, y, Color.clear);
                else if (y > topEdge - 6) tex.SetPixel(x, y, surfaceColor);
                else
                {
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.15f);
                    tex.SetPixel(x, y, Color.Lerp(mudColor * 0.85f, mudColor * 1.15f, noise));
                }
            }
        }
        tex.Apply();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), 100f);
        sr.sortingOrder = 1;

        transform.localScale = new Vector3((width * 100f) / texW, (height * 100f) / texH, 1f);
    }

    Sprite CreateStoneSprite()
    {
        int w = 128;
        int h = 64;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;

        Color baseStone = new Color(0.42f, 0.39f, 0.35f, 1f);
        Color darkCrack = new Color(0.22f, 0.20f, 0.18f, 1f);
        Color highlight = new Color(0.56f, 0.52f, 0.47f, 1f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < 3 || x >= w - 3 || y < 3) tex.SetPixel(x, y, darkCrack);
                else if (y >= h - 4) tex.SetPixel(x, y, highlight);
                else
                {
                    float grain = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                    float crack = Mathf.PerlinNoise(x * 0.05f + 10f, y * 0.25f);
                    Color c = Color.Lerp(baseStone * 0.8f, baseStone * 1.15f, grain);
                    if (crack > 0.78f && crack < 0.82f) c = darkCrack;
                    tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateRopeSprite()
    {
        int w = 32;
        int h = 128;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;

        Color ropeLight = new Color(0.76f, 0.58f, 0.34f, 1f);
        Color ropeDark = new Color(0.42f, 0.28f, 0.13f, 1f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float twist = Mathf.Sin((y * 0.35f) + (x * 0.4f));
                Color c = Color.Lerp(ropeDark, ropeLight, (twist + 1f) * 0.5f);
                float edge = Mathf.Abs((x / (float)w) - 0.5f) * 2f;
                c *= (1f - edge * 0.35f);
                c.a = 1f;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    void BuildObstacleCourse()
    {
        Sprite stoneSprite = CreateStoneSprite();
        Sprite ropeSprite = CreateRopeSprite();

        Vector3 center = transform.position;
        float leftEdge = center.x - (width / 2f);
        float rightEdge = center.x + (width / 2f);
        float mudBottom = center.y - (height / 2f);
        float mudTop = center.y + (height / 2f);

        CreateStoneBlock("StoneStep_1", new Vector3(leftEdge - 2.2f, mudBottom + 0.35f, 0f), new Vector2(2.4f, 0.7f), stoneSprite);
        CreateStoneBlock("StoneStep_2", new Vector3(leftEdge - 0.5f, mudBottom + 0.7f, 0f), new Vector2(2.2f, 1.4f), stoneSprite);
        CreateStoneBlock("StonePlatform_1", new Vector3(leftEdge + width * 0.23f, mudTop + 0.6f, 0f), new Vector2(2.8f, 0.6f), stoneSprite);
        CreateSwingingRope(new Vector3(center.x, mudTop + ropeHeightOffset, 0f), ropeSprite);
        CreateStoneBlock("StonePlatform_2", new Vector3(rightEdge - width * 0.23f, mudTop + 0.6f, 0f), new Vector2(2.8f, 0.6f), stoneSprite);
        CreateStoneBlock("StoneStep_Exit", new Vector3(rightEdge + 0.6f, mudBottom + 0.55f, 0f), new Vector2(2.4f, 1.1f), stoneSprite);
    }

    void CreateStoneBlock(string objName, Vector3 pos, Vector2 worldSize, Sprite sprite)
    {
        GameObject plat = new GameObject(objName);
        plat.transform.SetParent(transform, true);
        plat.transform.position = pos;

        Vector3 parentScale = transform.localScale;
        plat.transform.localScale = new Vector3(
            (worldSize.x / 1.28f) / parentScale.x,
            (worldSize.y / 0.64f) / parentScale.y,
            1f
        );

        SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        float topSurfaceY = pos.y + (worldSize.y / 2f);
        platforms.Add(new Rect(pos.x - worldSize.x / 2f, topSurfaceY, worldSize.x, worldSize.y));
    }

    void CreateSwingingRope(Vector3 pivotPos, Sprite ropeSprite)
    {
        GameObject pivot = new GameObject("RopePivot");
        pivot.transform.SetParent(transform, true);
        pivot.transform.position = pivotPos;

        Vector3 parentScale = transform.localScale;
        pivot.transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f);
        ropePivotTransform = pivot.transform;

        GameObject ropeVisual = new GameObject("RopeLine");
        ropeVisual.transform.SetParent(pivot.transform, false);
        ropeVisual.transform.localPosition = new Vector3(0f, -ropeLength / 2f, 0f);
        ropeVisual.transform.localScale = new Vector3(0.22f / 0.32f, ropeLength / 1.28f, 1f);

        SpriteRenderer sr = ropeVisual.AddComponent<SpriteRenderer>();
        sr.sprite = ropeSprite;
        sr.sortingOrder = 2;

        GameObject knot = new GameObject("RopeKnot");
        knot.transform.SetParent(pivot.transform, false);
        knot.transform.localPosition = new Vector3(0f, -ropeLength, 0f);
        knot.transform.localScale = new Vector3(0.32f / 0.32f, 0.32f / 1.28f, 1f);
        SpriteRenderer knotSr = knot.AddComponent<SpriteRenderer>();
        knotSr.sprite = ropeSprite;
        knotSr.color = new Color(0.65f, 0.45f, 0.22f, 1f);
        knotSr.sortingOrder = 3;

        GameObject grabPoint = new GameObject("RopeGrabPoint");
        grabPoint.transform.SetParent(pivot.transform, false);
        grabPoint.transform.localPosition = new Vector3(0f, -ropeLength + 0.1f, 0f);
        ropeGrabPoint = grabPoint.transform;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (width != lastWidth || height != lastHeight || ropeLength != lastRopeLength || ropeHeightOffset != lastRopeHeightOffset || transform.position != lastPos)
            {
                RebuildAll();
            }
            return;
        }

        // 1. עדכון תנועת החבל וטיפול בתפיסה/זינוק
        UpdateRopeLogic();

        if (playerTransform == null || playerScript == null) return;

        float feetOffset = pivotToFeetDistance + footAdjustment;
        float targetFloorY = baseFloorY;

        foreach (Rect p in platforms)
        {
            if (playerTransform.position.x >= p.xMin && playerTransform.position.x <= p.xMax)
            {
                float stepFloorY = p.y + feetOffset;
                if (playerTransform.position.y >= stepFloorY - 0.25f)
                {
                    targetFloorY = Mathf.Max(targetFloorY, stepFloorY);
                }
            }
        }

        playerScript.currentFloorY = targetFloorY;

        if (!isPlayerOnRope)
        {
            float leftMud = transform.position.x - (width / 2f) + 0.3f;
            float rightMud = transform.position.x + (width / 2f) - 0.3f;
            float mudSurfacePlayerY = (transform.position.y + height / 2f) + feetOffset - 0.15f;

            if (playerTransform.position.x > leftMud && playerTransform.position.x < rightMud)
            {
                if (playerTransform.position.y <= mudSurfacePlayerY)
                {
                    RespawnPlayer();
                }
            }
        }
    }

    void UpdateRopeLogic()
    {
        if (ropePivotTransform == null || ropeGrabPoint == null) return;

        float angle = Mathf.Sin(Time.time * ropeSwingSpeed) * ropeSwingAngle;
        ropePivotTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (ropeCooldown > 0f) ropeCooldown -= Time.deltaTime;

        if (playerTransform == null) return;

        if (isPlayerOnRope)
        {
            playerTransform.position = ropeGrabPoint.position + new Vector3(0f, playerHangOffsetY, 0f);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                isPlayerOnRope = false;
                if (playerScript != null) playerScript.LaunchFromRope(10f, 9f);
                ropeCooldown = 0.8f;
            }
        }
        else if (ropeCooldown <= 0f)
        {
            Vector3 playerHandsPos = playerTransform.position - new Vector3(0f, playerHangOffsetY, 0f);
            if (Vector2.Distance(playerHandsPos, ropeGrabPoint.position) < 1.3f)
            {
                isPlayerOnRope = true;
                if (playerScript != null) playerScript.isOnRope = true;
            }
        }
    }

    void RespawnPlayer()
    {
        isPlayerOnRope = false;
        if (playerScript != null)
        {
            playerScript.isOnRope = false;
            playerScript.currentFloorY = baseFloorY;
        }

        if (Checkpoint.hasCheckpoint)
            playerTransform.position = Checkpoint.currentRespawnPos;
        else if (fallbackRespawnPoint != null)
            playerTransform.position = fallbackRespawnPoint.position;
        else
            playerTransform.position = defaultStartPos;
    }
}