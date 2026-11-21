using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 currentRespawnPos;
    public static bool hasCheckpoint = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasCheckpoint = true;
            currentRespawnPos = transform.position;
        }
    }

    private void OnDestroy()
    {
        // מאפס את הצ'ק פוינט כשעוברים סצנה
        hasCheckpoint = false;
    }
}