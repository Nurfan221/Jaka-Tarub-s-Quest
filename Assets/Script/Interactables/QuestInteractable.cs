using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInteractable : Interactable
{
    [SerializeField] QuestManager questManager;
    [SerializeField] NPCManager npcManager;
    [SerializeField] NPCBehavior npcBehavior;
    [SerializeField] protected DialogueSystem dialogueSystem;
    public Dialogues currentDialogue; // Properti untuk menyimpan dialog
    public GameObject npcObject;
    public bool isQuest;
    public bool isJob;



    void Start()
    {

        // Jika tidak diassign melalui Inspector, berikan peringatan
        if (questManager == null || dialogueSystem == null)
        {
            Debug.LogWarning("QuestManager atau DialogueSystem belum dihubungkan di Inspector!");
        }

        npcBehavior = gameObject.GetComponent<NPCBehavior>();

        
    }

   protected override void Interact()
   {
        if (isQuest)
        {
            if (currentDialogue != null)
            {
                dialogueSystem.theDialogues = currentDialogue;

                // Beri tahu DialogueSystem NPC mana yang harus dihapus setelah dialog selesai
                dialogueSystem.npcToDestroy = npcObject;

                dialogueSystem.StartDialogue();
                

                //if (npcObject != null)
                //{

                //    //mulai dialogue untuk mimpi jaka tarub
                //    questManager.CurrentActiveQuest.currentQuestState = 
                //}

            }
            else
            {
                Debug.LogWarning("No dialogue assigned to this NPC!");
            }
        }else if(isJob)
        {
            if (npcManager.GiveReward(npcBehavior.pekerjaanNPC))
            {
                isJob = true;
            }
            else
            {
                isJob= false;
            }
        }
    }

    public void SetCurrentDialogue(Dialogues dialogue)
    {
        currentDialogue = dialogue;
    }
}
