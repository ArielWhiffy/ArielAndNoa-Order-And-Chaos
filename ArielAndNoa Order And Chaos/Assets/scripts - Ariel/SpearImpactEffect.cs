using UnityEngine;

public class SpearImpactEffect : MonoBehaviour
{
    [Header("Impact Settings")]
    public GameObject slamEffectPrefab; // הפריפאב של האפקט
    public Transform spearTip;          // מיקום קצה החנית

    // הפונקציה שתופעל מתוך האנימציה
    public void PlaySpearSlamEffect()
    {
        if (slamEffectPrefab != null && spearTip != null)
        {
            // יצירת האפקט במיקום קצה החנית
            GameObject effectInstance = Instantiate(slamEffectPrefab, spearTip.position, Quaternion.identity);

            // מחיקת אובייקט האפקט כעבור 2 שניות למניעת עומס
            Destroy(effectInstance, 2f);
        }
    }
}
