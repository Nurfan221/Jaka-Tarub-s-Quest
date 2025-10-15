using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : Interactable, ISaveable
{

    public List<ItemData> currentSeasonItems; // List yang sedang aktif
    public List<ItemData> itemToSell; // list item yang sedang dijual 

    public int minItemShop;
    public int maxItemShop;
    public TypeShop typeShop;
    private Season currenSeason;


    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
        TimeManager.OnSeasonChanged += HandleNewSeason;
    
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
        TimeManager.OnSeasonChanged -= HandleNewSeason;
       
    }

    public object CaptureState()
    {
        Debug.Log($"[SAVE-CAPTURE] ShopInteractable menangkap {itemToSell.Count} item di list.");

        // Buat SATU objek untuk menyimpan semua data, bukan list
        var saveData = new ItemShopSaveData
        {
            typeName = typeShop.ToString(),
            typeShop = typeShop,
            // Langsung salin semua item yang ada di itemToSell ke dalam list 'items'
            items = new List<ItemData>(itemToSell)
        };

        return saveData; // Kembalikan satu objek saja
    }

    public void RestoreState(object state)
    {
        Debug.Log("[LOAD] Merestorasi data item di Itemshop...");

        // Cast 'state' menjadi satu objek ItemShopSaveData (sekarang ini akan berhasil)
        ItemShopSaveData data = (ItemShopSaveData)state;

        // Pastikan tipe tokonya sama untuk menghindari bug
        if (data.typeShop == this.typeShop)
        {
            itemToSell.Clear();

            itemToSell.AddRange(data.items);

            Debug.Log($"[LOAD] Berhasil merestorasi {itemToSell.Count} item.");
        }
    }
    // Gunakan Awake() untuk pendaftaran agar dieksekusi lebih awal
    void Awake()
    {
        Debug.Log("Daftarkan Toko interactable ke main environment ");
        // Daftarkan diri ke MainEnvironmentManager saat objek dibuat
        if (MainEnvironmentManager.Instance != null)
        {

            // Anda bisa menambahkan Debug.Log di sini jika perlu
            MainEnvironmentManager.Instance.RegisterShop(this);
        }else
        {
            Debug.Log("mainenvironment kosong bro ");
        }
    }
    private void OnDestroy()
    {
        // Hapus pendaftaran saat objek nonaktif atau hancur
        if (MainEnvironmentManager.Instance != null)
        {
            MainEnvironmentManager.Instance.UnregisterShop(this);
        }
    }

    private void Start()
    {
        HandleNewDay();

    }


    protected override void Interact()
    {
        MechanicController.Instance.HandleOpenShop(typeShop, currentSeasonItems, itemToSell, this);
    }
    public void HandleNewDay()
    {
        //RestockDaily(currentSeason)
        currenSeason = TimeManager.Instance.GetCurrentSeason();
        UpdateItemInShop(currenSeason);

    }

    public void HandleNewSeason()
    {
        currenSeason = TimeManager.Instance.GetCurrentSeason();
    }

    public void UpdateItemInShop(Season season)
    {
        ShopTypeDatabase shopTypeDatabase = DatabaseManager.Instance.GetTypeShopDatabase(typeShop);
        Debug.Log("Update Item di Shop untuk season: " + season + " dan tipe shop: " + shopTypeDatabase.typeShopName);
        ItemShopDatabase itemShopDatabase = DatabaseManager.Instance.GetCurrentItemShopDatabase(season, shopTypeDatabase);
        AddItemToList(itemShopDatabase.itemsForSale, itemShopDatabase.itemWajib);
    }
    public void AddItemToList(List<Item> Items, List<Item> itemWajib)
    {
        // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama
        currentSeasonItems.Clear();
        itemToSell.Clear(); // Mungkin Anda juga ingin mengosongkan ini? Sesuaikan jika perlu.

        foreach (var item in Items)
        {
            //Debug.Log($"Sebelum Clear Item: {item.itemName}, Jumlah: {item.stackCount}");
            ItemData itemData = new ItemData(item.itemName, 1, item.quality, item.health);
            int randomCount = UnityEngine.Random.Range(minItemShop, maxItemShop + 1);
            itemData.count = randomCount;
            currentSeasonItems.Add(itemData);

        }

        foreach (var item in itemWajib)
        {
            ItemData itemData = new ItemData(item.itemName, 1, item.quality, item.health);
            int randomCount = UnityEngine.Random.Range(minItemShop, maxItemShop + 1);
            itemData.count = randomCount;
            currentSeasonItems.Add(itemData);
        }


    }
}
