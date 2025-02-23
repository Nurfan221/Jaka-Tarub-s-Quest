using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class StorageUI : MonoBehaviour
{

    public static StorageUI Instance;

    /// <summary>
    /// Store, take, storage limit
    /// </summary>


    private Transform lastClickedItem = null; // Menyimpan item yang terakhir kali diklik

    public StorageInteractable theStorage;
    List<Item> Items = new();



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
    private Item selectedItem;
    private int selectedItemCount;
    private bool isTakingFromStorage = false; // False = store ke storage, True = take dari storage


    [Header("Button Action")]
    
    // [SerializeField] Button itemAction;
    public Button storeAllItems;
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

        storeAllItems.onClick.RemoveAllListeners();
        storeAllItems.onClick.AddListener(StoreAllItems);
        Debug.Log("Listener storeAllItems ditambahkan");

        takeAllButton.onClick.RemoveAllListeners();
        takeAllButton.onClick.AddListener(TakeAllItems);
        Debug.Log("Listener takeAllButton ditambahkan");
    }

    private void CloseStorage()
    {
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


    void RefreshInventoryItems()
    {
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

           
            storeAllItems.onClick.RemoveAllListeners();
            //storeAllItems.onClick.AddListener(()=> StoreAllItems());
            storeAllItems.onClick.AddListener(() => RefreshInventoryItems());
            inventoryUI.UpdateSixItemDisplay();

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

        // Tambahkan fungsi ke tombol UI
        plusItem.onClick.AddListener(IncreaseItemCount);
        minusItem.onClick.AddListener(DecreaseItemCount);
        maxItem.onClick.AddListener(MaximizeItemCount);
        minItem.onClick.AddListener(MinimizeItemCount);

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
        Debug.Log("Minus Coy");
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
        theStorage.Items = Items;  // Simpan item kembali ke storage
        popUp.gameObject.SetActive(false);
        Debug.Log("Jumlah item sebelum dipindahkan = " + selectedItem.stackCount);

        bool itemExists = false;

        //Cek apakah item sudah ada di storage
        foreach (Item item in Items)
        {
            if (item.itemName == selectedItem.itemName) // Gunakan itemName untuk perbandingan
            {
                if (selectedItem.isStackable)
                {
                    item.stackCount += selectedItemCount;
                }
                else
                {
                    Item newItem = Instantiate(selectedItem);
                    newItem.stackCount = 1;
                    Items.Add(newItem);
                }
                itemExists = true;
                break;
            }
        }

        //Jika item belum ada di storage, tambahkan sebagai item baru
        if (!itemExists)
        {
            Item newItem = Instantiate(selectedItem); // Duplikasi item untuk storage
            newItem.stackCount = selectedItemCount;
            Items.Add(newItem);
        }

        //Hapus item dari inventory setelah dipastikan tersimpan di storage
        DeleteItemFromInventory();
        RefreshInventoryItems();

        Debug.Log("Jumlah item terakhir di storage: " + selectedItem.stackCount);
    }


    private void ConfirmTakeFromStorage()
    {
        popUp.gameObject.SetActive(false);
        Debug.Log($"Mengambil {selectedItemCount} {selectedItem.itemName} dari storage.");

        bool itemExistsInInventory = false;

        //Cek apakah item sudah ada di inventory
        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            if (item.itemName == selectedItem.itemName)
            {
                if (selectedItem.isStackable)
                {
                    item.stackCount += selectedItemCount;
                }
                else
                {
                    Item newItem = Instantiate(selectedItem);
                    newItem.stackCount = 1;
                    Player_Inventory.Instance.itemList.Add(newItem);
                }
                itemExistsInInventory = true;
                break;
            }
        }

        //Jika item belum ada di inventory, tambahkan sebagai item baru
        if (!itemExistsInInventory)
        {
            Item newItem = Instantiate(selectedItem); // Duplikasi item untuk inventory
            newItem.stackCount = selectedItem.isStackable ? selectedItemCount : 1;
            Player_Inventory.Instance.itemList.Add(newItem);
        }

        //Kurangi jumlah item di storage atau hapus jika habis
        DeleteItemFromStorage();
        RefreshInventoryItems();

        Debug.Log($"Item di inventory: {selectedItem.itemName}, Jumlah: {selectedItem.stackCount}");
    }





    private void DeleteItemFromInventory()
    {
        for (int i = Player_Inventory.Instance.itemList.Count - 1; i >= 0; i--)
        {
            Item item = Player_Inventory.Instance.itemList[i];

            if (selectedItem.itemName == item.itemName)
            {
                item.stackCount = Mathf.Max(0, item.stackCount - selectedItemCount);

                if (item.stackCount <= 0)
                {
                    Player_Inventory.Instance.itemList.RemoveAt(i); // Hapus item dari inventory jika habis
                }
                return;
            }
        }
    }

    private void DeleteItemFromStorage()
    {
        Debug.Log("Deleteitem from Storage di panggil");
        for (int i = 0; i < theStorage.Items.Count; i++)
        {
            Item item = theStorage.Items[i];

            if (selectedItem.itemName == item.itemName)
            {
                Debug.Log($"Sebelum dikurangi: {item.itemName} = {item.stackCount}");

                item.stackCount -= selectedItemCount;

                Debug.Log($"Setelah dikurangi: {item.itemName} = {item.stackCount}");

                if (item.stackCount <= 0)
                {
                    theStorage.Items.RemoveAt(i); // Hapus item dari storage jika habis
                    Debug.Log($"Item {item.itemName} dihapus dari storage.");
                }

                RefreshInventoryItems(); // Pastikan UI diperbarui
                return;
            }else
            {
                Debug.Log("itemname tidak sama ");
            }
        }
    }

    private void StoreAllItems()
    {
        Debug.Log("Memindahkan semua item dari storage ke inventory...");

        List<Item> itemsToRemove = new List<Item>(); // Menyimpan item yang akan dihapus dari storage

        foreach (Item itemInStorage in Items)
        {
            bool itemExistsInInventory = false;

            // Cek apakah item sudah ada di inventory
            foreach (Item itemInInventory in Player_Inventory.Instance.itemList)
            {
                if (itemInInventory.itemName == itemInStorage.itemName)
                {
                    if (itemInInventory.isStackable)
                    {
                        itemInInventory.stackCount += itemInStorage.stackCount; // Tambahkan jumlah item
                        Debug.Log($" {itemInStorage.itemName} bertambah di inventory: {itemInStorage.stackCount}");
                    }
                    else
                    {
                        Item newItem = Instantiate(itemInStorage);
                        newItem.stackCount = 1;
                        Player_Inventory.Instance.itemList.Add(newItem);
                        Debug.Log($" Item {itemInStorage.itemName} tidak bisa di-stack, menambahkan satu item baru.");
                    }

                    itemExistsInInventory = true;
                    break;
                }
            }

            // Jika item belum ada di inventory, tambahkan sebagai item baru
            if (!itemExistsInInventory)
            {
                Item newItem = Instantiate(itemInStorage);
                newItem.stackCount = itemInStorage.stackCount;
                Player_Inventory.Instance.itemList.Add(newItem);
                Debug.Log($" Item baru {newItem.itemName} ditambahkan ke inventory.");
            }

            // Tandai item untuk dihapus dari storage
            itemsToRemove.Add(itemInStorage);
        }

        // Hapus semua item yang telah dipindahkan dari storage
        foreach (Item item in itemsToRemove)
        {
            Debug.Log($"Menghapus {item.itemName} dari storage.");
            Items.Remove(item);
        }

        // Simpan perubahan kembali ke storage
        theStorage.Items = Items;
        Debug.Log("Semua item telah dipindahkan ke inventory!");

        // Perbarui UI
        RefreshInventoryItems();
    }



    private void TakeAllItems()
    {
        Debug.Log("Mengambil semua item dari storage ke inventory...");

        List<Item> itemsToRemove = new List<Item>(); // Menyimpan item yang akan dihapus dari storage

        foreach (Item itemInStorage in Items)
        {
            bool itemExistsInInventory = false;

            // Cek apakah item sudah ada di inventory
            foreach (Item itemInInventory in Player_Inventory.Instance.itemList)
            {
                if (itemInInventory.itemName == itemInStorage.itemName)
                {
                    if (itemInInventory.isStackable)
                    {
                        itemInInventory.stackCount += itemInStorage.stackCount; // Tambahkan jumlah item
                        Debug.Log($"Item {itemInStorage.itemName} bertambah di inventory: {itemInStorage.stackCount}");
                    }
                    else
                    {
                        Item newItem = Instantiate(itemInStorage);
                        newItem.stackCount = 1;
                        Player_Inventory.Instance.itemList.Add(newItem);
                    }

                    itemExistsInInventory = true;
                    break;
                }
            }

            // Jika item belum ada di inventory, tambahkan sebagai item baru
            if (!itemExistsInInventory)
            {
                Item newItem = Instantiate(itemInStorage);
                newItem.stackCount = itemInStorage.stackCount;
                Player_Inventory.Instance.itemList.Add(newItem);
                Debug.Log($"Item baru {newItem.itemName} ditambahkan ke inventory.");
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

        // Perbarui UI
        RefreshInventoryItems();


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
