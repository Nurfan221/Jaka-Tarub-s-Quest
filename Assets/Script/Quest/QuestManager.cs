using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questName;
        
        public Dialogues dialogueQuest;
        public GameObject NPC;
        public ItemQuest[] itemQuests;
        public int reward;
        public int date;
        public string questInfo;
        public string questDetail;
        public Dialogues finish;
        public bool questActive = false;
    }

    [System.Serializable]

    public class ItemQuest
    {
        public Item item;
        public int jumlah;
    }

    public Quest[] dailyQuest;

    [Header("HUBUNGAN")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    public TextMeshProUGUI TextQuest;

    public Dialogues notFinished;




    // buat arrat baru untuk menyimpan quest active 

     


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckQuest()
    {

        foreach(var quest in dailyQuest)
        {
            Debug.Log("Tanggal quest active" + quest.date);
            if ((timeManager.date + 1) == quest.date)
            {
                quest.questActive = true;
            }
        }

        DisplayActiveQuests();
        npcManager.CheckNPCQuest();


        //inputkan nilai dialogue ke dalam quest interactable 
        //AccsessQuestInteractable();
    }

    private void DisplayActiveQuests()
    {
        TextQuest.text = "";

        foreach(var quest in dailyQuest)
        {
            if (quest.questActive == true)
            {
                TextQuest.text += quest.questInfo + "\n";
                 
            }
        }
    }

    public void AccsessQuestInteractable()
    {
        foreach (var quest in dailyQuest)
        {
            if (quest.questActive && quest.NPC != null)
            {
                QuestInteractable interactable = quest.NPC.GetComponent<QuestInteractable>();

                if (interactable != null)
                {
                    interactable.SetCurrentDialogue(quest.dialogueQuest);
                }
            }
        }
    }
}
