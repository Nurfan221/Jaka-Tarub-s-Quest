using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInteractable : Interactable
{
    [SerializeField] QuestManager questManager;
    [SerializeField] protected DialogueSystem dialogueSystem;
    public Dialogues currentDialogue; // Properti untuk menyimpan dialog



    void Start()
    {
        // Jika tidak diassign melalui Inspector, berikan peringatan
        if (questManager == null || dialogueSystem == null)
        {
            Debug.LogWarning("QuestManager atau DialogueSystem belum dihubungkan di Inspector!");
        }

        
    }

   protected override void Interact()
    {
        if (currentDialogue != null)
        {
            dialogueSystem.theDialogues = currentDialogue;
            dialogueSystem.StartDialogue();
        }
        else
        {
            Debug.LogWarning("No dialogue assigned to this NPC!");
        }
    }
}
