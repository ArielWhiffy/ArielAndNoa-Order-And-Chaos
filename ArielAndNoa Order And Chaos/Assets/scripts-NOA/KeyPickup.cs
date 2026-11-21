using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("הגדרות מיקום")]
    public Transform holdPoint;
    public Vector3 offset = new Vector3(0.5f, 0.2f, 0f);

    [Header("הגדרות תצוגה")]
    public bool hideOnPickup = false;

    private bool isPickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isPickedUp && other.CompareTag("Player"))
        {
            PickUp(other.transform);
        }
    }

    void PickUp(Transform playerTransform)
    {
        isPickedUp = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        if (hideOnPickup)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (holdPoint != null)
            {
                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(playerTransform);
                transform.localPosition = offset;
            }
        }

        maincaracter player = playerTransform.GetComponent<maincaracter>();
        if (player != null)
        {
            player.hasKey = true;
            Debug.Log("המפתח נאסף בהצלחה!");
        }
        else
        {
            Debug.LogWarning("לא נמצא סקריפט maincaracter על השחקן!");
        }
    }
}