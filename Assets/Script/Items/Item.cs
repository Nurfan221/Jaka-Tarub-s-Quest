using UnityEngine;

[System.Flags]
public enum ItemType
{
    Melee_Combat = 0,
    Ranged_Combat = 1,
    Heal = 2,
    Buff = 4,
    Item = 8,
    Quest = 16,
    ItemPrefab = 32,
    Animal = 64,
    Pestisida = 128,
    None = 256,
    PenyiramTanaman = 512,
    Cangkul = 1024,
}

[System.Flags]
public enum ItemCategory
{
    None = 0,
    Fruit = 1,
    Meat = 2,
    Fuel = 4,
    Vegetable = 8,
    Food = 16,
    Drink = 32,
    Medicine = 64,
    Ammo = 128,
    Weapon = 256,
    Crafting_Material = 512,
    PlantSeed = 1024,
    TreeSeed = 2048,
    ItemPrefab = 4096,
    Insectisida = 8192,
    Hunt = 16384,
}

public enum ItemQuality
{
    Normal = 0,   // Bintang 1
    Baik = 1,     // Bintang 2
    Sempurna = 2  // Bintang 3
}

[CreateAssetMenu(menuName = "Make an Item")]
public class Item : ScriptableObject
{
    [Header("STATS")]
    public int itemID;
    public string itemName;
    public ItemQuality quality;
    public ItemType types;
    public ItemCategory categories;
    public Sprite sprite;
    [TextArea]
    public string itemDescription;
    public int QuantityFuel;

    public int maxhealth;
    public int health; //deklarasikan health untuk menentukan berapa kali item di gunakan
    public float waktuBuffDamage;
    public int buffDamage;
    public float waktuBuffSprint;
    public int buffSprint;
    public float waktuBuffProtection;
    public int buffProtection;
    public int countHeal;
    public int countStamina;


    // Combat Item
    [Header("COMBAT")]
    public bool isItemCombat;
    public int Level;
    public int MaxLevel;
    public int Damage;
    public int AreaOfEffect;
    public int SpecialAttackCD;
    public int SpecialAttackStamina;
    public int UpgradeCost;
    public int UseStamina;
    public GameObject RangedWeapon_ProjectilePrefab;

    // Regular Item
    [Header("REGULAR")]
    public bool isStackable = false;
    //public int stackCount;
    public int maxStackCount;
    public int BuyValue;
    public int SellValue;
    public int BurningTime;
    public int CookTime;
    public string itemDropName; // nama item yang dijatuhkan au dihasilkan
    public string namePrefab; // nama prefab yang akan ditanam khusus untuk seed atau tanaman 



    // Seed Properties (khusus untuk benih)
    [Header("SEED PROPERTIES")]
    public float growthTime; // Lama pertumbuhan dalam hari
    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    //public GameObject plantPrefab; // Prefab tanaman yang akan tumbuh dari benih


    public bool IsInCategory(ItemCategory category)
    {
        return (categories & category) == category;
    }

    public bool IsInType(ItemType type)
    {
        return (types & type) == type;
    }


}
