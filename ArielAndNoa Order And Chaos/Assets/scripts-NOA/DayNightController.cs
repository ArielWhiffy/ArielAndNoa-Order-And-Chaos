using UnityEngine;
using System.Collections;

public class DayNightController : MonoBehaviour
{
    [Header("הגדרות זמנים (בשניות)")]
    public float starsDuration = 3f;      // כמה זמן יש כוכבים
    public float sunriseDuration = 3f;    // משך המעבר לזריחה
    public float fadeToBlackDuration = 2f;// משך המעבר לשחור

    private SpriteRenderer starsRenderer;
    private SpriteRenderer sunriseRenderer;
    private SpriteRenderer fadeToBlackRenderer;

    void Start()
    {
        // 1. יצירת אובייקטים וטקסטורות ישירות מהקוד
        starsRenderer = CreateVisualObject("StarsObject", CreateStarsTexture(), 1);
        sunriseRenderer = CreateVisualObject("SunriseObject", CreateColorTexture(new Color(1f, 0.45f, 0.15f)), 2); // צבע כתום זריחה
        fadeToBlackRenderer = CreateVisualObject("FadeToBlackObject", CreateColorTexture(Color.black), 100);

        // 2. הפעלת הרצף
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // מצב התחלתי: כוכבים נראים, זריחה ושחור מוסתרים
        SetAlpha(starsRenderer, 1f);
        SetAlpha(sunriseRenderer, 0f);
        SetAlpha(fadeToBlackRenderer, 0f);

        // א. השהייה עם הכוכבים
        yield return new WaitForSeconds(starsDuration);

        // ב. מעבר מכוכבים לזריחה
        float elapsed = 0f;
        while (elapsed < sunriseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / sunriseDuration;

            SetAlpha(starsRenderer, Mathf.Lerp(1f, 0f, t));
            SetAlpha(sunriseRenderer, Mathf.Lerp(0f, 1f, t));

            yield return null;
        }

        // השהייה קצרה בשיא הזריחה
        yield return new WaitForSeconds(1.5f);

        // ג. מעבר לשחור מוחלט
        elapsed = 0f;
        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeToBlackDuration;

            SetAlpha(fadeToBlackRenderer, Mathf.Lerp(0f, 1f, t));

            yield return null;
        }

        SetAlpha(fadeToBlackRenderer, 1f);
    }

    // יצירת אובייקט ויזואלי ומעקב אחרי המצלמה
    private SpriteRenderer CreateVisualObject(string name, Texture2D texture, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        // חיבור למצלמה כדי שיזוז איתה
        obj.transform.SetParent(Camera.main.transform);
        obj.transform.localPosition = new Vector3(0, 0, 10);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        // התאמת גודל לכיסוי כל המסך
        obj.transform.localScale = new Vector3(50f, 50f, 1f);

        return sr;
    }

    // יצירת טקסטורת צבע חלק (זריחה / שחור)
    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // יצירת טקסטורת כוכבים אקראיים בקוד
    private Texture2D CreateStarsTexture()
    {
        int width = 256;
        int height = 256;
        Texture2D tex = new Texture2D(width, height);

        // רקע לילה כהה
        Color skyColor = new Color(0.02f, 0.02f, 0.08f, 1f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, skyColor);
            }
        }

        // פיזור כוכבים לבנים/צהובים קטנים
        for (int i = 0; i < 150; i++)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);
            tex.SetPixel(rx, ry, Color.white);
        }

        tex.Apply();
        return tex;
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}