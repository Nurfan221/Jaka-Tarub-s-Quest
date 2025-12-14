using UnityEngine;

public class WargaDesaInteractable : Interactable
{
    [SerializeField] Dialogues theDialogue;
    protected override void Interact()
    {
        // if (GameEventSystem.Instance.DoneDialogue_DanauPertamaKeDesa && !GameEventSystem.Instance.DoneDialogue_FirstKakRen)
        // {
        //     DialogueSystem.Instance.StartDialogue(theDialogue);
        // }
    }
}
