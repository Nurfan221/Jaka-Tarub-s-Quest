using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Ini adalah "wadah" untuk data setiap adegan. Letakkan di file yang sama.
[System.Serializable]
public class QuestStateData
{
    // Ganti 'MainQuest1State' dengan enum dari controller yang sesuai

    public MainQuest1_Controller.MainQuest1State state;
    [HideInInspector]
    public string nameState;

    [Header("Data untuk Adegan Ini")]
    public Dialogues dialogueToPlay;
    public Sprite spriteToShow;
    // public VideoClip videoToPlay; // Anda bisa tambahkan ini
    //public string questStatus; // Misalnya "Sedang Berlangsung", "Selesai", dll.
    public string objectiveInfoForUI;

    [Header("Syarat Pindah ke Adegan Berikutnya")]
    public Vector2 npcPositionStartQuest;
    public string locationToEnterTrigger;
    public GameObject[] prefabsToSpawn;
    public string animalToDefeatTrigger;

    public void OnValidate()
    {
        // Update nameState agar selalu sama dengan nama enum 'state' yang dipilih
        nameState = state.ToString();
    }
}


// Ini adalah skrip "Sutradara" utama Anda
public class MainQuest1_Controller : MainQuestController  // Pastikan mewarisi dari kelas dasar Anda
{
    public enum MainQuest1State { BelumMulai, AdeganMimpi, ApaArtiMimpiItu, PerjodohanDenganLaraswati, PermintaanJhorgeo, PergiKeHutan, CariRusa, MunculkanHarimau, BerikanHasilBuruan, CariTempatAman, ApakahIniDanauItu, KabarKesedihan, PengingatMainQuest , Selesai }

    [Header("Progres Cerita")]
    [SerializeField] private MainQuest1State currentState = MainQuest1State.BelumMulai;
    private bool isChangingState = false; // Flag "rem" kita
    private string objectiveInfoForUI;
    //public string lokasiYangDitunggu;
    public Transform lokasiYangDitunggu;
    private string nameLokasiYangDitunggu;
    private AnimalBehavior animalBehavior;
    private Vector2 positionNpc;
    private Dialogues questtDialogue;
    private NPCBehavior npcToCommand;


    //KUMPULAN "KARTU ADEGAN"
    [Header("Data untuk Setiap Adegan")]
    public List<QuestStateData> stateDataList;

    // Variabel lain yang spesifik untuk quest ini
    public List<GameObject> spawnedQuestAnimals = new List<GameObject>();
    private string targetNpcName;
    private void OnEnable()
    {
        // Berlangganan ke semua pemicu yang mungkin dibutuhkan quest ini
        //PlayerQuest.OnPlayerEnteredLocation += HandleLocationTrigger;
        AnimalBehavior.OnAnimalPickItem += HandleAnimalTrigger;
        // Berlangganan ke event akhir dialog
        DialogueSystem.OnDialogueEnded += HandleDialogueEnd;
        PlayerQuest.OnPlayerEnteredLocation += HandleLocationTrigger;
        AnimalBehavior.OnAnimalDied += HandleAnimalDied;
        TimeManager.OnDayChanged += HandleDayChange ; // Misalnya, jika quest ini berhubungan dengan pergantian hari
        Player_Health.Sekarat += HandlePlayerSekarat; // Misalnya, jika quest ini berhubungan dengan kematian pemain

        //DialogueSystem.OnDialogueEnded += HandleDialogueTrigger;
    }

    private void AnimalBehavior_OnAnimalPickItem()
    {
        throw new System.NotImplementedException();
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat selesai
        //PlayerQuest.OnPlayerEnteredLocation -= HandleLocationTrigger;
        AnimalBehavior.OnAnimalPickItem -= HandleAnimalTrigger;
        DialogueSystem.OnDialogueEnded -= HandleDialogueEnd;
        PlayerQuest.OnPlayerEnteredLocation -= HandleLocationTrigger;
        AnimalBehavior.OnAnimalDied -= HandleAnimalDied;
        Player_Health.Sekarat -= HandlePlayerSekarat; // Misalnya, jika quest ini berhubungan dengan kematian pemain
        TimeManager.OnDayChanged -= HandleDayChange; // Misalnya, jika quest ini berhubungan dengan pergantian hari
        //DialogueSystem.OnDialogueEnded -= HandleDialogueTrigger;
    }

    public override void StartQuest(QuestManager manager, MainQuestSO so, PlayerMainQuestStatus status)
    {
        base.StartQuest(manager, so, status); // Panggil implementasi dasar

        // Sekarang Anda bisa mengakses nama NPC dari naskah
        this.targetNpcName = questData.namaNpcQuest;
        Debug.Log($"NPC target untuk quest ini adalah: {this.targetNpcName}");

        // Mulai adegan pertama
        if (!string.IsNullOrEmpty(playerQuestStatus.CurrentStateName))
        {
            // Konversi string kembali ke enum
            if (System.Enum.TryParse(playerQuestStatus.CurrentStateName, out MainQuest1State loadedState))
            {
                ChangeState(loadedState); // Set state berdasarkan data yang dimuat
            }
            else
            {
                Debug.LogWarning($"Gagal memuat state '{playerQuestStatus.CurrentStateName}'. Menggunakan state awal.");
                ChangeState(MainQuest1State.AdeganMimpi); // Fallback ke state awal
            }
        }
        else
        {
            ChangeState(MainQuest1State.AdeganMimpi); // Jika tidak ada state yang dimuat, mulai dari awal
        }
    }

    private void ChangeState(MainQuest1State newState)
    {
        QuestManager.Instance.SaveMainQuest();

        if (isChangingState) return; // Jika sedang sibuk, jangan lakukan apa-apa

        isChangingState = true;
        currentState = newState;
        if (playerQuestStatus != null)
        {
            playerQuestStatus.CurrentStateName = newState.ToString();
        }
        //Debug.Log($"Main Quest '{questName}' masuk ke adegan: {currentState}");

        QuestStateData data = stateDataList.FirstOrDefault(s => s.state == newState);
        if (data == null) return;



        switch (newState)
        {
            case MainQuest1State.AdeganMimpi:
                HandleSpriteAndDialogue(MainQuest1State.AdeganMimpi);
                Debug.Log("Memulai adegan mimpi...");
                //QuestManager.Instance
                //DialogueSystem.Instance.StartDialogue(dialogMimpi);
                break;
            case MainQuest1State.ApaArtiMimpiItu:
                HandleSpriteAndDialogue(MainQuest1State.ApaArtiMimpiItu);
                break;
            case MainQuest1State.PerjodohanDenganLaraswati:
                nameLokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PerjodohanDenganLaraswati)?.locationToEnterTrigger ?? "";
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PerjodohanDenganLaraswati)?.objectiveInfoForUI ?? "";
                QuestManager.Instance.CreateTemplateQuest();


                break;

            case MainQuest1State.PermintaanJhorgeo:
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PermintaanJhorgeo)?.objectiveInfoForUI ?? "";

                positionNpc = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PermintaanJhorgeo)?.npcPositionStartQuest ?? Vector2.zero;
                questtDialogue = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PermintaanJhorgeo)?.dialogueToPlay;

                //UpdateQuest update quest UI 
                npcToCommand = NPCManager.Instance.GetActiveNpcByName(targetNpcName);

                if (npcToCommand != null)
                {
                    npcToCommand.OverrideForQuest(positionNpc, questtDialogue);
                }
                else
                {
                    Debug.LogError($"Gagal memberi perintah: NPC aktif dengan nama '{targetNpcName}' tidak ditemukan di scene!");
                }

                QuestManager.Instance.CreateTemplateQuest();
                break;

            case MainQuest1State.PergiKeHutan:
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PergiKeHutan)?.objectiveInfoForUI ?? "";
                nameLokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PergiKeHutan)?.locationToEnterTrigger ?? "";
                QuestManager.Instance.CreateTemplateQuest();

                // Logika untuk adegan pergi ke hutan
                break;
            case MainQuest1State.CariRusa:
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.CariRusa)?.objectiveInfoForUI ?? "";
                QuestManager.Instance.CreateTemplateQuest();
                HandleSpriteAndDialogue(MainQuest1State.CariRusa);
                SpawnPrefabsForState(MainQuest1State.CariRusa);


                break;
            case MainQuest1State.MunculkanHarimau:
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.MunculkanHarimau)?.objectiveInfoForUI ?? "";
                QuestManager.Instance.CreateTemplateQuest();
                HandleSpriteAndDialogue(MainQuest1State.MunculkanHarimau);
                SpawnPrefabsForState(MainQuest1State.MunculkanHarimau);
                break;
            case MainQuest1State.BerikanHasilBuruan:
                Debug.Log("Memulai adegan Berikan Hasil Buruan...");
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.BerikanHasilBuruan)?.objectiveInfoForUI ?? "";
                QuestManager.Instance.CreateTemplateQuest();
                HandleSpriteAndDialogue(MainQuest1State.BerikanHasilBuruan);
                if (animalBehavior != null)
                {
                    Debug.Log("Mengubah tipe hewan menjadi isQuest untuk harimau.");
                    animalBehavior.ChangeAnimalType(AnimalType.isQuest);
                }
                break;
             case MainQuest1State.CariTempatAman:
                Debug.Log("Memulai adegan Cari Tempat Aman...");
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.CariTempatAman)?.objectiveInfoForUI ?? "";
                nameLokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.CariTempatAman)?.locationToEnterTrigger ?? "";
                QuestManager.Instance.CreateTemplateQuest();
                HandleSpriteAndDialogue(MainQuest1State.CariTempatAman);


                break;
             case MainQuest1State.ApakahIniDanauItu:
                Debug.Log("Memulai adegan Apakah Ini Danau Itu...");
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.ApakahIniDanauItu)?.objectiveInfoForUI ?? "";
                nameLokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.ApakahIniDanauItu)?.locationToEnterTrigger ?? "";
                QuestManager.Instance.CreateTemplateQuest();
                HandleSpriteAndDialogue(MainQuest1State.ApakahIniDanauItu);
                break;
             case MainQuest1State.KabarKesedihan:
                Debug.Log("Memulai adegan Kabar Kesedihan...");
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.KabarKesedihan)?.objectiveInfoForUI ?? "";
                nameLokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.KabarKesedihan)?.locationToEnterTrigger ?? "";
                QuestManager.Instance.CreateTemplateQuest();
                HandleSpriteAndDialogue(MainQuest1State.KabarKesedihan);
                PlayerController.Instance.HandlePlayerIsGreaf();
                break;
             case MainQuest1State.PengingatMainQuest:
                Debug.Log("Memulai adegan Pengingat Main Quest...");
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PengingatMainQuest)?.objectiveInfoForUI ?? "";

                positionNpc = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PengingatMainQuest)?.npcPositionStartQuest ?? Vector2.zero;
                questtDialogue = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PengingatMainQuest)?.dialogueToPlay;

                //UpdateQuest update quest UI 
                npcToCommand = NPCManager.Instance.GetActiveNpcByName(targetNpcName);

                if (npcToCommand != null)
                {
                    npcToCommand.OverrideForQuest(positionNpc, questtDialogue);
                }
                else
                {
                    Debug.LogError($"Gagal memberi perintah: NPC aktif dengan nama '{targetNpcName}' tidak ditemukan di scene!");
                }

                QuestManager.Instance.CreateTemplateQuest();
                break;
        }
        StartCoroutine(FinishStateChange());
    }



    private IEnumerator FinishStateChange()
    {
        // Tunggu satu frame agar semua proses inisialisasi state selesai.
        yield return null;
        isChangingState = false; 
    }

    private void HandleDialogueEnd()
    {
        Debug.Log($"Dialog selesai untuk adegan: {currentState}");
        // Cek dulu apakah rem sedang aktif.
        if (isChangingState)
        {
            Debug.Log("HandleDialogueEnd diabaikan karena sedang dalam proses ganti state.");
            return;
        }
        // Cek adegan mana yang sedang berjalan untuk menentukan aksi
        switch (currentState)
        {
            case MainQuest1State.AdeganMimpi:
                // Setelah dialog mimpi selesai, pindah ke adegan berikutnya
                //ChangeState(MainQuest1State.ApaArtiMimpiItu);
                nameLokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.ApaArtiMimpiItu)?.locationToEnterTrigger ?? "";
                break;
            case MainQuest1State.ApaArtiMimpiItu:
                ChangeState(MainQuest1State.PerjodohanDenganLaraswati);
                break;
            case MainQuest1State.PerjodohanDenganLaraswati:
                ChangeState(MainQuest1State.PermintaanJhorgeo);
                break; 

            case MainQuest1State.PermintaanJhorgeo:

                NPCBehavior npcToCommand = NPCManager.Instance.GetActiveNpcByName(targetNpcName);

                if (npcToCommand != null)
                {
                    npcToCommand.ReturnToPreQuestPosition();
                }
                else
                {
                    Debug.LogError($"Gagal memberi perintah: NPC aktif dengan nama '{targetNpcName}' tidak ditemukan di scene!");
                }


                // Lalu, lanjutkan ke adegan berikutnya
                ChangeState(MainQuest1State.PergiKeHutan);
                break;
            case MainQuest1State.PergiKeHutan:

                ChangeState(MainQuest1State.CariRusa);
                break;
            //case MainQuest1State.CariRusa:
            //    ChangeState(MainQuest1State.MunculkanHarimau);
            //    break;


                // Tambahkan case lain jika diperlukan
        }
    }



    private void HandleAnimalTrigger()
    {
        // Logika untuk mengurangi daftar hewan dan pindah state jika semua sudah kalah
        Debug.Log("HandleAnimalTrigger dipanggil. Mungkin ada hewan yang dipilih.");
        ChangeState(MainQuest1State.CariTempatAman);

    }
    
    private void HandleItemGive(ItemData itemDate)
    {
        // Logika untuk menangani item yang diberikan
        Debug.Log($"Item '{itemDate.itemName}' telah diberikan. Mungkin ada aksi yang perlu dilakukan.");
        // Misalnya, pindah ke adegan berikutnya

    }

    public override void UpdateQuest() { /* Dibiarkan kosong karena berbasis event */ }
    private void HandlePlayerSekarat()
    {
        // --- TAMBAHKAN PENGECEKAN INI ---
        // Hanya bereaksi jika kita sedang dalam adegan melawan harimau.
        if (currentState == MainQuest1State.MunculkanHarimau)
        {
            Debug.Log("Sinyal sekarat diterima SAAT melawan harimau. Mengganti state...");
            ChangeState(MainQuest1State.BerikanHasilBuruan);
        }
        else
        {
            // Abaikan sinyal jika tidak relevan dengan state saat ini.
            Debug.Log("Sinyal sekarat diterima, tetapi tidak dalam state LawanHarimau. Diabaikan.");
        }
    }

    public override void SetInitialState(System.Enum state)
    {
        //    untuk quest ini (MainQuest1State).
        if (state is MainQuest1State loadedState)
        {
            //    ke adegan yang benar tanpa melalui adegan awal.
            ChangeState(loadedState);
        }
        else
        {
            // Beri peringatan jika ada yang aneh, misalnya mencoba memuat state dari Quest 2
            Debug.LogWarning($"Gagal memuat state untuk Main Quest 1. Tipe state tidak cocok: {state.GetType()}");
        }
    }

    public override string GetCurrentObjectiveInfo()
    {
        // Cari data untuk state saat ini
        QuestStateData data = stateDataList.FirstOrDefault(s => s.state == currentState);

        // Kembalikan teks tujuannya jika ada, atau string default jika tidak ada.
        if (data != null && !string.IsNullOrEmpty(data.objectiveInfoForUI))
        {
            return data.objectiveInfoForUI;
        }

        return "Lanjutkan petualanganmu...";
    }

    public void HandleSpriteAndDialogue(MainQuest1State currentState)
    {

        Sprite spriteQuest = stateDataList.FirstOrDefault(s => s.state == currentState)?.spriteToShow;
        Dialogues dialoguesSprite = stateDataList.FirstOrDefault(s => s.state == currentState)?.dialogueToPlay ?? null;
        Vector2 npcPosition = stateDataList.FirstOrDefault(s => s.state == currentState)?.npcPositionStartQuest ?? Vector2.zero;

        bool useDialogue = dialoguesSprite != null;

        if (spriteQuest != null)
        {
            QuestManager.Instance.HandleContentStory(spriteQuest);
        }
        if (useDialogue)
        {
            DialogueSystem.Instance.theDialogues = dialoguesSprite;
            DialogueSystem.Instance.StartDialogue();
        }

    }

    private void HandleLocationTrigger(Transform enteredLocationTransform)
    {

        LocationConfiguration locationConfig = enteredLocationTransform.GetComponent<LocationConfiguration>();
        // Cek apakah nama dari Transform yang dimasuki sama dengan nama yang kita tunggu
        if (locationConfig.locationName == nameLokasiYangDitunggu)
        {
            Debug.Log($"Syarat lokasi terpenuhi! Pemain masuk ke '{enteredLocationTransform.name}'.");

            // Simpan Transform dari area trigger ini untuk digunakan nanti.
            this.lokasiYangDitunggu = enteredLocationTransform;

            // Lanjutkan ke adegan berikutnya
            if (currentState == MainQuest1State.AdeganMimpi)
            {
                ChangeState(MainQuest1State.ApaArtiMimpiItu);
            }
            if (currentState == MainQuest1State.PerjodohanDenganLaraswati)
            {
                HandleSpriteAndDialogue(MainQuest1State.PerjodohanDenganLaraswati);
            }
            if (currentState == MainQuest1State.PergiKeHutan)
            {
                ChangeState(MainQuest1State.CariRusa);
            }
            if (currentState == MainQuest1State.CariTempatAman)
            {
                ChangeState(MainQuest1State.ApakahIniDanauItu);
            }
            if (currentState == MainQuest1State.ApakahIniDanauItu)
            {
                ChangeState(MainQuest1State.KabarKesedihan);
            }
        }
    }
    private void HandleAnimalDied(AnimalBehavior animalThatDied)
    {
        //Cek dulu apakah kita sedang dalam state yang peduli tentang ini (misal: CariRusa)
        if (currentState != MainQuest1State.CariRusa)
        {
            return; // Jika tidak, abaikan saja
        }

        // Cek apakah hewan yang mati adalah salah satu dari hewan quest yang kita lacak
        if (spawnedQuestAnimals.Contains(animalThatDied.gameObject))
        {
            Debug.Log($"Quest: Hewan '{animalThatDied.name}' telah dikalahkan. Mencoret dari daftar.");

            //Hapus hewan tersebut dari daftar kita
            spawnedQuestAnimals.Remove(animalThatDied.gameObject);

            //Cek apakah daftar sekarang sudah kosong
            if (spawnedQuestAnimals.Count == 0)
            {
                Debug.Log("SEMUA HEWAN QUEST TELAH DIKALAHKAN!");
                // Jika ya, pindah ke adegan berikutnya
                ChangeState(MainQuest1State.MunculkanHarimau);
            }
        }
    }

    private void HandleDayChange()
    {
        // Cek apakah kita sedang dalam state yang peduli tentang ini (misal: PergiKeHutan)
        if (currentState != MainQuest1State.KabarKesedihan)
        {
            return; // Jika tidak, abaikan saja
        }

        ChangeState(MainQuest1State.PengingatMainQuest);
        // Logika untuk menangani perubahan hari saat berada di PergiKeHutan
        Debug.Log("Hari telah berubah. Mungkin ada yang perlu dilakukan di PergiKeHutan.");
        // Misalnya, spawn hewan baru atau ubah kondisi lingkungan
    }

    public void SpawnPrefabsForState(MainQuest1State state)
    {
        // Ambil data adegan berdasarkan state
        QuestStateData data = stateDataList.FirstOrDefault(s => s.state == state);
        if (data == null || data.prefabsToSpawn == null || data.prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning($"Tidak ada prefab untuk state {state}");
            return;
        }
        // Hapus prefab yang sudah ada sebelumnya
        foreach (GameObject spawnedAnimal in spawnedQuestAnimals)
        {
            Destroy(spawnedAnimal);
        }
        spawnedQuestAnimals.Clear();
        // Spawn prefab baru
        foreach (GameObject prefab in data.prefabsToSpawn)
        {
            GameObject spawnedObject = Instantiate(prefab, lokasiYangDitunggu);

            AnimalBehavior behavior = spawnedObject.GetComponent<AnimalBehavior>();
            if (behavior.namaHewan == "Harimau")
            {
                this.animalBehavior = behavior; // Simpan referensi ke hewan harimau
            }

            Debug.Log($"Spawned prefab {prefab.name} for state {state} at location {lokasiYangDitunggu.name}");
            spawnedQuestAnimals.Add(spawnedObject);
        }


    }

    public bool TryProcessGivenItem(ItemData givenItemData)
    {
        // Pastikan kita punya status quest yang valid
        if (playerQuestStatus == null || playerQuestStatus.Progress != QuestProgress.Accepted)
        {
            Debug.LogWarning("Tidak ada Main Quest aktif atau sudah selesai untuk memproses item.");
            return false;
        }

        Debug.Log($"Main Quest Controller: Memproses item '{givenItemData.itemName}'...");

        ItemData requiredItem = GetRequiredItem(givenItemData.itemName); // Menggunakan fungsi dari base class
        if (requiredItem == null) return false;

        int neededAmount = GetNeededItemCount(givenItemData.itemName); // Menggunakan fungsi dari base class
        if (neededAmount <= 0) return false;

        int amountToProcess = Mathf.Min(givenItemData.count, neededAmount);

        if (amountToProcess > 0)
        {
            // Update progres item di PlayerMainQuestStatus yang disimpan di base class
            playerQuestStatus.itemProgress[givenItemData.itemName] += amountToProcess;

            Debug.Log($"Progres '{givenItemData.itemName}' untuk '{questName}' diupdate: {playerQuestStatus.itemProgress[givenItemData.itemName]}/{requiredItem.count}");

            if (AreAllItemRequirementsMet()) // Menggunakan fungsi dari base class tanpa parameter
            {
                Debug.Log($"SEMUA ITEM DIBUTUHKAN UNTUK MAIN QUEST '{questName}' TELAH TERPENUHI!");
                ChangeState(MainQuest1State.Selesai);
            }
            return true;
        }
        return false;
    }

}