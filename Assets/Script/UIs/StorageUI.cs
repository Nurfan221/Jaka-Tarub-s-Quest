using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : MonoBehaviour
{
    /// <summary>
    /// Store, take, storage limit
    /// </summary>


    private Transform lastClickedItem = null; // Menyimpan item yang terakhir kali diklik

    StorageInteractable theStorage;
    List<Item> Items = new();

    private int maxItem = 11;

    [Header("Slots")]
    [SerializeField] Transform StorageContainer;
    [SerializeField] Transform InventoryContainer;
    [SerializeField] Transform itemSlotTemplate;

    [SerializeField] InventoryUI inventoryUI;

    [Header("Button Action")]
    
    // [SerializeField] Button itemAction;
    public Button storeButton;
    public Button store1stack;
    public Button storeAllItems;
    public Button takeButton;
    public Button  takeAllButton;      
    // [SerializeField] Button take;


    [Header("Button")]
    public Button closeStorageButton;

    // Need to refresh both inventory and storage slots
    private void Start()
    {
        if (closeStorageButton != null)
        {
            closeStorageButton.onClick.AddListener(CloseStorage);
            Debug.Log("tombol close listener added.");
        }else
        {
            Debug.Log("tombol close belum terhubung");
        }

       

       

        
       
    }

    private void Update()
    {
        // Close
       
    }

    public void OpenStorage(StorageInteractable theStorage, List<Item> Items)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

        this.theStorage = theStorage;
        this.Items = new();
        foreach (Item item in Items)
        {
            this.Items.Add(item);
        }
        RefreshInventoryItems();
        storeButton.gameObject.SetActive(false);
        store1stack.gameObject.SetActive(false);
        takeButton.gameObject.SetActive(false);
        takeAllButton.gameObject.SetActive(false);
    }

    private void CloseStorage()
    {
       
        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        theStorage.Items = Items;
             
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

            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(item, true));
            storeAllItems.onClick.RemoveAllListeners();
            storeAllItems.onClick.AddListener(()=> StoreAllItems());
            storeAllItems.onClick.AddListener(() => RefreshInventoryItems());
            inventoryUI.UpdateSixItemDisplay();

            // Menambahkan logika toggle opacity
            itemInInventory.GetComponent<Button>().onClick.AddListener(() =>
            {
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
                        storeButton.gameObject.SetActive(false);
                        store1stack.gameObject.SetActive(false);
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
                        storeButton.gameObject.SetActive(false);
                        store1stack.gameObject.SetActive(false);
                    }

                    // Ubah opacity item yang baru diklik
                    ChangeItemOpacity(itemInInventory, 0.5f);  // Buat lebih transparan item yang diklik
                    // Simpan referensi ke item yang terakhir kali diklik
                    storeButton.gameObject.SetActive(true);
                    store1stack.gameObject.SetActive(true);
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

            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(item, false));

            // Menambahkan logika toggle opacity
            itemInInventory.GetComponent<Button>().onClick.AddListener(() =>
            {
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
                        takeButton.gameObject.SetActive(false);
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
                        takeButton.gameObject.SetActive(false);
                        takeAllButton.gameObject.SetActive(false);
                    }

                    // Ubah opacity item yang baru diklik
                    ChangeItemOpacity(itemInInventory, 0.5f);  // Buat lebih transparan item yang diklik
                    takeButton.gameObject.SetActive(true);
                    takeAllButton.gameObject.SetActive(true);
                    // Simpan referensi ke item yang terakhir kali diklik
                    lastClickedItem = itemInInventory;

                    
                }
            });
            
        }

        
    }


    public void SetDescription(Item item, bool storeOrTake)
    {
        // Set the button functionality
        storeButton.onClick.RemoveAllListeners();
        store1stack.onClick.RemoveAllListeners();
        takeButton.onClick.RemoveAllListeners();
        store1stack.onClick.RemoveAllListeners();
      


       
        
        if (storeOrTake)
        {
            storeButton.onClick.AddListener(() => StoreItem(item));
            store1stack.onClick.AddListener(() => Store1stack(item));
            
        }
        else
        {
            takeButton.onClick.AddListener(() => TakeItem(item));

            takeAllButton.onClick.AddListener(() => TakeAllItems(item));
        }
        storeButton.onClick.AddListener(() => RefreshInventoryItems());
        store1stack.onClick.AddListener(() => RefreshInventoryItems());
        takeButton.onClick.AddListener(() => RefreshInventoryItems());
        takeAllButton.onClick.AddListener(() => RefreshInventoryItems());

        // if (item.stackCount <= 1)
        // {
            
        //     if (storeOrTake)
        //         {
                    
        //             storeButton.onClick.AddListener(() => SetDescription(Items[0], false));
        //         }
        //     else
        //         takeButton.onClick.AddListener(() => SetDescription(Player_Inventory.Instance.itemList[0], true));
        // }

       

       
    }

  

//    public void SetDescription(Item item, bool storeOrTake)
//     {
//         // Set item's texts
//         itemSprite.sprite = item.sprite;
//         itemName.text = item.itemName;
//         itemDesc.text = item.itemDescription;

//         // Set the button functionality
//         itemAction.onClick.RemoveAllListeners();
//         if (storeOrTake)
//         {
//             itemAction.onClick.AddListener(() => StoreItem(item));
//             itemAction.GetComponentInChildren<TMP_Text>().text = "Store";
//         }
//         else
//         {
//             itemAction.onClick.AddListener(() => TakeItem(item));
//             itemAction.GetComponentInChildren<TMP_Text>().text = "Take";
//         }
//         itemAction.onClick.AddListener(() => RefreshInventoryItems());

//         if (item.stackCount <= 1)
//         {
//             if (storeOrTake)
//                 itemAction.onClick.AddListener(() => SetDescription(Items[0], false));
//             else
//                 itemAction.onClick.AddListener(() => SetDescription(Player_Inventory.Instance.itemList[0], true));
//         }
//     }

    void StoreItem(Item item)
    {
     Debug.Log("jumlah item = "+ item.stackCount);
     int nilaiCountAwal = item.stackCount;

        item = ItemPool.Instance.GetItem(item.itemName);

        

        // Add item to Storage
        if (Items.Count <= maxItem ||(item.isStackable && Items.Exists(x => x.itemName == item.itemName)) )
        {
            Debug.Log("jumlah item di dalam storage : " + Items.Count);
            if (item.isStackable && Items.Exists(x => x.itemName == item.itemName))
                {
                    Items.Find(x => x.itemName == item.itemName).stackCount++;
                      // Remove item from inventory
                     Player_Inventory.Instance.RemoveItem(item);
                }
                else
                {
                    item.stackCount = 1;
                    Items.Add(item);
                      // Remove item from inventory
                    Player_Inventory.Instance.RemoveItem(item);
                }
        }else
        {
            Debug.Log("item full");
        }
      

         RefreshInventoryItems();
         nilaiCountAwal--;
         Debug.Log("nilai count awal adalah " +nilaiCountAwal); 
        

        if (nilaiCountAwal < 1)
        {
            storeButton.gameObject.SetActive(false);
            store1stack.gameObject.SetActive(false);
        }else
        {
            storeButton.gameObject.SetActive(true);
            store1stack.gameObject.SetActive(true);
        }
        Debug.Log("jumlah item terakhir"+ item.stackCount);
    }

    public void Store1stack(Item item)
    {
        // Iterasi melalui semua item di inventory
        int stackCount = item.stackCount;
            
            // Jika item stackable dan jumlahnya lebih dari 0
            if (stackCount > 0)
            {
                // Mengulang sesuai jumlah stackCount item yang dimiliki
                for (int i = 0; i < stackCount; i++)
                {
                    StoreItem(item);  // Memanggil fungsi StoreItem untuk menyimpan item ke storage
                }
            }

        // Setelah semua item disimpan, segarkan tampilan inventory dan storage
        RefreshInventoryItems();
        Debug.Log("Semua item telah disimpan.");
    }

    public void StoreAllItems()
    {
        // Buat salinan dari itemList untuk iterasi
        var itemsToStore = new List<Item>(Player_Inventory.Instance.itemList);

        // Iterasi melalui semua item yang telah disalin
        foreach (var item in itemsToStore)
        {
            // Panggil StoreItem untuk setiap item
            Store1stack(item);
        }

        // Setelah semua item disimpan, segarkan tampilan inventory dan storage
        RefreshInventoryItems();
        Debug.Log("Semua item telah disimpan.");
    }








    void TakeItem(Item item)
    {
        if (item == null)
        {
            Debug.LogError("Item is null in TakeItem");
            return;
        }

        int nilaiCountAwal = item.stackCount;

        // Ambil item dari pool
        Item newItem = ItemPool.Instance.GetItem(item.itemName);
        if (newItem == null)
        {
            Debug.LogError("Item not found in ItemPool");
            return;
        }

        // Tambahkan ke inventory player
        Player_Inventory.Instance.AddItem(newItem);

        // Hapus dari storage
        Item storageItem = Items.Find(x => x.itemName == item.itemName);
        if (storageItem != null)
        {
            if (storageItem.isStackable)
            {
                storageItem.stackCount--;
                if (storageItem.stackCount <= 0)
                {
                    Items.Remove(storageItem);
                }
            }
            else
            {
                Items.Remove(storageItem);
            }
        }

        nilaiCountAwal--;
        Debug.Log("nilai count awal adalah " + nilaiCountAwal);

        takeButton.gameObject.SetActive(nilaiCountAwal > 0);
        takeAllButton.gameObject.SetActive(nilaiCountAwal > 0);
        
        Debug.Log("jumlah item terakhir " + (storageItem != null ? storageItem.stackCount : 0));
    }

    public void TakeAllItems(Item item)
    {
        if (item == null)
        {
            Debug.LogError("Item is null");
            return;
        }

        // Iterasi melalui semua item di inventory
        int stackCount = item.stackCount;

        // Jika item stackable dan jumlahnya lebih dari 0
        if (stackCount > 0)
        {
            // Mengulang sesuai jumlah stackCount item yang dimiliki
            for (int i = 0; i < stackCount; i++)
            {
                TakeItem(item);  // Memanggil fungsi Take untuk menyimpan item ke storage
            }
        }

        // Setelah semua item disimpan, segarkan tampilan inventory dan storage
        RefreshInventoryItems();
        Debug.Log("Semua item telah disimpan.");
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
