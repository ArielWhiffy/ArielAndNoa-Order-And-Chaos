using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    [Tooltip("השם המדויק של הסצנה שאליה רוצים לעבור")]
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // בודק אם מי שנכנס לדלת הוא השחקן
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("שכחת לכתוב את שם הסצנה הבאה בהגדרות של הדלת!");
            }
        }
    }
}