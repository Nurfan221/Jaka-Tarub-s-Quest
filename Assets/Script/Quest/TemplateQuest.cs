using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TemplateQuest
{
    [Header("Info Dasar")]
    public string questName;
    [TextArea(3, 10)]
    public string questInfo; // Deskripsi singkat untuk di UI
    [TextArea(3, 10)]
    public string DeskripsiAwal; // Deskripsi awal
    [TextArea(3, 10)]
    public string DeskripsiAkhir; // Deskripsi akhir
    public Vector2 startLocateNpcQuest;
    public Vector2 finishLocateNpcQuest;
    public QuestType questType;
    public QuestProgress questProgress = QuestProgress.Accepted;
    public EmoticonTemplate questEmoticon;

    [Header("Kondisi Aktivasi")]
    public int dateToActivate;
    public int MonthToActivate;

    [Header("Tujuan & Hadiah")]
    public string npcName;
    public List<ItemData> itemRequirements;
    public int goldReward;
    public List<ItemData> itemRewards;
    public ItemData NPCItem;

    [Header("Dialog")]
    public Dialogues startDialogue;
    public Dialogues finishDialogue;

    [Header("Flags & Events")]
    public bool isMainQuest;
    public bool isInGrief;
    public bool isSpawner;
    public bool isNPCItem;
    public string spawnerToActivate;

    [Header("Variabel Clean Quest")]
    public bool isTheCleanupQuest;
    public int cleanupQuestIndex;
    public string objectToClean;
    public bool isTheCleanupObjectDone;
    public int cleanupQuestTotal;
    public EnvironmentType tipeCleanObject;

    // Constructor default (kosong), baik untuk dimiliki
    public TemplateQuest() { }

    public TemplateQuest(QuestSO blueprint)
    {
        this.questName = blueprint.questName;
        this.questInfo = blueprint.questInfo;
        this.DeskripsiAwal = blueprint.DeskripsiAwal;
        this.DeskripsiAkhir = blueprint.DeskripsiAkhir;
        this.startLocateNpcQuest = blueprint.startLocateNpcQuest;
        this.finishLocateNpcQuest = blueprint.finishLocateNpcQuest;
        this.questType = blueprint.questType;
        this.questProgress = blueprint.questProgress;
        this.questEmoticon = blueprint.questEmoticon; // Asumsi EmoticonTemplate adalah ScriptableObject/aset

        this.dateToActivate = blueprint.dateToActivate;
        this.MonthToActivate = blueprint.MonthToActivate;

        this.npcName = blueprint.npcName;
        // PENTING: Buat salinan list baru agar tidak merujuk ke list yang sama
        this.itemRequirements = new List<ItemData>(blueprint.itemRequirements);
        this.goldReward = blueprint.goldReward;
        // Jika ItemData adalah class, ini membuat salinan dangkal (shallow copy).
        // Jika ItemData adalah struct, ini membuat salinan penuh.
        // Untuk sistem quest, salinan dangkal biasanya sudah cukup.

        this.startDialogue = blueprint.startDialogue; // Asumsi Dialogues adalah ScriptableObject/aset
        this.finishDialogue = blueprint.finishDialogue; // Asumsi Dialogues adalah ScriptableObject/aset

        this.isMainQuest = blueprint.isMainQuest;
        this.isInGrief = blueprint.isInGrief;
        this.isSpawner = blueprint.isSpawner;
        this.isNPCItem = blueprint.isNPCItem;
        this.spawnerToActivate = blueprint.spawnerToActivate;

        this.isTheCleanupQuest = blueprint.isTheCleanupQuest;
        this.cleanupQuestIndex = blueprint.cleanupQuestIndex;
        this.objectToClean = blueprint.objectToClean;
        this.isTheCleanupObjectDone = blueprint.isTheCleanupObjectDone;
        this.cleanupQuestTotal = blueprint.cleanupQuestTotal;
        this.tipeCleanObject = blueprint.tipeCleanObject;

        // Buat "Salinan Dalam" (Deep Copy) untuk Lists
        this.itemRequirements = new List<ItemData>();
        foreach (var item in blueprint.itemRequirements)
        {
            // Buat ItemData BARU berdasarkan data dari blueprint
            this.itemRequirements.Add(new ItemData(item));
        }

        this.itemRewards = new List<ItemData>();
        foreach (var item in blueprint.itemRewards)
        {
            this.itemRewards.Add(new ItemData(item));
        }

        if (blueprint.NPCItem != null)
        {
            this.NPCItem = new ItemData(blueprint.NPCItem);
        }

        // Inisialisasi data runtime
        this.questProgress = QuestProgress.Accepted;
        // this.itemProgress = new Dictionary<string, int>(); // Jika Anda pakai dictionary
    }
}
