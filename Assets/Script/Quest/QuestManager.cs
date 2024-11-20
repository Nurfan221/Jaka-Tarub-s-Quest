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
        public Item itemQuest;
        public Dialogues dialogueQuest;
        public GameObject nameNPC;
        public int jumlahItem;
        public int reward;
        public int date;
        public string questInfo;
        public string questDetail;
        public Dialogues finish;
        public bool questActive = false;
    }

    public Quest[] dailyQuest;

    [Header("HUBUNGAN")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    public TextMeshProUGUI TextQuest;

    public Dialogues notFinished;
    public List<Quest> activeQuests = new List<Quest>(); 

    public void AddQuest(Quest quest)
    {
        activeQuests.Add(quest);
    }

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
        Debug.Log("CheckQuest active");
        foreach(var quest in dailyQuest)
        {
            if ((timeManager.date + 1) == quest.date)
            {
                quest.questActive = true;
                activeQuests.Add(quest);
            }
        }

        DisplayActiveQuests();
        npcManager.CheckNPCQuest();
    }

    private void DisplayActiveQuests()
    {
        TextQuest.text = "";

        foreach(var quest in activeQuests)
        {
            TextQuest.text += quest.questInfo + "\n";
        }
    }

    
}
