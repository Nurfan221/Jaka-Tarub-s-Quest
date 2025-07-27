using System.Collections.Generic;
using UnityEngine; // Perlu untuk [System.Serializable]

[System.Serializable]
public class PlayerQuestStatus
{
    public QuestSO Quest { get; private set; } // Referensi ke definisi quest (bisa QuestSO, SideQuestSO, MiniQuestSO)
    public QuestProgress Progress { get; set; }

    // Dictionary untuk melacak progres item: <"NamaItem", JumlahTerkumpul>
    public Dictionary<string, int> itemProgress;

    // Konstruktor untuk inisialisasi status quest non-Main Quest
    public PlayerQuestStatus(QuestSO quest)
    {
        this.Quest = quest;
        this.Progress = QuestProgress.Accepted; // Default saat diterima
        this.itemProgress = new Dictionary<string, int>();

        // Inisialisasi semua item yang dibutuhkan dengan progres 0
        if (quest.itemRequirements != null) // Penting: cek null
        {
            foreach (var item in quest.itemRequirements)
            {
                // Pastikan item.itemName tidak null/kosong sebelum menambahkan ke dictionary
                if (!string.IsNullOrEmpty(item.itemName))
                {
                    itemProgress[item.itemName] = 0;
                }
            }
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
    // MainQuest (opsional, jika Anda ingin QuestSO punya enum Type juga)
}