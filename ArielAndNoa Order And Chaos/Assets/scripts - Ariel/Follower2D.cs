using UnityEngine;

public class Follower2D : MonoBehaviour
{
    [Header("Target & Movement Settings")]
    [Tooltip("Drag your Main Player GameObject here")]
    public Transform targetPlayer;

    public float moveSpeed = 4f;

    [Tooltip("Distance to keep from the player so they don't overlap")]
    public float stoppingDistance = 1.5f;

    [Header("Animation / Direction (Optional)")]
    public bool flipSpriteBasedOnDirection = true;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // If targetPlayer is not assigned manually in Inspector, try to find by tag
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (targetPlayer == null) return;

        // Calculate distance between follower and player
        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        // Move only if the follower is further away than the stopping distance
        if (distanceToPlayer > stoppingDistance)
        {
            // Calculate direction towards player
            Vector3 direction = (targetPlayer.position - transform.position).normalized;

            // Move towards player position
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);

            // Flip sprite depending on movement direction (Left/Right)
            if (flipSpriteBasedOnDirection && spriteRenderer != null)
            {
                if (direction.x > 0.1f)
                {
                    spriteRenderer.flipX = false;
                }
                else if (direction.x < -0.1f)
                {
                    spriteRenderer.flipX = true;
                }
            }
        }
    }
}