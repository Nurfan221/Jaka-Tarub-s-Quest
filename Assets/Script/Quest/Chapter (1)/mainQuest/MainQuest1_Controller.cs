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
}


// Ini adalah skrip "Sutradara" utama Anda
public class MainQuest1_Controller : MainQuestController  // Pastikan mewarisi dari kelas dasar Anda
{
    public enum MainQuest1State { BelumMulai, AdeganMimpi, PermintaanJhorgeo, PergiKeHutan, CariRusa, MunculkanHarimau, Selesai }

    [Header("Progres Cerita")]
    [SerializeField] private MainQuest1State currentState = MainQuest1State.BelumMulai;
    private bool isChangingState = false; // Flag "rem" kita
    private string objectiveInfoForUI;
    public string lokasiYangDitunggu;

    //KUMPULAN "KARTU ADEGAN"
    [Header("Data untuk Setiap Adegan")]
    public List<QuestStateData> stateDataList;

    // Variabel lain yang spesifik untuk quest ini
    private List<GameObject> spawnedQuestAnimals = new List<GameObject>();
    private string targetNpcName;
    private void OnEnable()
    {
        // Berlangganan ke semua pemicu yang mungkin dibutuhkan quest ini
        //PlayerQuest.OnPlayerEnteredLocation += HandleLocationTrigger;
        AnimalBehavior.OnAnimalDied += HandleAnimalTrigger;
        // Berlangganan ke event akhir dialog
        DialogueSystem.OnDialogueEnded += HandleDialogueEnd;
        PlayerQuest.OnPlayerEnteredLocation += HandleLocationTrigger;
        //DialogueSystem.OnDialogueEnded += HandleDialogueTrigger;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat selesai
        //PlayerQuest.OnPlayerEnteredLocation -= HandleLocationTrigger;
        AnimalBehavior.OnAnimalDied -= HandleAnimalTrigger;
        DialogueSystem.OnDialogueEnded -= HandleDialogueEnd;
        PlayerQuest.OnPlayerEnteredLocation -= HandleLocationTrigger;
        //DialogueSystem.OnDialogueEnded -= HandleDialogueTrigger;
    }

    public override void StartQuest(QuestManager manager, MainQuestSO so)
    {
        base.StartQuest(manager, so); // Panggil implementasi dasar

        // Sekarang Anda bisa mengakses nama NPC dari naskah
        this.targetNpcName = questData.namaNpcQuest;
        Debug.Log($"NPC target untuk quest ini adalah: {this.targetNpcName}");

        // Mulai adegan pertama
        ChangeState(MainQuest1State.AdeganMimpi);
    }

    private void ChangeState(MainQuest1State newState)
    {

        if (isChangingState) return; // Jika sedang sibuk, jangan lakukan apa-apa

        isChangingState = true;
        currentState = newState;
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

            case MainQuest1State.PermintaanJhorgeo:
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PermintaanJhorgeo)?.objectiveInfoForUI ?? "";

                Vector2 positionNpc = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PermintaanJhorgeo)?.npcPositionStartQuest ?? Vector2.zero;
                Dialogues questtDialogue = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PermintaanJhorgeo)?.dialogueToPlay;

                //UpdateQuest update quest UI 
                NPCBehavior npcToCommand = NPCManager.Instance.GetActiveNpcByName(targetNpcName);

                // 2. Pastikan NPC-nya ditemukan.
                if (npcToCommand != null)
                {
                    // 3. Beri perintah pada NPC yang sudah "hidup" di scene tersebut.
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

                lokasiYangDitunggu = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.PergiKeHutan)?.locationToEnterTrigger ?? "";
                QuestManager.Instance.CreateTemplateQuest();

                // Logika untuk adegan pergi ke hutan
                break;
            case MainQuest1State.CariRusa:
                objectiveInfoForUI = stateDataList.FirstOrDefault(s => s.state == MainQuest1State.CariRusa)?.objectiveInfoForUI ?? "";
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


                // Tambahkan case lain jika diperlukan
        }
    }



    private void HandleAnimalTrigger(AnimalBehavior animal)
    {
        // Logika untuk mengurangi daftar hewan dan pindah state jika semua sudah kalah
    }

    public override void UpdateQuest() { /* Dibiarkan kosong karena berbasis event */ }

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

    private void HandleLocationTrigger(string locationName)
    {
        // Jika tidak ada quest aktif atau tidak ada lokasi yang ditunggu, abaikan saja.
        if (currentState == MainQuest1State.BelumMulai || string.IsNullOrEmpty(lokasiYangDitunggu))
        {
            return;
        }

        // Cek apakah nama lokasi yang dimasuki pemain sama dengan yang ditunggu oleh state saat ini.
        if (locationName == lokasiYangDitunggu)
        {
            Debug.Log($"Syarat lokasi terpenuhi! Pemain masuk ke '{locationName}'.");

            // Contoh: Jika sedang dalam state PergiKeHutan, pindah ke CariRusa
            if (currentState == MainQuest1State.PergiKeHutan)
            {
                HandleSpriteAndDialogue(MainQuest1State.PergiKeHutan);

            }

        }
    }

}