using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;
    string playerName;

    [SerializeField] Dialogues[] theDialogues;
    Dialogues currentDialogues;
    Queue<Dialogues.Dialogue> dialogues = new();

    [SerializeField] float dialogueSpd = 2;
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TMP_Text[] speakersText;
    string firstSpeaker;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] TMP_Text narrationText;

    [SerializeField] Button NextButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartDialogue(theDialogues[1]);
        }
    }

    public void StartFirstDialogue()
    {
        StartDialogue(theDialogues[0]);
    }

    public void StartDialogue(Dialogues theDialogues)
    {
        GameController.Instance.PauseGame();

        dialogueUI.SetActive(true);
        currentDialogues = theDialogues;
        firstSpeaker = theDialogues.mainSpeaker;
        dialogues = new();
        foreach (Dialogues.Dialogue dialogue in theDialogues.TheDialogues)
        {
            dialogues.Enqueue(dialogue);
        }

        NextButton.onClick.RemoveAllListeners();
        NextButton.onClick.AddListener(NextDialogue);

        NextDialogue();
    }

    public void NextDialogue()
    {
        if (dialogues.Count == 0)
        {
            EndDialogue();
            return;
        }

        Dialogues.Dialogue theDialogue = dialogues.Dequeue();
        Dialogues.Dialogue dialogue = new();

        string playerName = GameController.Instance.playerName;
        dialogue.sentence = theDialogue.sentence.Replace("Charibert", playerName);
        dialogue.name = theDialogue.name.Replace("Charibert", playerName);


        if (dialogue.name == string.Empty)
        {
            speakersText[0].gameObject.SetActive(false);
            speakersText[1].gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(false);

            narrationText.gameObject.SetActive(true);
            SetText(narrationText, dialogue.sentence);
        }
        else
        {
            dialogueText.gameObject.SetActive(true);
            narrationText.gameObject.SetActive(false);

            if (dialogue.name == firstSpeaker || dialogue.name == playerName)
            {
                speakersText[0].text = dialogue.name;
                speakersText[0].gameObject.SetActive(true);
                speakersText[1].gameObject.SetActive(false);
            }
            else
            {
                speakersText[1].text = dialogue.name;
                speakersText[0].gameObject.SetActive(false);
                speakersText[1].gameObject.SetActive(true);
            }

            SetText(dialogueText, dialogue.sentence);
        }
    }

    public void EndDialogue()
    {
        print("End of conversations");
        currentDialogues.AfterDialogue();
        dialogueUI.SetActive(false);
        GameController.Instance.ResumeGame();
    }


    void SetText(TMP_Text text, string value)
    {
        StartCoroutine(SettingText(text, value, dialogueSpd));
    }
    IEnumerator SettingText(TMP_Text text, string value, float dur = 1)
    {
        float startTime = Time.time;

        NextButton.onClick.RemoveAllListeners();
        NextButton.onClick.AddListener(StopAllCoroutines);
        NextButton.onClick.AddListener(() => text.text = value);
        NextButton.onClick.AddListener(() => NextButton.onClick.RemoveAllListeners());
        NextButton.onClick.AddListener(() => NextButton.onClick.AddListener(NextDialogue));
        //while (text.text != value)
        //{
        //    text.text = value[..Mathf.Min((int)((Time.time - startTime) / dur * value.Length), value.Length)];
        //    yield return null;
        //}

        for (int i = 0; i < value.Length; i++)
        {
            text.text = value[..(i + 1)];
            yield return null;
        }
        NextButton.onClick.RemoveAllListeners();
        NextButton.onClick.AddListener(NextDialogue);

    }

      public void PlayClickSound()
    {
        SoundManager.Instance.PlaySound("Click");
    }
}
