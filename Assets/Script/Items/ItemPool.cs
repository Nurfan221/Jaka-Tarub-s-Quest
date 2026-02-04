using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

    [Header("Daftar Hubungan")]
    public ItemPoolDatabase databaseItemPool; // Ini adalah list TEMPLATE
    public List<Item> items; 
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
        return itemTemplate;
        //if (itemTemplate != null)
        //{
        //    // Panggil AddNewItem untuk membuat kloningan dengan kualitas yang spesifik
        //    //Item newItemInstance = AddNewItem(itemTemplate, count, quality);
        //    newItemInstance.Level = level; // Atur level pada instance baru
        //    return newItemInstance;
        //}
        //else
        //{
        //    Debug.LogWarning($"Item with name '{name}' not found in ItemPool!");
        //    return null;
        //}
    }

    //Jalan pintas untuk mendapatkan item dengan kualitas Normal.
    public Item GetItem(string name, int count = 1, int level = 1)
    {
        // Fungsi ini sekarang hanya menjadi wrapper/jalan pintas yang aman.
        return GetItemWithQuality(name, ItemQuality.Normal, count, level);
    }


    // Asumsi fungsi ini ada di dalam ItemPool.cs
    public void DropItem(string itemName, int healthItem, ItemQuality itemQuality, Vector2 pos, int count, int level = 1)
    {
        Item itemDefinition = GetItemWithQuality(itemName, itemQuality);
        if (itemDefinition == null)
        {
            Debug.LogError($"Definisi untuk item '{itemName}' tidak ditemukan di ItemPool.");
            return;
        }

        GameObject itemPrefab = DatabaseManager.Instance.itemWorldPrefab;
        if (itemPrefab == null)
        {
            Debug.LogError("itemWorldPrefab belum diatur di DatabaseManager!");
            return;
        }

        GameObject droppedItemGO = Instantiate(itemPrefab, pos, Quaternion.identity);
        droppedItemGO.name = $"{itemDefinition.itemName}_Dropped";

        ItemDropInteractable interactable = droppedItemGO.GetComponent<ItemDropInteractable>();

        if (interactable != null)
        {
            ItemData newItemData = new ItemData(itemName, count, itemQuality, healthItem);
            interactable.itemdata = newItemData;
            interactable.isPickable = false;

            Debug.Log($"data item yang di drop : nama item {itemName}, jumlah item {count}, item quality {itemQuality}, health item {healthItem}");
        }




        Transform visualChild = droppedItemGO.transform.Find("Visual");

        if (visualChild != null)
        {
            // Ambil komponen dari anak tersebut
            SpriteRenderer spriteRenderer = visualChild.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = itemDefinition.sprite;

        }
        else
        {
            Debug.LogError("Gawat! Tidak ada anak bernama 'Visual' di objek ini!" + gameObject.name);
        }

        Rigidbody2D rb = droppedItemGO.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Berikan "tendangan" awal seperti biasa
            float force = 2.5f;
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * force, ForceMode2D.Impulse);


            // dan suruh ia berhenti setelah 1.0 detik.
            if (interactable != null)
            {
                interactable.StartCoroutine(interactable.FreezeAfterDelay(0.3f));
            }
        }
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
    public bool IsInventoryFull()
    {
        return PlayerController.Instance.inventory.Count >= PlayerController.Instance.playerData.maxItem;
    }

    public bool AddItem(ItemData itemDataToAdd)
    {
        // Validasi Awal: Cek apakah data item valid
        if (itemDataToAdd == null || itemDataToAdd.count <= 0)
        {
            return false;
        }

        // Validasi Database: Pastikan item terdaftar di database (ItemSO)
        Item itemTemplate = GetItemWithQuality(itemDataToAdd.itemName, itemDataToAdd.quality);
        if (itemTemplate == null)
        {
            Debug.LogError($"Gagal AddItem: '{itemDataToAdd.itemName}' tidak ditemukan di Database!");
            return false;
        }

        int initialAmount = itemDataToAdd.count; // Jumlah awal yang ingin dimasukkan
        int amountLeft = initialAmount;          // Sisa yang belum masuk (akan kita kurangi)

        // Cek slot yang sudah ada DULUAN. Ini memungkinkan item masuk walau slot tas penuh.
        if (itemTemplate.isStackable)
        {
            foreach (ItemData slot in PlayerController.Instance.inventory)
            {
                // Cek apakah: Nama sama, Kualitas sama, dan Slot belum full stack
                if (slot.itemName == itemDataToAdd.itemName &&
                    slot.quality == itemDataToAdd.quality &&
                    slot.count < itemTemplate.maxStackCount)
                {

                    // Hitung berapa ruang kosong di slot ini
                    int availableSpace = itemTemplate.maxStackCount - slot.count;

                    // Ambil jumlah yang bisa masuk (min antara sisa item vs ruang kosong)
                    int amountToStack = Mathf.Min(availableSpace, amountLeft);

                    // Masukkan ke slot
                    slot.count += amountToStack;

                    // Kurangi sisa item yang perlu dimasukkan
                    amountLeft -= amountToStack;

                    // Jika sudah habis, berhenti mencari
                    if (amountLeft <= 0) break;
                }
            }
        }

        // Hanya dijalankan jika masih ada sisa item (amountLeft > 0)
        while (amountLeft > 0)
        {
            // PENTING: Cek apakah tas penuh SEBELUM membuat slot baru
            if (IsInventoryFull())
            {
                Debug.Log("Inventaris Penuh! Tidak bisa membuat slot baru.");
                break; // Berhenti paksa, sisa item tidak bisa masuk
            }

            // Tentukan isi slot baru (maksimal 1 stack penuh)
            int amountForNewSlot = Mathf.Min(amountLeft, itemTemplate.maxStackCount);

            // Buat data baru
            ItemData newSlot = new ItemData(
                itemDataToAdd.itemName,
                amountForNewSlot,
                itemDataToAdd.quality,
                itemDataToAdd.itemHealth
            );
            
            // Masukkan ke list inventory
            PlayerController.Instance.inventory.Add(newSlot);

            // Kurangi sisa
            amountLeft -= amountForNewSlot;
        }


        // Hitung total yang BENAR-BENAR masuk
        int totalAdded = initialAmount - amountLeft;

        if (totalAdded > 0)
        {
            // Update sistem mekanik (misal UI slot inventory utama)
            MechanicController.Instance.HandleUpdateInventory();

            // Tampilkan Popup "Mendapatkan Item"
            // Kita buat dummy data agar UI menampilkan angka yang jujur (totalAdded), bukan angka awal
            ItemData dataForUI = new ItemData(itemDataToAdd.itemName, totalAdded, itemDataToAdd.quality, itemDataToAdd.itemHealth);
            ItemGetPanelManager.Instance.ShowItems(dataForUI);

            // Debug info
            if (amountLeft > 0)
            {
                Debug.LogWarning($"Inventaris penuh sebagian! Masuk: {totalAdded}, Terbuang: {amountLeft}");
            }
            SoundManager.Instance.PlaySound(SoundName.TakeItem);

            return true; // BERHASIL (Setidaknya sebagian masuk)
        }
        else
        {
            // Tidak ada satupun yang masuk (Tas Penuh Total dan tidak bisa ditumpuk)
            Debug.Log("Gagal menambahkan item: Inventaris Penuh.");
            return false; // GAGAL
        }
    }

    public void RemoveItemsFromInventory(ItemData itemDataToRemove)
    {
        // Jumlah yang masih harus dihapus, diambil dari argumen.
        int remainingToRemove = itemDataToRemove.count;

        // Nama item yang akan dihapus untuk perbandingan.
        string itemNameToRemove = itemDataToRemove.itemName;

        // Loop dari BELAKANG ke depan. Ini wajib dilakukan agar aman saat
        // menghapus elemen dari sebuah list di tengah-tengah perulangan.
        for (int i = PlayerController.Instance.inventory.Count - 1; i >= 0; i--)
        {
            ItemData slot = PlayerController.Instance.inventory[i];

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
                    PlayerController.Instance.inventory.RemoveAt(i);
                }
            }

            // Jika semua item yang perlu dihapus sudah terpenuhi, hentikan loop untuk efisiensi.
            if (remainingToRemove <= 0)
            {
                break;
            }
        }
    }

    public void DropRandomItemsOnPassOut() // Nama fungsi diubah agar lebih jelas
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogError("PlayerController Instance is null!");
            return;
        }

        PlayerController player = PlayerController.Instance;
        Vector3 playerPosition = PlayerController.Instance.HandleGetPlayerPosition();
        List<ItemData> inventory = player.inventory;

        if (inventory.Count == 0)
        {
            Debug.Log("Inventaris kosong, tidak ada item yang dijatuhkan.");
            return;
        }

        // Hitung berapa banyak SLOT item yang akan dijatuhkan
        int minItemsToDrop = Mathf.CeilToInt(inventory.Count * 0.5f);
        int maxItemsToDrop = Mathf.FloorToInt(inventory.Count * 0.75f);
        int slotsToDropCount = UnityEngine.Random.Range(minItemsToDrop, maxItemsToDrop + 1);

        // Pilih slot-slot unik secara acak untuk dijatuhkan
        List<int> allIndices = Enumerable.Range(0, inventory.Count).ToList();
        List<int> shuffledIndices = allIndices.OrderBy(x => UnityEngine.Random.value).ToList();
        List<int> indicesToDrop = shuffledIndices.Take(slotsToDropCount).ToList();

        Debug.Log($"Pemain pingsan! Menjatuhkan {slotsToDropCount} dari {inventory.Count} slot item.");

        // Siapkan daftar untuk menampung item yang akan dihapus nanti
        List<ItemData> itemsToRemove = new List<ItemData>();

        //  Loop melalui INDEKS ACAK yang sudah dipilih
        foreach (int randomIndex in indicesToDrop)
        {
            ItemData itemToDrop = inventory[randomIndex];

            // Pastikan item ada dan bisa didapatkan datanya
            // (Anda bisa skip pengecekan GetItemWithQuality jika tidak perlu)
            Item itemData = GetItemWithQuality(itemToDrop.itemName, itemToDrop.quality);
            if (itemData != null)
            {
                //  Jatuhkan semua item dari tumpukan (stack) ini 
                for (int i = 0; i < itemToDrop.count; i++)
                {
                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                    DropItem(itemToDrop.itemName, itemToDrop.itemHealth, itemToDrop.quality, playerPosition + offset, 1);
                }
            }

            // Tandai item ini untuk dihapus dari inventaris
            itemsToRemove.Add(itemToDrop);
        }

        foreach (ItemData item in itemsToRemove)
        {
            inventory.Remove(item);
        }

        Debug.Log("Proses menjatuhkan item dan menghapus dari inventaris selesai.");
    }




    [ContextMenu("Pindahkan Items ke database ")]
    public void AddItemsToDatabaseInEditor()
    {
        // Pengecekan Keamanan
        if (databaseItemPool == null)
        {
            Debug.LogError("Target WorldTreeDatabaseSO belum diatur! Harap seret asetnya ke Inspector.");
            return;
        }

        // Pastikan environmentList sudah diisi dengan data
        if (items.Count == 0)
        {
            Debug.LogWarning("environmentList masih kosong. Jalankan RegisterAllObject terlebih dahulu jika perlu.");
            return;
        }

        // Kosongkan list di SO untuk menghindari data duplikat
        databaseItemPool.items.Clear();

        Debug.Log($"Memulai migrasi {items.Count} data Item ke {databaseItemPool.name}...");

        // Loop melalui setiap entri di environmentList
        foreach (Item itemData in items)
        {
           

            // Tambahkan data baru ke dalam list di ScriptableObject
            databaseItemPool.items.Add(itemData);
        }

        // Tandai aset ScriptableObject sebagai "kotor" agar Unity menyimpan perubahan
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(databaseItemPool);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"Migrasi selesai! {databaseItemPool.items.Count} data pohon berhasil dipindahkan.");
    }
}