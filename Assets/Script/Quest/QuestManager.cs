using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static QuestManager;
//using Microsoft.Unity.VisualStudio.Editor;
using static UnityEditor.Progress;
using System.Net.NetworkInformation;
using static MiniQuest;

public enum QuestType
{
    Side,
    Mini
}



public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class Chapter
    {
        public int idChapter;
        public Quest[] sideQuest;
        //public MainQuest[] mainQuest;
        public int currentSideQuest;

        
    }

    //[System.Serializable]
    //public class MainQuest
    //{
    //    public string questName;
    //    //public MainQuest1State currentQuestState = MainQuest1State.None;

    //    //inputkan dialogue sesuai jalan cerita dari awal hingga akhir
    //    public Dialogues[] dialogueQuest;
    //    public Dialogues dialoguePengingat;
    //    public GameObject NPC;
    //    public int date;
    //    public string questDetail;
    //    public Dialogues finish;
    //    public Dialogues rewardItemQuest;
    //    public bool questActive = false;
    //    public bool questComplete = false;
    //    public Vector3 locateNpcQuest;

    //    //tentukan sprite sesuai dengan jalan cerita mulai dari awal sampai akhir
    //    public Sprite[] spriteQuest;
    //    public int reward;
    //    public Reward[] rewards;
    //    public List<Item> itemQuests;
    //    public int indexLocation;
    //    public locationMainQuest[] locationMainQuest;


    //}
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
        public List<Item> itemQuests;
        public int[] countItem;
        public int date;
        //public int bulan;
        public int reward;
        public Reward[] rewards;
        public string questInfo;
        public string questDetail;
        public string deskripsiAwal;
        public string deskripsiAkhir;
        public Dialogues finish;
        public Dialogues rewardItemQuest;
        public bool questActive = false;
        public bool questComplete = false;
        public bool isObjectHidden;
        public bool isInGrief;
        public bool isSpawner;
        public GameObject spawner;
        public GameObject objectHidden;
        public Vector3 locateNpcQuest;

    }

   

    [System.Serializable]
    public class Reward
    {
        public Item itemReward;
        public int jumlahItemReward;
    }

    public Chapter[] chapters;

    //antrian/queue main quest 
    //public Queue<MainQuest> mainQuestQueue = new Queue<MainQuest>();
    //public MainQuest currentMainQuest = null; // Menyimpan MainQuest yang sedang aktif
    [Header("Manajemen Main Quest")]
    public GameObject[] mainQuestPrefabs; // array penampung prefab mainquest

    //Variabel ini akan menampung skrip quest yang sedang aktif
    // Ubah nama variabel private menjadi _currentActiveQuest (konvensi umum)
    private MainQuestController _currentActiveQuest;

    // Inilah "etalase kaca" publik kita
    public MainQuestController CurrentActiveQuest
    {
        get { return _currentActiveQuest; } // 'get' berarti skrip lain boleh MEMBACA nilainya
        private set { _currentActiveQuest = value; } // 'private set' berarti HANYA QuestManager yang boleh MENGUBAH nilainya
    }

    public int countCurrentMainQuest = 0;
    // Variabel baru untuk mengelola penjadwalan Main Quest
    private int scheduledMainQuestIndex = -1; // -1 berarti tidak ada yang dijadwalkan
    private int scheduledMainQuestDate = -1;

    [Header("HUBUNGAN")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] public DialogueSystem dialogueSystem;
    [SerializeField] public  NPCManager npcManager;
    [SerializeField] LoadingScreenUI loadingScreenUI;
    [SerializeField] public PlayerQuest playerQuest;
    [SerializeField] QuestInfoUI questInfoUI;
    //[SerializeField] LocationConfiguration locationConfiguration;
    [SerializeField] SpawnerManager spawnerManager;
    [SerializeField] LocationManager locationManager;

    //hubungan quest perchapter
    [SerializeField] MainQuest2 mainQuest2;
    public bool chapter1IsDone;
    public Transform questUI;
    public Transform displayMainQuest;





    [Header("Quest")]
    public Transform ContentGO;
    public Transform SlotTemplate;
    public Transform childContentGo;
    public TextMeshProUGUI childTemplateContentGo;
    public int jedaMainQuest;
    public string mainQuestInfo;
    public Dialogues mainQuestDialogue;
    public bool playerSekaratSudahDiproses = false;

    //deklarasi untuk menampung mini quest
    public MiniQuestList currentMiniQuest;



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
                //Debug.Log("Tanggal quest active: " + quest.date);

                if ((timeManager.timeData_SO.date + 1) == quest.date  && !quest.questActive)
                {
                    quest.questActive = true;
                    questInfoUI.DisplayActiveQuest(quest);
                    AddItemToList(quest);

                    if(quest.isSpawner && quest.spawner != null)
                    {
                        quest.spawner.gameObject.SetActive(true);
                    }

                }
            }
        }





        DisplayActiveQuests();
        npcManager.CheckNPCQuest();

        Debug.Log("tanggal sekarang : " + timeManager.timeData_SO.date + 1);
        CheckForScheduledQuest();
    }

    public void AddItemToList(Quest questActive)
    {
        // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama
        List<Item> newItemList = new List<Item>();

        if(questActive != null && questActive.itemQuests.Count > 0)
        {
            // Menambahkan item baru ke dalam itemQuests berdasarkan countItem
            for (int i = 0; i < questActive.itemQuests.Count; i++)
            {
                // Membuat salinan baru dari item yang ada untuk menghindari modifikasi referensi langsung
                Item itemCopy = new Item
                {
                    itemName = questActive.itemQuests[i].itemName, // Salin nama item
                    //stackCount = questActive.countItem[i]          // Set stackCount dari countItem
                };

                // Menambahkan item baru ke dalam newItemList
                newItemList.Add(itemCopy);
            }
        }

        // Menghapus semua item lama setelah menambahkan item baru
        questActive.itemQuests.Clear();

        // Menambahkan item baru dari newItemList ke quest.itemQuests
        foreach (var item in newItemList)
        {
            // Mendapatkan salinan item dari ItemPool (menggunakan Instantiate)
            //Item itemFromPool = ItemPool.Instance.GetItem(item.itemName, item.stackCount);

            //// Menambahkan item yang diinstansiasi ke dalam quest.itemQuests
            //if (itemFromPool != null)
            //{
            //    questActive.itemQuests.Add(itemFromPool);
            //    Debug.Log($"Item: {itemFromPool.itemName}, Jumlah: {itemFromPool.stackCount}");
            //}
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


       

    }
    
    public void UpdateDisplayQuest(string questInfo)
    { 
        childContentGo = ContentGO.transform.Find(questInfo);
        childTemplateContentGo = childContentGo.GetComponentInChildren<TextMeshProUGUI>();
        childContentGo.name = mainQuestInfo;
        childTemplateContentGo.text = mainQuestInfo;
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

    

    public void UpdateDateSideQuest()
    {
        foreach (var chapter in chapters)
        {
            if (chapter.idChapter == countCurrentMainQuest)
            {
                int tanggalSekarang = timeManager.timeData_SO.date;

                foreach (var quest in chapter.sideQuest)
                {
                    // Tambahkan 5 hari dan buat tanggal wrap ke 1–28
                    tanggalSekarang = ((tanggalSekarang + 5 - 1) % 28) + 1;

                    // Set tanggal quest saat ini
                    quest.date = tanggalSekarang;
                }
            }
        }
    }

    // Fungsi baru untuk menjadwalkan Main Quest
    public void ScheduleNextMainQuest(int completedChapterId)
    {
        // Tentukan main quest mana yang akan dijadwalkan (berdasarkan chapter yang selesai)
        int nextQuestNumber = completedChapterId; // Asumsi Chapter 1 akan memicu Main Quest 1, dst.

        // Tentukan tanggal mulainya, misalnya 2 hari dari sekarang
        int startDate = timeManager.timeData_SO.date + 2; // Anda bisa membuat jeda ini menjadi variabel

        // Simpan informasi penjadwalan ini
        scheduledMainQuestIndex = nextQuestNumber;
        scheduledMainQuestDate = startDate;

        Debug.Log($"Semua side quest untuk Chapter {completedChapterId} selesai. Menjadwalkan Main Quest {nextQuestNumber} pada tanggal {startDate}.");
    }

    // Fungsi ini harus dipanggil sekali setiap hari baru
    public void CheckForScheduledQuest()
    {
        // Cek apakah ada quest yang dijadwalkan DAN apakah tanggalnya sudah tiba
        if (scheduledMainQuestIndex != -1 && timeManager.timeData_SO.date == scheduledMainQuestDate)
        {
            // Waktunya memulai quest!
            // Kita gunakan countCurrentMainQuest dari variabel yang terjadwal
            countCurrentMainQuest = scheduledMainQuestIndex;
            MulaiMainQuest(); // Panggil fungsi yang sudah kita buat sebelumnya

            // Reset penjadwalan agar tidak berjalan lagi
            scheduledMainQuestIndex = -1;
            scheduledMainQuestDate = -1;
        }
    }

    public void MulaiMainQuest()
    {
        // Hancurkan quest lama jika ada
        if (CurrentActiveQuest != null)
        {
            Destroy(CurrentActiveQuest.gameObject);
        }

        int questIndex = countCurrentMainQuest - 1;

        if (questIndex >= 0 && questIndex < mainQuestPrefabs.Length)
        {
            // 1. Buat objek quest dari prefab. HANYA SATU KALI.
            GameObject questObject = Instantiate(mainQuestPrefabs[questIndex]);

            // 2. Dapatkan komponen controllernya.
            CurrentActiveQuest = questObject.GetComponent<MainQuestController>();

            // 3. Jika berhasil, mulai questnya.
            if (CurrentActiveQuest != null)
            {
                // Menampilkan nama quest di UI.
                CreateQuestDisplay(CurrentActiveQuest.questName);
                // Menjalankan fungsi StartQuest() yang ada di dalam MainQuest1_Controller.
                CurrentActiveQuest.StartQuest(this);
            }
            else
            {
                Debug.LogError("Prefab Main Quest tidak memiliki skrip MainQuestController!");
            }
        }
        else
        {
            Debug.LogWarning("Quest nomor " + countCurrentMainQuest + " tidak ditemukan.");
        }
    }

    // Anda juga perlu fungsi untuk menandai quest selesai
    public void MainQuestSelesai()
    {
        if (_currentActiveQuest != null)
        {
            Debug.Log($"Main Quest {_currentActiveQuest.questName} telah selesai!");
            Destroy(_currentActiveQuest.gameObject);
            _currentActiveQuest = null;

            // Tambah hitungan untuk persiapan quest berikutnya
            countCurrentMainQuest++;

            // Anda bisa langsung memicu quest berikutnya di sini jika mau,
            // atau menunggunya dipicu oleh event lain.
            // Contoh: MulaiMainQuest(); 
        }
    }

    //public void PlayMainQuest()
    //{
    //    Debug.Log("Play main Quest di jalankan");
    //    //set cerita untuk mimpi jaka tarub 

    //    //mulai dialogue untuk mimpi jaka tarub
    //    currentMainQuest.currentQuestState = MainQuest1State.Play;
    //    NextQuestState();

    //    GameObject npcMainQuest = currentMainQuest.NPC;
    //    Vector3 locationNpcMainQuest = currentMainQuest.locateNpcQuest;
    //    Dialogues dialoguesMainQuest = currentMainQuest.dialogueQuest[1];

    //    npcManager.CheckNPCMainQuest(npcMainQuest, locationNpcMainQuest, dialoguesMainQuest);


    //}

    

        

   

    public void MunculkanSpawnerBandit()
    {
        //playerQuest.environmentObject.gameObject.SetActive(false);

        //if (currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].isSpawner)
        //{
        //    currentMainQuest.locationMainQuest[currentMainQuest.indexLocation].spawner.gameObject.SetActive(true);
        //}
    }

    public void HapusSparnerBandit()
    {
        //currentMainQuest.locationMainQuest[currentMainQuest.indexLocation -1 ].spawner.gameObject.SetActive(false);
    }

    
    
}
