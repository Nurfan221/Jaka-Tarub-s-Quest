using UnityEngine;


public class PlayerQuest : MonoBehaviour
{

    [SerializeField] QuestManager questManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] LocationManager locationManager;
    public GameObject locationMainQuest;
    public Dialogues dialogueInLocation;
    public GameObject environmentObject;
    public bool mainQuestInLocation = false;
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
    if (!mainQuestInLocation)
    {
        if (other.gameObject == locationMainQuest)
        {
            // Cek apakah objek yang masuk adalah quest location
            switch (questManager.currentMainQuest.indexLocation)
            {
                case 0:
                    ProsesLocationMainQuest(other, MainQuest1State.CariRusa);
                    break;
                case 1:
                    ProsesLocationMainQuest(other, MainQuest1State.SceneDanauIndah);
                    break;
                
            }
        }
    }

    // Bagian ini untuk child atau lokasi spesifik lain, gak bergantung ke flag quest utama
    LocationConfiguration locationConfiguration = other.GetComponent<LocationConfiguration>();
    if (locationConfiguration != null)
    {
        if (locationConfiguration.lokasiSaatIni == Lokasi.Danau)
        {
                dialogueSystem.theDialogues = questManager.currentMainQuest.dialogueQuest[6];
                dialogueSystem.StartDialogue();
                locationConfiguration.inDanau = true;
        }else if(locationConfiguration.lokasiSaatIni == Lokasi.RumahJaka)
            {
                locationConfiguration.inRumahJaka = true;
            }
    }
}


    public void OnTriggerExit(Collider other)
    {
        LocationConfiguration locationConfiguration = other.GetComponent<LocationConfiguration>();
        if (locationConfiguration != null)
        {
            if (locationConfiguration.lokasiSaatIni == Lokasi.Danau)
            {
                if (locationConfiguration.mainQuestDanau)
                {
                    locationConfiguration.inDanau = false;
                    locationConfiguration.mainQuestDanau = false;

                    dialogueSystem.theDialogues = questManager.currentMainQuest.dialogueQuest[7];
                    dialogueSystem.StartDialogue();
                    questManager.currentMainQuest.indexLocation += 1;

                    questManager.currentMainQuest.currentQuestState = MainQuest1State.Pulang;
                    questManager.NextQuestState();


                }
                else
                {
                    locationConfiguration.inDanau = false;
                }
            }
        }

    }

    public void ProsesLocationMainQuest(Collider2D other, MainQuest1State mainQuest1State)
    {
        if (other.gameObject == locationMainQuest && !mainQuestInLocation)
        {
            // Panggil fungsi playMainLocationQuest dengan index yang sesuai
            int indexLocation = questManager.currentMainQuest.indexLocation;
            dialogueSystem.theDialogues = dialogueInLocation;
            dialogueSystem.StartDialogue();

            StartCoroutine(dialogueSystem.WaitForDialogueToEnd());

            questManager.currentMainQuest.currentQuestState = mainQuest1State;

            questManager.NextQuestState();


            // Set agar fungsi tidak bisa dipanggil lagi
            mainQuestInLocation = true;
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
