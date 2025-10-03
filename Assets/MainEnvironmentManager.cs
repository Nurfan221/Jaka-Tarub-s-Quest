using System.Collections.Generic;
using UnityEngine;

public class MainEnvironmentManager : MonoBehaviour
{
    public static MainEnvironmentManager Instance { get; private set; }

    // Referensi ke manajer spesifik (ini adalah "radio" Anda)
    // Anda bisa menyeret skrip EnvironmentManager dari objek lain ke sini di Inspector
    public BatuManager batuManager;
    public TreesManager pohonManager;
    public EnvironmentManager kuburanManager;
    public EnvironmentManager tumbuhanManager;
    public PlantContainer plantContainer;
    public BungaManager bungaManager;
    public JamurManager jamurManager;
    public StorageSystem storageManager;

    // Kunci: Enum TypeShop, Nilai: Referensi ke skrip ShopInteractable.
    private Dictionary<TypeShop, ShopInteractable> registeredShops = new Dictionary<TypeShop, ShopInteractable>();


    // Fungsi ini akan dipanggil oleh setiap Player_Movement baru yang muncul.
    // Satu fungsi untuk mendaftarkan SEMUA jenis toko
    public void RegisterShop(ShopInteractable shop)
    {
        Debug.Log("Mencoba daftarkan toko " +  shop.typeShop);
        // Pengecekan dasar
        if (shop == null) return;

        TypeShop shopType = shop.typeShop;

        if (registeredShops.ContainsKey(shopType))
        {
            // Jika toko dengan tipe yang sama sudah ada, perbarui referensinya.
            Debug.LogWarning($"Toko dengan tipe '{shopType}' sudah terdaftar. Referensi akan diperbarui ke '{shop.gameObject.name}'.");
            registeredShops[shopType] = shop;
        }
        else
        {
            // Jika belum ada, tambahkan yang baru.
            registeredShops.Add(shopType, shop);
            Debug.Log($"Toko '{shop.gameObject.name}' dengan tipe '{shopType}' berhasil terdaftar.");
        }
    }

    // Satu fungsi untuk menghapus pendaftaran SEMUA jenis toko
    public void UnregisterShop(ShopInteractable shop)
    {
        if (shop == null) return;

        TypeShop shopType = shop.typeShop;

        // Hapus dari dictionary jika ada
        if (registeredShops.ContainsKey(shopType))
        {
            registeredShops.Remove(shopType);
            Debug.Log($"Toko dengan tipe '{shopType}' berhasil dihapus dari daftar.");
        }
    }

    // Fungsi untuk mendapatkan referensi toko berdasarkan tipenya
    public ShopInteractable GetShop(TypeShop shopType)
    {
        if (registeredShops.TryGetValue(shopType, out ShopInteractable shop))
        {
            return shop; // Kembalikan toko jika ditemukan
        }

        Debug.LogWarning($"Pencarian toko: Toko dengan tipe '{shopType}' tidak ditemukan.");
        return null; // Kembalikan null jika tidak ditemukan
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(this.gameObject); jika diperlukan
        }
    }


    // Logika penambahan item menjadi lebih dinamis
    public void HandleAddItemSellToShops(List<ItemShopSaveData> itemSellList)
    {
        Debug.Log("Mencoba menambahkan item ke toko dari data simpanan...");
        foreach (var itemSaveData in itemSellList)
        {
            // Dapatkan toko yang sesuai dari dictionary berdasarkan tipe
            ShopInteractable targetShop = GetShop(itemSaveData.typeShop);

            if (targetShop != null)
            {
                Debug.Log($"Toko '{itemSaveData.typeShop}' ditemukan. Menambahkan {itemSaveData.items.Count} item.");
                // Tambahkan semua item dari data simpanan ke daftar item toko
                targetShop.itemToSell.AddRange(itemSaveData.items);
            }
            else
            {
                Debug.Log($"Toko dengan tipe '{itemSaveData.typeShop}' tidak ditemukan untuk ditambahkan item.");
            }
        }
    }



}