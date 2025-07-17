// Di bagian bawah file QuestManager.cs atau di file terpisah

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestSaveData
{
    public string questName;
    public QuestProgress progress;
    public List<string> itemProgressKeys;
    public List<int> itemProgressValues;

    public QuestSaveData(PlayerQuestStatus status)
    {
        this.questName = status.Quest.questName;
        this.progress = status.Progress;
        this.itemProgressKeys = new List<string>();
        this.itemProgressValues = new List<int>();
        foreach (var pair in status.itemProgress)
        {
            this.itemProgressKeys.Add(pair.Key);
            this.itemProgressValues.Add(pair.Value);
        }
    }
}

// Definisikan kelas Serialization<T> di sini, di luar kelas lain
[System.Serializable]
public class Serialization<T>
{
    [SerializeField]
    List<T> items;
    public List<T> ToList() { return items; }
    public Serialization(List<T> items) { this.items = items; }
}

// Tambahkan class ini di file yang sama dengan QuestSaveData
[System.Serializable]
public class MainQuestSaveData
{
    public string questName; // Untuk tahu quest mana yang aktif
    public string currentState; // Menyimpan nama state enum saat ini (misal, "PergiKeHutan")
}