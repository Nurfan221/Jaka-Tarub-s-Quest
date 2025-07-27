using System.Collections.Generic;
using UnityEngine; // Perlu untuk [System.Serializable]

[System.Serializable]
public class PlayerMainQuestStatus
{
    public MainQuestSO MainQuestDefinition { get; private set; } // Referensi ke definisi Main Quest
    public QuestProgress Progress { get; set; } // Progres keseluruhan Main Quest

    // Dictionary untuk melacak progres item spesifik untuk Main Quest
    public Dictionary<string, int> itemProgress;
    public string CurrentStateName { get; set; } // Simpan sebagai string untuk fleksibilitas

    // Konstruktor untuk inisialisasi status Main Quest
    public PlayerMainQuestStatus(MainQuestSO mainQuestSO)
    {
        this.MainQuestDefinition = mainQuestSO;
        this.Progress = QuestProgress.Accepted;
        this.itemProgress = new Dictionary<string, int>();
        this.CurrentStateName = ""; // Inisialisasi kosong

        if (mainQuestSO.itemRequirements != null)
        {
            foreach (var item in mainQuestSO.itemRequirements)
            {
                if (!string.IsNullOrEmpty(item.itemName))
                {
                    itemProgress[item.itemName] = 0;
                }
            }
        }
    }
}