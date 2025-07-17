using System.Collections.Generic;
using System.IO; // Penting untuk operasi file
using System.Linq;
using NUnit.Framework.Interfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    [Header("Template Quest UI")]
    public Transform TemplateQuest;
    public Transform ContentTemplate;

    [Header("Database Quest (Aset SO)")]
    // Cukup seret semua aset ChapterSO Anda ke sini.
    public List<ChapterSO> allChapters;
    //MainQuestController 
    public MainQuestSO pendingMainQuest; // Quest yang menunggu untuk diaktifkan
    public MainQuestController activeMainQuestController; // Controller yang sedang berjalan


    [Header("Status Quest Pemain")]
    // List ini akan melacak semua quest (side quest) yang sedang aktif atau sudah selesai.
    public List<PlayerQuestStatus> questLog = new List<PlayerQuestStatus>();


    // Event ini akan memberitahu UI atau sistem lain jika ada pembaruan pada log quest.
    public static event System.Action OnQuestLogUpdated;


    private void Awake()
    {
        // Logika Singleton standar
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadQuests(); // Panggil Load saat Awake
        }
    }

    private void OnEnable()
    {
        // Berlangganan event pergantian hari dari TimeManager
        TimeManager.OnDayChanged += CheckForNewQuests;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan untuk menghindari error
        TimeManager.OnDayChanged -= CheckForNewQuests;
    }
    // Dipanggil setiap hari baru untuk memeriksa apakah ada quest baru yang harus diaktifkan.
    public void CheckForNewQuests()
    {
        // --- Cek Side Quest (Logika Anda sudah benar) ---
        foreach (var chapterAsset in allChapters)
        {
            foreach (var questAsset in chapterAsset.sideQuests)
            {
                if (IsQuestInLog(questAsset.questName)) continue;
                if (TimeManager.Instance.timeData_SO.date == questAsset.dateToActivate && TimeManager.Instance.timeData_SO.bulan == questAsset.MonthToActivate)
                {
                    ActivateQuest(questAsset);
                }
            }
        }

        // --- Cek Main Quest yang Tertunda ---
        if (pendingMainQuest != null && activeMainQuestController == null)
        {
            if (TimeManager.Instance.timeData_SO.date == pendingMainQuest.dateToActivate && TimeManager.Instance.timeData_SO.bulan == pendingMainQuest.monthToActivate)
            {
                StartMainQuest(pendingMainQuest);
            }
        }
    }

    // Mengaktifkan sebuah quest dan menambahkannya ke log pemain.
    public void ActivateQuest(QuestSO questToActivate)
    {
        Debug.Log($"Mengaktifkan Side Quest: {questToActivate.questName}");

        // Buat data status baru untuk quest ini
        PlayerQuestStatus newQuestStatus = new PlayerQuestStatus(questToActivate);
        questLog.Add(newQuestStatus);

        // Siarkan event agar UI bisa memperbarui tampilannya
        //OnQuestLogUpdated?.Invoke();
        CreateTemplateQuest();
    }
    // Fungsi helper untuk mengecek apakah sebuah quest sudah ada di log.

    public bool IsQuestInLog(string questName)
    {
        return questLog.Any(q => q.Quest.questName == questName);
    }

    // Fungsi untuk mendapatkan status quest yang aktif untuk NPC tertentu.

    public PlayerQuestStatus GetActiveQuestForNPC(string npcName)
    {
        return questLog.FirstOrDefault(q =>
            q.Quest.npcName == npcName &&
            q.Progress == QuestProgress.Accepted
        );
    }


    public void CreateTemplateQuest()
    {
        Debug.Log("Membuat template quest...");
        if (ContentTemplate == null)
        {
            Debug.LogError("ContentTemplate belum diatur di Inspector!");
            return;
        }

        foreach (Transform child in ContentTemplate)
        {
            Destroy(child.gameObject);
        }

        foreach (var questStatus in questLog)
        {
            // Tampilkan hanya jika belum selesai
            if (questStatus.Progress != QuestProgress.Completed)
            {
                // Buat sebuah fungsi helper untuk menghindari duplikasi kode
                InstantiateQuestUI(questStatus.Quest.questName, questStatus.Quest.questInfo);
            }
        }

      
        // Cek dulu apakah 'activeMainQuestController' ada, BARU akses propertinya.
        if (activeMainQuestController != null && !activeMainQuestController.IsComplete())
        {
            // Ini membutuhkan sebuah fungsi publik baru di MainQuestController Anda.
            string currentObjective = activeMainQuestController.GetCurrentObjectiveInfo();

            // Gunakan fungsi helper yang sama untuk membuat UI-nya
            InstantiateQuestUI(activeMainQuestController.questName, currentObjective);
        }
    }

 
    private void InstantiateQuestUI(string questName, string objectiveInfo)
    {
        Transform questTransform = Instantiate(TemplateQuest, ContentTemplate);
        questTransform.gameObject.SetActive(true);
        questTransform.name = $"Quest - {questName}";

        TMP_Text questNameText = questTransform.GetComponentInChildren<TMP_Text>();
        if (questNameText != null)
        {
            // Tampilkan nama quest DAN tujuan/info saat ini
            questNameText.text = $"{questName}\n- {objectiveInfo}";
        }
        else
        {
            Debug.LogWarning("Tidak ditemukan TMP_Text di dalam TemplateQuest!");
        }
    }

    public void UpdateQuestProgress(string questName, string itemName, int amount)
    {
        PlayerQuestStatus questStatus = questLog.FirstOrDefault(q => q.Quest.questName == questName && q.Progress == QuestProgress.Accepted);

        if (questStatus != null)
        {
            if (questStatus.itemProgress.ContainsKey(itemName))
            {
                questStatus.itemProgress[itemName] += amount;
                Debug.Log($"Progres untuk item {itemName} di quest {questName} diperbarui.");
                // Di sini Anda bisa memanggil fungsi untuk cek penyelesaian quest
            }
        }
    }

    // Fungsi untuk menyelesaikan quest
    public void CompleteQuest(PlayerQuestStatus questStatus)
    {
        // Pengecekan keamanan di awal
        if (questStatus == null || questStatus.Progress != QuestProgress.Accepted) return;


        questStatus.Progress = QuestProgress.Completed;
        Debug.Log($"Quest '{questStatus.Quest.questName}' telah selesai!");

        GameEconomy.Instance.GainMoney(questStatus.Quest.goldReward);
        CreateTemplateQuest();
        // (Tambahkan logika untuk memberi item reward di sini jika ada)

        // Pastikan referensi DialogueSystem ada dan benar
        if (questStatus.Quest.finishDialogue != null)
        {
            DialogueSystem.Instance.theDialogues = questStatus.Quest.finishDialogue;
            DialogueSystem.Instance.StartDialogue();
        }

        CreateTemplateQuest();

        SaveQuests();

        ChapterSO parentChapter = FindChapterForQuest(questStatus.Quest);
        if (parentChapter != null && AreAllSideQuestsComplete(parentChapter))
        {
            if (parentChapter.mainQuest != null)
            {
                SetNextMainQuest(parentChapter.mainQuest);
            }
        }
    }

    // Fungsi bantuan untuk menemukan chapter dari sebuah quest.
    private ChapterSO FindChapterForQuest(QuestSO questToFind)
    {
        // Cari di semua chapter yang ada
        foreach (var chapter in allChapters)
        {
            // Cek apakah list side quest di chapter ini mengandung quest yang kita cari
            if (chapter.sideQuests.Contains(questToFind))
            {
                return chapter; // Jika ditemukan, kembalikan chapternya
            }
        }
        return null; // Jika tidak ditemukan
    }

    private string GetSavePath()
    {
        // Application.persistentDataPath adalah folder aman di setiap platform (PC, Android, iOS)
        // untuk menyimpan data game.
        return Path.Combine(Application.persistentDataPath, "questprogress.json");
    }

    public void SaveQuests()
    {
        // Buat list baru yang berisi data yang siap disimpan
        List<QuestSaveData> saveDataList = new List<QuestSaveData>();
        foreach (var questStatus in questLog)
        {
            saveDataList.Add(new QuestSaveData(questStatus));
        }

        // Ubah list tersebut menjadi format teks JSON
        string json = JsonUtility.ToJson(new Serialization<QuestSaveData>(saveDataList), true);

        // Tulis teks JSON tersebut ke dalam sebuah file
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("Progres quest berhasil disimpan ke: " + GetSavePath());
    }

    // Di dalam QuestManager.cs
    public void LoadQuests()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            List<QuestSaveData> saveDataList = JsonUtility.FromJson<Serialization<QuestSaveData>>(json).ToList();

            questLog.Clear();
            foreach (var saveData in saveDataList)
            {
                QuestSO questSO = FindQuestSOByName(saveData.questName);
                if (questSO != null)
                {
                    PlayerQuestStatus status = new PlayerQuestStatus(questSO);
                    status.Progress = saveData.progress;


                    // Bangun kembali Dictionary dari dua list
                    status.itemProgress = new Dictionary<string, int>();
                    for (int i = 0; i < saveData.itemProgressKeys.Count; i++)
                    {
                        string key = saveData.itemProgressKeys[i];
                        int value = saveData.itemProgressValues[i];
                        status.itemProgress[key] = value;
                    }

                    questLog.Add(status);
                }
            }
            Debug.Log("Progres quest berhasil dimuat!");
        }

        CreateTemplateQuest();
    }

    public void DeleteSaveData()
    {
        string path = GetSavePath();

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.LogWarning("FILE SAVE QUEST TELAH DIHAPUS!");

            // Kosongkan log yang ada di memori saat ini
            questLog.Clear();

            // Pastikan Anda sudah menambahkan "using UnityEngine.SceneManagement;" di bagian atas
            GameController.Instance.PindahKeScene("Village");
        }
        else
        {
            Debug.Log("Tidak ada file save quest yang ditemukan untuk dihapus.");
        }
    }



    //pengecekan apakah seluruh Side Quest sudah selesai
    private bool AreAllSideQuestsComplete(ChapterSO chapter)
    {
        // Jika chapter ini tidak punya side quest, anggap saja "selesai".
        if (chapter.sideQuests == null || chapter.sideQuests.Count == 0)
        {
            return true;
        }

        // Gunakan LINQ 'All' untuk memeriksa setiap side quest yang dibutuhkan.
        // 'All' akan mengembalikan true hanya jika SEMUA elemen memenuhi kondisi.
        return chapter.sideQuests.All(requiredQuestSO =>
        {
            // Untuk setiap 'requiredQuestSO', cari statusnya di dalam 'questLog'.
            PlayerQuestStatus statusInLog = questLog.FirstOrDefault(status => status.Quest == requiredQuestSO);

            // Kondisinya adalah: statusnya harus ada di log DAN progresnya harus 'Completed'.
            return statusInLog != null && statusInLog.Progress == QuestProgress.Completed;
        });
    }



    // Fungsi helper untuk mencari QuestSO berdasarkan nama
    private QuestSO FindQuestSOByName(string name)
    {
        foreach (var chapter in allChapters)
        {
            foreach (var quest in chapter.sideQuests)
            {
                if (quest.questName == name)
                {
                    return quest;
                }
            }
        }
        return null;
    }

    //Logika menjalankan Main Quest
    public void SetNextMainQuest(MainQuestSO mainQuest)
    {
        // Hanya siapkan quest jika belum ada yang disiapkan atau sedang aktif
        if (pendingMainQuest == null && activeMainQuestController == null)
        {
            pendingMainQuest = mainQuest;
            pendingMainQuest.dateToActivate = TimeManager.Instance.timeData_SO.date + 1;
            pendingMainQuest.monthToActivate = TimeManager.Instance.timeData_SO.bulan;
            Debug.Log($"Main Quest '{mainQuest.questName}' disiapkan untuk dimulai pada tanggal {mainQuest.dateToActivate}.");
        }
    }
    public void StartMainQuest(MainQuestSO mainQuestSO)
    {
        if (activeMainQuestController != null) return;
        if (mainQuestSO.questControllerPrefab == null)
        {
            Debug.LogError($"Prefab controller untuk '{mainQuestSO.questName}' belum diatur!");
            return;
        }

        Debug.Log($"MEMULAI MAIN QUEST: {mainQuestSO.questName}");
        GameObject controllerGO = Instantiate(mainQuestSO.questControllerPrefab, this.transform);
        activeMainQuestController = controllerGO.GetComponent<MainQuestController>();

        if (activeMainQuestController != null)
        {
            activeMainQuestController.StartQuest(this); // Langsung mulai HANYA SEKALI.
            pendingMainQuest = null; // Hapus dari antrian setelah dimulai
            CreateTemplateQuest(); // Perbarui UI
        }
    }
}

