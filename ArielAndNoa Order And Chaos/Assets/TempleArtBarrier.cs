using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class TempleArtBarrier : MonoBehaviour
{
    [Header("מבנה קיר האמנות")]
    public float wallWidth = 1.2f;
    public float wallHeight = 4.2f;
    public int tileCount = 3;

    [Header("אינטראקציה ופתיחה")]
    public float interactDistance = 3.0f;
    public float openSpeed = 3.5f;

    [Header("הודעת הוראה לשחקן")]
    public string hintText = "גלול בגלגלת העכבר לסיבוב האריחים";
    public float hintHeightOffset = 0.7f;

    private Transform wallVisual;
    private Transform playerTransform;
    private List<ArtTileData> tiles = new List<ArtTileData>();
    private bool isSolved = false;
    private float startWallY;
    private float lastPlayerX;
    private float scrollCooldown = 0f;

    private GameObject hintBubbleObj;
    private TextMesh hintTextMesh;

    private float lastWidth;
    private float lastHeight;
    private Vector3 lastPos;

    private class ArtTileData
    {
        public Transform tileTransform;
        public SpriteRenderer frameRenderer;
        public int currentStep;
    }

    void OnEnable()
    {
        RebuildAll();
    }

    void Start()
    {
        RebuildAll();
        startWallY = transform.position.y;

        if (!Application.isPlaying) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            lastPlayerX = player.transform.position.x;
        }
    }

    public void RebuildAll()
    {
        // הגנה מחלוקה ב-0 בזמן הקלדה ב-Inspector
        float safeWidth = Mathf.Max(0.2f, wallWidth);
        float safeHeight = Mathf.Max(0.5f, wallHeight);
        int safeTileCount = Mathf.Clamp(tileCount, 1, 10);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }

        tiles.Clear();
        isSolved = false;

        // 1. עמוד האבן המרכזי שחוסם את המעבר
        GameObject wall = new GameObject("ArtWall");
        wall.transform.SetParent(transform, false);
        wall.transform.localPosition = new Vector3(0f, safeHeight / 2f, 0f);
        float wallScaleX = safeWidth / 0.64f;
        float wallScaleY = safeHeight / 1.28f;
        wall.transform.localScale = new Vector3(wallScaleX, wallScaleY, 1f);

        SpriteRenderer wallSr = wall.AddComponent<SpriteRenderer>();
        wallSr.sprite = CreateCarvedWallSprite();
        wallSr.sortingOrder = 3;
        wallVisual = wall.transform;

        // 2. יצירת אריחי הציור המוזהבים לאורך הקיר
        int[] initialRotations = new int[] { 1, 2, 3 };

        for (int i = 0; i < safeTileCount; i++)
        {
            float yOffset = safeTileCount == 1
                ? safeHeight * 0.5f
                : 0.85f + i * ((safeHeight - 1.4f) / (safeTileCount - 1));

            GameObject frameObj = new GameObject("ArtFrame_" + (i + 1));
            frameObj.transform.SetParent(wall.transform, true);
            frameObj.transform.position = transform.position + new Vector3(-0.35f, yOffset, 0f);
            frameObj.transform.localScale = new Vector3(
                (0.95f / 0.64f) / wallScaleX,
                (0.95f / 0.64f) / wallScaleY,
                1f
            );

            SpriteRenderer frameSr = frameObj.AddComponent<SpriteRenderer>();
            frameSr.sprite = CreateFrameSprite();
            frameSr.color = new Color(0.85f, 0.65f, 0.2f, 1f);
            frameSr.sortingOrder = 4;

            GameObject artObj = new GameObject("ArtPainting_" + (i + 1));
            artObj.transform.SetParent(frameObj.transform, false);
            artObj.transform.localPosition = Vector3.zero;
            artObj.transform.localScale = new Vector3(0.82f, 0.82f, 1f);

            SpriteRenderer artSr = artObj.AddComponent<SpriteRenderer>();
            artSr.sprite = CreatePaintingSprite(i);
            artSr.sortingOrder = 5;

            int startStep = Application.isPlaying ? initialRotations[i % initialRotations.Length] : 0;
            artObj.transform.localRotation = Quaternion.Euler(0f, 0f, -startStep * 90f);

            tiles.Add(new ArtTileData
            {
                tileTransform = artObj.transform,
                frameRenderer = frameSr,
                currentStep = startStep
            });
        }

        // 3. יצירת שלט ההודעה המרחפת מעל הקיר
        BuildFloatingHint(safeHeight);

        lastWidth = wallWidth;
        lastHeight = wallHeight;
        lastPos = transform.position;
    }

    void BuildFloatingHint(float safeHeight)
    {
        hintBubbleObj = new GameObject("ScrollHintSign");
        hintBubbleObj.transform.SetParent(transform, false);
        hintBubbleObj.transform.localPosition = new Vector3(-0.8f, safeHeight + hintHeightOffset, 0f);

        GameObject bg = new GameObject("HintBackground");
        bg.transform.SetParent(hintBubbleObj.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        bg.transform.localScale = new Vector3(4.6f / 0.64f, 0.75f / 0.64f, 1f);

        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateHintBoxSprite();
        bgSr.sortingOrder = 15;

        GameObject txtObj = new GameObject("HintText");
        txtObj.transform.SetParent(hintBubbleObj.transform, false);
        txtObj.transform.localPosition = Vector3.zero;
        txtObj.transform.localScale = new Vector3(0.08f, 0.08f, 1f);

        hintTextMesh = txtObj.AddComponent<TextMesh>();
        hintTextMesh.text = ReverseHebrewForTextMesh(hintText);
        hintTextMesh.fontSize = 36;
        hintTextMesh.characterSize = 1f;
        hintTextMesh.anchor = TextAnchor.MiddleCenter;
        hintTextMesh.alignment = TextAlignment.Center;
        hintTextMesh.color = new Color(1f, 0.92f, 0.55f, 1f);

        MeshRenderer mr = txtObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 16;

        if (Application.isPlaying)
        {
            hintBubbleObj.SetActive(false);
        }
    }

    string ReverseHebrewForTextMesh(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        char[] arr = input.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }

    Sprite CreateHintBoxSprite()
    {
        int w = 64;
        int h = 64;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;

        Color fill = new Color(0.12f, 0.10f, 0.08f, 0.92f);
        Color border = new Color(0.88f, 0.68f, 0.22f, 1f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < 3 || x >= w - 3 || y < 4 || y >= h - 4)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, fill);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateCarvedWallSprite()
    {
        int w = 64;
        int h = 128;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;

        Color stone = new Color(0.35f, 0.31f, 0.27f, 1f);
        Color goldTrim = new Color(0.72f, 0.54f, 0.20f, 1f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < 4 || x >= w - 4 || y < 4 || y >= h - 4)
                    tex.SetPixel(x, y, goldTrim);
                else
                {
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.1f);
                    tex.SetPixel(x, y, Color.Lerp(stone * 0.8f, stone * 1.2f, noise));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateFrameSprite()
    {
        int s = 64;
        Texture2D tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                if (x < 6 || x >= s - 6 || y < 6 || y >= s - 6)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, new Color(0.16f, 0.14f, 0.12f, 1f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreatePaintingSprite(int variant)
    {
        int s = 64;
        Texture2D tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Bilinear;

        Color bg = variant == 0 ? new Color(0.18f, 0.28f, 0.42f, 1f) :
                   variant == 1 ? new Color(0.42f, 0.18f, 0.16f, 1f) :
                                  new Color(0.20f, 0.36f, 0.24f, 1f);
        Color gold = new Color(0.98f, 0.80f, 0.25f, 1f);
        Color sky = new Color(0.95f, 0.92f, 0.78f, 1f);

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                tex.SetPixel(x, y, bg);

                float dx = Mathf.Abs(x - 32f);
                if (y > 34 && y < 56 && dx < (56 - y) * 0.7f)
                    tex.SetPixel(x, y, gold);

                if (y >= 10 && y <= 30 && dx < (34 - y) * 0.9f)
                    tex.SetPixel(x, y, sky);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (wallWidth != lastWidth || wallHeight != lastHeight || transform.position != lastPos)
            {
                RebuildAll();
            }
            return;
        }

        if (playerTransform == null) return;

        if (scrollCooldown > 0f) scrollCooldown -= Time.deltaTime;

        float safeWidth = Mathf.Max(0.2f, wallWidth);
        float safeHeight = Mathf.Max(0.5f, wallHeight);

        if (!isSolved)
        {
            float wallLeftEdge = transform.position.x - (safeWidth * 0.5f) - 0.45f;
            if (playerTransform.position.x > wallLeftEdge && lastPlayerX <= wallLeftEdge + 0.2f)
            {
                playerTransform.position = new Vector3(wallLeftEdge, playerTransform.position.y, playerTransform.position.z);
            }

            float dist = Mathf.Abs(playerTransform.position.x - transform.position.x);
            bool isPlayerClose = dist <= interactDistance;

            if (hintBubbleObj != null && hintBubbleObj.activeSelf != isPlayerClose)
            {
                hintBubbleObj.SetActive(isPlayerClose);
            }

            if (isPlayerClose)
            {
                if (hintBubbleObj != null)
                {
                    float bob = Mathf.Sin(Time.time * 4f) * 0.08f;
                    hintBubbleObj.transform.localPosition = new Vector3(-0.8f, safeHeight + hintHeightOffset + bob, 0f);
                }

                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > 0.05f && scrollCooldown <= 0f)
                {
                    int direction = scroll > 0 ? 1 : -1;
                    RotateNextUnsolvedTile(direction);
                    scrollCooldown = 0.14f;
                }
            }
        }
        else
        {
            if (hintBubbleObj != null && hintBubbleObj.activeSelf)
            {
                hintBubbleObj.SetActive(false);
            }

            Vector3 targetPos = new Vector3(transform.position.x, startWallY + safeHeight + 0.5f, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, openSpeed * Time.deltaTime);
        }

        lastPlayerX = playerTransform.position.x;
    }

    void RotateNextUnsolvedTile(int direction)
    {
        foreach (ArtTileData t in tiles)
        {
            if (t.currentStep != 0)
            {
                t.currentStep = (t.currentStep + direction + 4) % 4;
                t.tileTransform.localRotation = Quaternion.Euler(0f, 0f, -t.currentStep * 90f);

                if (t.currentStep == 0)
                    t.frameRenderer.color = new Color(0.35f, 0.95f, 0.45f, 1f);
                break;
            }
        }

        bool allAligned = true;
        foreach (ArtTileData t in tiles)
        {
            if (t.currentStep != 0)
            {
                allAligned = false;
                break;
            }
        }

        if (allAligned)
        {
            isSolved = true;
        }
    }
}