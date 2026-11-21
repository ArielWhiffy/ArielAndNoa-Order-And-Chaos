using UnityEngine;

public class CameraFollow1 : MonoBehaviour
{
    [Header("הגדרות מעקב")]
    public Transform player;
    public float smoothSpeed = 5f;

    [Header("גבולות מצלמה")]
    [Tooltip("הגובה המינימלי שהמצלמה יכולה לרדת אליו (כדי לא לעבור את הרצפה)")]
    public float minY = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            float targetY = Mathf.Max(player.position.y, minY);

            Vector3 targetPosition = new Vector3(
                player.position.x,
                targetY,
                -10f
            );

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}