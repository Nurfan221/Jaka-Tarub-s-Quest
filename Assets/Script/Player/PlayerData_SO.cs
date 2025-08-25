using System.Collections.Generic;
using UnityEngine;


// [CreateAssetMenu] adalah perintah agar kita bisa membuat file ini dari menu Assets di Unity.
[CreateAssetMenu(fileName = "NewPlayerData", menuName = "JakaTarub/Player Data Container")]
public class PlayerData_SO : ScriptableObject
{
    [Header("Pengaturan Gerakan")]
    public float walkSpd = 5f;
    public float dashStamina = 40f;
    public float dashDistance = 5;
    public float dashForce = 5;
    public Vector2 lastPosition;

    [Header("Player Health")]

    [Header("HEALTH VALUE")]
    public int maxHealth = 100;
    public int health = 100;
    public int currentHealthCap;


    [Header("STAMINA VALUE")]
    public int maxStamina = 100;
    public float stamina = 100;
    public float staminaRegenRate = 15;
    public float currentStaminaCap;

    [Header("Emotional Cap System")]
    [Range(0, 100)]
    public float initialGriefPenalty = 30f;
    public bool isInGrief = false;
    public float currentGriefPenalty;
    public int healingQuestsCompleted = 0;
    public int totalHealingQuests = 5;

    [Header("Pengaturan Inventory")]
    public List<Item> itemList = new();
    public List<ItemData> inventory = new List<ItemData>();
    public int maxItem = 18;

    public ItemData emptyItemTemplate;
    public Item emptyItem;
    public List<Item> equippedCombat = new List<Item>(2);
    public List<ItemData> equippedItemData = new List<ItemData>(2);
    public ItemData equippedWeaponTemplate;
    public Item equippedWeapon;
    public ItemData equippedItemTemplate;
    public Item equippedItem;
    public List<Item> quickSlots = new List<Item>(2);
    public List<ItemData> itemUseData = new List<ItemData>(2);
    public bool equipped1 = true;
    public bool itemUse1 = true;
    // BARU: Sistem Kelelahan (Fatigue)
    [Header("Fatigue System")]
    [Range(0, 100)]
    public float currentFatiguePenalty = 0f; // Penalti stamina dari kelelahan (dalam persen)
    [Tooltip("Batas maksimum penalti kelelahan yang bisa diakumulasi.")]
    public float maxFatiguePenalty = 50f; // Contoh: maks 50% dari stamina bisa hilang karena lelah
}