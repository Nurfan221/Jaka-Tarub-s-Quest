using UnityEngine;
using System.Linq;

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
}

[CreateAssetMenu(menuName = "Make an Item")]
public class Item : ScriptableObject
{
    [Header("STATS")]
    public int itemID;
    public string itemName;
    public ItemType types;
    public ItemCategory categories;
    public Sprite sprite;
    [TextArea]
    public string itemDescription;
    public int QuantityFuel;
    public float maxhealth;
    public float health; //deklarasikan health untuk menentukan berapa kali item di gunakan

    // Combat Item
    [Header("COMBAT")]
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
    public int stackCount;
    public int maxStackCount;
    public int BuyValue;
    public int SellValue;
    public int BurningTime;
    public int CookTime;
    public GameObject prefabItem;
    

    // Seed Properties (khusus untuk benih)
    [Header("SEED PROPERTIES")]
    public float growthTime; // Lama pertumbuhan dalam detik
    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    public GameObject[] growthObject; // objek untuk setiap tahapan pertumbuhan pohon # khusus pohon
    //public GameObject plantPrefab; // Prefab tanaman yang akan tumbuh dari benih
    public GameObject dropItem; //prefab untuk buah yang akan di hasilkan

    public bool IsInCategory(ItemCategory category)
    {
        return (categories & category) == category;
    }

    public bool IsInType(ItemType type)
    {
        return (types & type) == type;
    }


}
