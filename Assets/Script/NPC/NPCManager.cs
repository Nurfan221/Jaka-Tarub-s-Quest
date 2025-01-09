using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public class NPCData
    {
        public GameObject prefab;                  // Prefab NPC
        public int totalFrendships;                // menghitung jumlah nilai pertemanan
        public int countGift;
        public Schedule[] schedules;              // Jadwal aktivitas NPC
        public Frendship frendship;              // Hubungan persahabatan NPC
    }

    [System.Serializable]
    public class Schedule
    {
        public string activityName;
        public Vector3[] waypoints;
        public float startTime;
    }

    [System.Serializable]
    public class Frendship
    {
        public int favoriteValue;
        public int likesValue;
        public int normalValue;
        public int hateValue;
        public Item[] favorites;
        public Item[] like;
        public Item[] normal;
        public Item[] hate;
    }

    public NPCData[] npcDataArray; // Array untuk menyimpan semua data NPC
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private DialogueSystem dialogueSystem;

    public GameObject wargaDesaParent; // Objek kosong untuk menampung NPC
    private Dictionary<string, GameObject> npcInstances = new Dictionary<string, GameObject>();

    private void Update()
    {
        CheckNPCActivities();
    }

    private void CheckNPCActivities()
    {
        foreach (var npcData in npcDataArray)
        {
            foreach (var schedule in npcData.schedules)
            {
                if (Mathf.Approximately(timeManager.hour, schedule.startTime))
                {
                    if (!npcInstances.ContainsKey(npcData.prefab.name))
                    {
                        GameObject instance = Instantiate(npcData.prefab, schedule.waypoints[0], Quaternion.identity);
                        instance.transform.SetParent(wargaDesaParent.transform);
                        npcInstances.Add(npcData.prefab.name, instance);
                    }

                    GameObject npcInstance = npcInstances[npcData.prefab.name];
                    NPCBehavior npcBehavior = npcInstance.GetComponent<NPCBehavior>();
                    if (npcBehavior != null)
                    {
                        npcBehavior.StartActivity(schedule);
                    }
                    break;
                }
            }
        }
    }

    public void CheckNPCQuest()
    {
        Debug.Log("check npc quest di jalankan");
       foreach (var quest in questManager.dailyQuest)
        {
            if (quest.questActive )
            {
                string nameNpc = quest.NPC.name;
                Debug.Log("nama npc dari struck quest adalah" + nameNpc);
                foreach (var NPC in npcDataArray)
                {
                    if (NPC.prefab.name == nameNpc)
                    {
                        QuestInteractable interactable = NPC.prefab.GetComponent<QuestInteractable>();
                        if (interactable != null)
                        {
                            interactable.currentDialogue = quest.dialogueQuest;
                            interactable.promptMessage = "Quest " + quest.questName;
                        }
                    }else
                    {
                        Debug.Log("nama npc berbeda");
                    }
                }
            }
        }
    }
}
