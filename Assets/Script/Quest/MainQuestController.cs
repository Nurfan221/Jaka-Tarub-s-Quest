using UnityEngine;
using System.Collections.Generic;

// Kita letakkan class Reward di sini atau di file terpisah agar rapi.
[System.Serializable]
public class Reward
{
    public Item itemReward;
    public int jumlahItemReward;
}

public abstract class MainQuestController : MonoBehaviour
{
    [Header("Data Umum Quest")]
    public string questName = "Nama Main Quest";
    [TextArea(3, 5)]
    public string questDescription = "Deskripsi singkat quest.";
    public Reward[] rewards;

    // Variabel status yang hanya bisa diakses oleh kelas ini dan turunannya.
    protected QuestManager questManager;
    public bool questActive;
    protected bool isQuestComplete = false;

    // FUNGSI UTAMA (KONTRAK)
    public virtual void StartQuest(QuestManager manager)
    {
        this.questManager = manager;
        Debug.Log($"Memulai Main Quest: {questName}");
    }

    // Fungsi yang WAJIB dibuat oleh setiap kelas turunan.

    public abstract void UpdateQuest();


    // Memberitahu QuestManager apakah quest sudah selesai.

    public bool IsComplete()
    {
        return isQuestComplete;
    }
}