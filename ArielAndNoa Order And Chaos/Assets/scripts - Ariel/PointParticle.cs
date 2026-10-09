using UnityEngine;

public class PointParticle : MonoBehaviour
{
    private Vector3 targetPosition;
    private Vector3 targetScale;
    private Vector3 currentVelocityPos;
    private Vector3 currentVelocityScale;
    private bool isMovingToTarget = false;
    private float smoothTime = 3f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InitParticle(Vector3 targetPos, Vector3 targetScal, Color targetColor, float explosionForce, float duration)
    {
        targetPosition = targetPos;
        targetScale = targetScal;
        smoothTime = duration;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }

        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        transform.position += randomDirection * explosionForce;

        isMovingToTarget = true;
    }

    void Update()
    {
        if (!isMovingToTarget) return;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocityPos, smoothTime);
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref currentVelocityScale, smoothTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.005f)
        {
            transform.position = targetPosition;
            transform.localScale = targetScale;
            isMovingToTarget = false;
        }
    }
}