// Di MainQuestSO.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Tambahkan ini untuk menggunakan .FirstOrDefault()

[CreateAssetMenu(fileName = "New Main Quest", menuName = "Quest System/Main Quest")]
public class MainQuestSO : ScriptableObject
{
    public string questName;
    [TextArea(3, 5)]
    public string description;

    public int dateToActivate;
    public int monthToActivate;
    public string namaNpcQuest;
    public List<ItemData> itemRequirements; // List item yang dibutuhkan quest
    public int goldReward;
    public List<ItemData> itemRewards;

    public GameObject questControllerPrefab;


    
}

