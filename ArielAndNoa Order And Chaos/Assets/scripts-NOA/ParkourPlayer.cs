using UnityEngine;

public class ParkourPlayer : MonoBehaviour
{
    [Header("הגדרות תנועה וקפיצה")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f; // עוצמת הקפיצה

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // מוודא שהפיזיקה פעילה ושהשחקן לא מתהפך על הצד
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // תנועה ימינה ושמאלה
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // היפוך כיוון הדמות
        if (moveInput > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // קפיצה כשלוחצים רווח והשחקן נוגע בפלטפורמה
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            if (animator != null) animator.Play("jump");
        }
    }

    // מזהה כשהשחקן נוחת על פלטפורמה מלמעלה
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            if (animator != null) animator.Play("walk");
        }
    }
}