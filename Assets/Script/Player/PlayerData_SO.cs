using UnityEngine;


public class PlayerData_SO : ScriptableObject
{
    [Header("Pengaturan Gerakan")]
    public float walkSpd = 5f;
    public float dashStamina = 40f;
    public float dashDistance = 5;
    public float dashForce = 5;

    [Header("HEALTH & STAMINA DASAR")]
    public float maxHealth = 100;
    public float maxStamina = 100;
    public float staminaRegenRate = 15;

    [Header("Emotional Cap System")]
    [Range(0, 100)]
    public float initialGriefPenalty = 30f;
    public int totalHealingQuests = 5;

    [Header("Pengaturan Inventory")]
    public int maxItem = 18;
    public ItemData emptyItemTemplate;
    public ItemData equippedWeaponTemplate;
    public ItemData equippedItemTemplate;

    [Header("Fatigue System")]
    [Tooltip("Batas maksimum penalti kelelahan.")]
    public float maxFatiguePenalty = 50f;






}