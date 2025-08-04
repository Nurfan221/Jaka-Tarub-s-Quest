using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : Interactable
{

    [SerializeField] NPCBehavior npcBehavior;
    //[SerializeField] protected DialogueSystem dialogueSystem;



    void Start()
    {



        npcBehavior = gameObject.GetComponent<NPCBehavior>();

        
    }

   protected override void Interact()
   {
        if (npcBehavior.isLockedForQuest)
        {
            DialogueSystem.Instance.HandlePlayDialogue(npcBehavior.questOverrideDialogue);
        }
        else
        {
            // Jika NPC tidak terkunci, tampilkan dialog normal
            DialogueSystem.Instance.HandlePlayDialogue(npcBehavior.normalDialogue);
        }


   }



}
