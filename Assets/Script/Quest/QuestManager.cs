using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static QuestManager;
//using Microsoft.Unity.VisualStudio.Editor;
using static UnityEditor.Progress;
using System.Net.NetworkInformation;

public enum MainQuest1State
{
    None,
    Play,
    MenemukanDanau,
    PergiKeLokasiQuest,
    CariRusa,
    BunuhRusa,
    MunculBandit,
    Sekarat,
    LariKeDanau,
    SceneDanauIndah,
    Pulang,
    SceneIbuMeninggal,
    Selesai
}

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
        public MainQuest1State currentQuestState = MainQuest1State.None;

        //inputkan dialogue sesuai jalan cerita dari awal hingga akhir
        public Dialogues[] dialogueQuest;
        public GameObject NPC;
        public int date;
        public string questDetail;
        public Dialogues finish;
        public Dialogues rewardItemQuest;
        public bool questActive = false;
        public bool questComplete = false;
        public Vector3 locateNpcQuest;

        //tentukan sprite sesuai dengan jalan cerita mulai dari awal sampai akhir
        public Sprite[] spriteQuest;
        public int reward;
        public Reward[] rewards;
        public ItemQuest[] itemQuests;
        public int indexLocation;
        public locationMainQuest[] locationMainQuest;


    }
    [System.Serializable]

    public class locationMainQuest
    {
        public string infoQuest;
        public GameObject locationQuest; //buat agar lokasi array di inputkan secara berurutan 
        public bool isSpawner;
        public GameObject spawner;
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
    public Queue<MainQuest> mainQuestQueue = new Queue<MainQuest>();
    public MainQuest currentMainQuest = null; // Menyimpan MainQuest yang sedang aktif
    public int countCurrentMainQuest = 1;

    [Header("HUBUNGAN")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    [SerializeField] LoadingScreenUI loadingScreenUI;
    [SerializeField] PlayerQuest playerQuest;
    [SerializeField] QuestInfoUI questInfoUI;
    public Transform questUI;
    public Transform displayMainQuest;





    [Header("Quest")]
    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate;
    public Transform childContentGo;
    public TextMeshProUGUI childTemplateContentGo;
    public int jedaMainQuest;
    public string mainQuestInfo;
    public Dialogues mainQuestDialogue;


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

                if ((timeManager.date + 1) == quest.date && !quest.questActive)
                {
                    quest.questActive = true;
                    questInfoUI.DisplayActiveQuest(quest);

                }
            }
        }



        DisplayActiveQuests();
        npcManager.CheckNPCQuest();

        Debug.Log("tanggal sekarang : " + timeManager.date + 1);
        if (currentMainQuest != null && (timeManager.date + 1) == currentMainQuest.date)
        {
            PlayMainQuest1();
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


        if (currentMainQuest != null && currentMainQuest.date == timeManager.date +1)
        {
            Debug.Log("currentMainQuest ada isinya");
            mainQuestInfo = "ikuti kata hatimu";
            CreateQuestDisplay(mainQuestInfo);
        }

    }

    public void CreateQuestDisplay(string questInfo)
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

    public void InputAntrianMainQuest()
    {
        foreach (var chapter in chapters)
        {
            if (chapter.idChapter == countCurrentMainQuest && chapter.currentSideQuest == chapter.sideQuest.Length)
            {
                foreach (var quest in chapter.mainQuest) // Loop setiap main quest dalam chapter
                {
                    mainQuestQueue.Enqueue(quest);  
                }
            }
        }

        if (mainQuestQueue.Count > 0) // Cek apakah ada quest dalam queue
        {
            currentMainQuest = mainQuestQueue.Dequeue(); // Ambil quest pertama dari queue
            currentMainQuest.questActive = true; // Tandai sebagai aktif

            currentMainQuest.date = timeManager.date + 2;

            Debug.Log($"Main Quest Dimulai: {currentMainQuest.questName}");
        }
        else
        {
            currentMainQuest = null; // Jika queue kosong, reset current quest
            Debug.Log("Tidak ada Main Quest yang tersisa!");
        }
    }

    public void PlayMainQuest1()
    {
        Debug.Log("Play main Quest 1 di jalankan");
        //set cerita untuk mimpi jaka tarub 
        questUI.gameObject.SetActive(true);
        //mulai dialogue untuk mimpi jaka tarub
        currentMainQuest.currentQuestState = MainQuest1State.Play;
        NextQuestState();

        GameObject npcMainQuest = currentMainQuest.NPC;
        Vector3 locationNpcMainQuest = currentMainQuest.locateNpcQuest;
        Dialogues dialoguesMainQuest = currentMainQuest.dialogueQuest[1];

        npcManager.CheckNPCMainQuest(npcMainQuest, locationNpcMainQuest, dialoguesMainQuest);


    }

    

    public void NextQuestState()
    {
        switch (currentMainQuest.currentQuestState)
        {
            case MainQuest1State.None:
                 

                currentMainQuest.currentQuestState = MainQuest1State.Play;
                break;
            case MainQuest1State.Play:
                currentMainQuest.currentQuestState = MainQuest1State.MenemukanDanau;
                ShowDialogueAndSprite(0,true);
                break;
            case MainQuest1State.PergiKeLokasiQuest:
                mainQuestInfo = currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].infoQuest;
                currentMainQuest.currentQuestState = MainQuest1State.PergiKeLokasiQuest;
                UpdateLocationMainQuest();
                break;
            case MainQuest1State.CariRusa:
                playerQuest.CariRusa();
                break;
            case MainQuest1State.BunuhRusa:
                MunculkanSpawnerBandit();
                break;
            case MainQuest1State.Sekarat:
                UpdateLocationMainQuest();
                break;

        }
    }

    public void ShowDialogueAndSprite(int index, bool pakaiImage)
    {
        if (pakaiImage)
        {
            Image questImageUI = questUI.GetChild(0).GetComponent<Image>();
            questImageUI.sprite = currentMainQuest.spriteQuest[index];

            // Pastikan index tidak melebihi batas array
            dialogueSystem.theDialogues = currentMainQuest.dialogueQuest[index];
            dialogueSystem.StartDialogue();
            StartCoroutine(dialogueSystem.WaitForDialogueToEnd());
        }else
        {
            // Pastikan index tidak melebihi batas array
            dialogueSystem.theDialogues = currentMainQuest.dialogueQuest[index];
            dialogueSystem.StartDialogue();
            StartCoroutine(dialogueSystem.WaitForDialogueToEnd());
        }

    }

    public void UpdateLocationMainQuest()
    {
        switch(currentMainQuest.indexLocation)
        {
            case 0:
                childContentGo = ContentGO.transform.Find("ikuti kata hatimu");
                childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
                childContentGo.name = mainQuestInfo;
                childTemplateContentGo.text = mainQuestInfo;
                playerQuest.locationMainQuest = currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].locationQuest;
                playerQuest.dialogueInLocation = currentMainQuest.dialogueQuest[2];
                break;
            case 1:
                childContentGo = ContentGO.transform.Find(mainQuestInfo);
                currentMainQuest.indexLocation += 1;
                mainQuestInfo = currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].infoQuest;
                childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
                childContentGo.name = mainQuestInfo;
                childTemplateContentGo.text = mainQuestInfo;

                dialogueSystem.theDialogues = currentMainQuest.dialogueQuest[3];
                dialogueSystem.StartDialogue();

                playerQuest.locationMainQuest = currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].locationQuest;
                playerQuest.dialogueInLocation = currentMainQuest.dialogueQuest[4];
                playerQuest.inLocation = false;
                break;
        }
    }

    public void MunculkanSpawnerBandit()
    {
        playerQuest.environmentObject.gameObject.SetActive(false);

        if (currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].isSpawner)
        {
            currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].spawner.gameObject.SetActive(true);
        } 
    }


}
