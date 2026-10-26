using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("יעד למעקב")]
    public Transform target; // גרי לכאן את השחקן

    [Header("הגדרות מעקב")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}