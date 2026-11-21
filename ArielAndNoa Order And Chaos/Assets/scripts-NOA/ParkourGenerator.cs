using UnityEngine;

public class ParkourGenerator : MonoBehaviour
{
    [Header("אובייקטים ליצירה")]
    public GameObject platformPrefab;
    public GameObject finishObjectPrefab;

    [Header("הגדרות המסלול")]
    public int numberOfPlatforms = 15;
    public float verticalStep = 3.2f;  // גובה בין פלטפורמה לפלטפורמה
    public float horizontalRange = 6f; // פיזור ימינה ושמאלה

    void Start()
    {
        GenerateParkour();
    }

    void GenerateParkour()
    {
        // מתחיל בדיוק מהמיקום של ParkourManager (שתציבי על הרצפה)
        Vector3 spawnPosition = transform.position;

        for (int i = 0; i < numberOfPlatforms; i++)
        {
            // הפלטפורמה הראשונה עולה צעד אחד מעל הרצפה, והבאות ממשיכות לעלות
            spawnPosition.y += verticalStep;
            spawnPosition.x = transform.position.x + Random.Range(-horizontalRange, horizontalRange);

            Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
        }

        if (finishObjectPrefab != null)
        {
            Vector3 finishPos = spawnPosition + new Vector3(0f, 1.5f, 0f);
            Instantiate(finishObjectPrefab, finishPos, Quaternion.identity);
        }
    }
}