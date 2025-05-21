using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static QuestManager;
using static UnityEditor.Progress;

public class StorageUI : MonoBehaviour
{

    public static StorageUI Instance;

    /// <summary>
    /// Store, take, storage limit
    /// </summary>


    private Transform lastClickedItem = null; // Menyimpan item yang terakhir kali diklik

    public StorageInteractable theStorage;
    public List<Item> Items = new();
    



    [Header("Slots")]
    [SerializeField] Transform StorageContainer;
    [SerializeField] Transform InventoryContainer;
    [SerializeField] Transform itemSlotTemplate;
    //[SerializeField] StorageInteractable currentStorage;

    [SerializeField] InventoryUI inventoryUI;
    //popUP
    public Image popUp;
    public Image itemImage;
    public TextMeshProUGUI itemCount;
    // Variabel untuk menyimpan item yang sedang dipilih
    public Item selectedItem;
    public int selectedItemCount;
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



    private void Update()
    {
        // Close
       
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public void OpenStorage(StorageInteractable theStorage, List<Item> Items)
    {

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        gameObject.SetActive(true);

        // Start the animation coroutine
        theStorage = theStorage;

        this.theStorage = theStorage;
        this.Items = new();
        foreach (Item item in Items)
        {
            this.Items.Add(item);
        }

        RefreshInventoryItems();

        takeAllButton.gameObject.SetActive(false);

        storeAllButton.onClick.RemoveAllListeners();
        storeAllButton.onClick.AddListener(StoreAllItems);

        takeAllButton.onClick.RemoveAllListeners();
        takeAllButton.onClick.AddListener(TakeAllItems);

        if (Items.Count > 0)
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

        theStorage.Items = Items;  // Simpan item kembali ke storage
    }


    private void RefreshInventoryItems()
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
        foreach (Item item in Player_Inventory.Instance.itemList)
        {

            Transform itemInInventory = Instantiate(itemSlotTemplate, InventoryContainer);
            itemInInventory.name = item.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

           
            //storeAllItems.onClick.RemoveAllListeners();
            ////storeAllItems.onClick.AddListener(()=> StoreAllItems());
            //storeAllItems.onClick.AddListener(() => RefreshInventoryItems());


            // Menambahkan logika toggle opacity
            itemInInventory.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnInventoryItemClick(item);
                // Cek apakah item yang sama diklik
                Image itemImage = itemInInventory.GetChild(0).GetComponent<Image>();
                
                if (lastClickedItem == itemInInventory)
                {
                    // Jika item yang sama diklik dan opacity 0.5, kembalikan ke 1
                    if (itemImage.color.a == 0.5f)
                    {
                        ChangeItemOpacity(itemInInventory, 1f); // Kembalikan ke opaque
                        lastClickedItem = null; // Reset lastClickedItem
                        // isItemActive = false; // Reset status item

                    }
                    // storeButton.gameObject.SetActive(true);
                }
                else
                {
                    // Jika ada item lain yang sebelumnya diklik, ubah opacity-nya kembali ke normal
                    if (lastClickedItem != null)
                    {
                        ChangeItemOpacity(lastClickedItem, 1f);  // Kembalikan opacity item yang sebelumnya
                        // isItemActive = false; // Reset status item yang sebelumnya

                    }

                    // Ubah opacity item yang baru diklik
                    ChangeItemOpacity(itemInInventory, 0.5f);  // Buat lebih transparan item yang diklik
                    // Simpan referensi ke item yang terakhir kali diklik

                    lastClickedItem = itemInInventory;

                    
                }
            });
        }
        // Set storage
        foreach (Item item in Items)
        {
            Transform itemInInventory = Instantiate(itemSlotTemplate, StorageContainer);
            itemInInventory.name = item.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

         

            // Menambahkan logika toggle opacity
            itemInInventory.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnStorageItemClick(item);
                // Cek apakah item yang sama diklik
                Image itemImage = itemInInventory.GetChild(0).GetComponent<Image>();
                
                if (lastClickedItem == itemInInventory)
                {
                    // Jika item yang sama diklik dan opacity 0.5, kembalikan ke 1
                    if (itemImage.color.a == 0.5f)
                    {
                        ChangeItemOpacity(itemInInventory, 1f); // Kembalikan ke opaque
                        lastClickedItem = null; // Reset lastClickedItem
                        // isItemActive = false; // Reset status item

                        takeAllButton.gameObject.SetActive(false);
                    }
                    // takeButton.gameObject.SetActive(true);
                }
                else
                {
                    // Jika ada item lain yang sebelumnya diklik, ubah opacity-nya kembali ke normal
                    if (lastClickedItem != null)
                    {
                        ChangeItemOpacity(lastClickedItem, 1f);  // Kembalikan opacity item yang sebelumnya
                        // isItemActive = false; // Reset status item yang sebelumnya

                        takeAllButton.gameObject.SetActive(false);
                    }

                    // Ubah opacity item yang baru diklik
                    ChangeItemOpacity(itemInInventory, 0.5f);  // Buat lebih transparan item yang diklik
                    takeAllButton.gameObject.SetActive(true);
                    // Simpan referensi ke item yang terakhir kali diklik
                    lastClickedItem = itemInInventory;

                    
                }
            });
            
        }

        //inventoryUI.RefreshInventoryItems();
        inventoryUI.UpdateSixItemDisplay();

    }

    public void OnStorageItemClick(Item item)
    {
        PopUpStoreOrTakeItems(item, true); // Mode "Take Item"
    }

    public void OnInventoryItemClick(Item item)
    {
        PopUpStoreOrTakeItems(item, false); // Mode "Store Item"
    }

    private void PopUpStoreOrTakeItems(Item item, bool takingFromStorage)
    {
        popUp.gameObject.SetActive(true);
        selectedItem = item;
        isTakingFromStorage = takingFromStorage;

        selectedItemCount = 1; // Default ke 1
        if (isTakingFromStorage) // Jika mengambil dari storage
        {
            selectedItemCount = Mathf.Min(selectedItem.stackCount, selectedItemCount);
        }

        // Tampilkan gambar dan jumlah item
        itemImage.sprite = item.sprite;
        itemCount.text = selectedItemCount.ToString();

        // Reset event listener
        minItem.onClick.RemoveAllListeners();
        plusItem.onClick.RemoveAllListeners();
        maxItem.onClick.RemoveAllListeners();
        confirm.onClick.RemoveAllListeners();
        cancel.onClick.RemoveAllListeners();

        // Tambahkan fungsi ke tombol UI
        plusItem.onClick.AddListener(IncreaseItemCount);
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
            confirm.onClick.AddListener(ConfirmTakeFromStorage);
        }
        else
        {
            confirm.onClick.AddListener(ConfirmStoreToStorage);
        }
    }




    private void IncreaseItemCount()
    {
        if (selectedItemCount < selectedItem.stackCount) // Tidak boleh lebih dari jumlah item yang tersedia
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
        selectedItemCount = selectedItem.stackCount;
        itemCount.text = selectedItemCount.ToString();
    }

    // Kembalikan jumlah item ke 1
    private void MinimizeItemCount()
    {
        selectedItemCount = 1;
        itemCount.text = selectedItemCount.ToString();
    }



    private void ClosePopUp()
    {
        popUp.gameObject.SetActive( false );
    }

    private void ConfirmStoreToStorage()
    {
        takeAllButton.gameObject.SetActive(true);
        popUp.gameObject.SetActive(false);
        theStorage.Items = Items; // Simpan item kembali ke storage

        int remainingToStore = selectedItemCount; // Jumlah item yang ingin dipindahkan

        foreach (Item item in Items)
        {
            if (item.itemName == selectedItem.itemName) // Cek apakah item sudah ada di storage
            {
                Debug.Log("Ada item yang sama di storage");

                int availableSpace = item.maxStackCount - item.stackCount;

                if (availableSpace > 0)
                {
                    Debug.Log("Masih bisa ditambahkan ke stack");
                    int amountToAdd = Mathf.Min(availableSpace, remainingToStore);
                    item.stackCount += amountToAdd;
                    remainingToStore -= amountToAdd;

                    // Jika sudah penuh, ubah status isStackable ke false
                    item.isStackable = item.stackCount < item.maxStackCount;
                }

                if (remainingToStore <= 0)
                    break; // Jika sudah cukup dipindahkan, keluar dari loop
            }
        }

        // Jika masih ada sisa item yang belum disimpan, buat slot baru
        while (remainingToStore > 0 && Items.Count < theStorage.maxItem)
        {
            Item newItem = Instantiate(selectedItem);
            int amountToStore = Mathf.Min(remainingToStore, newItem.maxStackCount);
            newItem.stackCount = amountToStore;
            remainingToStore -= amountToStore;

            // Pastikan `isStackable` benar
            newItem.isStackable = newItem.stackCount < newItem.maxStackCount;

            Items.Add(newItem);
        }

        // Hapus item dari inventory setelah dipindahkan
        DeleteItemFromInventory();
        RefreshInventoryItems();
        
    }



    private void ConfirmTakeFromStorage()
    {
        popUp.gameObject.SetActive(false);
        int remainingToTake = selectedItemCount; // Jumlah item yang ingin diambil

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            if (item.itemName == selectedItem.itemName) // Cek apakah item sudah ada di inventory
            {
                int availableSpace = item.maxStackCount - item.stackCount;
                int amountToAdd = Mathf.Min(availableSpace, remainingToTake);

                item.stackCount += amountToAdd;
                remainingToTake -= amountToAdd;

                // Perbaiki `isStackable`
                item.isStackable = item.stackCount < item.maxStackCount;

                if (remainingToTake <= 0)
                    break;
            }
        }

        // Jika masih ada sisa item, buat slot baru di inventory
        while (remainingToTake > 0 && Player_Inventory.Instance.itemList.Count < Player_Inventory.Instance.maxItem)
        {
            Item newItem = Instantiate(selectedItem);
            int amountToTake = Mathf.Min(remainingToTake, newItem.maxStackCount);
            newItem.stackCount = amountToTake;
            remainingToTake -= amountToTake;

            newItem.isStackable = newItem.stackCount < newItem.maxStackCount;

            Player_Inventory.Instance.itemList.Add(newItem);
        }

        DeleteItemFromStorage();
        RefreshInventoryItems();
    }


    private void DeleteItemFromInventory()
    {
        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = Player_Inventory.Instance.itemList.Count - 1; i >= 0; i--)
        {
            Item item = Player_Inventory.Instance.itemList[i];

            if (selectedItem.itemName == item.itemName)
            {
                if (item.stackCount > remainingToRemove)
                {
                    item.stackCount -= remainingToRemove;
                    item.isStackable = item.stackCount < item.maxStackCount;
                    return;
                }
                else
                {
                    remainingToRemove -= item.stackCount;
                    Player_Inventory.Instance.itemList.RemoveAt(i);

                    if (remainingToRemove <= 0)
                        return;
                }
            }
        }
    }



    private void DeleteItemFromStorage()
    {
        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus
        List<Item> itemsToRemove = new List<Item>(); // Menyimpan item yang perlu dihapus

        for (int i = theStorage.Items.Count - 1; i >= 0; i--)
        {
            Item item = theStorage.Items[i];

            if (selectedItem.itemName == item.itemName)
            {
                if (item.stackCount > remainingToRemove)
                {
                    Debug.Log("stack count lebih besar dari jumlah yang ingin di hapus");
                    item.stackCount -= remainingToRemove;
                    item.isStackable = item.stackCount < item.maxStackCount;
                    return; // Menghentikan setelah mengurangi stackCount
                }
                else
                {
                    Debug.Log("hapus semua item");
                    remainingToRemove -= item.stackCount;
                    itemsToRemove.Add(item); // Tandai item untuk dihapus

                    if (remainingToRemove <= 0)
                        break; // Semua item sudah dihapus, keluar dari loop
                }
            }
        }

        // Hapus item setelah loop selesai
        foreach (Item item in itemsToRemove)
        {
            theStorage.Items.Remove(item); // Menghapus item yang ditandai
            Items.Remove(item);
            Debug.Log("Item dihapus dari storage: " + item.itemName);
        }
    }



    private void StoreAllItems()
    {
        Debug.Log("Memindahkan semua item dari inventory ke storage...");

        List<Item> itemsToRemove = new List<Item>(); // Menyimpan item yang akan dihapus dari inventory

        foreach (Item itemInInventory in Player_Inventory.Instance.itemList)
        {
            int remainingToStore = itemInInventory.stackCount;


            // Cek apakah item sudah ada di storage
            foreach (Item itemInStorage in Items)
            {
                if (itemInStorage.itemName == itemInInventory.itemName)
                {
                    int availableSpace = itemInStorage.maxStackCount - itemInStorage.stackCount;
                    int amountToAdd = Mathf.Min(availableSpace, remainingToStore);

                    itemInStorage.stackCount += amountToAdd;
                    remainingToStore -= amountToAdd;

                    itemInStorage.isStackable = itemInStorage.stackCount < itemInStorage.maxStackCount;

                    if (remainingToStore <= 0) break; // Semua item sudah dipindahkan
                }
            }

            // Jika masih ada sisa item, buat slot baru
            while (remainingToStore > 0 && Items.Count < theStorage.maxItem)
            {
                Item newItem = Instantiate(itemInInventory);
                int amountToStore = Mathf.Min(remainingToStore, newItem.maxStackCount);
                newItem.stackCount = amountToStore;
                remainingToStore -= amountToStore;

                newItem.isStackable = newItem.stackCount < newItem.maxStackCount;

                Items.Add(newItem);
            }

            // Tandai item untuk dihapus dari inventory
            itemsToRemove.Add(itemInInventory);
        }

        // Hapus semua item yang telah dipindahkan dari inventory
        foreach (Item item in itemsToRemove)
        {
            Player_Inventory.Instance.itemList.Remove(item);
        }

        // Simpan perubahan kembali ke storage
        theStorage.Items = Items;
        RefreshInventoryItems();
        takeAllButton.gameObject.SetActive(true);

        Debug.Log("Semua item telah dipindahkan ke storage!");
    }

    private void TakeAllItems()
    {
        Debug.Log("Mengambil semua item dari storage ke inventory...");

        List<Item> itemsToRemove = new List<Item>(); // Menyimpan item yang akan dihapus dari storage

        foreach (Item itemInStorage in Items)
        {
            int remainingToTake = itemInStorage.stackCount;

            // Cek apakah item sudah ada di inventory
            foreach (Item itemInInventory in Player_Inventory.Instance.itemList)
            {
                if (itemInInventory.itemName == itemInStorage.itemName)
                {
                    int availableSpace = itemInInventory.maxStackCount - itemInInventory.stackCount;
                    int amountToAdd = Mathf.Min(availableSpace, remainingToTake);

                    itemInInventory.stackCount += amountToAdd;
                    remainingToTake -= amountToAdd;

                    itemInInventory.isStackable = itemInInventory.stackCount < itemInInventory.maxStackCount;

                    if (remainingToTake <= 0) break; // Semua item sudah dipindahkan
                }
            }

            // Jika masih ada sisa item, buat slot baru di inventory
            while (remainingToTake > 0 && Player_Inventory.Instance.itemList.Count < Player_Inventory.Instance.maxItem)
            {
                Item newItem = Instantiate(itemInStorage);
                int amountToTake = Mathf.Min(remainingToTake, newItem.maxStackCount);
                newItem.stackCount = amountToTake;
                remainingToTake -= amountToTake;

                newItem.isStackable = newItem.stackCount < newItem.maxStackCount;

                Player_Inventory.Instance.itemList.Add(newItem);
            }

            // Tandai item untuk dihapus dari storage
            itemsToRemove.Add(itemInStorage);
        }

        // Hapus semua item yang telah dipindahkan dari storage
        foreach (Item item in itemsToRemove)
        {
            Items.Remove(item);
        }

        // Simpan perubahan kembali ke storage
        theStorage.Items = Items;
        RefreshInventoryItems();
        takeAllButton.gameObject.SetActive(false);

        Debug.Log("Semua item telah dipindahkan ke inventory!");
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
