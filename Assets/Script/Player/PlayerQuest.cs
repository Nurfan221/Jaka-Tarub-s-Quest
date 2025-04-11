using UnityEngine;
using static LocationManager;


public class PlayerQuest : MonoBehaviour
{

    [SerializeField] QuestManager questManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] LocationManager locationManager;
    [SerializeField] Player_Health player_Health;
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
        // Pastikan hanya memicu sekali
        if (!mainQuestInLocation)
        {
            // Cek lokasi quest utama seperti biasa
            if (other.gameObject == locationMainQuest)
            {
                switch (questManager.currentMainQuest.indexLocation)
                {
                    case 0:
                        ProsesLocationMainQuest(other, MainQuest1State.CariRusa);
                        break;
                    case 1:
                        ProsesLocationMainQuest(other, MainQuest1State.SceneDanauIndah);
                        break;
                    case 2:
                        ProsesLocationMainQuest(other, MainQuest1State.KabarKesedihan);
                        player_Health.isInGrief = true;
                        player_Health.emotionalHealthCap = player_Health.maxHealth /2;
                        player_Health.emotionalStaminaCap = player_Health.maxStamina /2;

                        break;
                    case 3:
                        ProsesLocationMainQuest(other, MainQuest1State.PermintaanMamat);
                        break;
                }
            }
        }

        // Loop untuk memeriksa semua lokasi dalam LocationArray
        foreach (GameObjectLocation gol in locationManager.LocationArray)
        {
            if (other.gameObject == gol.Location)
            {
                //Debug.Log("Sedang di lokasi : " + locationMainQuest.name);
                // Panggil logika tambahan jika lokasi terdeteksi

                if (locationMainQuest != null && locationMainQuest.name == "SekitarDanau" && !locationManager.mainQuestDanau)
                {
                    locationManager.mainQuestDanau = true;
                   
                }
                locationManager.HandleLocationEnter(gol);
            }
        }
    }


    public void OnTriggerExit2D(Collider2D collision)
    {
        foreach (GameObjectLocation gol in locationManager.LocationArray)
        {
            if (collision.gameObject == gol.Location)
            {

                // Panggil logika tambahan jika lokasi terdeteksi
                locationManager.HandleLocationExit(gol);
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
