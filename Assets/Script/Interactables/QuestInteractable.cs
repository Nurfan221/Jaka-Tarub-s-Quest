using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainQuest1_Controller;

public class QuestInteractable : Interactable
{
    [SerializeField] QuestManager questManager;
    [SerializeField] NPCManager npcManager;
    [SerializeField] NPCBehavior npcBehavior;
    [SerializeField] protected DialogueSystem dialogueSystem;
    public Dialogues currentDialogue; // Properti untuk menyimpan dialog
    public GameObject npcObject;
    // Variabel ini menandakan apakah NPC ini penting untuk sebuah quest
    public bool isQuestNPC = false;
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

        if (!isQuestNPC && isJob)
        {
            dialogueSystem.theDialogues = currentDialogue;
            dialogueSystem.StartDialogue();
            if (npcManager.GiveReward(npcBehavior.pekerjaanNPC))
            {
                isJob = true;
            }
            else
            {
                isJob = false;
            }
            return;
        }
       

        // Coba dapatkan MainQuest1_Controller dari quest yang aktif
        MainQuest1_Controller mq1 = questManager.CurrentActiveQuest as MainQuest1_Controller;

        // Jika quest yang aktif adalah MainQuest1...
        if (mq1 != null)
        {
            Debug.Log("Dialogue npc di picu");
            // Beritahu skrip quest tersebut bahwa NPC ini di-interact
            NPCBehavior npcBehavior = gameObject.GetComponent<NPCBehavior>();
            mq1.OnNPCInteracted(npcBehavior.npcName);
           

        }
        else
        {
            // Jika main quest lain yang aktif atau tidak ada main quest,
            // mainkan dialog basa-basi.
            dialogueSystem.theDialogues = currentDialogue;
            dialogueSystem.StartDialogue();
        }

        
       
    }

    public void SetCurrentDialogue(Dialogues dialogue)
    {
        currentDialogue = dialogue;
    }

}
