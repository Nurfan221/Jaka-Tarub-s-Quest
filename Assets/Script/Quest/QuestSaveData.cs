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
    // Mengidentifikasi Main Quest SO mana yang sedang aktif
    public string questNameID; // Menggunakan nama SO sebagai ID unik (misalnya "MainQuestJakaTarub")

    // Progres keseluruhan Main Quest (misal Accepted, Completed)
    public QuestProgress progress;

    // State spesifik MainQuestController (misal "BerikanHasilBuruan")
    public string currentStateName;

    // Progres item, disimpan sebagai dua list terpisah karena Dictionary tidak langsung Serializable
    public List<string> itemProgressKeys;
    public List<int> itemProgressValues;

    // Konstruktor untuk membuat MainQuestSaveData dari PlayerMainQuestStatus
    public MainQuestSaveData(PlayerMainQuestStatus status)
    {
        // Memastikan MainQuestDefinition tidak null sebelum mengakses namanya
        this.questNameID = status.MainQuestDefinition != null ? status.MainQuestDefinition.name : ""; // Mengambil nama SO
        this.progress = status.Progress;
        this.currentStateName = status.CurrentStateName; // Mengambil state yang sudah kita tambahkan

        // Mengonversi Dictionary itemProgress ke dua List terpisah
        this.itemProgressKeys = new List<string>();
        this.itemProgressValues = new List<int>();
        if (status.itemProgress != null) // Cek null untuk itemProgress
        {
            foreach (var pair in status.itemProgress)
            {
                this.itemProgressKeys.Add(pair.Key);
                this.itemProgressValues.Add(pair.Value);
            }
        }
    }

    // Metode pembantu untuk mengonversi kembali ke Dictionary (opsional, bisa juga dilakukan di Loader)
    public Dictionary<string, int> GetItemProgressDictionary()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        if (itemProgressKeys != null && itemProgressValues != null && itemProgressKeys.Count == itemProgressValues.Count)
        {
            for (int i = 0; i < itemProgressKeys.Count; i++)
            {
                dict[itemProgressKeys[i]] = itemProgressValues[i];
            }
        }
        return dict;
    }
}