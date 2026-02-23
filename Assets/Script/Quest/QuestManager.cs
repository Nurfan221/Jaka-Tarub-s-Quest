using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public MainQuestController activeMainQuestController;
    public TemplateMainQuest mainQuestActive;

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
        CheckForNewQuests();


    }

    // Di dalam QuestManager.cs
    public object CaptureState()
    {
        Debug.Log("[SAVE-CAPTURE] QuestManager menangkap data quest aktif...");

        // Buat "salinan" baru dari list questActive.
        var snapshot = new List<ChapterQuestActiveDatabase>(questActive);
        return snapshot;
    }

    public void RestoreState(object state)
    {
        Debug.Log("[LOAD-RESTORE] QuestManager merestorasi data quest aktif...");
        questActive.Clear();

        var loadedData = state as List<ChapterQuestActiveDatabase>;

        if (loadedData != null)
        {
            questActive = loadedData;

            Debug.Log($"Data quest berhasil direstorasi. {questActive.Count} chapter aktif dimuat.");
            RestoreActiveQuestLogic();
            ReconnectQuestReferences();
            // Gunakan Coroutine untuk menunggu NPC siap.
            StartCoroutine(WaitAndInitializeQuests());
        }
        else
        {
            Debug.LogWarning("Gagal merestorasi data quest: data tidak valid atau corrupt.");
        }
    }
    private void ReconnectQuestReferences()
    {
        Debug.Log("[RE-LINK] Memulai penyambungan ulang referensi aset ScriptableObject...");

        // Loop semua chapter yang ada di Save Data (questActive)
        foreach (var activeChapter in questActive)
        {
            // Cari Referensi Chapter Asli di Database (allChapters) berdasarkan ID
            ChapterSO originalChapter = allChapters.Find(c => c.chapterID == activeChapter.chapterID);

            if (originalChapter == null)
            {
                Debug.LogError($"[ERROR] Chapter ID {activeChapter.chapterID} tidak ditemukan di Database allChapters!");
                continue;
            }

            //  Perbaiki setiap Side Quest di chapter ini
            foreach (var loadedQuest in activeChapter.sideQuests)
            {
                // Cari blueprint asli quest ini berdasarkan Nama
                QuestSO originalQuest = originalChapter.sideQuests.Find(q => q.questName == loadedQuest.questName);

                if (originalQuest != null)
                {
                    loadedQuest.startDialogue = originalQuest.startDialogue;
                    loadedQuest.finishDialogue = originalQuest.finishDialogue;
                    loadedQuest.NPCItem = originalQuest.NPCItem;
                    loadedQuest.itemRewards = originalQuest.itemRewards; // List item juga perlu direferensikan ulang

                    // Jangan reset progress (count), cukup referensi asetnya saja

                    Debug.Log($"[SUKSES] Referensi dialog untuk quest '{loadedQuest.questName}' berhasil dipulihkan.");
                }
                else
                {
                    Debug.LogWarning($"Quest '{loadedQuest.questName}' ada di Save Data tapi tidak ditemukan di Database Chapter Asli.");
                }
            }

            // Perbaiki Main Quest (Jika ada dan aktif)
            if (activeChapter.isMainQuestActive && activeChapter.mainQuest != null)
            {
                
            }
        }
    }
    private IEnumerator WaitAndInitializeQuests()
    {
        yield return new WaitForEndOfFrame();

    

        Debug.Log("Inisialisasi Quest tertunda dijalankan (Menunggu NPC Load)...");
        CreateTemplateQuest();
    }
    // Di dalam QuestManager.cs
    private void RestoreActiveQuestLogic()
    {
        // Bersihkan controller lama jika ada (untuk keamanan)
        if (activeMainQuestController != null)
        {
            Destroy(activeMainQuestController.gameObject);
            activeMainQuestController = null;
        }

        // Cari data main quest yang aktif di dalam list 'questActive'
        TemplateMainQuest activeQuestData = null;
        foreach (var chapter in questActive)
        {
            // Gunakan flag 'isMainQuestActive' yang sudah Anda siapkan
            if (chapter.isMainQuestActive && !chapter.IsMainQuestEmpty())
            {
                activeQuestData = chapter.mainQuest;
                break; // Hanya boleh ada satu main quest aktif
            }
        }

        //  Jika data quest aktif ditemukan, hidupkan controllernya!
        if (activeQuestData != null)
        {
            Debug.Log($"Merestorasi logika untuk Main Quest: {activeQuestData.questName}");
            // Cari prefab langsung dari database chapter yang sudah ada!
            GameObject prefab = GetMainQuestPrefabByName(activeQuestData.questName);

            if (prefab != null)
            {

                // Panggil fungsi StartMainQuest yang sudah Anda miliki.
                // Ini akan membuat ulang prefab dan mengisi 'activeMainQuestController'
                SetNextMainQuest(activeQuestData);
                StartMainQuest(activeQuestData, prefab);
            }

        }
        else
        {
            Debug.Log("Tidak ada main quest aktif yang ditemukan di data save.");
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


        if (IsMainQuestActiveEmpty())
        {
            return; // Tidak ada yang perlu dicek
        }


        if (activeMainQuestController != null)
        {
            return; // Sudah ada main quest yang aktif
        }


        if (TimeManager.Instance.date == mainQuestActive.dateToActivate &&
        TimeManager.Instance.bulan == mainQuestActive.monthToActivate)
        {
            GameObject prefab = GetMainQuestPrefabByName(mainQuestActive.questName);
            Debug.Log($"Memulai Main Quest yang tertunda: {mainQuestActive.questName}");
            StartMainQuest(mainQuestActive, prefab);
        }
        else
        {
            // Ganti LogError menjadi Log biasa agar tidak spam
            Debug.Log($"Main Quest '{mainQuestActive.questName}' menunggu hari aktivasi. (Sekarang: {TimeManager.Instance.date}, Aktivasi: {mainQuestActive.dateToActivate})");
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
            newChapter.mainQuest = null; 

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
            foreach (var sideQuest in questStatus.sideQuests)
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
                        if (npcTargetQuest != null)
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

                        }
                        else
                        {
                            Debug.LogError("spawner tidak ditemukan " + sideQuest.spawnerToActivate);
                        }
                    }

                    if (sideQuest.startLocateNpcQuest != Vector2.zero && sideQuest.finishLocateNpcQuest != Vector2.zero)
                    {
                        npcTargetQuest.transform.position = sideQuest.startLocateNpcQuest;
                        // Panggil metode yang sudah kita siapkan di NPCBehavior
                        npcTargetQuest.OverrideForQuest(sideQuest.startLocateNpcQuest, sideQuest.finishLocateNpcQuest, sideQuest.startDialogue, "Peringatan");
                    }
                    NPCManager.Instance.AddDialogueForNPCQuest(sideQuest.npcName, sideQuest.startDialogue);
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
        foreach (var itemReward in questStatus.itemRewards)
        {
            //  Coba masukkan ke tas
            bool isSuccess = ItemPool.Instance.AddItem(itemReward);

            if (isSuccess)
            {
                Debug.Log($"Menerima reward masuk tas: {itemReward.itemName} x{itemReward.count}");
            }
            else
            {
                //  JIKA GAGAL (Tas Penuh), JATUHKAN KE TANAH
                Debug.LogWarning($"Tas penuh! Menjatuhkan {itemReward.itemName} ke tanah.");

                // Asumsi Anda punya fungsi untuk spawn item di dunia (World Item)
                // Posisi jatuhnya di dekat pemain
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                Vector3 playerPosition = PlayerUI.Instance.player.transform.position;
                // Panggil fungsi drop item Anda (sesuaikan dengan sistem Anda)
                ItemPool.Instance.DropItem(itemReward.itemName, itemReward.itemHealth, itemReward.quality, playerPosition + offset, 1);
                // Atau: Instantiate(itemReward.prefab, dropPosition, Quaternion.identity);
            }
        }

        // Setelah loop selesai, quest aman untuk ditutup/diselesaikan
        // karena semua item sudah diberikan (entah di tas atau di tanah).

        NPCBehavior behavior = NPCManager.Instance.GetActiveNpcByName(questStatus.npcName);
        if (behavior.emoticonTransform != null && behavior.emoticonTransform.gameObject.activeSelf)
        {
            // Mulai ulang animasinya
            behavior.HideEmoticon();
        }

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
            if (parentChapter.completedSideQuestCount >= parentChapter.totalSideQuestsRequired)
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
                        if (TimeManager.Instance.date >= 28)
                        {
                            newMainQuest.dateToActivate = 1;
                            newMainQuest.monthToActivate = TimeManager.Instance.bulan + 1;
                        }
                        else
                        {
                            newMainQuest.dateToActivate = TimeManager.Instance.date + 1;
                            newMainQuest.monthToActivate = TimeManager.Instance.bulan;
                        }
                        //if (IsMainQuestActiveEmpty())
                        //{
                        //    mainQuestActive = newMainQuest;
                        //}
                        SetNextMainQuest(newMainQuest);
                        parentChapter.mainQuest = newMainQuest;
                        parentChapter.isMainQuestActive = true;

                        Debug.Log("cek main quest di chapter aktif " + parentChapter.mainQuest.questName);
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
    public bool IsMainQuestActiveEmpty()
    {
        //  Cek apakah referensi 'mainQuestActive' itu sendiri null
        // ATAU (jika tidak null) cek apakah 'questName' di dalamnya kosong
        return mainQuestActive == null || string.IsNullOrEmpty(mainQuestActive.questName);
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

    public GameObject GetMainQuestPrefabByName(string questName)
    {
        // Cari di semua chapter
        foreach (var chapter in allChapters)
        {
            //  Cek apakah chapter ini punya Main Quest dan namanya cocok
            if (chapter.mainQuest != null && chapter.mainQuest.questName == questName)
            {
                // KEMBALIKAN PREFAB DARI BLUEPRINT ASLI
                return chapter.mainQuest.questControllerPrefab;
            }
        }

        Debug.LogError($"QuestManager: Tidak menemukan blueprint Main Quest dengan nama '{questName}' di daftar allChapters.");
        return null;
    }

    ////Logika menjalankan Main Quest
    public void SetNextMainQuest(TemplateMainQuest mainQuest)
    {
        // Hanya siapkan quest jika belum ada yang disiapkan atau sedang aktif
        if (IsMainQuestActiveEmpty() && activeMainQuestController == null)
        {
            mainQuestActive = mainQuest;

            Debug.Log($"Main Quest '{mainQuest.questName}' disiapkan untuk dimulai pada tanggal {mainQuest.dateToActivate}.");
        }
    }
    public void StartMainQuest(TemplateMainQuest mainQuestTemplate, GameObject prefabMainQuestController)
    {
        if (IsMainQuestActiveEmpty())
        {
            Debug.LogWarning($"Main Quest '{mainQuestTemplate.questName}' sudah aktif. Tidak bisa memulai .");
            return;
        }
        if (prefabMainQuestController == null)
        {
            Debug.LogError($"Prefab controller untuk '{mainQuestTemplate.questName}' belum diatur!");
            return;
        }

        Debug.Log($"MEMULAI MAIN QUEST: {mainQuestTemplate.questName}");

        //Instantiate controller prefab
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject controllerGO = Instantiate(prefabMainQuestController, this.transform);
        activeMainQuestController = controllerGO.GetComponent<MainQuestController>();


        if (activeMainQuestController != null)
        {
            // Panggil StartQuest pada controller, dan berikan status Main Quest yang baru dibuat
            activeMainQuestController.StartQuest(this, mainQuestTemplate);
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

