using UnityEngine;
using TMPro;

public class NPCDialogueTrigger2D : MonoBehaviour
{
    [Header("Dialogue Content")]
    [TextArea(2, 5)]
    public string[] dialogueLines; // רשימת המשפטים של הדמות

    [Header("UI Reference")]
    public GameObject dialogueBox;     // חלון הדיאלוג
    public TMP_Text dialogueText;      // רכיב הטקסט

    private int currentLineIndex = 0;
    private bool isPlayerInRange = false;

    private void Update()
    {
        // מעבר למשפט הבא בלחיצה על מקש E כשהשחקן בטווח
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentLineIndex = 0; // התחלה מהמשפט הראשון
            ShowDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideDialogue();
        }
    }

    private void ShowDialogue()
    {
        if (dialogueBox != null) dialogueBox.SetActive(true);
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (dialogueText != null && dialogueLines.Length > 0)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;

        // אם הגענו לסוף רשימת המשפטים - סוגרים את חלון הדיאלוג
        if (currentLineIndex >= dialogueLines.Length)
        {
            HideDialogue();
        }
        else
        {
            DisplayCurrentLine();
        }
    }

    private void HideDialogue()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }
}