using System;
using System.Collections.Generic;
using System.IO; // Penting untuk operasi file
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using NUnit.Framework.Interfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
//using UnityEngine.UIElements;

public class QuestManager : MonoBehaviour, ISaveable
{
    public static QuestManager Instance { get; private set; }
    [Header("Template Quest UI")]
    public Transform templateQuest;
    public Transform contentTemplate;
    public Transform contentStory;

    [Header("Database Quest (Aset SO)")]
    // Cukup seret semua aset ChapterSO Anda ke sini.
    public List<ChapterSO> allChapters;
    public Dialogues dialogueIfItemNotComplate;
  
    public int currentChapterQuestIndex = 1;

    [Header("Status Quest Pemain")]
    // List ini akan melacak semua quest (side quest) yang sedang aktif atau sudah selesai.
    //public List<PlayerQuestStatus> questLog = new List<PlayerQuestStatus>();
    public List<ChapterQuestActiveDatabase> questActive = new List<ChapterQuestActiveDatabase>();


    // Event ini akan memberitahu UI atau sistem lain jika ada pembaruan pada log quest.
    // public static event System.Action OnQuestLogUpdated;



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

    public object CaptureState()
    {
        Debug.Log("[SAVE-CAPTURE] QuestManager menangkap data quest aktif...");

     
        return questActive;
    }

    public void RestoreState(object state)
    {
        Debug.Log("[LOAD-RESTORE] QuestManager merestorasi data quest aktif...");
        questActive.Clear();
        // Coba cast 'state' yang datang kembali ke tipe aslinya.
        var loadedData = state as List<ChapterQuestActiveDatabase>;

        if (loadedData != null)
        {
            //  Ganti list 'questActive' saat ini dengan data dari file save.
            questActive = loadedData;

            Debug.Log($"Data quest berhasil direstorasi. {questActive.Count} chapter aktif dimuat.");

            // Anda sudah punya fungsi ini, jadi kita panggil saja.
            CreateTemplateQuest();
        }
        else
        {
            Debug.LogWarning("Gagal merestorasi data quest: data tidak valid atau corrupt.");
        }
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

      
    }
    public bool IsQuestInLog(string questName)
    {
        return questActive.SelectMany(chapter => chapter.sideQuests)
                          .Any(quest => quest.questName == questName);
    }


    public void AddQuestActivetoList(TemplateQuest questCopy, int chapterID, string chapterName, int totalQuests)
    {
        Debug.Log("Total quest di chapter ini: " + totalQuests);
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
            existingChapter.sideQuests.Add(questCopy);
        }
        else
        {
            ChapterQuestActiveDatabase newChapter = new ChapterQuestActiveDatabase();
            newChapter.chapterID = chapterID;
            newChapter.chapterName = chapterName;
            newChapter.sideQuests = new List<TemplateQuest>();

            newChapter.totalSideQuestsRequired = totalQuests; // Simpan totalnya
            newChapter.completedSideQuestCount = 0; // Mulai dari 0
            newChapter.mainQuest = null; // Main quest belum aktif
                                         // ---------------------

            newChapter.sideQuests.Add(questCopy);
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
            int totalQuestsInChapter = parentChapter.sideQuests.Count;
            AddQuestActivetoList(newActiveQuest, parentChapter.chapterID, parentChapter.chapterName, totalQuestsInChapter);
        }
        else
        {
            Debug.LogError($"GAGAL AKTIVASI: Quest '{template.questName}' tidak terdaftar di ChapterSO manapun! Periksa database 'allChapters'.");
            return; // Hentikan aktivasi jika chapter tidak ditemukan
        }
        Debug.Log($"Mengaktifkan Side Quest: {questToActivate.questName}");

       
        //OnQuestLogUpdated?.Invoke();
        CreateTemplateQuest();

    }
    // Fungsi helper untuk mengecek apakah sebuah quest sudah ada di log.






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
            DialogueSystem.Instance.HandlePlayDialogue(dialogueIfItemNotComplate);
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

                            // Panggil fungsi CompleteQuest di sini!
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
                    // Buat data status baru untuk quest ini
                    NPCBehavior npcTargetQuest = NPCManager.Instance.GetActiveNpcByName(sideQuest.npcName);




                    if (sideQuest.isNPCItem)
                    {
                        Debug.Log("mengaktifkan NPCItem");
                        //Panggil fungsi dari NPCManager untuk mencari Jhorgeo di daftar NPC aktif.


                        //    jika karena suatu alasan NPC tidak ditemukan.
                        if (npcTargetQuest != null )
                        {
                            // Jika ditemukan, Anda sekarang memiliki akses penuh ke komponen NPCBehavior-nya
                            //    dan bisa memanggil semua metode publiknya.
                            Debug.Log($"NPC {sideQuest.npcName} ditemukan! Memberi perintah untuk pindah...");


                          
                            npcTargetQuest.itemQuestToGive = sideQuest.NPCItem;
                            npcTargetQuest.isGivenItemForQuest = true;
                            Debug.Log("itemQuestToGive" + npcTargetQuest.itemQuestToGive.itemName + "jumlah item : " + npcTargetQuest.itemQuestToGive.ToString());
                        }
                        else
                        {
                            Debug.LogError($"Gagal memberi perintah: NPC aktif bernama {sideQuest.npcName} tidak ditemukan!");
                        }
                    }
                    if (sideQuest.isSpawner)
                    {

                        MainEnvironmentManager.Instance.spawnerManager.AddSpawnerToList();
                        Enemy_Spawner enemy_Spawner = MainEnvironmentManager.Instance.spawnerManager.GetEnemySpawner(sideQuest.spawnerToActivate);
                        if (enemy_Spawner != null)
                        {
                            enemy_Spawner.gameObject.SetActive(true);
                            Debug.Log("mengaktifkan spawner quest di " + sideQuest.spawnerToActivate);

                        }else
                        {
                            Debug.LogError("spawner tidak ditemukan " + sideQuest.spawnerToActivate);
                        }
                    }

                    if(sideQuest.startLocateNpcQuest != Vector2.zero && sideQuest.finishLocateNpcQuest != Vector2.zero)
                    {
                        npcTargetQuest.transform.position = sideQuest.startLocateNpcQuest;
                        // Panggil metode yang sudah kita siapkan di NPCBehavior
                        npcTargetQuest.OverrideForQuest(sideQuest.startLocateNpcQuest, sideQuest.finishLocateNpcQuest, sideQuest.startDialogue, "Peringatan");
                    }
                    NPCManager.Instance.AddDialogueForNPCQuest(sideQuest.npcName, sideQuest.startDialogue);
                }
            }
        }

      
        //// Cek dulu apakah 'activeMainQuestController' ada, BARU akses propertinya.
        //if (activeMainQuestController != null && !activeMainQuestController.IsComplete())
        //{
        //    // Ini membutuhkan sebuah fungsi publik baru di MainQuestController Anda.
        //    string currentObjective = activeMainQuestController.GetCurrentObjectiveInfo();

        //    // Gunakan fungsi helper yang sama untuk membuat UI-nya
        //    InstantiateQuestUI(activeMainQuestController.questName, currentObjective);
        //}
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
        Debug.Log($"memanggil fungsi complateQuest '{questStatus.questName} {questStatus.questProgress}' telah selesai! ");
        // Pengecekan keamanan di awal
        if (questStatus == null || questStatus.questProgress != QuestProgress.Completed)
        {
            Debug.LogWarning("Quest tidak valid atau belum diterima. Tidak dapat menyelesaikan quest.");
            return;
        }
        else
        {
            Debug.Log($"Menyelesaikan Side Quest: {questStatus.questName}");
        }


        // (Tambahkan logika untuk memberi item reward di sini jika ada)
        GameEconomy.Instance.GainMoney(questStatus.goldReward);
        foreach (var item in questStatus.itemRewards)
        {
            ItemPool.Instance.AddItem(item);
        }

        NPCBehavior behavior = NPCManager.Instance.GetActiveNpcByName(questStatus.npcName);
        behavior.emoticonTransform.gameObject.SetActive(false);

        // Pastikan referensi DialogueSystem ada dan benar
        if (questStatus.finishDialogue != null)
        {
            DialogueSystem.Instance.npcName = questStatus.npcName;
            DialogueSystem.Instance.theDialogues = questStatus.finishDialogue;
            DialogueSystem.Instance.StartDialogue();
        }

        behavior.ReturnToNormalSchedule();

        ChapterQuestActiveDatabase parentChapter = FindActiveChapterForQuest(questStatus);

        if (parentChapter != null)
        {
            // Tambah counter chapter tersebut
            parentChapter.completedSideQuestCount++;
            Debug.Log($"Chapter {parentChapter.chapterName}: {parentChapter.completedSideQuestCount} / {parentChapter.totalSideQuestsRequired} side quest selesai.");

            //Cek apakah sudah selesai SEMUA DAN main quest BELUM aktif
            if (parentChapter.completedSideQuestCount >= parentChapter.totalSideQuestsRequired )
            {
                if (parentChapter.IsMainQuestEmpty())
                {
                    Debug.Log($"SEMUA SIDE QUEST SELESAI! Mengaktifkan Main Quest untuk {parentChapter.chapterName}...");

                    // Cari blueprint Main Quest dari allChapters
                    ChapterSO chapterAsset = allChapters.FirstOrDefault(ch => ch.chapterID == parentChapter.chapterID);
                    if (chapterAsset != null && chapterAsset.mainQuest != null)
                    {
                        // Buat dan aktifkan main quest
                        TemplateMainQuest newMainQuest = new TemplateMainQuest(chapterAsset.mainQuest);
                        Debug.Log($"Main Quest '{newMainQuest.questName}' diaktifkan untuk Chapter '{parentChapter.chapterName}'.");
                        parentChapter.mainQuest = newMainQuest;
                        Debug.Log("cek main quest di chapter aktif " + parentChapter.mainQuest.questName);
                        Debug.Break(); // Ini akan mem-pause game
                                       // (Opsional) Langsung panggil StartMainQuest jika Anda mau,
                                       // atau biarkan CheckForNewQuests() yang menanganinya
                                       // StartMainQuest(chapterAsset.mainQuest); 
                    }
                    else
                    {
                        Debug.Log("Gagal mengaktifkan Main Quest: Blueprint Main Quest tidak ditemukan di ChapterSO.");
                    }
                }
                else
                {
                    // Ini akan mencegah main quest dibuat berulang kali
                    Debug.Log("Main quest sudah aktif, tidak perlu membuat lagi.");
                }
               
            }
        }


        CreateTemplateQuest();


        //ChapterSO parentChapter = FindChapterForQuest(questStatus.Quest);
        //if (parentChapter != null && AreAllSideQuestsComplete(parentChapter))
        //{
        //    if (parentChapter.mainQuest != null)
        //    {
        //        SetNextMainQuest(parentChapter.mainQuest);
        //    }
        //}
    }
    private ChapterQuestActiveDatabase FindActiveChapterForQuest(TemplateQuest questToFind)
    {
        foreach (var chapter in questActive)
        {
            if (chapter.sideQuests.Contains(questToFind))
            {
                return chapter;
            }
        }
        return null; // Tidak ditemukan
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

  

    ////Logika menjalankan Main Quest
    //public void SetNextMainQuest(TemplateMainQuest mainQuest)
    //{
    //    // Hanya siapkan quest jika belum ada yang disiapkan atau sedang aktif
    //    if (pendingMainQuest == null && activeMainQuestController == null)
    //    {
    //        pendingMainQuest = mainQuest;
    //        pendingMainQuest.dateToActivate = TimeManager.Instance.date + 1;
    //        pendingMainQuest.monthToActivate = TimeManager.Instance.bulan;
    //        Debug.Log($"Main Quest '{mainQuest.questName}' disiapkan untuk dimulai pada tanggal {mainQuest.dateToActivate}.");
    //    }
    //}
    //public void StartMainQuest(MainQuestSO mainQuestSO)
    //{
    //    if (activeMainQuestController != null)
    //    {
    //        Debug.LogWarning($"Main Quest '{activeMainQuestController.questName}' sudah aktif. Tidak bisa memulai '{mainQuestSO.questName}'.");
    //        return;
    //    }
    //    if (mainQuestSO.questControllerPrefab == null)
    //    {
    //        Debug.LogError($"Prefab controller untuk '{mainQuestSO.questName}' belum diatur!");
    //        return;
    //    }

    //    Debug.Log($"MEMULAI MAIN QUEST: {mainQuestSO.questName}");

    //    // Instantiate controller prefab
    //    GameObject controllerGO = Instantiate(mainQuestSO.questControllerPrefab, this.transform);
    //    activeMainQuestController = controllerGO.GetComponent<MainQuestController>();

    //    // Buat dan simpan status Main Quest yang baru ke variabel kelas
    //    this.activePlayerMainQuestStatus = new PlayerMainQuestStatus(mainQuestSO); // <<< PENTING: Assign ke variabel kelas

    //    if (activeMainQuestController != null)
    //    {
    //        // Panggil StartQuest pada controller, dan berikan status Main Quest yang baru dibuat
    //        activeMainQuestController.StartQuest(this, mainQuestSO, activePlayerMainQuestStatus);
    //        pendingMainQuest = null; // Hapus dari antrian setelah dimulai
    //        CreateTemplateQuest(); // Perbarui UI


    //    }
    //}

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

