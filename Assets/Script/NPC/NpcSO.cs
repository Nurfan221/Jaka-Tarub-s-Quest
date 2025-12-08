using UnityEngine;

// Definisikan Schedule dan Frendship di sini agar bisa digunakan oleh NpcSO
[System.Serializable]
public class Schedule
{
    public string activityName;
    public Vector2[] waypoints;
    public int startTime;
    [Tooltip("Jika dicentang, NPC akan menghilang (masuk rumah) saat sampai di tujuan.")]
    public bool hideOnArrival; //
    // Hapus 'hasStarted' dan 'isOngoing' dari sini, karena itu adalah data runtime
}

[System.Serializable]
public class FrendshipDefinition
{
    public int favoriteValue;
    public int likesValue;
    public int normalValue;
    public int hateValue;
    public Item[] favorites; // Sebaiknya diganti menjadi List<ItemData> atau List<ItemSO>
    public Item[] like;
    public Item[] normal;
    public Item[] hate;
}

[CreateAssetMenu(fileName = "New NPC", menuName = "NPC/NPC Definition")]
public class NpcSO : ScriptableObject
{
    [Header("Identitas NPC (Data Statis)")]
    public string npcName; // ID unik, misal: "Budi"
    public string fullName;
    public string pekerjaan;
    public string hobi;
    public int tanggalUltah;
    public int bulanUltah;
    public Sprite npcSprite;
    public bool isChild;
    //public GameObject prefab;
    public Schedule[] schedules;
    public FrendshipDefinition frendship;

    [Header("animasi dan audio")]
    public AnimatorOverrideController bajuAnimator;
    public AnimatorOverrideController celanaAnimator;
    public AnimatorOverrideController rambutAnimator;
    public AnimatorOverrideController sepatuAnimator;

}