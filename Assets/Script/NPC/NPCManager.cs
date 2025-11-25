using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("Daftar hubungan script")]
    public Transform wargaDesaParent; // Parent untuk semua NPC yang akan di-spawn

    [Header("Database Definisi NPC")]
    public List<NpcSO> allNpcDefinitions; // Seret semua aset NpcSO Anda ke sini
    public List<NPCBehavior> activeNpcs = new List<NPCBehavior>();

    // 'Dictionary' adalah cara super efisien untuk menyimpan dan mencari data relasi
    public Dictionary<string, NpcRelationshipData> playerNpcRelationships = new Dictionary<string, NpcRelationshipData>();

    private void OnEnable()
    {
        TimeManager.OnHourChanged += HandleNewDay;
    }
    private void OnDisable()
    {
        TimeManager.OnHourChanged -= HandleNewDay;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else { Instance = this; DontDestroyOnLoad(this.gameObject); }
        SpawnAllNpcs();

    }


    public void HandleNewDay()
    {
        int currentHour = TimeManager.Instance.hour;
        CheckNPCWakingSchedule(currentHour);
    }
    // Fungsi ini akan membuat semua NPC dari database saat game dimulai
    private void SpawnAllNpcs()
    {
        foreach (var data in allNpcDefinitions)
        {
            GameObject npcObject = DatabaseManager.Instance.NPCWorldPrefab;
            GameObject npcGO = Instantiate(npcObject, wargaDesaParent);
            NPCBehavior behavior = npcGO.GetComponent<NPCBehavior>();
            if (behavior != null)
            {
                // Berikan "identitas" pada NPC yang baru dibuat
                behavior.Initialize(data);
                activeNpcs.Add(behavior);
            }
            int currentHour = TimeManager.Instance.hour;
            int wakeUpTime = behavior.GetStartingHour();
            if (currentHour >= wakeUpTime)
            {
                npcGO.SetActive(true);
            }
            else
            {
                npcGO.SetActive(false);

            }
        }
    }


    // Panggil fungsi ini saat pergantian hari atau jam 6 pagi
    public void CheckNPCWakingSchedule(int currentHour)
    {

        foreach (NPCBehavior npc in activeNpcs)
        {
            // Kita butuh fungsi untuk mencari jadwal mana yang aktif di jam ini
            Schedule activeSchedule = npc.GetScheduleForHour(currentHour);

            if (activeSchedule != null)
            {
                // bangunkan npc jika sudah waktunya menjalankan aktiftasnya
                if (!npc.gameObject.activeSelf && !activeSchedule.hideOnArrival)
                {
                    Debug.Log($"Waktunya {npc.npcName} keluar rumah untuk: {activeSchedule.activityName}");

                    // Pindahkan ke posisi awal jadwal tersebut
                    if (activeSchedule.waypoints.Length > 0)
                    {
                        npc.transform.position = activeSchedule.waypoints[0];
                    }

                    npc.gameObject.SetActive(true);

                    // Paksa NPC membaca jadwal dan bergerak
                    npc.ReturnToNormalSchedule();
                }
            }
        }
    }

    private void WakeUpSpecificNPC(NPCBehavior npc)
    {
        Debug.Log($"Waktunya {npc.npcName} bangun/keluar rumah!");

        // Pindahkan posisi ke titik awal jadwal pertama
        if (npc.npcData.schedules.Length > 0)
        {
            npc.transform.position = npc.npcData.schedules[0].waypoints[0];
        }

        // Nyalakan NPC
        npc.gameObject.SetActive(true);

        // Reset logic agar dia mulai jalan
        npc.ReturnToNormalSchedule();
    }

    // Mencari NPC yang sedang aktif di scene berdasarkan nama.
    public NPCBehavior GetActiveNpcByName(string npcName)
    {
        return activeNpcs.FirstOrDefault(npcBehavior =>
            npcBehavior.npcName.Equals(npcName, System.StringComparison.OrdinalIgnoreCase)
        );
    }

    //public GameObject GetNpcPrefabByName(string npcFullName)
    //{
    //    NpcSO foundNpcData = allNpcDefinitions.FirstOrDefault(npc =>
    //        npc.fullName.Equals(npcFullName, System.StringComparison.OrdinalIgnoreCase)
    //    );

    //    // Periksa apakah data NPC ditemukan
    //    if (foundNpcData != null)
    //    {
    //        // Jika ditemukan, kembalikan prefab yang tersimpan di dalamnya.
    //        Debug.Log($"Prefab untuk '{npcFullName}' ditemukan.");
    //        return foundNpcData.prefab;
    //    }
    //    else
    //    {
    //        // Jika tidak ditemukan, beri peringatan dan kembalikan null.
    //        Debug.LogWarning($"Data NPC dengan nama '{npcFullName}' tidak ditemukan di npcDataArray.");
    //        return null;
    //    }
    //}


    public void UpdateScheduleNPC()
    {
        // Perbarui jadwal semua NPC aktif
        foreach (var npc in activeNpcs)
        {
            if (npc != null && !npc.isLockedForQuest)
            {

            }
        }
    }

    public void AddDialogueForNPCQuest(string npcName, Dialogues newDialogue)
    {
        Debug.Log($"Mencoba menambahkan dialog untuk NPC: {npcName}");
        NPCBehavior npc = GetActiveNpcByName(npcName);
        if (npc != null && newDialogue!= null)
        {
            npc.questOverrideDialogue = newDialogue;
            npc.isLockedForQuest = true; // Kunci NPC untuk quest
            npc.ShowEmoticon("Peringatan");
            Debug.Log($"NPC {npcName} dikunci untuk quest dengan dialog baru.");
        }
        else
        {
            Debug.LogWarning($"NPC dengan nama {npcName} tidak ditemukan.");
        }
    }


}

[System.Serializable]
public class NpcRelationshipData
{
    public string npcName; // Kunci untuk menghubungkan ke NpcSO
    public int friendshipPoints;
    public int giftsGivenThisWeek;
    public bool hasBeenTalkedToToday;

    public NpcRelationshipData(string name)
    {
        this.npcName = name;
        this.friendshipPoints = 0;
        this.giftsGivenThisWeek = 0;
        this.hasBeenTalkedToToday = false;
    }
}