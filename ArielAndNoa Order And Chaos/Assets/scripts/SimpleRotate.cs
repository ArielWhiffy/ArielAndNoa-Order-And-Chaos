using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [Header("מטרת הסיבוב")]
    public Transform target; // האובייקט שסביבו נסתובב (למשל המרכז)

    [Header("הגדרות תנועה")]
    public Vector3 axis = Vector3.forward; // ציר הסיבוב (ב-2D משתמשים ב-forward / ציר Z)
    public float rotationSpeed = 50f;      // מהירות הסיבוב

    void Update()
    {
        if (target != null)
        {
            // סיבוב האובייקט סביב המטרה לפי הציר והמהירות
            transform.RotateAround(target.position, axis, rotationSpeed * Time.deltaTime);
        }
    }
}