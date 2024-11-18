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
        public bool questActive = false;
    }

    public Quest[] dailyQuest;

    [Header("HUBUNGAN")]
    [SerializeField] TimeManager timeManager;
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] NPCManager npcManager;
    public TextMeshProUGUI TextQuest;


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
                TextQuest.text = quest.questInfo;
            }
        }

        npcManager.CheckNPCQuest();
    }

    
}
