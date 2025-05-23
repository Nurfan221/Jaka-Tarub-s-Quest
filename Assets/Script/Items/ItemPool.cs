using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance;

    [SerializeField]public List<Item> items;

    private void Awake()
    {
        Instance = this;

        // Inisialisasi itemIDs berdasarkan urutan dalam list
        for (int i = 0; i < items.Count; i++)
        {
            items[i].itemID = i + 1; // Mengatur itemID sesuai urutan, dimulai dari 1
        }
    }

    public Item GetItem(string name, int count = 1, int level = 1)
    {
        Item itemToGet = items.Find(x => x.itemName == name);
        AddNewItem(itemToGet, itemToGet.stackCount);
        if (itemToGet != null)
        {
            itemToGet.stackCount = count; // Ini akan menentukan jumlah item yang ada di stack

            itemToGet.Level = level;
            return Instantiate(itemToGet);
        }
        else
        {
            Debug.LogWarning($"Item with name {name} not found in ItemPool!");
            return null;
        }
    }

    public void DropItem(string itemName, Vector2 pos, GameObject itemDrop, int count = 1, int level = 1)
    {
        if (itemDrop == null)
        {
            Debug.LogError($"Item drop prefab untuk {itemName} tidak valid.");
            return;
        }

        GameObject droppedItem = Instantiate(itemDrop, pos, Quaternion.identity);

        // Set tag menjadi ItemDrop agar bisa dideteksi saat player mengambilnya
        droppedItem.tag = "ItemDrop";

        // Jika item memiliki komponen visual (misalnya SpriteRenderer)
        SpriteRenderer spriteRenderer = droppedItem.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Item itemData = ItemPool.Instance.items.Find(item => item.itemName == itemName);

            if (itemData != null)
            {
                spriteRenderer.sprite = itemData.sprite; // Ganti sprite sesuai dengan item
            }
        }

        // Menambahkan Rigidbody2D dan force untuk efek jatuh
        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0.5f;
        rb.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), -1f), ForceMode2D.Impulse);

        // Panggil StopGravity dari komponen ItemDropInteractable
        ItemDropInteractable interactable = droppedItem.GetComponent<ItemDropInteractable>();
        if (interactable != null)
        {
            Debug.Log("item berhenti");
            interactable.StartCoroutine(interactable.StopGravity(rb, 0.8f));
        }

        droppedItem.GetComponent<ItemDropInteractable>().item = GetItem(itemName, count, level);
    }

    public Item AddNewItem(Item item, int stackCount)
    {
        // Membuat salinan mendalam dari item
        Item newItem = ScriptableObject.CreateInstance<Item>(); // Membuat instance baru dari Item

        // Salin semua properti dari item lama
        newItem.itemID = item.itemID;
        newItem.itemName = item.itemName;
        newItem.types = item.types;
        newItem.categories = item.categories;
        newItem.sprite = item.sprite;
        newItem.itemDescription = item.itemDescription;
        newItem.QuantityFuel = item.QuantityFuel;
        newItem.maxhealth = item.maxhealth;
        newItem.health = item.health;
        newItem.Level = item.Level;
        newItem.MaxLevel = item.MaxLevel;
        newItem.Damage = item.Damage;
        newItem.AreaOfEffect = item.AreaOfEffect;
        newItem.SpecialAttackCD = item.SpecialAttackCD;
        newItem.SpecialAttackStamina = item.SpecialAttackStamina;
        newItem.UpgradeCost = item.UpgradeCost;
        newItem.UseStamina = item.UseStamina;
        newItem.RangedWeapon_ProjectilePrefab = item.RangedWeapon_ProjectilePrefab;
        newItem.isStackable = item.isStackable;
        newItem.stackCount = stackCount;
        newItem.maxStackCount = item.maxStackCount;
        newItem.BuyValue = item.BuyValue;
        newItem.SellValue = item.SellValue;
        newItem.BurningTime = item.BurningTime;
        newItem.CookTime = item.CookTime;
        newItem.prefabItem = item.prefabItem;
        newItem.growthTime = item.growthTime;
        newItem.dropItem = item.dropItem;

        // Membuat salinan array (Jika ada array atau objek lainnya)
        newItem.growthImages = item.growthImages.ToArray();  // Membuat salinan dari array
        newItem.growthObject = item.growthObject.ToArray();  // Membuat salinan dari array

        newItem.name = newItem.itemName;
        return newItem;
    }




}
