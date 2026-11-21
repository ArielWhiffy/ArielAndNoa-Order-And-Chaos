using UnityEngine;

public class TemplePlayer : MonoBehaviour
{
    [Header("תנועה וקפיצה")]
    public float moveSpeed = 5.5f;
    public float jumpForce = 11f;
    public float gravity = 28f;

    [Header("רכיבים (יישאבו אוטומטית)")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public bool hasKey = false;

    [HideInInspector] public float currentFloorY;
    [HideInInspector] public bool isOnRope = false;

    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 10;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.freezeRotation = true;
        }

        currentFloorY = transform.position.y;
        isGrounded = true;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.enabled = true;
            animator.Play("walk");
            animator.speed = 0f;
        }
    }

    void Update()
    {
        if (isOnRope)
        {
            verticalVelocity = 0f;
            isGrounded = false;
            return;
        }

        // 1. לחימה (F / Ctrl)
        if ((Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.LeftControl)) && !isAttacking)
        {
            isAttacking = true;
            attackTimer = 0.5f;
            if (animator != null)
            {
                animator.speed = 1f;
                animator.Play("fight");
            }
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f) isAttacking = false;
            return;
        }

        // 2. קפיצה (Space או חץ למעלה)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            if (animator != null)
            {
                animator.speed = 1f;
                animator.Play("jump");
            }
        }

        // 3. תנועה אופקית (חצים או A/D)
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            transform.position += Vector3.right * moveInput * moveSpeed * Time.deltaTime;

            float absX = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(moveInput > 0 ? absX : -absX, transform.localScale.y, transform.localScale.z);

            if (animator != null && isGrounded)
            {
                animator.Play("walk");
                animator.speed = 1f;
            }
        }
        else if (animator != null && isGrounded)
        {
            animator.speed = 0f;
        }

        // 4. כבידה ונחיתה על הרצפה/מדרגה הנוכחית
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            float nextY = transform.position.y + verticalVelocity * Time.deltaTime;

            if (nextY <= currentFloorY && verticalVelocity <= 0f)
            {
                nextY = currentFloorY;
                verticalVelocity = 0f;
                isGrounded = true;
                if (animator != null)
                {
                    animator.Play("walk");
                    animator.speed = 0f;
                }
            }

            transform.position = new Vector3(transform.position.x, nextY, transform.position.z);
        }
        else
        {
            if (transform.position.y > currentFloorY + 0.05f)
            {
                isGrounded = false;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, currentFloorY, transform.position.z);
            }
        }
    }

    public void LaunchFromRope(float forwardSpeed, float upForce)
    {
        isOnRope = false;
        isGrounded = false;
        verticalVelocity = upForce;
        transform.position += new Vector3(forwardSpeed * 0.15f, 0.2f, 0f);
    }
}