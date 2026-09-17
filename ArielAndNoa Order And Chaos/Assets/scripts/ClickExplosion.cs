using UnityEngine;

public class ClickExplosion : MonoBehaviour
{
    public ParticleSystem explosionParticles;

    [Header("הגדרות צבעי מסך")]
    public Camera mainCamera;
    public Color[] screenColors = new Color[] { Color.black, Color.blue, Color.magenta, Color.cyan }; // רשימת צבעים
    public int clicksToChangeColor = 3; // כל כמה לחיצות הצבע יתחלף

    private int clickCount = 0;
    private int currentColorIndex = 0;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. מיקום והפעלת הפיצוץ
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            explosionParticles.transform.position = worldPos;
            explosionParticles.Play();

            // 2. ספירת לחיצות ושינוי צבע הרקע
            clickCount++;
            if (clickCount >= clicksToChangeColor)
            {
                clickCount = 0;
                currentColorIndex = (currentColorIndex + 1) % screenColors.Length;
                mainCamera.backgroundColor = screenColors[currentColorIndex];
            }
        }
    }
}