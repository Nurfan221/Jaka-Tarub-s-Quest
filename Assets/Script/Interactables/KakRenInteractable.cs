using UnityEngine;

public class KakRenInteractable : Interactable
{
    [SerializeField] Dialogues SecDialogue;
    [SerializeField] Dialogues FinishDialogue;
    protected override void Interact()
    {
        // if (GameEventSystem.Instance.DoneDialogue_FirstDesaWarga && !GameEventSystem.Instance.DoneDialogue_FirstKakRen)
        // {
        //     Debug.Log("cerita kak ren jalan ");
        //     DialogueSystem.Instance.StartDialogue(SecDialogue);
        // }

        // if (GameController.CanFinishStory)
        // {
        //     DialogueSystem.Instance.StartDialogue(FinishDialogue);
        // }
    }
}
