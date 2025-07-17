using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO; // Penting untuk operasi file

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    [Header("Template Quest UI")]
    public Transform TemplateQuest;
    public Transform ContentTemplate;

    [Header("Database Quest (Aset SO)")]
    // Cukup seret semua aset ChapterSO Anda ke sini.
    public List<ChapterSO> allChapters;

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

    /// <summary>
    /// Dipanggil setiap hari baru untuk memeriksa apakah ada quest baru yang harus diaktifkan.
    /// </summary>
    public void CheckForNewQuests()
    {
        // Loop melalui setiap aset Chapter yang kita miliki
        foreach (var chapterAsset in allChapters)
        {
            // Loop melalui setiap aset Side Quest di dalam chapter tersebut
            foreach (var questAsset in chapterAsset.sideQuests)
            {
                // Lewati jika quest ini sudah ada di log pemain (baik aktif maupun selesai)
                if (IsQuestInLog(questAsset.questName)) continue;

                // Cek apakah tanggal aktivasi quest sudah tiba
                if (TimeManager.Instance.timeData_SO.date == questAsset.dateToActivate)
                {
                    ActivateQuest(questAsset);
                }
            }
        }
    }

    /// Mengaktifkan sebuah quest dan menambahkannya ke log pemain.
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
        // Pastikan ContentTemplate sudah diatur di Inspector
        if (ContentTemplate == null)
        {
            Debug.LogError("ContentTemplate belum diatur di Inspector!");
            return;
        }
        // Hapus semua anak dari ContentTemplate sebelum membuat yang baru
        foreach (Transform child in ContentTemplate)
        {
            // Hati-hati, jika TemplateQuest adalah anak dari ContentTemplate, ini akan menghapusnya.
            // Sebaiknya simpan TemplateQuest di luar ContentTemplate.
            Destroy(child.gameObject);
        }

        // Buat template untuk setiap quest yang ada di log
        foreach (var questStatus in questLog)
        {
           if(questStatus.Progress != QuestProgress.Completed)
            {
                Transform questTransform = Instantiate(TemplateQuest, ContentTemplate);
                questTransform.gameObject.SetActive(true);

                questTransform.gameObject.name = $"Quest - {questStatus.Quest.questName}";


                TMP_Text questNameText = questTransform.GetComponentInChildren<TMP_Text>();
                if (questNameText != null)
                {
                    questNameText.text = questStatus.Quest.questName;
                }
                else
                {
                    Debug.LogWarning("Tidak ditemukan TMP_Text di dalam TemplateQuest!");
                }
            }

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
        PlayerQuestStatus quest = questStatus;
        if (questStatus != null && questStatus.Progress == QuestProgress.Accepted)
        {
            //questStatus.Progress = QuestProgress.Completed;
            questStatus.Progress = QuestProgress.Completed;
            Debug.Log($"Quest '{questStatus.Quest.questName}' telah selesai!");

            // Berikan hadiah ke pemain di sini
            GameEconomy.Instance.GainMoney(questStatus.Quest.goldReward);
            CreateTemplateQuest();
            GameEconomy.Instance.UpdateMoneyText();
            SaveQuests();
            // gameEconomy.money += questStatus.Quest.goldReward;
            // foreach(var item in questStatus.Quest.itemRewards) { ... }

            //OnQuestLogUpdated?.Invoke(); // Beri tahu UI untuk refresh
        }
    }

    private string GetSavePath()
    {
        // Application.persistentDataPath adalah folder aman di setiap platform (PC, Android, iOS)
        // untuk menyimpan data game.
        return Path.Combine(Application.persistentDataPath, "questprogress.json");
    }

    public void SaveQuests()
    {
        // 1. Buat list baru yang berisi data yang siap disimpan
        List<QuestSaveData> saveDataList = new List<QuestSaveData>();
        foreach (var questStatus in questLog)
        {
            saveDataList.Add(new QuestSaveData(questStatus));
        }

        // 2. Ubah list tersebut menjadi format teks JSON
        string json = JsonUtility.ToJson(new Serialization<QuestSaveData>(saveDataList), true);

        //// 3. Tulis teks JSON tersebut ke dalam sebuah file
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

                    // --- PERBAIKAN ---
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
}

