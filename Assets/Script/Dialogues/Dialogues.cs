using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string name;
    [TextArea(1, 5)]
    public string sentence;
}

[CreateAssetMenu(menuName = "Create a dialogue")]
public class Dialogues : ScriptableObject
{
    

    public List<Dialogue> TheDialogues;
    public string mainSpeaker;

    public string promptAfterDialogue;
    public void AfterDialogue()
    {
        // GameEventSystem.Instance.DoAfterDialogue(promptAfterDialogue);
    }
}
