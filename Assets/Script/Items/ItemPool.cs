using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ItemCategoryGroup
{
    public string nameCategory;
    public ItemCategory categories;
    public List<Item> items = new List<Item>();
}

public class ItemPool : MonoBehaviour
{
    public static ItemPool Instance;

    [SerializeField] public List<Item> items; // Ini adalah list TEMPLATE
    public List<ItemCategoryGroup> itemCategoryGroups = new List<ItemCategoryGroup>();

    private void Awake()
    {
        Instance = this;

        // Inisialisasi itemIDs berdasarkan urutan dalam list
        for (int i = 0; i < items.Count; i++)
        {
            items[i].itemID = i + 1;
        }
        AddItemCategories();
    }

    // Fungsi utama untuk mendapatkan instance item baru dengan kualitas tertentu.
    public Item GetItemWithQuality(string name, ItemQuality quality, int count = 1, int level = 1)
    {
        Item itemTemplate = items.Find(x => x.itemName == name);
        if (itemTemplate != null)
        {
            // Panggil AddNewItem untuk membuat kloningan dengan kualitas yang spesifik
            Item newItemInstance = AddNewItem(itemTemplate, count, quality);
            newItemInstance.Level = level; // Atur level pada instance baru
            return newItemInstance;
        }
        else
        {
            Debug.LogWarning($"Item with name '{name}' not found in ItemPool!");
            return null;
        }
    }

    //Jalan pintas untuk mendapatkan item dengan kualitas Normal.
    public Item GetItem(string name, int count = 1, int level = 1)
    {
        // Fungsi ini sekarang hanya menjadi wrapper/jalan pintas yang aman.
        return GetItemWithQuality(name, ItemQuality.Normal, count, level);
    }


    public void DropItem(string itemName, Vector2 pos, GameObject itemDrop, int count = 1, int level = 1)
    {
        if (itemDrop == null)
        {
            Debug.LogError($"Item drop prefab untuk {itemName} tidak valid.");
            return;
        }

        GameObject droppedItem = Instantiate(itemDrop, pos, Quaternion.identity);


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
        rb.AddForce(new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), -1f), ForceMode2D.Impulse);

        // Panggil StopGravity dari komponen ItemDropInteractable
        ItemDropInteractable interactable = droppedItem.GetComponent<ItemDropInteractable>();
        interactable.isPickable = false; // Pastikan item bisa diambil

        if (interactable != null)
        {
            Debug.Log("item berhenti");
            interactable.StartCoroutine(interactable.StopGravity(rb, 0.8f));
        }

        //droppedItem.GetComponent<ItemDropInteractable>().item = GetItem(itemName, count, level);
    }

    // Membuat kloningan (deep copy) dari sebuah item template dengan jumlah dan kualitas tertentu.
    private Item AddNewItem(Item itemTemplate, int stackCount, ItemQuality quality)
    {
        // Membuat instance baru dari Item, ini adalah kloningan kosong
        Item newItem = ScriptableObject.CreateInstance<Item>();

        // Salin SEMUA properti dari template ke instance baru
        newItem.itemID = itemTemplate.itemID;
        newItem.itemName = itemTemplate.itemName;
        newItem.types = itemTemplate.types;
        newItem.categories = itemTemplate.categories;
        newItem.sprite = itemTemplate.sprite;
        newItem.itemDescription = itemTemplate.itemDescription;
        newItem.QuantityFuel = itemTemplate.QuantityFuel;
        newItem.maxhealth = itemTemplate.maxhealth;
        newItem.health = itemTemplate.health;
        newItem.MaxLevel = itemTemplate.MaxLevel;
        newItem.Damage = itemTemplate.Damage;
        newItem.AreaOfEffect = itemTemplate.AreaOfEffect;
        newItem.SpecialAttackCD = itemTemplate.SpecialAttackCD;
        newItem.SpecialAttackStamina = itemTemplate.SpecialAttackStamina;
        newItem.UpgradeCost = itemTemplate.UpgradeCost;
        newItem.UseStamina = itemTemplate.UseStamina;
        newItem.RangedWeapon_ProjectilePrefab = itemTemplate.RangedWeapon_ProjectilePrefab;
        newItem.isStackable = itemTemplate.isStackable;
        newItem.maxStackCount = itemTemplate.maxStackCount;
        newItem.BuyValue = itemTemplate.BuyValue;
        newItem.SellValue = itemTemplate.SellValue; // Ini adalah harga jual dasar
        newItem.BurningTime = itemTemplate.BurningTime;
        newItem.CookTime = itemTemplate.CookTime;
        newItem.prefabItem = itemTemplate.prefabItem;
        newItem.growthTime = itemTemplate.growthTime;
        newItem.dropItem = itemTemplate.dropItem;
        newItem.buffSprint = itemTemplate.buffSprint;
        newItem.buffDamage = itemTemplate.buffDamage;
        newItem.buffProtection = itemTemplate.buffProtection;
        newItem.countHeal = itemTemplate.countHeal;
        newItem.countStamina = itemTemplate.countStamina;
        newItem.waktuBuffDamage = itemTemplate.waktuBuffDamage;
        newItem.waktuBuffProtection = itemTemplate.waktuBuffProtection;
        newItem.waktuBuffSprint = itemTemplate.waktuBuffSprint;

        // Salinan untuk array agar tidak menggunakan referensi yang sama
        newItem.growthImages = (Sprite[])itemTemplate.growthImages.Clone();
        newItem.growthObject = (GameObject[])itemTemplate.growthObject.Clone();

        //menetapkan data unik untuk setiap instance
        //newItem.stackCount = stackCount;
        newItem.quality = quality; // Menetapkan kualitas pada kloningan baru

        // Beri nama berbeda pada asset instance agar mudah di-debug di Unity Editor
        newItem.name = $"{newItem.itemName} (Instance - {quality})";
        return newItem;
    }

    public void AddItemCategories()
    {
        itemCategoryGroups.Clear(); // Pastikan list kosong sebelum isi ulang

        foreach (Item item in items)
        {
            foreach (ItemCategory kategori in Enum.GetValues(typeof(ItemCategory)))
            {
                if (kategori == ItemCategory.None) continue; // Lewati 'None' jika ada

                // Cek apakah item termasuk dalam kategori ini
                if (item.IsInCategory(kategori))
                {
                    // Cari grup yang sudah ada
                    ItemCategoryGroup group = itemCategoryGroups.Find(g => g.categories == kategori);

                    if (group == null)
                    {
                        group = new ItemCategoryGroup { categories = kategori, nameCategory = kategori.ToString() };
                        itemCategoryGroups.Add(group);

                    }

                    // Tambahkan item ke grup yang sesuai
                    if (!group.items.Contains(item))
                    {
                        group.items.Add(item);
                    }
                }
            }
        }
    }

    public void AddItem(ItemData itemDataToAdd)
    {
        //Validasi awal
        if (itemDataToAdd == null || itemDataToAdd.count <= 0) return;

        //Dapatkan "Katalog Produk" (ItemSO) dari database menggunakan nama dari paket data
        Item itemTemplate = GetItemWithQuality(itemDataToAdd.itemName, itemDataToAdd.quality);
        if (itemTemplate == null)
        {
            Debug.LogError($"Tidak ada item dengan nama '{itemDataToAdd.itemName}' di ItemDatabase!");
            return;
        }

        int amountToAdd = itemDataToAdd.count;

        //FASE MENUMPUK (STACKING)
        if (itemTemplate.isStackable)
        {
            // Cari slot di inventaris yang itemnya sama, kualitasnya sama, dan belum penuh
            foreach (ItemData slot in PlayerController.Instance.playerData.inventory)
            {
                if (slot.itemName == itemDataToAdd.itemName && slot.quality == itemDataToAdd.quality && slot.count < itemTemplate.maxStackCount)
                {
                    int availableSpace = itemTemplate.maxStackCount - slot.count;
                    int amountToStack = Mathf.Min(availableSpace, amountToAdd);

                    slot.count += amountToStack;
                    amountToAdd -= amountToStack;

                    if (amountToAdd <= 0) break;
                }
            }
        }

        // FASE MEMBUAT SLOT BARU
        while (amountToAdd > 0 && PlayerController.Instance.playerData.inventory.Count < PlayerController.Instance.playerData.maxItem)
        {
            int amountForNewSlot = Mathf.Min(amountToAdd, itemTemplate.maxStackCount);

            // Buat "Catatan Stok" (ItemData) baru dari data yang diterima
            ItemData newSlot = new ItemData(itemDataToAdd.itemName, amountForNewSlot, itemDataToAdd.quality);
            PlayerController.Instance.playerData.inventory.Add(newSlot);

            amountToAdd -= amountForNewSlot;
        }

        // ... (peringatan jika inventory penuh) ...

        //// Siarkan berita bahwa inventory telah berubah!
        //OnInventoryUpdated?.Invoke();
    }

    public void RemoveItemsFromInventory(ItemData itemDataToRemove)
    {
        // Jumlah yang masih harus dihapus, diambil dari argumen.
        int remainingToRemove = itemDataToRemove.count;

        // Nama item yang akan dihapus untuk perbandingan.
        string itemNameToRemove = itemDataToRemove.itemName;

        // Loop dari BELAKANG ke depan. Ini wajib dilakukan agar aman saat
        // menghapus elemen dari sebuah list di tengah-tengah perulangan.
        for (int i = PlayerController.Instance.playerData.inventory.Count - 1; i >= 0; i--)
        {
            ItemData slot = PlayerController.Instance.playerData.inventory[i];

            // Jika nama item di slot ini cocok
            if (slot.itemName == itemNameToRemove)
            {
                int amountInSlot = slot.count;

                // Jika jumlah di slot ini lebih dari cukup untuk memenuhi sisa yang perlu dihapus
                if (amountInSlot > remainingToRemove)
                {
                    // Kurangi jumlah di slot, dan selesai.
                    slot.count -= remainingToRemove;
                    remainingToRemove = 0;
                }
                // Jika jumlah di slot ini pas atau kurang dari yang perlu dihapus
                else
                {
                    // Kurangi sisa yang perlu dihapus dengan seluruh isi slot ini
                    remainingToRemove -= amountInSlot;
                    // Hapus seluruh slot dari inventory karena isinya diambil semua.
                    PlayerController.Instance.playerData.inventory.RemoveAt(i);
                }
            }

            // Jika semua item yang perlu dihapus sudah terpenuhi, hentikan loop untuk efisiensi.
            if (remainingToRemove <= 0)
            {
                break;
            }
        }
    }
}