// File: QuestSO.cs (versi baru)
using System.Collections.Generic;
using UnityEngine;

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
}