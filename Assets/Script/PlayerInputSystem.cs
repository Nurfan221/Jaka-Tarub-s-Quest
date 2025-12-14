using TMPro;
using UnityEngine;

public class PlayerInputSystem : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Dialogues dialogueAfter;


    public void InputPlayerName()
    {
        GameController.Instance.playerName = inputField.text;
        // DialogueSystem.Instance.StartDialogue(dialogueAfter);

    }
}
