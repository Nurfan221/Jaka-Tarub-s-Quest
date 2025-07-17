using System.Collections.Generic;

[System.Serializable]
public class PlayerQuestStatus
{
    public QuestSO Quest { get; private set; } // Referensi ke definisi quest
    public QuestProgress Progress { get; set; }

    // Dictionary untuk melacak progres item, misal: <"Kayu", 5> artinya sudah kumpul 5 kayu
    public Dictionary<string, int> itemProgress;

    public PlayerQuestStatus(QuestSO quest)
    {
        this.Quest = quest;
        this.Progress = QuestProgress.Accepted;
        this.itemProgress = new Dictionary<string, int>();

        // Inisialisasi semua item yang dibutuhkan dengan progres 0
        foreach (var item in quest.itemRequirements)
        {
            itemProgress[item.itemName] = 0;
        }
    }
}

public enum QuestProgress
{
    NotAccepted,
    Accepted,
    Completed
}

public enum QuestType
{

    SideQuest,
    MiniQuest,
}   