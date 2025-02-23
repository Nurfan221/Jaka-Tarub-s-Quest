using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static QuestManager;
//using Microsoft.Unity.VisualStudio.Editor;
using static UnityEditor.Progress;
using System.Net.NetworkInformation;


public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class Chapter
    {
        public int idChapter;
        public Quest[] sideQuest;
        public MainQuest[] mainQuest;
        public int currentSideQuest;

        
    }

    [System.Serializable]
    public class MainQuest
    {
        public string questName;
        public Dialogues dialogueQuest;
        public Dialogues sideDialogue;
        public GameObject NPC;
        public int date;
        public string questInfo;
        public string questDetail;
        public Dialogues finish;
        public Dialogues rewardItemQuest;
        public bool questActive = false;
        public bool questComplete = false;
        public Vector3 locateNpcQuest;

        public Sprite spriteQuest;
        public int reward;
        public Reward[] rewards;
        public ItemQuest[] itemQuests;
        public locationMainQuest[] locationMainQuest;


    }
    [System.Serializable]

    public class locationMainQuest
    {
        public string infoQuest;
        public GameObject locationQuest; //buat agar lokasi array di inputkan secara berurutan 
        public Dialogues dialogueLocation;
        public GameObject spawner;
        public GameObject[] prefabObjek;
    }

    [System.Serializable]
    public class Quest
    {
        public string questName;
        public Dialogues dialogueQuest;
        public GameObject NPC;
        public ItemQuest[] itemQuests;
        public int date;
        public int reward;
        public Reward[] rewards;
        public string questInfo;
        public string questDetail;
        public Dialogues finish;
        public Dialogues rewardItemQuest;
        public bool questActive = false;
        public bool questComplete = false;
        public Vector3 locateNpcQuest;
    }

    [System.Serializable]
    public class ItemQuest
    {
        public Item item;
        public int jumlah;
    }

    [System.Serializable]
    public class Reward
    {
        public GameObject itemReward;
        public int jumlahItemReward;
    }

    public Chapter[] chapters;

    //antrian/queue main quest 
    private Queue<MainQuest> mainQuestQueue = new Queue<MainQuest>();
    private MainQuest currentMainQuest = null; // Menyimpan MainQuest yang sedang aktif

    [Header("HUBUNGAN")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    [SerializeField] LoadingScreenUI loadingScreenUI;
    [SerializeField] PlayerQuest playerQuest;
    public Transform questUI;
    public Transform displayMainQuest;
    public string locationInfo;
    public int indexLocation;


    [Header("Quest")]
    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate;
    public int jedaMainQuest;


    public Dialogues notFinished;
    

    // Start is called before the first frame update
    void Start()
    {
        // Inisialisasi awal, jika diperlukan
    }

    // Update is called once per frame
    void Update()
    {
        // Update rutin, jika diperlukan
    }

    public void CheckQuest()
    {

        foreach (var chapter in chapters)
        {
            foreach (var quest in chapter.sideQuest)
            {
                Debug.Log("Tanggal quest active: " + quest.date);

                if ((timeManager.date + 1) == quest.date)
                {
                    quest.questActive = true;
                }

            }
        }



        DisplayActiveQuests();
        npcManager.CheckNPCQuest();

        // Input nilai dialogue ke dalam quest interactable
        // AccsessQuestInteractable();


    }


    //cek nilai currentsidequest apakah == chapter.sidequest.lenght
    //jika true masukan main quest dari idChapter ke dalam antrian
    public void CheckMainQuest(int idChapter)
    {
        foreach (var chapter in chapters)
        {
            if (chapter.idChapter == idChapter)
            {
                if (chapter.currentSideQuest == chapter.sideQuest.Length)
                {
                    Debug.Log("main quest berjalan");
                    foreach (var mainQuest in chapter.mainQuest)
                    {
                        mainQuestQueue.Enqueue(mainQuest); // Tambahkan ke queue
                        if (currentMainQuest == null)
                        {
                            TriggerNextMainQuest();
                        }
                    }
                }
            }
        }
    }

    public void TriggerNextMainQuest()
    {
        if (mainQuestQueue.Count > 0)
        {
            currentMainQuest = mainQuestQueue.Dequeue(); // Ambil quest dari queue
            currentMainQuest.date = timeManager.date + jedaMainQuest; // Ubah date ke waktu saat ini (atau logika lain)
            currentMainQuest.questActive = true; // Tandai quest sebagai aktif
            Debug.Log($"MainQuest aktif: {currentMainQuest.questName}, Date: {currentMainQuest.date}");
        }
        else
        {
            Debug.Log("Tidak ada MainQuest tersisa di queue.");
            currentMainQuest = null;
        }
    }

    private void DisplayActiveQuests()
    {
        Debug.Log("Menjalankan fungsi menampilkan quest");

        // Bersihkan quest sebelumnya dengan menghancurkan child dalam ContentParent
        foreach (Transform child in ContentGO)
        {
            if (child == SlotTemplate) continue;
            Destroy(child.gameObject);
        }

        // Tampilkan semua sideQuest yang aktif
        foreach (var chapter in chapters)
        {
            foreach (var quest in chapter.sideQuest)
            {
                if (quest.questActive)
                {
                    CreateQuestDisplay(quest.questInfo);
                }
            }
        }

        // Tambahkan Main Quest jika kondisinya terpenuhi
        if (currentMainQuest != null && (timeManager.date + 1) == currentMainQuest.date)
        {
            CreateQuestDisplay(currentMainQuest.questInfo);
            PlayMainQuest();

        }
    }

    private void CreateQuestDisplay(string questInfo)
    {
        // Duplikasi TextQuestTemplate
        Transform questObject = Instantiate(SlotTemplate, ContentGO);
        questObject.gameObject.name = questInfo;

        // Aktifkan objek yang diduplikasi
        questObject.gameObject.SetActive(true);

        // Temukan TextMeshPro di dalam objek dan atur teksnya
        TextMeshProUGUI textComponent = questObject.GetComponentInChildren<TextMeshProUGUI>();

        //simpan display quest ke dalam objek displayMainQuest
        displayMainQuest = questObject;
        if (textComponent != null)
        {
            textComponent.text = questInfo;
        }
        else
        {
            Debug.LogError("TextMeshPro tidak ditemukan di dalam TextQuest!");
        }
    }

    public void UpdateDisplayQuest()
    {
        if (displayMainQuest != null && displayMainQuest.name == currentMainQuest.questInfo)
        {
            // Temukan TextMeshPro di dalam objek dan atur teksnya
            TextMeshProUGUI textComponent = displayMainQuest.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                
                // Cek apakah locationMainQuest memiliki elemen
                if (currentMainQuest.locationMainQuest != null && currentMainQuest.locationMainQuest.Length > 0)
                {
                    if (indexLocation < currentMainQuest.locationMainQuest.Length)
                    {
                        // Ambil index ke-0 dari locationMainQuest
                        locationInfo = currentMainQuest.locationMainQuest[indexLocation].infoQuest;
                        textComponent.text = locationInfo;

                        playerQuest.objekMainQuest = currentMainQuest.locationMainQuest[indexLocation].locationQuest;
                        playerQuest.indexLocation = indexLocation;
                        indexLocation++;
                    }


                }

                else
                {
                    textComponent.text = "Lokasi: Tidak Ditemukan";
                }
            }
            else
            {
                Debug.LogError("TextMeshPro tidak ditemukan dalam displayMainQuest!");
            }

        }
    }

    public void playMainLocationQuest(int indexLocation)
    {
        Debug.Log("fungsi playMainLocationQuest di panggil");
        // Ambil lokasi quest saat ini berdasarkan indexLocation
        GameObject parentLocation = currentMainQuest.locationMainQuest[indexLocation].locationQuest;

        if (parentLocation == null)
        {
            Debug.LogError("Parent lokasi quest tidak ditemukan!");
            return;
        }

        // Loop melalui semua prefab yang akan di-spawn
        GameObject prefabUnitLocation = currentMainQuest.locationMainQuest[indexLocation].prefabObjek[indexLocation];
        if (prefabUnitLocation != null)
        {
            // Spawn prefab dengan menjadikannya child dari parentLocation
            GameObject spawnedObject = Instantiate(prefabUnitLocation, parentLocation.transform);

            // Atur posisi lokal prefab agar berada di pusat parentLocation
            spawnedObject.transform.localPosition = Vector3.zero;

            Debug.Log($"Prefab {spawnedObject.name} telah di-instantiate di {parentLocation.name}!");

            dialogueSystem.theDialogues = currentMainQuest.locationMainQuest[indexLocation].dialogueLocation;
            dialogueSystem.StartDialogue();

            StartCoroutine(WaitForDialogueToEnd());
        }
        else
        {
            Debug.LogWarning("Prefab objek tidak valid di dalam array prefabObjek!");
        }
    }




    private void PlayMainQuest()
    {

        if (currentMainQuest != null)
        {
            //LoadingScreenUI.Instance.LoadScene(1); // Memuat scene dengan index 1
            questUI.gameObject.SetActive(true);

            questUI.GetChild(0).GetComponent<Image>().sprite = currentMainQuest.spriteQuest;

            


            //MulticastIPAddressInformation dialog
            dialogueSystem.theDialogues = currentMainQuest.dialogueQuest;
            dialogueSystem.StartDialogue();

            // Tunggu sampai dialog selesai, lalu lanjutkan logika main quest
            StartCoroutine(WaitForDialogueToEnd());

            GameObject npcMainQuest = currentMainQuest.NPC;
            Vector3 locationNpcMainQuest = currentMainQuest.locateNpcQuest;
            Dialogues dialoguesMainQuest = currentMainQuest.sideDialogue;

            npcManager.CheckNPCMainQuest(npcMainQuest, locationNpcMainQuest, dialoguesMainQuest);
        }
    }

    IEnumerator WaitForDialogueToEnd()
    {
        // Tunggu sampai UI dialog tidak aktif (berarti dialog selesai)
        while (dialogueSystem.dialogueUI.activeSelf)
        {
            yield return null; // Tunggu satu frame
        }

        Debug.Log("Dialog main quest selesai!");

        // Tandai quest selesai atau lanjutkan logika lain
        if (currentMainQuest != null)
        {
            questUI.gameObject.SetActive(false);


        }
    }

    //// Memanggil saat MainQuest selesai
    //public void CompleteCurrentMainQuest()
    //{
    //    if (currentMainQuest != null)
    //    {
    //        currentMainQuest.questComplete = true; // Tandai quest sebagai selesai
    //        currentMainQuest.questActive = false; // Matikan status aktif
    //        Debug.Log($"MainQuest selesai: {currentMainQuest.questName}");

    //        // Trigger quest berikutnya
    //        TriggerNextMainQuest();
    //    }
    //}







    //private void DisplayActiveMainQuest()
    //{
    //    Debug.Log("Quest di tampilkan");
    //    Debug.Log(currentMainQuest.questInfo + "\n");
    //    TextQuest.text = "";
    //    TextQuest.text += currentMainQuest.questInfo + "\n";
    //}

    public void AccsessQuestInteractable()
    {
        foreach (var chapter in chapters)
        {
            foreach (var quest in chapter.sideQuest)
            {
                if (quest.questActive && quest.NPC != null)
                {
                    QuestInteractable interactable = quest.NPC.GetComponent<QuestInteractable>();

                    if (interactable != null)
                    {
                        interactable.SetCurrentDialogue(quest.dialogueQuest);
                    }
                }
            }
        }
    }
}
