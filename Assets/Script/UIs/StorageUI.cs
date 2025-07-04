using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuestManager;
using static UnityEditor.Progress;

public class StorageUI : MonoBehaviour
{





    private Transform lastClickedItem = null; // Menyimpan item yang terakhir kali diklik

    public StorageInteractable theStorage;
    



    [Header("Slots")]
    [SerializeField] Transform StorageContainer;
    [SerializeField] Transform InventoryContainer;
    [SerializeField] Transform itemSlotTemplate;
    //[SerializeField] StorageInteractable currentStorage;

    //[SerializeField] InventoryUI inventoryUI;
    //popUP
    public Image popUp;
    public Image itemImage;
    public TextMeshProUGUI itemCount;
    // Variabel untuk menyimpan item yang sedang dipilih
    public Item selectedItem;
    public int selectedItemCount;
    public string qualityLevel;
    public bool isTakingFromStorage = false; // False = store ke storage, True = take dari storage


    [Header("Button Action")]

    // [SerializeField] Button itemAction;
    public Button storeAllButton;
    public Button takeAllButton;
    public Button plusItem;
    public Button minusItem;
    public Button confirm;
    public Button cancel;
    public Button maxItem;
    public Button minItem;

    // [SerializeField] Button take;


    [Header("Button")]
    public Button closeStorageButton;



    // Need to refresh both inventory and storage slots
    private void Start()
    {
        //StorageSystem.Instance.RegisterStorage(this);
        // Saat StorageUI muncul, ia langsung melapor ke "Penjaga Gerbang"
        if (MechanicController.Instance != null)
        {
            MechanicController.Instance.RegisterStorage(this); // 'this' merujuk ke skrip StorageUI ini
        }
        else
        {
            Debug.LogError("MechanicController tidak ditemukan!");
        }
        Debug.Log("StorageUI Start() dipanggil!"); // Debug awal

        if (closeStorageButton != null)
        {
            closeStorageButton.onClick.AddListener(CloseStorage);
            Debug.Log("Tombol close listener added.");
        }
        else
        {
            Debug.Log("Tombol close belum terhubung");
        }

        gameObject.SetActive(false);  // Nonaktifkan StorageUI setelah Awake() terpanggil


    }

    // Jangan lupa untuk "unregister" agar tidak ada referensi yang menggantung
    private void OnDestroy()
    {
        if (MechanicController.Instance != null)
        {
            MechanicController.Instance.UnregisterStorage(this);
        }
    }

    private void Update()
    {
        // Close
       
    }

    private PlayerData_SO stats;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }



    public void OpenStorage(StorageInteractable theStorage)
    {
        //GameController.Instance.PindahKeScene("Village");

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        gameObject.SetActive(true);

        // Start the animation coroutine
        //theStorage = theStorage;

        this.theStorage = theStorage;


        RefreshInventoryItems();

        takeAllButton.gameObject.SetActive(false);

        storeAllButton.onClick.RemoveAllListeners();
        //storeAllButton.onClick.AddListener(StoreAllItems);

        takeAllButton.onClick.RemoveAllListeners();
        //takeAllButton.onClick.AddListener(TakeAllItems);

        if (theStorage.storage.Count > 0)
        {
            takeAllButton.gameObject.SetActive(true);
        }
    }

    

    private void CloseStorage()
    {
        GameController.Instance.ResumeGame();
        // Tutup UI Storage
        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);

        // Panggil animasi tutup dari storage yang sedang terbuka
        if (theStorage != null)
        {
            theStorage.StartAnimationClose();  // Jalankan animasi tutup
        }

        ////theStorage.Items = Items;  // Simpan item kembali ke storage
        //MechanicController.Instance.HandleRefreshInventoryUI();
    }


    public void RefreshInventoryItems()
    {
        Debug.Log("Refresh item in inventory");

        foreach (Transform child in StorageContainer)
        {
            if (child == itemSlotTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in InventoryContainer)
        {
            if (child == itemSlotTemplate) continue;
            Destroy(child.gameObject);
        }


        // Set Inventory Slots
        for (int i = 0; i < stats.inventory.Count; i++)
        {
          // Buat salinan lokal dari data yang akan digunakan oleh listener.
            ItemData currentItemData = stats.inventory[i];
            Item currentItemSO = ItemPool.Instance.GetItemWithQuality(currentItemData.itemName, currentItemData.quality);

            if (currentItemSO == null) continue;

            Transform itemInInventory = Instantiate(itemSlotTemplate, InventoryContainer);
            itemInInventory.name = currentItemSO.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = currentItemSO.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = currentItemData.count.ToString();

            // Sekarang, listener akan menggunakan salinan lokal yang nilainya tidak akan berubah.
            itemInInventory.GetComponent<Button>().onClick.AddListener(() =>
            {
                // Menggunakan 'currentItemData' yang nilainya "terkunci" untuk iterasi ini.
                OnInventoryItemClick(currentItemData);
            });
        }

        // Set storage
        for (int i = 0; i < theStorage.storage.Count; i++)
        {

            // Buat salinan lokal lagi untuk loop ini.
            ItemData currentStorageData = theStorage.storage[i];
            Item currentStorageSO = ItemPool.Instance.GetItemWithQuality(currentStorageData.itemName, currentStorageData.quality);

            if (currentStorageSO == null) continue;

            Transform itemInInventory = Instantiate(itemSlotTemplate, StorageContainer);
            itemInInventory.name = currentStorageSO.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = currentStorageSO.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = currentStorageData.count.ToString();

            // Listener sekarang menggunakan salinan lokal 'currentStorageData'.
            itemInInventory.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnStorageItemClick(currentStorageData);
            });
        }

        //MechanicController.Instance.HandleRefreshInventoryUI();
    }

    public void OnStorageItemClick(ItemData data)
    {
        // Sekarang Anda bekerja dengan ItemData yang benar
        PopUpStoreOrTakeItems(data, true);
    }

    public void OnInventoryItemClick(ItemData data)
    {
        PopUpStoreOrTakeItems(data, false);
    }


    private void PopUpStoreOrTakeItems(ItemData item, bool takingFromStorage)
    {
        ItemData itemData = item as ItemData;
        popUp.gameObject.SetActive(true);
        selectedItem = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality); ;
        isTakingFromStorage = takingFromStorage;

        selectedItemCount = 1; // Default ke 1
        if (isTakingFromStorage) // Jika mengambil dari storage
        {
            selectedItemCount = Mathf.Min(itemData.count, selectedItemCount);
        }

        // Tampilkan gambar dan jumlah item
        itemImage.sprite = selectedItem.sprite;
        itemCount.text = selectedItemCount.ToString();

        // Reset event listener
        minItem.onClick.RemoveAllListeners();
        plusItem.onClick.RemoveAllListeners();
        maxItem.onClick.RemoveAllListeners();
        confirm.onClick.RemoveAllListeners();
        cancel.onClick.RemoveAllListeners();

        // Tambahkan fungsi ke tombol UI
        // BENAR: Ini memberikan "resep" atau "perintah" baru
        plusItem.onClick.AddListener(() => IncreaseItemCount(item.count));
        minusItem.onClick.AddListener(DecreaseItemCount);
        maxItem.onClick.AddListener(MaximizeItemCount);
        minItem.onClick.AddListener(MinimizeItemCount);
        cancel.onClick.AddListener(() =>
        {
            popUp.gameObject.SetActive(false);
        });

        // Pilih fungsi konfirmasi berdasarkan mode operasi
        if (isTakingFromStorage)
        {
            confirm.onClick.AddListener(()=>
            {
                ConfirmTakeFromStorage(itemData);
            });

        }
        else
        {
            confirm.onClick.AddListener(() =>
            {
                ConfirmStoreToStorage(itemData);
            });
        }
    }

    public void ConfirmStoreToStorage(ItemData itemData)
    {
        MechanicController.Instance.MoveItem(
         stats.inventory,                     // List Asal
         theStorage.storage, // List Tujuan
         itemData,                               // Item yang dipilih
         selectedItemCount                                  // Jumlah yang ingin dipindah
     );
    }

    public void ConfirmTakeFromStorage(ItemData itemData)
    {
        MechanicController.Instance.MoveItem(
         theStorage.storage,                     // List Asal
         PlayerController.Instance.playerData.inventory, // List Tujuan
         itemData,                               // Item yang dipilih
         selectedItemCount                                  // Jumlah yang ingin dipindah
     );
    }


    private void IncreaseItemCount(int stackCount)
    {
        if (selectedItemCount < stackCount) // Tidak boleh lebih dari jumlah item yang tersedia
        {
            selectedItemCount++;
            itemCount.text = selectedItemCount.ToString();
        }
    }

    private void DecreaseItemCount()
    {
        if (selectedItemCount > 1) // Tidak boleh kurang dari 1
        {
            selectedItemCount--;
            itemCount.text = selectedItemCount.ToString();
        }
    }

    // Maksimalkan jumlah item yang bisa dipilih
    private void MaximizeItemCount()
    {
        //selectedItemCount = selectedItem.stackCount;
        itemCount.text = selectedItemCount.ToString();
    }

    // Kembalikan jumlah item ke 1
    private void MinimizeItemCount()
    {
        selectedItemCount = 1;
        itemCount.text = selectedItemCount.ToString();
    }



    public void ClosePopUp()
    {
        popUp.gameObject.SetActive( false );
    }

    






    private void ChangeItemOpacity(Transform itemSlot, float opacity)
    {
        // Mendapatkan komponen Image dari item
        Image itemImage = itemSlot.GetChild(0).GetComponent<Image>();

        // Mengubah nilai alpha dari warna untuk mengubah opacity
        Color newColor = itemImage.color;
        newColor.a = opacity; // Nilai opacity (0 = transparan, 1 = opaque)
        itemImage.color = newColor;
    }


}
