using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : Interactable
{
    public NPCBehavior npcBehavior;
    //[SerializeField] protected DialogueSystem dialogueSystem;

    private void OnEnable()
    {
        DialogueSystem.OnDialogueEnded += DialogueSystem_OnDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueSystem.OnDialogueEnded -= DialogueSystem_OnDialogueEnded;
    }
    private void DialogueSystem_OnDialogueEnded()
    {
        if (npcBehavior.isLockedForQuest)
        {
            npcBehavior.ReturnToPreQuestPosition();
        }
    }

    void Start()
    {



        npcBehavior = gameObject.GetComponent<NPCBehavior>();

        
    }

   protected override void Interact()
   {
        if (npcBehavior.isLockedForQuest)
        {
            DialogueSystem.Instance.HandlePlayDialogue(npcBehavior.questOverrideDialogue);
            if (npcBehavior && npcBehavior.isGivenItemForQuest)
            {
                Debug.Log($" NPC {npcBehavior.npcName} sudah diberikan item untuk quest.");
                ItemPool.Instance.AddItem(npcBehavior.itemQuestToGive);
                npcBehavior.isGivenItemForQuest = true;
                //StartCoroutine(NPCGiveItemCoroutine());
            }
        }
        else
        {
            // Jika NPC tidak terkunci, tampilkan dialog normal
            DialogueSystem.Instance.HandlePlayDialogue(npcBehavior.normalDialogue);
        }


   }




}
