using System.Collections.Generic;

[System.Serializable]
public class QuestManagerSaveData
{
    public int currentChapterQuestIndex;
    public List<QuestSaveData> sideQuests; // Ini adalah saveDataList Anda
    // Tambahkan data main quest di sini juga
}