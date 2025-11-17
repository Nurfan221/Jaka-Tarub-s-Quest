using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
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
        if (npcBehavior.isLockedForQuest && npcBehavior.islocked && DialogueSystem.Instance.npcName == npcBehavior.npcName)
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
            DialogueSystem.Instance.npcName = npcBehavior.npcName;
            DialogueSystem.Instance.HandlePlayDialogue(npcBehavior.questOverrideDialogue);
            if (npcBehavior && npcBehavior.isGivenItemForQuest)
            {
                Debug.Log($" NPC {npcBehavior.npcName} sudah diberikan item untuk quest.");

                // Di dalam CookUI / Result Button Listener
                bool isSuccess = ItemPool.Instance.AddItem(npcBehavior.itemQuestToGive);

                if (isSuccess)
                {
                    // Hapus item dari tungku
                    npcBehavior.isGivenItemForQuest = true;
                }
                else
                {
                    // Jangan hapus, biarkan di tungku
                    Debug.Log("Tas penuh, item tetap di tungku.");
                    // Opsional: Munculkan teks "Tas Penuh!"
                }
                //StartCoroutine(NPCGiveItemCoroutine());
            }
        }
        else
        {
            // Jika NPC tidak terkunci, tampilkan dialog normal
            DialogueSystem.Instance.npcName = npcBehavior.npcName;
            DialogueSystem.Instance.HandlePlayDialogue(npcBehavior.normalDialogue);
        }


   }




}
