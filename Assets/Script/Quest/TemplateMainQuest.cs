using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TemplateMainQuest 
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
    public Dialogues questNotComplate;
    public Dialogues finishDialogue;

    public GameObject questControllerPrefab;

    public TemplateMainQuest() { }

    public TemplateMainQuest(MainQuestSO blueprint)
    {
        this.questName = blueprint.questName;
        this.description = blueprint.description;
        this.dateToActivate = blueprint.dateToActivate;
        this.monthToActivate = blueprint.monthToActivate;
        this.namaNpcQuest = blueprint.namaNpcQuest;
        this.itemRequirements = new List<ItemData>(blueprint.itemRequirements);
        this.goldReward = blueprint.goldReward;
        this.itemRewards = new List<ItemData>(blueprint.itemRewards);
        this.questNotComplate = blueprint.questNotComplate;
        this.finishDialogue = blueprint.finishDialogue;
        this.questControllerPrefab = blueprint.questControllerPrefab;
    }
}
