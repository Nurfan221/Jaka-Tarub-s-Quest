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

    [Header("Runtime Progress")]
    public string CurrentStateName; // Ini akan menyimpan "AdeganMimpi", "PergiKeHutan", dll.
    public TemplateMainQuest() { }

    public TemplateMainQuest(MainQuestSO blueprint)
    {
        this.questName = blueprint.questName;
        this.description = blueprint.description;
        this.dateToActivate = blueprint.dateToActivate;
        this.monthToActivate = blueprint.monthToActivate;
        this.namaNpcQuest = blueprint.namaNpcQuest;
        this.goldReward = blueprint.goldReward;
        this.questNotComplate = blueprint.questNotComplate;
        this.finishDialogue = blueprint.finishDialogue;


        this.itemRequirements = new List<ItemData>();
        // Loop list blueprint dan buat SALINAN BARU dari setiap ItemData
        foreach (var item in blueprint.itemRequirements)
        {
            // harus menyalin field satu per satu.
            this.itemRequirements.Add(new ItemData(item));
        }

        this.itemRewards = new List<ItemData>();
        foreach (var item in blueprint.itemRewards)
        {
            this.itemRewards.Add(new ItemData(item));
        }
    }
}
