using UnityEngine;


public enum ItemType
{
    Melee_Combat,
    Ranged_Combat,
    Heal,
    Buff,
    Item,
    Quest
}

[CreateAssetMenu(menuName = "Make an Item")]
public class Item : ScriptableObject
{
    [Header("STATS")]
    public string itemName;
    public ItemType type;
    public Sprite sprite;
    [TextArea]
    public string itemDescription;

    // Combat Item
    [Header("COMBAT")]
    public int Level;
    public int MaxLevel;
    public int Damage;
    public int AreaOfEffect;
    public int SpecialAttackCD;
    public int SpecialAttackStamina;
    public int UpgradeCost;
    public GameObject RangedWeapon_ProjectilePrefab;

    // Regular Item
    [Header("REGULAR")]
    public bool isStackable = false;
    public int stackCount;
    public int BuyValue;
    public int SellValue;
}
