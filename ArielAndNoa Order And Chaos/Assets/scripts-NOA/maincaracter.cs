using UnityEngine;
using System.Collections;

public class maincaracter : MonoBehaviour
{
    [Header("רכיבים")]
    public Animator animator;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    [Header("הגדרות תנועה וקפיצה")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2.5f;   // גובה הקפיצה
    public float jumpDuration = 0.5f; // משך הקפיצה בשניות

    [Header("נקודת התחלה ונחיתה")]
    public Transform spawnPointUpper;
    public float groundY = -3.5f;
    public float groundOffset = 1.2f; // היסט להתאמת הרגליים לרצפה

    private bool isFallingIn = false;
    private bool isGrounded = false;
    private bool isAttacking = false;
    private bool isJumping = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void StartFallingEntry(float targetGroundY)
    {
        groundY = targetGroundY + groundOffset;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 10;
        }

        if (spawnPointUpper != null)
        {
            transform.position = new Vector3(spawnPointUpper.position.x, spawnPointUpper.position.y, 0f);
        }
        else
        {
            transform.position = new Vector3(0f, 6f, 0f);
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("flip");
            animator.speed = 1f;
        }

        isFallingIn = true;
        isGrounded = false;
    }

    void Update()
    {
        // 1. כניסה ראשונית בציפה ממעלה המסך
        if (isFallingIn)
        {
            transform.Translate(Vector3.down * 6f * Time.deltaTime, Space.World);

            if (transform.position.y <= groundY)
            {
                transform.position = new Vector3(transform.position.x, groundY, 0f);
                isFallingIn = false;
                isGrounded = true;

                if (animator != null)
                {
                    animator.Play("walk");
                    animator.speed = 0;
                }
            }
            return;
        }

        // 2. פעולות שחקן
        if (isGrounded)
        {
            HandleActions();
        }
    }

    void HandleActions()
    {
        // א. מקש לחימה (F / Ctrl)
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (animator != null && !isAttacking && !isJumping)
            {
                StartCoroutine(PlayAttackAnimation());
            }
            return;
        }

        if (isAttacking) return;

        // ב. מקש קפיצה (Space)
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartCoroutine(PerformJump());
            return;
        }

        // ג. תנועה ימינה/שמאלה
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
        {
            transform.Translate(Vector3.right * moveInput * moveSpeed * Time.deltaTime);

            if (moveInput > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (moveInput < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            if (animator != null && !isJumping)
            {
                animator.Play("walk");
                animator.speed = 1f;
            }
        }
        else
        {
            if (animator != null && !isJumping)
            {
                animator.speed = 0f;
            }
        }
    }

    // קורוטינה שמבצעת תנועה קשתית של קפיצה
    IEnumerator PerformJump()
    {
        isJumping = true;

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.Play("jump");
        }

        float elapsedTime = 0f;
        float startY = groundY;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;

            // חישוב גובה קשתי (עולה ויורד)
            float currentY = startY + Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

            yield return null;
        }

        // החזרה לרצפה בסיום הקפיצה
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        isJumping = false;
    }

    IEnumerator PlayAttackAnimation()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.Play("fight");
        }

        yield return new WaitForSeconds(0.6f);

        isAttacking = false;
    }
}