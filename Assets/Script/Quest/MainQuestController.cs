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
    protected bool isQuestComplete = false;

   
    public virtual void StartQuest(QuestManager manager)
    {
        this.questManager = manager;
        this.isQuestComplete = false;
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