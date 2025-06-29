using System;
using UnityEngine;
using static LocationManager;


public class PlayerQuest : MonoBehaviour
{

    // Ini adalah "pengeras suara" yang bisa didengar oleh skrip lain.
    // Ia akan menyiarkan sebuah string (nama lokasi) saat pemain masuk ke sebuah lokasi.
    public static event Action<string> OnPlayerEnteredLocation;


    [SerializeField] QuestManager questManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] LocationManager locationManager;
    [SerializeField] Player_Health player_Health;
    //public GameObject locationMainQuest;
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
        // Cek apakah objek yang kita masuki punya skrip LocationTrigger
        LocationConfiguration location = other.GetComponent<LocationConfiguration>();

        if (location != null)
        {
            // "Teriakkan" atau siarkan nama lokasi tersebut ke seluruh penjuru game.
            Debug.Log($"Pemain masuk ke lokasi: {location.locationName}");
            OnPlayerEnteredLocation?.Invoke(location.locationName);
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

  
    //public void ProsesLocationMainQuest(Collider2D other, MainQuest1State mainQuest1State)
    //{
    //    Debug.Log("proses location main quest di panggil");
    //    if (other.gameObject == locationMainQuest && !mainQuestInLocation)
    //    {
    //        // Panggil fungsi playMainLocationQuest dengan index yang sesuai
    //        //int indexLocation = questManager.currentMainQuest.indexLocation;
    //        dialogueSystem.theDialogues = dialogueInLocation;
    //        dialogueSystem.StartDialogue();

    //        StartCoroutine(dialogueSystem.WaitForDialogueToEnd());

    //        //questManager.currentMainQuest.currentQuestState = mainQuest1State;

    //        questManager.NextQuestState();


    //        // Set agar fungsi tidak bisa dipanggil lagi
    //        mainQuestInLocation = true;
    //    }
    //}

    //public void CariRusa()
    //{
    //    Transform childTransform = locationMainQuest.transform.Find("Domba");
    //    if (childTransform != null)
    //    {
    //        environmentObject = childTransform.gameObject;
    //        environmentObject.gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Child tidak ditemukan!");
    //    }
    //}

}
