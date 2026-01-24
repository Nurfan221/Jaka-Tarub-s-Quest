using UnityEngine;

[System.Flags]
public enum ItemType
{
    None = 0,
    Ranged_Combat = 1,
    Heal = 2,
    Buff = 4,
    Item = 8,
    Quest = 16,
    ItemPrefab = 32,
    Animal = 64,
    Pestisida = 128,
    Melee_Combat = 256,
    PenyiramTanaman = 512,
    Cangkul = 1024,
    PickAxe = 2048,
    Pedang = 4096,
    Kapak = 8192,
    PenggarukSampah = 16384,
    Sabit = 32768,
    Perangkap = 65536,
    Pelebur = 131072,
    ItemShop = 262144,
    FoodAndDrink = 524288,



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
    Smelt = 32768,
    tools = 65536,
    Ingot = 131072,
    Pupuk = 262144,
    Seed = 524288,
    Produce = 1048576,
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemQuality
{
    Broken,  // Rusak/Busuk
    Normal,
    Good,
    Perfect
}
[CreateAssetMenu(menuName = "Make an Item")]
public class Item : ScriptableObject
{
    [Header("STATS")]
    public int itemID;
    public string itemName;
    public ItemQuality quality;
    public ItemRarity rarity;
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
    public LevelUpgradeTools level;
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
    public int CookTime;
    public string itemDropName; // nama item yang dijatuhkan au dihasilkan
    public string namePrefab; // nama prefab yang akan ditanam khusus untuk seed atau tanaman 



    // Seed Properties (khusus untuk benih)
    [Header("SEED PROPERTIES")]
    public float growthTime; // Lama pertumbuhan dalam hari
    public bool canRegrow; // Apakah tanaman bisa tumbuh
    public float regrowTime; // Lama waktu untuk tumbuh kembali setelah dipanen
    public float persentasePupuk; // Persentase pengaruh pupuk terhadap pertumbuhan
    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    public SeedType seedType; // Jenis benih (misal: sayuran, buah, dll)
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
