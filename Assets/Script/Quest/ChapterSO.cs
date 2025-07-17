using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chapter", menuName = "Quest System/Chapter")]
public class ChapterSO : ScriptableObject
{
    public int chapterID;
    public string chapterName;
    public List<QuestSO> sideQuests; // Sekarang berisi list dari ASET QuestSO
    // public List<QuestSO> mainQuests; // Jika Anda ingin memisahkan main quest
}