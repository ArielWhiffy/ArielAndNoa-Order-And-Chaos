using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[ExecuteAlways]
public class TempleArtPortals : MonoBehaviour
{
    [System.Serializable]
    public class PaintingPortalOption
    {
        public string title = "ציור 1";
        public string sceneName = "Scene1";     // שם הסצנה שאליה עוברים
        public Sprite paintingSprite;           // גררי לכאן את הציור כשתסיימי לצייר (אם ריק - יוצג מרובע זמני)
        public Color placeholderColor = new Color(0.18f, 0.25f, 0.35f, 1f);
    }

    [Header("הגדרות שני הציורים / פורטלים")]
    public List<PaintingPortalOption> portals = new List<PaintingPortalOption>()
    {
        new PaintingPortalOption { title = "ציור 1", sceneName = "NextScene_1", placeholderColor = new Color(0.18f, 0.28f, 0.42f, 1f) },
        new PaintingPortalOption { title = "ציור 2", sceneName = "NextScene_2", placeholderColor = new Color(0.42f, 0.20f, 0.18f, 1f) }
    };

    [Header("גודל ומיקום הציורים")]
    public float paintingWidth = 2.4f;
    public float paintingHeight = 3.0f;
    public float spacing = 4.2f;
    public float heightAboveFloor = 2.1f;

    [Header("שלט הוראה מעל הגלריה")]
    public string bannerText = "בחר ציור והתקרב אליו כדי להיכנס";
    public float bannerHeightOffset = 4.4f;

    [Header("אינטראקציה וכניסה לסצנה")]
    public float enterDistance = 1.4f;
    public bool enterAutomaticallyOnTouch = false; // אם מסומן V - נכנס מיד כשנוגעים; אם כבוי - לוחצים E/Enter/רווח
    public KeyCode enterKey = KeyCode.E;

    private Transform playerTransform;
    private List<SpawnedPortal> spawnedPortals = new List<SpawnedPortal>();
    private GameObject promptObj;
    private TextMesh promptTextMesh;
    private bool isTransitioning = false;

    private int lastCount;
    private float lastWidth;
    private float lastHeight;
    private float lastSpacing;
    private Vector3 lastPos;

    private class SpawnedPortal
    {
        public PaintingPortalOption config;
        public Transform portalTransform;
        public SpriteRenderer frameRenderer;
        public SpriteRenderer artRenderer;
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

        spawnedPortals.Clear();

        float safeW = Mathf.Max(0.5f, paintingWidth);
        float safeH = Mathf.Max(0.5f, paintingHeight);
        Sprite frameSprite = CreateGoldFrameSprite();
        Sprite placeholderSprite = CreatePlaceholderSquareSprite();

        for (int i = 0; i < portals.Count; i++)
        {
            PaintingPortalOption opt = portals[i];
            float offsetX = i * spacing;
            Vector3 pos = transform.position + new Vector3(offsetX, heightAboveFloor, 0f);

            // 1. מסגרת הזהב של הציור
            GameObject frameObj = new GameObject("PortalFrame_" + (i + 1) + "_" + opt.sceneName);
            frameObj.transform.SetParent(transform, true);
            frameObj.transform.position = pos;
            float frameScaleX = (safeW + 0.35f) / 0.64f;
            float frameScaleY = (safeH + 0.35f) / 0.64f;
            frameObj.transform.localScale = new Vector3(frameScaleX, frameScaleY, 1f);

            SpriteRenderer frameSr = frameObj.AddComponent<SpriteRenderer>();
            frameSr.sprite = frameSprite;
            frameSr.color = new Color(0.88f, 0.68f, 0.22f, 1f);
            frameSr.sortingOrder = 2;

            // 2. הציור עצמו (או מרובע ריק אם עדיין לא שמת תמונה)
            GameObject artObj = new GameObject("PaintingCanvas_" + (i + 1));
            artObj.transform.SetParent(frameObj.transform, false);
            artObj.transform.localPosition = Vector3.zero;

            SpriteRenderer artSr = artObj.AddComponent<SpriteRenderer>();
            artSr.sortingOrder = 3;

            if (opt.paintingSprite != null)
            {
                artSr.sprite = opt.paintingSprite;
                artSr.color = Color.white;
                // מתאים אוטומטית את גודל הציור שלך לתוך המסגרת
                Vector2 spriteSize = opt.paintingSprite.bounds.size;
                float targetWorldW = safeW;
                float targetWorldH = safeH;
                artObj.transform.localScale = new Vector3(
                    (targetWorldW / Mathf.Max(0.01f, spriteSize.x)) / frameScaleX,
                    (targetWorldH / Mathf.Max(0.01f, spriteSize.y)) / frameScaleY,
                    1f
                );
            }
            else
            {
                artSr.sprite = placeholderSprite;
                artSr.color = opt.placeholderColor;
                artObj.transform.localScale = new Vector3(
                    (safeW / 0.64f) / frameScaleX,
                    (safeH / 0.64f) / frameScaleY,
                    1f
                );
            }

            spawnedPortals.Add(new SpawnedPortal
            {
                config = opt,
                portalTransform = frameObj.transform,
                frameRenderer = frameSr,
                artRenderer = artSr
            });
        }

        // 3. שלט ראשי מעל הציורים ("בחר ציור והתקרב אליו...")
        BuildTopBanner();

        // 4. הודעת כניסה קופצת כשהשחקן עומד מול אחד הציורים
        BuildEnterPrompt();

        lastCount = portals.Count;
        lastWidth = paintingWidth;
        lastHeight = paintingHeight;
        lastSpacing = spacing;
        lastPos = transform.position;
    }

    void BuildTopBanner()
    {
        float centerX = portals.Count > 1 ? ((portals.Count - 1) * spacing) * 0.5f : 0f;

        GameObject bannerObj = new GameObject("GalleryInstructionSign");
        bannerObj.transform.SetParent(transform, false);
        bannerObj.transform.localPosition = new Vector3(centerX, bannerHeightOffset, 0f);

        GameObject bg = new GameObject("BannerBg");
        bg.transform.SetParent(bannerObj.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        bg.transform.localScale = new Vector3(6.2f / 0.64f, 0.85f / 0.64f, 1f);

        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateSignBgSprite();
        bgSr.sortingOrder = 10;

        GameObject txtObj = new GameObject("BannerText");
        txtObj.transform.SetParent(bannerObj.transform, false);
        txtObj.transform.localPosition = Vector3.zero;
        txtObj.transform.localScale = new Vector3(0.09f, 0.09f, 1f);

        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = ReverseHebrew(bannerText);
        tm.fontSize = 36;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1f, 0.92f, 0.55f, 1f);

        MeshRenderer mr = txtObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 11;
    }

    void BuildEnterPrompt()
    {
        promptObj = new GameObject("EnterPaintingPrompt");
        promptObj.transform.SetParent(transform, false);

        GameObject bg = new GameObject("PromptBg");
        bg.transform.SetParent(promptObj.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        bg.transform.localScale = new Vector3(3.2f / 0.64f, 0.65f / 0.64f, 1f);

        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateSignBgSprite();
        bgSr.sortingOrder = 15;

        GameObject txtObj = new GameObject("PromptText");
        txtObj.transform.SetParent(promptObj.transform, false);
        txtObj.transform.localPosition = Vector3.zero;
        txtObj.transform.localScale = new Vector3(0.075f, 0.075f, 1f);

        promptTextMesh = txtObj.AddComponent<TextMesh>();
        promptTextMesh.text = ReverseHebrew("לחץ E להיכנס לציור");
        promptTextMesh.fontSize = 34;
        promptTextMesh.anchor = TextAnchor.MiddleCenter;
        promptTextMesh.alignment = TextAlignment.Center;
        promptTextMesh.color = new Color(0.45f, 1f, 0.55f, 1f);

        MeshRenderer mr = txtObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 16;

        promptObj.SetActive(false);
    }

    string ReverseHebrew(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        char[] arr = input.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }

    Sprite CreateGoldFrameSprite()
    {
        int s = 64;
        Texture2D tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                if (x < 5 || x >= s - 5 || y < 5 || y >= s - 5)
                    tex.SetPixel(x, y, Color.white);
                else
                    tex.SetPixel(x, y, new Color(0.45f, 0.35f, 0.12f, 1f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreatePlaceholderSquareSprite()
    {
        int s = 64;
        Texture2D tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                float vignette = 1f - (Vector2.Distance(new Vector2(x, y), new Vector2(32, 32)) / 50f) * 0.25f;
                tex.SetPixel(x, y, Color.white * vignette);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateSignBgSprite()
    {
        int s = 64;
        Texture2D tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Bilinear;

        Color fill = new Color(0.10f, 0.08f, 0.06f, 0.92f);
        Color border = new Color(0.88f, 0.68f, 0.22f, 1f);

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                if (x < 3 || x >= s - 3 || y < 4 || y >= s - 4)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, fill);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (portals.Count != lastCount || paintingWidth != lastWidth || paintingHeight != lastHeight || spacing != lastSpacing || transform.position != lastPos)
            {
                RebuildAll();
            }
            return;
        }

        if (playerTransform == null || isTransitioning) return;

        SpawnedPortal closestPortal = null;
        float closestDist = float.MaxValue;

        foreach (SpawnedPortal sp in spawnedPortals)
        {
            float distX = Mathf.Abs(playerTransform.position.x - sp.portalTransform.position.x);
            bool isHighlighted = distX <= enterDistance;

            // מאיר את המסגרת בזהב-ירוק זוהר כשהשחקן עומד מול הציור
            sp.frameRenderer.color = isHighlighted
                ? new Color(0.45f, 1f, 0.55f, 1f)
                : new Color(0.88f, 0.68f, 0.22f, 1f);

            if (isHighlighted && distX < closestDist)
            {
                closestDist = distX;
                closestPortal = sp;
            }
        }
        if (Checkpoint.hasCheckpoint)
        {
            transform.position = Checkpoint.currentRespawnPos;
        }
        if (closestPortal != null)
        {
            if (enterAutomaticallyOnTouch)
            {
                EnterScene(closestPortal.config.sceneName);
                return;
            }

            if (promptObj != null)
            {
                promptObj.SetActive(true);
                float bob = Mathf.Sin(Time.time * 5f) * 0.07f;
                promptObj.transform.position = closestPortal.portalTransform.position + new Vector3(0f, (paintingHeight * 0.5f) + 0.65f + bob, 0f);
            }

            if (Input.GetKeyDown(enterKey) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                EnterScene(closestPortal.config.sceneName);
            }
        }
        else if (promptObj != null && promptObj.activeSelf)
        {
            promptObj.SetActive(false);
        }
    }

    void EnterScene(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene)) return;
        isTransitioning = true;
        SceneManager.LoadScene(targetScene);
    }
}