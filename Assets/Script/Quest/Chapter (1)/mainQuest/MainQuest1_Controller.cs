using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    public string objectiveInfoForUI;

    [Header("Syarat Pindah ke Adegan Berikutnya")]
    public Vector2 npcPositionStartQuest;
    public string locationToEnterTrigger;
    public string animalToDefeatTrigger;
}

// Ini adalah skrip "Sutradara" utama Anda
public class MainQuest1_Controller : MainQuestController  // Pastikan mewarisi dari kelas dasar Anda
{
    public enum MainQuest1State { BelumMulai, AdeganMimpi, PermintaanJhorgeo, PergiKeHutan, CariRusa, MunculkanHarimau, Selesai }

    [Header("Progres Cerita")]
    [SerializeField] private MainQuest1State currentState = MainQuest1State.BelumMulai;

    // 2. KUMPULAN "KARTU ADEGAN"
    [Header("Data untuk Setiap Adegan")]
    public List<QuestStateData> stateDataList;

    // Variabel lain yang spesifik untuk quest ini
    private List<GameObject> spawnedQuestAnimals = new List<GameObject>();

    private void OnEnable()
    {
        // Berlangganan ke semua pemicu yang mungkin dibutuhkan quest ini
        PlayerQuest.OnPlayerEnteredLocation += HandleLocationTrigger;
        AnimalBehavior.OnAnimalDied += HandleAnimalTrigger;
        //DialogueSystem.OnDialogueEnded += HandleDialogueTrigger;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat selesai
        PlayerQuest.OnPlayerEnteredLocation -= HandleLocationTrigger;
        AnimalBehavior.OnAnimalDied -= HandleAnimalTrigger;
        //DialogueSystem.OnDialogueEnded -= HandleDialogueTrigger;
    }

    public override void StartQuest(QuestManager manager)
    {
        base.StartQuest(manager);
        ChangeState(MainQuest1State.AdeganMimpi); // Mulai adegan pertama
    }

    private void ChangeState(MainQuest1State newState)
    {
        currentState = newState;
        //Debug.Log($"Main Quest '{questName}' masuk ke adegan: {currentState}");

        QuestStateData data = stateDataList.FirstOrDefault(s => s.state == newState);
        if (data == null) return;

        // Jalankan semua AKSI untuk adegan ini
        //if (data.dialogueToPlay != null) DialogueSystem.Instance.StartDialogue(data.dialogueToPlay);
        //if (data.spriteToShow != null) UIManager.Instance.ShowQuestImage(data.spriteToShow);
        //if (!string.IsNullOrEmpty(data.objectiveInfoForUI)) UIManager.Instance.UpdateQuestObjective(data.objectiveInfoForUI);
    }

    private void HandleDialogueTrigger()
    {
        if (currentState == MainQuest1State.AdeganMimpi)
        {
            ChangeState(MainQuest1State.PergiKeHutan);
        }
    }

    private void HandleLocationTrigger(string locationName)
    {
        QuestStateData data = stateDataList.FirstOrDefault(s => s.state == currentState);
        if (data != null && locationName == data.locationToEnterTrigger)
        {
            // Contoh: jika sedang di state PergiKeHutan dan masuk ke lokasi yang benar
            ChangeState(MainQuest1State.CariRusa);
        }
    }

    private void HandleAnimalTrigger(AnimalBehavior animal)
    {
        // Logika untuk mengurangi daftar hewan dan pindah state jika semua sudah kalah
    }

    public override void UpdateQuest() { /* Dibiarkan kosong karena berbasis event */ }

    public override void SetInitialState(System.Enum state)
    {
        // 1. Lakukan pengecekan tipe untuk memastikan state yang dimuat cocok
        //    untuk quest ini (MainQuest1State).
        if (state is MainQuest1State loadedState)
        {
            // 2. Jika tipe cocok, langsung panggil ChangeState untuk "melompat"
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
}