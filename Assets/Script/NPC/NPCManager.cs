using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public class NPCData
    {
        public string npcName;
        public string fullName;
        public string pekerjaan;
        public Sprite npcSprite;
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
        public bool hasStarted = false; // Tambahkan penanda
        public bool isOngoing = false; // tanda apakah schedule sudah selesai
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


    private void Update()
    {
        CheckNPCActivities();
    }

    private void CheckNPCActivities()
    {
        foreach (var npcData in npcDataArray)
        {
            GameObject npcInstance = npcData.prefab.gameObject;
            NPCBehavior npcBehavior = npcInstance.GetComponent<NPCBehavior>();

            if (npcBehavior == null)
                continue;

            foreach (var schedule in npcData.schedules)
            {
                // Cek apakah waktu cocok dan aktivitas belum dimulai
                if (Mathf.Approximately(timeManager.hour, schedule.startTime) && !schedule.hasStarted)
                {
                    npcBehavior.StartActivity(schedule); // Mulai aktivitas
                    schedule.isOngoing = true;          // Tandai aktivitas sedang berlangsung
                    schedule.hasStarted = true;         // Tandai aktivitas sudah dimulai
                    break; // Keluar dari loop karena hanya satu aktivitas yang boleh dimulai
                }

                // Jika aktivitas sedang berlangsung, biarkan NPC menyelesaikan aktivitasnya
                if (schedule.isOngoing)
                {
                    // Jika NPC tidak lagi bergerak, tandai aktivitas selesai
                    if (!npcBehavior.IsMoving())
                    {
                        schedule.isOngoing = false; // Tandai aktivitas selesai
                    }
                }
            }
        }
    }






    public void CheckNPCQuest()
    {
        Debug.Log("check npc quest di jalankan");
       foreach (var chapter in questManager.chapters)
        {
            foreach(var quest in chapter.sideQuest)
            {
                if (quest.questActive)
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
                        }
                        else
                        {
                            Debug.Log("nama npc berbeda");
                        }
                    }
                }
            }
        }
    }

    public void CheckNPCMainQuest(GameObject npc, Vector3 locationQuest, Dialogues dialoguesMainQuest)
    {
        foreach (var objekNPC in npcDataArray)
        {
            //string nameNPC = npc.gameObject.name;
            if (objekNPC.prefab == npc)
            {
                GameObject npcPrefab = objekNPC.prefab;

                if (npcPrefab != null)
                {
                    GameObject newNpc = Instantiate(npcPrefab, locationQuest, Quaternion.identity);
                    QuestInteractable questScript = newNpc.GetComponent<QuestInteractable>();
                    NPCBehavior behaviorScript = newNpc.GetComponent<NPCBehavior>();

                    if (behaviorScript != null)
                    {
                        behaviorScript.isMoving = false;
                    }

                    if (questScript != null)
                    {
                        questScript.currentDialogue = dialoguesMainQuest;
                        questScript.npcObject = newNpc;
                    }

                    newNpc.name = objekNPC.npcName + "_Clone";
                    //questManager.UpdateDisplayQuest();
                }
                else
                {
                    Debug.Log($"NPC {npcPrefab.name} berhasil di duplikasi");
                }
                return;
            }
        }
    Debug.LogWarning($"NPC {npc.name} tidak ditemukan dalam npcDataArray");
    }


}
