using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class TempleFireTraps : MonoBehaviour
{
    [Header("הגדרות עמודי האש")]
    public int pillarCount = 3;
    public float spacing = 3.2f;
    public float flameHeight = 2.8f;
    public float flameWidth = 0.85f;

    [Header("תזמון האש (בשניות)")]
    public float safeDuration = 1.6f;     // זמן רגוע למעבר
    public float warningDuration = 0.7f;  // גיצים/אזהרה לפני פריצת האש
    public float fireDuration = 1.3f;     // משך הזמן שעמוד האש בוער
    public float phaseDelay = 0.65f;      // הפרש תזמון בין עמוד לעמוד (יוצר גל)

    [Header("החזרת שחקן (אם אין צ'ק פוינט)")]
    public Transform fallbackRespawnPoint;

    private List<FirePillarData> pillars = new List<FirePillarData>();
    private Transform playerTransform;
    private TemplePlayer playerScript;
    private Vector3 defaultStartPos;
    private float baseFloorY;

    private int lastCount;
    private float lastSpacing;
    private float lastFlameHeight;
    private Vector3 lastPos;

    private class FirePillarData
    {
        public Transform baseTransform;
        public SpriteRenderer flameRenderer;
        public Transform flameTransform;
        public float timeOffset;
    }

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

        pillars.Clear();

        Sprite altarSprite = CreateAltarSprite();
        Sprite flameSprite = CreateFlameSprite();

        for (int i = 0; i < pillarCount; i++)
        {
            float offsetX = i * spacing;
            Vector3 altarPos = transform.position + new Vector3(offsetX, 0.25f, 0f);

            // 1. בסיס אבן עתיק (מזבח להבות)
            GameObject altar = new GameObject("FireAltar_" + (i + 1));
            altar.transform.SetParent(transform, true);
            altar.transform.position = altarPos;
            altar.transform.localScale = new Vector3(1.1f / 1.28f, 0.5f / 0.64f, 1f);

            SpriteRenderer altarSr = altar.AddComponent<SpriteRenderer>();
            altarSr.sprite = altarSprite;
            altarSr.sortingOrder = 2;

            // 2. עמוד האש עצמו
            GameObject flame = new GameObject("FlameColumn_" + (i + 1));
            flame.transform.SetParent(altar.transform, false);
            flame.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            flame.transform.localScale = new Vector3(
                (flameWidth / 0.64f) / altar.transform.localScale.x,
                (flameHeight / 1.28f) / altar.transform.localScale.y,
                1f
            );

            SpriteRenderer flameSr = flame.AddComponent<SpriteRenderer>();
            flameSr.sprite = flameSprite;
            flameSr.sortingOrder = 4;

            FirePillarData data = new FirePillarData
            {
                baseTransform = altar.transform,
                flameRenderer = flameSr,
                flameTransform = flame.transform,
                timeOffset = i * phaseDelay
            };
            pillars.Add(data);
        }

        lastCount = pillarCount;
        lastSpacing = spacing;
        lastFlameHeight = flameHeight;
        lastPos = transform.position;
    }

    Sprite CreateAltarSprite()
    {
        int w = 128;
        int h = 64;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;

        Color darkStone = new Color(0.28f, 0.25f, 0.22f, 1f);
        Color carvedGold = new Color(0.68f, 0.50f, 0.18f, 1f);
        Color emberGlow = new Color(0.95f, 0.35f, 0.05f, 1f);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (y >= h - 6 && x > 18 && x < w - 18)
                {
                    tex.SetPixel(x, y, emberGlow);
                }
                else if ((y > 18 && y < 26) || x < 6 || x >= w - 6)
                {
                    tex.SetPixel(x, y, carvedGold);
                }
                else
                {
                    float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                    tex.SetPixel(x, y, Color.Lerp(darkStone * 0.8f, darkStone * 1.25f, noise));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateFlameSprite()
    {
        int w = 64;
        int h = 128;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;

        Color outerRed = new Color(0.92f, 0.22f, 0.04f, 0.9f);
        Color midOrange = new Color(1.0f, 0.55f, 0.08f, 0.95f);
        Color innerYellow = new Color(1.0f, 0.92f, 0.45f, 1f);

        for (int x = 0; x < w; x++)
        {
            float normX = Mathf.Abs((x / (float)w) - 0.5f) * 2f;
            for (int y = 0; y < h; y++)
            {
                float normY = y / (float)h;
                float tongue = Mathf.Sin(normX * 6f + normY * 8f) * 0.12f;
                float maxWidth = (1f - normY * 0.85f) + tongue;

                if (normX > maxWidth)
                {
                    tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    float heat = (1f - normX / Mathf.Max(0.1f, maxWidth)) * (1f - normY * 0.6f);
                    Color c = heat > 0.55f ? innerYellow : (heat > 0.25f ? midOrange : outerRed);
                    c.a = Mathf.Clamp01(1.2f - normY * 0.8f);
                    tex.SetPixel(x, y, c);
                }
            }
        }
        tex.Apply();
        // Pivot בתחתית הלהבה (0.5, 0) כדי שתצמח ישר מתוך המזבח
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 100f);
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (pillarCount != lastCount || spacing != lastSpacing || flameHeight != lastFlameHeight || transform.position != lastPos)
            {
                RebuildAll();
            }
            return;
        }

        float totalCycle = safeDuration + warningDuration + fireDuration;

        foreach (FirePillarData p in pillars)
        {
            float cycleTime = (Time.time + p.timeOffset) % totalCycle;

            if (cycleTime < safeDuration)
            {
                // שלב 1: כבוי ובטוח למעבר
                p.flameRenderer.enabled = false;
            }
            else if (cycleTime < safeDuration + warningDuration)
            {
                // שלב 2: אזהרה - להבה נמוכה ומהבהבת לפני ההתפרצות
                p.flameRenderer.enabled = true;
                float flicker = 0.18f + Mathf.PingPong(Time.time * 8f, 0.12f);
                Vector3 s = p.flameTransform.localScale;
                p.flameTransform.localScale = new Vector3(s.x, flicker, 1f);
            }
            else
            {
                // שלב 3: התפרצות עמוד האש המלא!
                p.flameRenderer.enabled = true;
                float fullScaleY = (flameHeight / 1.28f) / p.baseTransform.localScale.y;
                float flicker = fullScaleY * (0.92f + Mathf.Sin(Time.time * 25f) * 0.08f);
                Vector3 s = p.flameTransform.localScale;
                p.flameTransform.localScale = new Vector3(s.x, flicker, 1f);

                // בדיקה אם השחקן נוגע בעמוד האש הבוער
                CheckPlayerHit(p);
            }
        }
    }

    void CheckPlayerHit(FirePillarData p)
    {
        if (playerTransform == null) return;

        float fireX = p.baseTransform.position.x;
        float fireBottomY = p.baseTransform.position.y;
        float fireTopY = fireBottomY + flameHeight;

        if (Mathf.Abs(playerTransform.position.x - fireX) < (flameWidth * 0.55f))
        {
            if (playerTransform.position.y >= fireBottomY - 0.3f && playerTransform.position.y <= fireTopY)
            {
                RespawnPlayer();
            }
        }
    }

    void RespawnPlayer()
    {
        if (playerScript != null) playerScript.currentFloorY = baseFloorY;

        if (Checkpoint.hasCheckpoint)
            playerTransform.position = Checkpoint.currentRespawnPos;
        else if (fallbackRespawnPoint != null)
            playerTransform.position = fallbackRespawnPoint.position;
        else
            playerTransform.position = defaultStartPos;
    }
}