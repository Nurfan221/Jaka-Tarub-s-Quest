using UnityEngine;

public class FirstDialogueInteractable : Interactable
{

    [SerializeField] Dialogues firDialogue;
    protected override void Interact()
    {
        SoundManager.Instance.PlaySound("BGMDanau");
        // if (GameEventSystem.Instance.DoneDialogue_TamashiiGiveName && !GameEventSystem.Instance.DoneDialogue_DanauPertamaKeDesa)
        //     DialogueSystem.Instance.StartDialogue(firDialogue);
    }
}
