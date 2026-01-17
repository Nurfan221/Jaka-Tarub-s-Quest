using System.Collections.Generic;
using System.Linq;
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
            GameObject npcObject = DatabaseManager.Instance.GetNPCPrefab(data.isChild);
            GameObject npcGO = Instantiate(npcObject, wargaDesaParent);
            NPCBehavior behavior = npcGO.GetComponent<NPCBehavior>();
            NPCInteractable interactable = npcGO.GetComponent<NPCInteractable>();
            GameObject spriteObject = npcGO.transform.Find("Sprite").gameObject;
            FootstepController footstep = spriteObject.GetComponent<FootstepController>();

            if (footstep != null)
            {
                footstep.tilemaps = PlayerUI.Instance.tilemapLayerPlayer;
            }

            // Inisialisasi data NPC


            if (behavior != null)
            {
                // Berikan "identitas" pada NPC yang baru dibuat
                behavior.AutoFindAnimators();

                behavior.Initialize(data);
                behavior.SetAnimators(
                    data.bajuAnimator,
                    data.celanaAnimator,
                    data.rambutAnimator,
                    data.sepatuAnimator
                );
                activeNpcs.Add(behavior);
            }

            if (interactable != null)
            {
                interactable.promptMessage = data.npcName;
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
        Debug.Log($"[1] Mencoba menambahkan dialog untuk NPC: {npcName}");

        NPCBehavior npc = GetActiveNpcByName(npcName);

        // Cek satu per satu untuk debug yang jelas
        if (npc == null)
        {
            Debug.LogError($"[GAGAL] NPC '{npcName}' tidak ditemukan di Scene/List Active NPC.");
            return;
        }

        if (newDialogue == null)
        {
            Debug.LogError($"[GAGAL] Dialogue untuk NPC '{npcName}' bernilai NULL.");
            return;
        }

        // Jika sampai sini, berarti NPC dan Dialogue AMAN
        try
        {
            npc.questOverrideDialogue = newDialogue;
            npc.isLockedForQuest = true;

            // Bungkus ini dengan pengecekan
            npc.ShowEmoticon("Peringatan");

            Debug.Log($"[SUKSES] NPC {npcName} dikunci untuk quest.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CRASH] Terjadi error saat setup NPC {npcName}: {e.Message}\n{e.StackTrace}");
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