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

public enum ItemCategory
{
    Fruit,
    Meat,
    Fuel,
    Vegetable,
    Food,
    Drink,
    Medicine,
    Ammo,
    Weapon,
    Crafting_Material,
    Seed
}

[CreateAssetMenu(menuName = "Make an Item")]
public class Item : ScriptableObject
{
    [Header("STATS")]
    public int itemID;
    public string itemName;
    public ItemType type;
    public ItemCategory category;
    public Sprite sprite;
    [TextArea]
    public string itemDescription;
    public int QuantityFuel;

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
    public int BurningTime;
    public int CookTime;

    // Seed Properties (khusus untuk benih)
    [Header("SEED PROPERTIES")]
    public float growthTime; // Lama pertumbuhan dalam detik
    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    public GameObject plantPrefab; // Prefab tanaman yang akan tumbuh dari benih
    public GameObject dropItem; //prefab untuk buah yang akan di hasilkan
}
