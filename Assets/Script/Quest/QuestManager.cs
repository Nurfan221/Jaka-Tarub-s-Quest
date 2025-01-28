using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static QuestManager;
using Microsoft.Unity.VisualStudio.Editor;
using static UnityEditor.Progress;

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
        public GameObject[] locationQuest; //buat agar lokasi array di inputkan secara berurutan 
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
    [Header("Quest")]
    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate;


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
            currentMainQuest.date = timeManager.date + 4; // Ubah date ke waktu saat ini (atau logika lain)
            currentMainQuest.questActive = true; // Tandai quest sebagai aktif
            Debug.Log($"MainQuest aktif: {currentMainQuest.questName}, Date: {currentMainQuest.date}");
        }
        else
        {
            Debug.Log("Tidak ada MainQuest tersisa di queue.");
            currentMainQuest = null;
        }
    }

    private void playMainQuest()
    {

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
        if (textComponent != null)
        {
            textComponent.text = questInfo;
        }
        else
        {
            Debug.LogError("TextMeshPro tidak ditemukan di dalam TextQuest!");
        }
    }



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
