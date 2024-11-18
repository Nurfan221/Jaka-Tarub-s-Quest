using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedangRenInteractable : Interactable
{
    [SerializeField] Dialogues pedangRenDialogue;

    private void Update()
    {
        // if (GameEventSystem.Instance.DoneDialogue_FirstKakRen && !GameEventSystem.Instance.DoneDialogue_FirstBandit)
        //     promptMessage = "Lihat Pedang";
        // else
        //     promptMessage = " ";

    }
    protected override void Interact()
    {
        // if (GameEventSystem.Instance.DoneDialogue_FirstKakRen && !GameEventSystem.Instance.DoneDialogue_FirstBandit)
        // {
        //     DialogueSystem.Instance.StartDialogue(pedangRenDialogue);
        //     Destroy(gameObject);
        // }
    }
}
