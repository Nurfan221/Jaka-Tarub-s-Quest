using UnityEngine;

public class MiniQuestInteractable : Interactable
{
    [SerializeField] MiniQuestUI miniQuestUI;
    public bool isObjectBroken = true;
    public Dialogues brokenObjectDialogue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void Interact()
    {
        if (isObjectBroken)
        {
            DialogueSystem.Instance.HandlePlayDialogue(brokenObjectDialogue);
        }
        else
        {
            miniQuestUI.Open();

        }
    }

}