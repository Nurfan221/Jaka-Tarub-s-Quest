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
    private List<NPCBehavior> activeNpcs = new List<NPCBehavior>();

    // 'Dictionary' adalah cara super efisien untuk menyimpan dan mencari data relasi
    public Dictionary<string, NpcRelationshipData> playerNpcRelationships = new Dictionary<string, NpcRelationshipData>();


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else { Instance = this; DontDestroyOnLoad(this.gameObject); }
    }

    private void Start()
    {
        SpawnAllNpcs();
    }

    // Fungsi ini akan membuat semua NPC dari database saat game dimulai
    private void SpawnAllNpcs()
    {
        foreach (var data in allNpcDefinitions)
        {
            if (data.prefab != null)
            {
                GameObject npcGO = Instantiate(data.prefab, wargaDesaParent);
                NPCBehavior behavior = npcGO.GetComponent<NPCBehavior>();
                if (behavior != null)
                {
                    // Berikan "identitas" pada NPC yang baru dibuat
                    behavior.Initialize(data);
                    activeNpcs.Add(behavior);
                }
            }
        }
    }

    // Mencari NPC yang sedang aktif di scene berdasarkan nama.
    public NPCBehavior GetActiveNpcByName(string npcName)
    {
        return activeNpcs.FirstOrDefault(npcBehavior =>
            npcBehavior.npcName.Equals(npcName, System.StringComparison.OrdinalIgnoreCase)
        );
    }

    public GameObject GetNpcPrefabByName(string npcFullName)
    {
        NpcSO foundNpcData = allNpcDefinitions.FirstOrDefault(npc =>
            npc.fullName.Equals(npcFullName, System.StringComparison.OrdinalIgnoreCase)
        );

        // Periksa apakah data NPC ditemukan
        if (foundNpcData != null)
        {
            // Jika ditemukan, kembalikan prefab yang tersimpan di dalamnya.
            Debug.Log($"Prefab untuk '{npcFullName}' ditemukan.");
            return foundNpcData.prefab;
        }
        else
        {
            // Jika tidak ditemukan, beri peringatan dan kembalikan null.
            Debug.LogWarning($"Data NPC dengan nama '{npcFullName}' tidak ditemukan di npcDataArray.");
            return null;
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