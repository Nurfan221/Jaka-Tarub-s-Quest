using System.Collections.Generic;
using UnityEngine;

// Baris ini memungkinkan Anda membuat aset quest dari menu Create di Project Window
[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Info Dasar")]
    public string questName;
    [TextArea(3, 10)]
    public string questInfo; // Deskripsi singkat untuk di UI
    [TextArea(3, 10)]
    public string DeskripsiAwal; // Deskripsi awal
    [TextArea(3, 10)]
    public string DeskripsiAkhir; // Deskripsi akhir
    public Vector3 locateNpcQuest; // Lokasi NPC yang memberikan quest, bisa diubah menjadi Vector3 atau string ID
    public QuestType questType; // Tipe quest, bisa MainQuest atau SideQuest
    public QuestProgress questProgress = QuestProgress.Accepted; // Progres quest, bisa NotAccepted, Accepted, atau Completed

    [Header("Kondisi Aktivasi")]
    public int dateToActivate; // Tanggal quest akan menjadi tersedia
    public int MonthToActivate; // Bulan quest akan menjadi tersedia
    // public int requiredChapter; // Bisa ditambahkan jika ada syarat chapter

    [Header("Tujuan & Hadiah")]
    public string npcName; // Ganti referensi GameObject NPC dengan ID (string)
    public List<ItemData> itemRequirements; // Ganti nama dari itemQuests
    public int goldReward;
    public List<ItemData> itemRewards; // Ganti nama dari rewards
    public ItemData NPCItem;

    [Header("Dialog")]
    public Dialogues startDialogue; // Ganti nama dari dialogueQuest
    public Dialogues finishDialogue; // Ganti nama dari finish
    //public Dialogues RewardItem

    [Header("Flags & Events")]
    public bool isMainQuest; // Untuk membedakan main/side quest
    public bool isInGrief;
    public bool isSpawner;
    public bool isNPCItem; // Jika true, itemRequirements adalah item yang dimiliki NPC untuk quest ini
    public string spawnerToActivate;

    [Header("Variabel Clean Quest")]
    public bool isTheCleanupQuest;
    public int cleanupQuestIndex;
    public string objectToClean;
    public bool isTheCleanupObjectDone;
    public int cleanupQuestTotal;
    public EnvironmentType tipeCleanObject;

    //public GameObject nameSpawnerToActive; // Ganti referensi GameObject
}

