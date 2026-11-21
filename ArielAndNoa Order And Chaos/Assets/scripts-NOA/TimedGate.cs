using UnityEngine;

public class TimedGate : MonoBehaviour
{
    [Header("הגדרות תזמון השער")]
    public float openTime = 2f;
    public float closedTime = 2f;
    public float startDelay = 0f;

    [Header("החזרת שחקן (אופציונלי)")]
    public Transform respawnPoint;

    private SpriteRenderer spriteRenderer;
    private Collider2D gateCollider;
    private float timer;
    private bool isOpen = false;
    private Vector3 defaultStartPos;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gateCollider = GetComponent<Collider2D>();
        timer = startDelay;
        UpdateGateState();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            defaultStartPos = player.transform.position;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isOpen = !isOpen;
            timer = isOpen ? openTime : closedTime;
            UpdateGateState();
        }
    }

    void UpdateGateState()
    {
        if (gateCollider != null) gateCollider.enabled = !isOpen;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = isOpen ? 0.2f : 1f;
            spriteRenderer.color = c;
        }
    }

    private Vector3 GetRespawnTarget()
    {
        // 1. אם השחקן כבר עבר צ'ק פוינט - חוזר לצ'ק פוינט!
        if (Checkpoint.hasCheckpoint) return Checkpoint.currentRespawnPos;
        // 2. אחרת, אם הוגדרה נקודה ידנית - חוזר אליה
        if (respawnPoint != null) return respawnPoint.position;
        // 3. אחרת - חוזר לתחילת הסצנה
        return defaultStartPos;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            other.transform.position = GetRespawnTarget();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isOpen && collision.gameObject.CompareTag("Player"))
        {
            collision.transform.position = GetRespawnTarget();
        }
    }
}