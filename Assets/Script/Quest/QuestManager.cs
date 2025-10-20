using System;
using System.Collections.Generic;
using System.IO; // Penting untuk operasi file
using System.Linq;
using JetBrains.Annotations;
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
    public int currentChapterQuestIndex = 1;

    [Header("Status Quest Pemain")]
    // List ini akan melacak semua quest (side quest) yang sedang aktif atau sudah selesai.
    //public List<PlayerQuestStatus> questLog = new List<PlayerQuestStatus>();
    public List<ChapterQuestActiveDatabase> questActive = new List<ChapterQuestActiveDatabase>();


    // Event ini akan memberitahu UI atau sistem lain jika ada pembaruan pada log quest.
    //public static event System.Action OnQuestLogUpdated;



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
        //StartMainQuest(pendingMainQuest);
        CheckForNewQuests();


    }

    public void DialogueEnd()
    {
        // Cek apakah ada quest yang sedang aktif
        HideContentStory();
    }
    // Dipanggil setiap hari baru untuk memeriksa apakah ada quest baru yang harus diaktifkan.
    public void CheckForNewQuests()
    {
        Debug.Log("Memeriksa quest baru pada tanggal " + TimeManager.Instance.date + ", bulan " + TimeManager.Instance.bulan);
        foreach (var chapterAsset in allChapters)
        {
            if (chapterAsset.chapterID == currentChapterQuestIndex)
            {
                foreach (var questAsset in chapterAsset.sideQuests)
                {
                    if (IsQuestInLog(questAsset.questName)) continue;
                    if (TimeManager.Instance.date == questAsset.dateToActivate && TimeManager.Instance.bulan == questAsset.MonthToActivate)
                    {
                        ActivateQuest(questAsset);
                    }
                }
            }
           
        }

        if (pendingMainQuest != null && activeMainQuestController == null)
        {
            if (TimeManager.Instance.date == pendingMainQuest.dateToActivate && TimeManager.Instance.bulan == pendingMainQuest.monthToActivate)
            {
                Debug.Log($"Memulai Main Quest yang tertunda: {pendingMainQuest.questName}");
                StartMainQuest(pendingMainQuest);
            }
            else
            {
                Debug.LogError($"Main Quest '{pendingMainQuest.questName}' belum bisa dimulai. Tanggal saat ini: {TimeManager.Instance.date}, Bulan: {TimeManager.Instance.bulan}. Tanggal aktivasi: {pendingMainQuest.dateToActivate}, Bulan: {pendingMainQuest.monthToActivate}");
            }
        }
    }
    public bool IsQuestInLog(string questName)
    {
        return questActive.SelectMany(chapter => chapter.sideQuests)
                          .Any(quest => quest.questName == questName);
    }


    public void AddQuestActivetoList(TemplateQuest questCopy, int chapterID, string chapterName)
    {
        //  Cari tahu apakah chapter ini sudah ada di dalam list quest aktif.
        ChapterQuestActiveDatabase existingChapter = null;
        foreach (var chapter in questActive)
        {
            if (chapter.chapterID == chapterID)
            {
                existingChapter = chapter;
                break; // Ditemukan, hentikan pencarian
            }
        }

        //  Jika chapter SUDAH ADA...
        if (existingChapter != null)
        {
            Debug.Log($"Chapter {chapterID} sudah ada. Menambahkan quest '{questCopy.questName}' ke dalamnya.");
            // Cukup tambahkan quest baru ke dalam list sideQuests di chapter yang sudah ada.
            existingChapter.sideQuests.Add(questCopy);
        }
        // Jika chapter BELUM ADA...
        else
        {
            Debug.Log($"Chapter {chapterID} belum ada. Membuat entri baru untuk chapter ini.");
            // Buat objek chapter BARU.
            ChapterQuestActiveDatabase newChapter = new ChapterQuestActiveDatabase();
            newChapter.chapterID = chapterID;
            newChapter.chapterName = chapterName;
            // PENTING: Inisialisasi list sideQuests sebelum digunakan!
            newChapter.sideQuests = new List<TemplateQuest>();

            // Tambahkan quest pertama ke chapter baru ini.
            newChapter.sideQuests.Add(questCopy);

            // DAN YANG PALING PENTING: Tambahkan chapter baru ini ke list utama `questActive`.
            questActive.Add(newChapter);
        }
    }

    // Mengaktifkan sebuah quest dan menambahkannya ke log pemain.
    public void ActivateQuest(QuestSO questToActivate)
    {
        var template = questToActivate;
        Debug.Log($"Mengaktifkan Side Quest: {template.questName}");

        ChapterSO parentChapter = FindChapterForQuest(questToActivate);

        // Tambahkan blok 'else' untuk penanganan error
        if (parentChapter != null)
        {
            TemplateQuest newActiveQuest = new TemplateQuest(questToActivate);

            // Tambahkan salinan baru ini ke list `questActive`
            AddQuestActivetoList(newActiveQuest, parentChapter.chapterID, parentChapter.chapterName);
        }
        else
        {
            Debug.LogError($"GAGAL AKTIVASI: Quest '{template.questName}' tidak terdaftar di ChapterSO manapun! Periksa database 'allChapters'.");
            return; // Hentikan aktivasi jika chapter tidak ditemukan
        }
        Debug.Log($"Mengaktifkan Side Quest: {questToActivate.questName}");

        // Buat data status baru untuk quest ini
        NPCBehavior npcTargetQuest = NPCManager.Instance.GetActiveNpcByName(questToActivate.npcName);



        if (parentChapter != null)
        {
            //    perubahan di masa depan tidak mempengaruhi aset aslinya.
            TemplateQuest questCopy = new TemplateQuest(template); // Butuh constructor penyalinan

            // Panggil fungsi baru kita untuk menambahkan salinan ini ke list `questActive`.
            AddQuestActivetoList(questCopy, parentChapter.chapterID, parentChapter.chapterName);
        }
        if (questToActivate.isSpawner)
        {
            Debug.Log("mengaktifkan spawner SpawnerQuest1_6");
            SpawnerManager.Instance.HandleSpawnerActive(questToActivate.spawnerToActivate);
        }



        if (questToActivate.isNPCItem)
        {
            Debug.Log("mengaktifkan NPCItem");
            //Panggil fungsi dari NPCManager untuk mencari Jhorgeo di daftar NPC aktif.


            // SELALU periksa apakah hasilnya null. Ini penting untuk menghindari error
            //    jika karena suatu alasan NPC tidak ditemukan.
            if (npcTargetQuest != null)
            {
                // Jika ditemukan, Anda sekarang memiliki akses penuh ke komponen NPCBehavior-nya
                //    dan bisa memanggil semua metode publiknya.
                Debug.Log($"NPC {questToActivate.npcName} ditemukan! Memberi perintah untuk pindah...");


                npcTargetQuest.transform.position = questToActivate.startLocateNpcQuest;
                // Panggil metode yang sudah kita siapkan di NPCBehavior
                npcTargetQuest.OverrideForQuest(questToActivate.startLocateNpcQuest, questToActivate.finishLocateNpcQuest, questToActivate.startDialogue, "Peringatan");
                npcTargetQuest.itemQuestToGive = questToActivate.NPCItem;
                npcTargetQuest.isGivenItemForQuest = true;
                Debug.Log("itemQuestToGive" + npcTargetQuest.itemQuestToGive.itemName + "jumlah item : " + npcTargetQuest.itemQuestToGive.ToString());
            }
            else
            {
                Debug.LogError($"Gagal memberi perintah: NPC aktif bernama {questToActivate.npcName} tidak ditemukan!");
            }
        }
        NPCManager.Instance.AddDialogueForNPCQuest(questToActivate.npcName, questToActivate.startDialogue);
        // Siarkan event agar UI bisa memperbarui tampilannya
        //OnQuestLogUpdated?.Invoke();
        CreateTemplateQuest();

    }
    // Fungsi helper untuk mengecek apakah sebuah quest sudah ada di log.




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
    private void CheckIfQuestIsComplete(TemplateQuest questStatus)
    {

        bool allRequirementsMet = questStatus.itemRequirements.All(item => item.count <= 0);

        if (allRequirementsMet)
        {
            Debug.Log($"Quest '{questStatus.questName}' TELAH SELESAI!");
            questStatus.questProgress = QuestProgress.Completed;
            CompleteQuest(questStatus);
        }
        else
        {
            // Jika belum selesai, Anda bisa memicu dialog "pengingat" di sini
            Debug.Log($"Side Quest '{questStatus.questName}' belum selesai, masih ada item yang dibutuhkan.");
        }


       
    }

    public int ProcessItemGivenToNPC(ItemData givenItemData, string npcName)
    {
        //  Temukan quest aktif yang relevan.
        TemplateQuest questUse = GetQuestForGiveItem(givenItemData.itemName, npcName);

        if (questUse != null)
        {
            //  Temukan item spesifik di dalam daftar persyaratan quest tersebut.
            ItemData requirementToUpdate = questUse.itemRequirements
                .FirstOrDefault(req => req.itemName.Equals(givenItemData.itemName, StringComparison.OrdinalIgnoreCase));

            // Pastikan kita menemukan itemnya dan masih ada yang dibutuhkan (count > 0)
            if (requirementToUpdate != null && requirementToUpdate.count > 0)
            {
                //  Tentukan berapa banyak yang bisa diberikan.
                int amountToGive = Mathf.Min(givenItemData.count, requirementToUpdate.count);

                if (amountToGive > 0)
                {
                    //   Langsung kurangi jumlah yang dibutuhkan di dalam salinan quest.
                    requirementToUpdate.count -= amountToGive;

                    Debug.Log($"Quest '{questUse.questName}': Menerima {amountToGive} {givenItemData.itemName}. Sisa yang dibutuhkan: {requirementToUpdate.count}");

                    //  Kurangi item dari inventaris pemain.
                    //InventoryManager.Instance.RemoveItem(givenItemData.itemName, amountToGive);

                    //  Cek apakah quest sudah selesai.
                    CheckIfQuestIsComplete(questUse);

                    return amountToGive; // Item berhasil diproses
                }
            }
        }

        Debug.Log($"QuestManager: Item '{givenItemData.itemName}' tidak diproses oleh quest manapun.");
        return 0;
    }

    public TemplateQuest GetQuestForGiveItem(string itemName, string npcName)
    {
        foreach (var chapter in questActive)
        {
            foreach (var sideQuest in chapter.sideQuests)
            {
                if (sideQuest.questProgress == QuestProgress.Accepted && sideQuest.npcName == npcName)
                {
                    // Cek apakah quest ini membutuhkan item tersebut
                    if (sideQuest.itemRequirements.Any(req => req.itemName == itemName))
                    {
                        Debug.Log($"Quest '{sideQuest.questName}' ditemukan dan membutuhkan '{itemName}'.");
                        return sideQuest;
                    }
                }
            }
        }

        // Pindahkan Debug.Log sebelum return
        Debug.Log($"Tidak ada Side Quest aktif untuk NPC '{npcName}' yang membutuhkan item '{itemName}'.");
        return null;
    }

    public void UpdateCleanupQuest(string nameObject, EnvironmentType environmentType)
    {
        foreach (var chapter in questActive)
        {
            // Tidak perlu lagi cek chapterID, karena kita ingin update semua quest aktif
            foreach (var sideQuest in chapter.sideQuests)
            {
                if (sideQuest.isTheCleanupQuest && sideQuest.questProgress == QuestProgress.Accepted)
                {
                    if (sideQuest.objectToClean == nameObject && sideQuest.tipeCleanObject == environmentType)
                    {
                        sideQuest.cleanupQuestTotal += 1;
                        Debug.Log($"Progres pembersihan untuk '{sideQuest.questName}' diperbarui. Total: {sideQuest.cleanupQuestTotal}/{sideQuest.cleanupQuestIndex}");

                        if (sideQuest.cleanupQuestTotal >= sideQuest.cleanupQuestIndex)
                        {
                            sideQuest.isTheCleanupObjectDone = true;
                            Debug.Log($"Target pembersihan untuk '{sideQuest.questName}' telah selesai.");

                            // PENTING: Panggil fungsi CompleteQuest di sini!
                            CompleteQuest(sideQuest);
                        }
                    }
                }
            }
        }
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

        foreach (var questStatus in questActive)
        {
            // Tampilkan hanya jika belum selesai
           foreach(var sideQuest in questStatus.sideQuests)
            {
                if (sideQuest.questProgress != QuestProgress.Completed)
                {
                    InstantiateQuestUI(sideQuest.questName, sideQuest.questInfo);
                }
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

 

    // Fungsi untuk menyelesaikan quest
    public void CompleteQuest(TemplateQuest questStatus)
    {
        // Pengecekan keamanan di awal
        if (questStatus == null || questStatus.questProgress != QuestProgress.Accepted) return;


        questStatus.questProgress = QuestProgress.Completed;
        Debug.Log($"Quest '{questStatus.questName}' telah selesai!");

        GameEconomy.Instance.GainMoney(questStatus.goldReward);
        foreach (var item in questStatus.itemRewards)
        {
            ItemPool.Instance.AddItem(item);
        }

        NPCBehavior behavior = NPCManager.Instance.GetActiveNpcByName(questStatus.npcName);
        behavior.emoticonTransform.gameObject.SetActive(false);
        // (Tambahkan logika untuk memberi item reward di sini jika ada)

        // Pastikan referensi DialogueSystem ada dan benar
        if (questStatus.finishDialogue != null)
        {
            DialogueSystem.Instance.npcName = questStatus.npcName;
            DialogueSystem.Instance.theDialogues = questStatus.finishDialogue;
            DialogueSystem.Instance.StartDialogue();
        }


        CreateTemplateQuest();

        //SaveQuests();

        //ChapterSO parentChapter = FindChapterForQuest(questStatus.Quest);
        //if (parentChapter != null && AreAllSideQuestsComplete(parentChapter))
        //{
        //    if (parentChapter.mainQuest != null)
        //    {
        //        SetNextMainQuest(parentChapter.mainQuest);
        //    }
        //}
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

  

    //Logika menjalankan Main Quest
    public void SetNextMainQuest(MainQuestSO mainQuest)
    {
        // Hanya siapkan quest jika belum ada yang disiapkan atau sedang aktif
        if (pendingMainQuest == null && activeMainQuestController == null)
        {
            pendingMainQuest = mainQuest;
            pendingMainQuest.dateToActivate = TimeManager.Instance.date + 1;
            pendingMainQuest.monthToActivate = TimeManager.Instance.bulan;
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

