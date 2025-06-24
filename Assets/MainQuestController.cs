using UnityEngine;
using System.Collections.Generic;
using static MainQuest1_Controller;

// Definisikan class data di luar agar bisa diakses dari mana saja jika perlu.
[System.Serializable]
public class Reward
{
    public Item itemReward;
    public int jumlahItemReward;
}

public abstract class MainQuestController : MonoBehaviour
{
    [Header("Data Umum Quest (Wajib Ada di Semua Quest)")]
    public string questName = "Nama Main Quest";
    [TextArea(3, 5)]
    public string questDetail = "Deskripsi singkat tentang quest ini.";
    public Reward[] rewards;
    public List<Item> itemQuests;
  

    public int date;
    public Dialogues finish;
    public Dialogues rewardItemQuest;
    public bool questActive = false;
    public bool questComplete = false;
    public Vector3 locateNpcQuest;
    public int reward;

    // Variabel status yang dikelola secara internal
    protected QuestManager questManager;
    protected bool isQuestComplete = false;

    // Fungsi untuk memulai quest. Dipanggil oleh QuestManager.
    public virtual void StartQuest(QuestManager manager)
    {
        this.questManager = manager;
        Debug.Log($"Memulai Main Quest: {questName}");
        UpdateQuest();
    }

    // Fungsi yang WAJIB dibuat oleh setiap skrip Main Quest spesifik.
    // Berisi semua logika unik dari quest tersebut.
    public abstract void UpdateQuest();
    // Fungsi untuk memberitahu QuestManager jika quest ini sudah selesai.
    public bool IsComplete()
    {
        return isQuestComplete;
    }
}