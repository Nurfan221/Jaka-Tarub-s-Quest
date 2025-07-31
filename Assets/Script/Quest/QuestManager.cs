using System;
using System.Collections.Generic;
using System.IO; // Penting untuk operasi file
using System.Linq;
using NUnit.Framework.Interfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.UIElements;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    [Header("Template Quest UI")]
    public Transform templateQuest;
    public Transform contentTemplate;
    public Transform contentStory;

    [Header("Database Quest (Aset SO)")]
    // Cukup seret semua aset ChapterSO Anda ke sini.
    public List<ChapterSO> allChapters;
    //MainQuestController 
    public MainQuestSO pendingMainQuest; // Quest yang menunggu untuk diaktifkan
    public MainQuestController activeMainQuestController; // Controller yang sedang berjalan
    public PlayerMainQuestStatus activePlayerMainQuestStatus;

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
            LoadMainQuest();
        }
    }

    private void OnEnable()
    {
        // Berlangganan event pergantian hari dari TimeManager
        TimeManager.OnDayChanged += CheckForNewQuests;
        DialogueSystem.OnDialogueEnded += DialogueEnd;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan untuk menghindari error
        TimeManager.OnDayChanged -= CheckForNewQuests;
        DialogueSystem.OnDialogueEnded -= DialogueEnd;
    }

    private void Start()
    {
        StartMainQuest(pendingMainQuest);

    }

    public void DialogueEnd()
    {
        // Cek apakah ada quest yang sedang aktif
        HideContentStory();
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
                Debug.Log($"Memulai Main Quest yang tertunda: {pendingMainQuest.questName}");
                StartMainQuest(pendingMainQuest);
            }
            else
            {
                Debug.LogError($"Main Quest '{pendingMainQuest.questName}' belum bisa dimulai. Tanggal saat ini: {TimeManager.Instance.timeData_SO.date}, Bulan: {TimeManager.Instance.timeData_SO.bulan}. Tanggal aktivasi: {pendingMainQuest.dateToActivate}, Bulan: {pendingMainQuest.monthToActivate}");
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
        if (questToActivate.isSpawner)
        {
            Debug.Log("mengaktifkan spawner SpawnerQuest1_6");
            SpawnerManager.Instance.HandleSpawnerActive(questToActivate.spawnerToActivate);
        }
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

    public PlayerMainQuestStatus GetActiveMainQuestStatus(string npcName)
    {
        // Pertama, cek apakah ada Main Quest yang aktif secara keseluruhan
        if (activePlayerMainQuestStatus == null || activePlayerMainQuestStatus.Progress != QuestProgress.Accepted)
        {
            return null; // Tidak ada Main Quest aktif atau belum diterima
        }

        // Kemudian, cek apakah NPC yang diberikan adalah NPC yang terkait dengan Main Quest ini
        // (Asumsi MainQuestSO.npcName sudah diset dengan benar)
        if (activePlayerMainQuestStatus.MainQuestDefinition != null &&
            activePlayerMainQuestStatus.MainQuestDefinition.namaNpcQuest.Equals(npcName, StringComparison.OrdinalIgnoreCase))
        {
            return activePlayerMainQuestStatus; // Main Quest aktif, dan NPC cocok
        }

        return null; // Main Quest aktif tetapi NPC tidak cocok, atau MainQuestDefinition null
    }

    // Mengembalikan TRUE jika item berhasil diproses oleh Main Quest ATAU Side Quest
    private void CheckIfQuestIsComplete(PlayerQuestStatus questStatus)
    {
        // Cek apakah quest valid
        if (questStatus == null || questStatus.Progress != QuestProgress.Accepted)
        {
            return;
        }

        bool allRequirementsMet = true;
        // Pastikan questStatus.Quest dan itemRequirements tidak null
        if (questStatus.Quest != null && questStatus.Quest.itemRequirements != null)
        {
            foreach (var requirement in questStatus.Quest.itemRequirements)
            {
                // Pastikan itemProgress mengandung kunci ini sebelum mengaksesnya
                if (!questStatus.itemProgress.ContainsKey(requirement.itemName) || questStatus.itemProgress[requirement.itemName] < requirement.count)
                {
                    allRequirementsMet = false;
                    break;
                }
            }
        }
        else
        {
            allRequirementsMet = false; // Jika tidak ada definisi quest atau itemRequirements
        }


        // JIKA SEMUA SYARAT TERPENUHI...
        if (allRequirementsMet)
        {
            Debug.Log($"Side Quest '{questStatus.Quest.questName}' TELAH TERPENUHI!");
            // Biarkan QuestManager yang mengurus sisanya (dialog, hadiah, save).
            // Pastikan fungsi CompleteQuest ini ada dan menangani Side Quest
            CompleteQuest(questStatus);
        }
        else
        {
            // Jika belum selesai, Anda bisa memicu dialog "pengingat" di sini
            Debug.Log($"Side Quest '{questStatus.Quest.questName}' belum selesai, masih ada item yang dibutuhkan.");
        }
    }

    public bool ProcessItemGivenToNPC(ItemData givenItemData, string npcName)
    {
        Debug.Log($"QuestManager: Menerima item '{givenItemData.itemName}' dari '{npcName}' untuk diproses.");

        //Logika pengecekan MainQuest
        PlayerMainQuestStatus currentMainQuestStatus = GetActiveMainQuestStatus(npcName);
        if (currentMainQuestStatus != null && currentMainQuestStatus.CurrentStateName == "ProsesToGiveItem")
        {
            if (currentMainQuestStatus.MainQuestDefinition != null &&
                currentMainQuestStatus.MainQuestDefinition.namaNpcQuest.Equals(npcName, StringComparison.OrdinalIgnoreCase))
            {
                if (activeMainQuestController != null)
                {
                    // TryProcessGivenItem di MainQuestController sudah memiliki logika pengecekan kelengkapan item
                    // dan memajukan state quest jika semua item terpenuhi.
                    bool processedByMainQuest = activeMainQuestController.TryProcessGivenItem(givenItemData);
                    if (processedByMainQuest)
                    {
                        Debug.Log($"QuestManager: Item '{givenItemData.itemName}' berhasil diproses oleh Main Quest.");
                        return true;
                    }
                    else
                    {
                        Debug.Log($"QuestManager: Item '{givenItemData.itemName}' tidak sepenuhnya diproses oleh Main Quest (mungkin sudah cukup atau tidak relevan untuk state saat ini).");
                    }
                }
            }
        }

        //logika pengecekan untuk sideQuest 
        PlayerQuestStatus activeSideQuestStatus = GetActiveQuestForNPC(npcName);
        if (activeSideQuestStatus != null)
        {
            ItemData requiredSideItem = activeSideQuestStatus.Quest.itemRequirements
                .FirstOrDefault(req => req.itemName.Equals(givenItemData.itemName, StringComparison.OrdinalIgnoreCase));

            if (requiredSideItem != null)
            {
                int currentProgressSide = activeSideQuestStatus.itemProgress.ContainsKey(givenItemData.itemName) ? activeSideQuestStatus.itemProgress[givenItemData.itemName] : 0;
                int neededSide = requiredSideItem.count - currentProgressSide;

                if (neededSide > 0)
                {
                    int amountToGiveSide = Mathf.Min(givenItemData.count, neededSide);
                    givenItemData.count -= amountToGiveSide;

                    if (amountToGiveSide > 0)
                    {
                        activeSideQuestStatus.itemProgress[givenItemData.itemName] += amountToGiveSide;
                        // givenItemData.count -= amountToGiveSide; // Pengurangan item diinventaris dilakukan di NPCBehavior setelah ini.
                        // Kurangi item dari inventaris pemain

                        Debug.Log($"QuestManager: Item '{givenItemData.itemName}' berhasil diproses oleh Side Quest.");

                        // <<< PANGGIL FUNGSI INI DI SINI UNTUK SIDE QUEST >>>
                        CheckIfQuestIsComplete(activeSideQuestStatus);

                        return true; // Side Quest berhasil memprosesnya
                    }
                }
            }
        }

        Debug.Log($"QuestManager: Item '{givenItemData.itemName}' tidak diproses oleh quest manapun.");
        return false; // Tidak ada quest yang memproses item ini
    }
    public void CreateTemplateQuest()
    {
        Debug.Log("Membuat template quest...");
        if (contentTemplate == null)
        {
            Debug.LogError("ContentTemplate belum diatur di Inspector!");
            return;
        }

        foreach (Transform child in contentTemplate)
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

 
    public void InstantiateQuestUI(string questName, string objectiveInfo)
    {
        Transform questTransform = Instantiate(templateQuest, contentTemplate);
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

    public void ComplateMainQuest(PlayerMainQuestStatus mainQuestStatus)
    {
        // Pengecekan keamanan di awal
        if (mainQuestStatus == null || mainQuestStatus.Progress != QuestProgress.Accepted) return;


        mainQuestStatus.Progress = QuestProgress.Completed;
        Debug.Log($"Quest '{mainQuestStatus.MainQuestDefinition.questName}' telah selesai!");

        //GameEconomy.Instance.GainMoney(mainQuestStatus.MainQuestDefinition.goldReward);
        CreateTemplateQuest();
        // (Tambahkan logika untuk memberi item reward di sini jika ada)

        // Pastikan referensi DialogueSystem ada dan benar
        if (mainQuestStatus.MainQuestDefinition.finishDialogue != null)
        {
            DialogueSystem.Instance.theDialogues = mainQuestStatus.MainQuestDefinition.finishDialogue;
            DialogueSystem.Instance.StartDialogue();
        }

        CreateTemplateQuest();

        SaveQuests();


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


        //buat logika save main quest
        //MainQuestSaveData mainQuestSaveData = new 
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

    public void SaveMainQuest()
    {
        // Pastikan ada Main Quest aktif yang bisa disimpan
        if (activePlayerMainQuestStatus == null)
        {
            Debug.Log("Tidak ada Main Quest aktif untuk disimpan.");
            return;
        }

        MainQuestSaveData mainQuestSaveData = new MainQuestSaveData(activePlayerMainQuestStatus);

        // JsonUtility.ToJson bisa langsung menserialisasi satu objek
        string json = JsonUtility.ToJson(mainQuestSaveData, true); // 'true' untuk pretty print (mudah dibaca)

        // Application.persistentDataPath adalah lokasi yang aman untuk menyimpan data di berbagai platform
        string filePath = Application.persistentDataPath + "/mainQuestSave.json"; // Gunakan ekstensi .json

        // Tulis string JSON ke file
        try
        {
            System.IO.File.WriteAllText(filePath, json);
            Debug.Log($"Main Quest data berhasil disimpan ke: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Gagal menyimpan Main Quest data: {e.Message}");
        }
    }
    public void LoadMainQuest()
    {
        string filePath = Application.persistentDataPath + "/mainQuestSave.json";

        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning("File save Main Quest tidak ditemukan. Memulai dari awal.");
            return;
        }

        string json = System.IO.File.ReadAllText(filePath);
        MainQuestSaveData loadedSaveData = JsonUtility.FromJson<MainQuestSaveData>(json);

        if (loadedSaveData == null)
        {
            Debug.LogError("Data Main Quest yang dimuat kosong atau tidak valid.");
            return;
        }

        Debug.Log($"Data JSON berhasil diparsing. questNameID dari save: '{loadedSaveData.questNameID}'"); // <<< Tambahkan ini

        MainQuestSO foundMainQuestSO = Resources.Load<MainQuestSO>(loadedSaveData.questNameID); // Line 404

        if (foundMainQuestSO == null)
        {
            Debug.LogError($"ERROR: MainQuestSO dengan ID '{loadedSaveData.questNameID}' tidak ditemukan saat memuat. Quest tidak dapat dilanjutkan.");
            return;
        }

        // Hancurkan controller yang ada jika game sedang berjalan sebelum memuat
        if (activeMainQuestController != null)
        {
            Destroy(activeMainQuestController.gameObject);
            activeMainQuestController = null;
        }

        // Instantiate kembali MainQuestController prefab yang sesuai
        GameObject controllerGO = Instantiate(foundMainQuestSO.questControllerPrefab, this.transform);
        activeMainQuestController = controllerGO.GetComponent<MainQuestController>();

        if (activeMainQuestController == null)
        {
            Debug.LogError($"ERROR: Prefab '{foundMainQuestSO.questControllerPrefab.name}' tidak memiliki komponen MainQuestController.");
            Destroy(controllerGO);
            return;
        }

        // Buat objek PlayerMainQuestStatus baru dari data yang dimuat
        activePlayerMainQuestStatus = new PlayerMainQuestStatus(foundMainQuestSO); // Inisialisasi dengan definisi SO
        activePlayerMainQuestStatus.Progress = loadedSaveData.progress;
        activePlayerMainQuestStatus.CurrentStateName = loadedSaveData.currentStateName;
        activePlayerMainQuestStatus.itemProgress = loadedSaveData.GetItemProgressDictionary(); // Isi dictionary progres item

        // Panggil StartQuest pada controller yang baru di-spawn, berikan status yang sudah dimuat
        activeMainQuestController.StartQuest(this, foundMainQuestSO, activePlayerMainQuestStatus);

        Debug.Log($"Main Quest '{activePlayerMainQuestStatus.MainQuestDefinition.questName}' berhasil diinisialisasi ulang ke state '{activePlayerMainQuestStatus.CurrentStateName}'.");

        // Jika Anda punya UI untuk Quest Manager, perbarui di sini
        // CreateTemplateQuest();
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
        if (activeMainQuestController != null)
        {
            Debug.LogWarning($"Main Quest '{activeMainQuestController.questName}' sudah aktif. Tidak bisa memulai '{mainQuestSO.questName}'.");
            return;
        }
        if (mainQuestSO.questControllerPrefab == null)
        {
            Debug.LogError($"Prefab controller untuk '{mainQuestSO.questName}' belum diatur!");
            return;
        }

        Debug.Log($"MEMULAI MAIN QUEST: {mainQuestSO.questName}");

        // Instantiate controller prefab
        GameObject controllerGO = Instantiate(mainQuestSO.questControllerPrefab, this.transform);
        activeMainQuestController = controllerGO.GetComponent<MainQuestController>();

        // Buat dan simpan status Main Quest yang baru ke variabel kelas
        this.activePlayerMainQuestStatus = new PlayerMainQuestStatus(mainQuestSO); // <<< PENTING: Assign ke variabel kelas

        if (activeMainQuestController != null)
        {
            // Panggil StartQuest pada controller, dan berikan status Main Quest yang baru dibuat
            activeMainQuestController.StartQuest(this, mainQuestSO, activePlayerMainQuestStatus);
            pendingMainQuest = null; // Hapus dari antrian setelah dimulai
            CreateTemplateQuest(); // Perbarui UI


        }
    }

    // Handle Template Story active
    public void HandleContentStory(Sprite sp)
    {
        contentStory.gameObject.SetActive(true);
        // Beritahu C# untuk menggunakan Image dari namespace UnityEngine.UI
        UnityEngine.UI.Image image = contentStory.Find("ImageScene").GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.sprite = sp;
        }
        else
        {
            Debug.LogError("Tidak ditemukan komponen Image di dalam ContentStory!");
        }
    }

    public void HideContentStory()
    {
        contentStory.gameObject.SetActive(false);
    }
}

