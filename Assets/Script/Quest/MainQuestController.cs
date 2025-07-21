using UnityEngine;

public abstract class MainQuestController : MonoBehaviour
{
    [Header("Data Umum Quest")]
    public string questName = "Nama Main Quest";
    [TextArea(3, 5)]
    public string questDescription = "Deskripsi singkat quest.";

    // Anda bisa mendefinisikan hadiah di sini jika semua main quest punya struktur hadiah yang sama.
    // public Reward[] rewards;

    // 'protected' berarti variabel ini hanya bisa diakses oleh kelas ini dan kelas turunannya.
    protected QuestManager questManager;
    protected MainQuestSO questData;
    protected bool isQuestComplete = false;

   
    public virtual void StartQuest(QuestManager manager, MainQuestSO so)
    {
        this.questManager = manager;
        this.questData = so; // Simpan referensi ke naskahnya
        this.isQuestComplete = false;
        // Ambil nama dari SO agar konsisten
        this.questName = so.questName;
        Debug.Log($"Memulai Main Quest: {questName}");
    }

  
    public abstract void UpdateQuest();

  
    public abstract void SetInitialState(System.Enum state);

  
    public bool IsComplete()
    {
        return isQuestComplete;
    }

    public abstract string GetCurrentObjectiveInfo();
}