using UnityEngine;

public class Follower2D : MonoBehaviour
{
    [Header("Target & Movement Settings")]
    public Transform targetPlayer;
    public float moveSpeed = 4f;
    public float stoppingDistance = 1.5f;

    [Header("Animation / Direction")]
    public bool flipSpriteBasedOnDirection = true;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //FindNewPlayer();
    }

    void Update()
    {
        // אם השחקן השתגר/נוצר מחדש וה-target הפך ל-null, נחפש את השחקן החדש
        if (targetPlayer == null)
        {
            FindNewPlayer();

            // אם עדיין לא נמצה שחקן בסצנה, נעצור את הלולאה לפריים הזה
            if (targetPlayer == null) return;
        }

        // חישוב מרחק ותנועה
        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);

            if (flipSpriteBasedOnDirection && spriteRenderer != null)
            {
                if (direction.x > 0.1f)
                    spriteRenderer.flipX = false;
                else if (direction.x < -0.1f)
                    spriteRenderer.flipX = true;
            }
        }
    }

    // פונקציה למציאת השחקן הנוכחי לפי ה-Tag
    void FindNewPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
        }
    }
}