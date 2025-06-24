using System.Collections.Generic;
using UnityEngine;
using static QuestManager;

public enum MainQuest2State
{
    None,
    Play,
    MenemukanDanau,
    PergiKeLokasiQuest,
    CariRusa,
    BunuhRusa,
    MunculBandit,
    Sekarat,
    SceneDanauIndah,
    Pulang,
    KabarKesedihan,
    MisiYangBelumSelesai,
    PermintaanMamat,
    Selesai,
}
public class MainQuest2 : MonoBehaviour
{
    [System.Serializable]
    public class MainQuest
    {
        public string questName;
        public MainQuest2State currentQuestState = MainQuest2State.None;

        //inputkan dialogue sesuai jalan cerita dari awal hingga akhir
        public Dialogues[] dialogueQuest;
        public Dialogues dialoguePengingat;
        public GameObject NPC;
        public int date;
        public string questDetail;
        public Dialogues finish;
        public Dialogues rewardItemQuest;
        public bool questActive = false;
        public bool questComplete = false;
        public Vector3 locateNpcQuest;

        //tentukan sprite sesuai dengan jalan cerita mulai dari awal sampai akhir
        public Sprite[] spriteQuest;
        public int reward;
        public Reward[] rewards;
        public List<Item> itemQuests;
        public int indexLocation;
        //public locationMainQuest[] locationMainQuest;


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMainQuest2()
    {

    }
}
