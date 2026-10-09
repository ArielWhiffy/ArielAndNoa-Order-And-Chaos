using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // ����� ���� ������ (�����/�����) �� A/D
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        // ���� ����� ���� �-X ������ �� ������ ��� �-Y ����� (������/�����)
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}
