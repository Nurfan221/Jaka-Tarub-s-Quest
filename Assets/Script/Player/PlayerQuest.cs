using UnityEngine;


public class PlayerQuest : MonoBehaviour
{

    [SerializeField] QuestManager questManager;
    [SerializeField] DialogueSystem dialogueSystem;
    public GameObject locationMainQuest;
    public Dialogues dialogueInLocation;
    public GameObject environmentObject;
    public bool inLocation = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pastikan hanya memicu sekali
        if (!inLocation)
        {
            // Cek apakah objek yang masuk adalah
            if (other.gameObject == locationMainQuest && !inLocation)
            {
                // Panggil fungsi playMainLocationQuest dengan index yang sesuai
                int indexLocation = questManager.currentMainQuest.indexLocation;
                dialogueSystem.theDialogues = dialogueInLocation;
                dialogueSystem.StartDialogue();

                StartCoroutine(dialogueSystem.WaitForDialogueToEnd());

                questManager.currentMainQuest.currentQuestState = MainQuest1State.CariRusa;

                questManager.NextQuestState();

               
                //// Set agar fungsi tidak bisa dipanggil lagi
                inLocation = true;
            }
            

        }

        

    }

    public void CariRusa()
    {
        Transform childTransform = locationMainQuest.transform.Find("Domba");
        if (childTransform != null)
        {
            environmentObject = childTransform.gameObject;
            environmentObject.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Child tidak ditemukan!");
        }
    }

}
