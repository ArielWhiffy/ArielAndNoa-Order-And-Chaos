using UnityEngine;
using System.Collections;

public class ClickExplosion : MonoBehaviour
{
    public ParticleSystem explosionParticles;

    [Header("הגדרות השמש והמעבר")]
    public Camera mainCamera;
    public GameObject sunObject; // השמש והכוכבים המסתובבים סביבה
    public int totalClicksToGalaxy = 5;

    [Header("הגדרות גלקסיה ומוזיקה")]
    public Color galaxyBackgroundColor = new Color(0.01f, 0.01f, 0.05f);
    public AudioSource backgroundMusic;  // השמע של המוזיקה שנעצרת
    public GameObject groundObject;     // הרצפה שתופיע לאחר 5 קליקים
    public maincaracter playerCharacter; // חיבור לשחקן (maincaracter)

    private int clickCount = 0;
    private bool galaxyActivated = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // עצירת הפעלה אוטומטית של חלקיקי הפיצוץ
        if (explosionParticles != null)
        {
            var main = explosionParticles.main;
            main.playOnAwake = false;

            var emission = explosionParticles.emission;
            emission.rateOverTime = 0;

            explosionParticles.Stop();
        }

        // וידוא שהרצפה מוסתרת בתחילת המשחק
        if (groundObject != null)
        {
            groundObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // פועל רק אם עדיין לא הגענו לגלקסיה
            if (!galaxyActivated)
            {
                // 1. מיקום הפיצוץ בדיוק בנקודת הלחיצה
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

                if (explosionParticles != null)
                {
                    explosionParticles.transform.position = worldPos;
                    explosionParticles.Emit(20); // פולט 20 חלקיקים בלחיצה בלבד
                }

                // 2. ספירת לחיצות
                clickCount++;
                if (clickCount >= totalClicksToGalaxy)
                {
                    galaxyActivated = true;
                    StartCoroutine(TransitionToGalaxy());
                }
            }
        }
    }

    IEnumerator TransitionToGalaxy()
    {
        // א. הפסקת המוזיקה בהדרגה (Fade Out)
        if (backgroundMusic != null)
        {
            StartCoroutine(FadeOutMusic(1.5f));
        }

        // ב. שינוי צבע הרקע
        mainCamera.backgroundColor = galaxyBackgroundColor;

        // ג. יצירת כוכבים מנצנצים ברקע
        CreateScreenFullOfTwinklingStars();

        // ד. הקטנת השמש והזזתה לצד המסך
        float screenHeight = mainCamera.orthographicSize;
        float screenWidth = screenHeight * mainCamera.aspect;
        Vector3 targetPosition = new Vector3(screenWidth - 1.8f, screenHeight - 1.8f, 0f);
        Vector3 targetScale = Vector3.one * 0.6f;

        if (sunObject != null)
        {
            Vector3 startPosition = sunObject.transform.position;
            Vector3 startScale = sunObject.transform.localScale;

            float duration = 2.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                sunObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                sunObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                yield return null;
            }
        }

        // ה. הופעת הרצפה והפעלת נפילת השחקן
        if (groundObject != null)
        {
            groundObject.SetActive(true);

            if (playerCharacter != null)
            {
                // מעביר את גובה הרצפה המדויק לשחקן
                float groundYPos = groundObject.transform.position.y;
                playerCharacter.StartFallingEntry(groundYPos);
            }
        }
    }

    IEnumerator FadeOutMusic(float fadeDuration)
    {
        float startVolume = backgroundMusic.volume;

        while (backgroundMusic.volume > 0)
        {
            backgroundMusic.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        backgroundMusic.Stop();
        backgroundMusic.volume = startVolume;
    }

    void CreateScreenFullOfTwinklingStars()
    {
        GameObject backgroundStars = new GameObject("BackgroundTwinklingStars");
        backgroundStars.transform.position = new Vector3(0, 0, 5f);

        ParticleSystem ps = backgroundStars.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;
        main.startColor = Color.white;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startLifetime = 100f;
        main.maxParticles = 300;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(mainCamera.orthographicSize * mainCamera.aspect * 2f, mainCamera.orthographicSize * 2f, 1f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(Color.white, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.2f, 0.0f),
                new GradientAlphaKey(1.0f, 0.5f),
                new GradientAlphaKey(0.2f, 1.0f)
            }
        );
        colorOverLifetime.color = grad;

        var emission = ps.emission;
        emission.rateOverTime = 0;

        ps.Play();
        ps.Emit(180);
    }
}