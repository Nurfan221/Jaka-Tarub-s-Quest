using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class MechanicController : MonoBehaviour
{
    public static MechanicController Instance { get; private set; }

    private StorageUI _storageUI;
    private InventoryUI _inventoryUI;
    public CraftUI _craftingUI;
    private ShopUI _shopUI;
    // --- Properti "Pintar" yang Bisa Mencari Sendiri ---
    public StorageUI StorageUI
    {
        get
        {
            // Jika kita belum pernah mencari StorageUI...
            if (_storageUI == null)
            {
                // ...cari di seluruh scene, TERMASUK yang tidak aktif. Ini kuncinya!
                _storageUI = FindObjectOfType<StorageUI>(true);
                if (_storageUI == null)
                {
                    Debug.LogError("MechanicController tidak bisa menemukan [StorageUI] di scene!");
                }
            }
            return _storageUI;
        }
    }

    public ShopUI ShopUI
    {
        get
        {
            // Lakukan hal yang sama untuk ShopUI
            if (_shopUI == null)
            {
                _shopUI = FindObjectOfType<ShopUI>(true);
                if (_shopUI == null)
                {
                    Debug.LogError("MechanicController tidak bisa menemukan [ShopUI] di scene!");
                }
            }
            return _shopUI;
        }
    }
    public InventoryUI InventoryUI
    {
        get
        {
            // Lakukan hal yang sama untuk InventoryUI
            if (_inventoryUI == null)
            {
                _inventoryUI = FindObjectOfType<InventoryUI>(true);
                if (_inventoryUI == null)
                {
                    Debug.LogError("MechanicController tidak bisa menemukan [InventoryUI] di scene!");
                }
            }
            return _inventoryUI;
        }
    }

   
    public CraftUI CraftingUI
    {
        get
        {
            // Lakukan hal yang sama untuk CraftingUI
            if (_craftingUI == null)
            {
                _craftingUI = FindObjectOfType<CraftUI>(true);
                if (_craftingUI == null)
                {
                    Debug.LogError("MechanicController tidak bisa menemukan [Craft] di scene!");
                }
            }
            return _craftingUI;
        }
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
            DontDestroyOnLoad(this.gameObject);
        }
    }



    public void MoveItem(List<ItemData> sourceList, List<ItemData> targetList, ItemData itemToMove, int amountToMove)
    {
        // Dapatkan data template dari database
        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToMove.itemName, itemToMove.quality); ; ;
        if (itemTemplate == null || amountToMove <= 0) return;

        // Pastikan kita tidak memindahkan lebih dari yang kita miliki
        amountToMove = Mathf.Min(amountToMove, itemToMove.count);

        int amountSuccessfullyMoved = 0;


        // Coba tumpuk di slot yang sudah ada di list tujuan
        if (itemTemplate.isStackable)
        {
            foreach (ItemData slot in targetList)
            {
                if (slot.itemName == itemTemplate.itemName && slot.count < itemTemplate.maxStackCount)
                {
                    int availableSpace = itemTemplate.maxStackCount - slot.count;
                    int amountToAdd = Mathf.Min(availableSpace, amountToMove - amountSuccessfullyMoved);

                    slot.count += amountToAdd;
                    amountSuccessfullyMoved += amountToAdd;

                    if (amountSuccessfullyMoved >= amountToMove) break;
                }
            }
        }

        // Buat slot baru di list tujuan jika masih ada sisa
        int remainingToAdd = amountToMove - amountSuccessfullyMoved;
        int maxSlots = 24; // Anda perlu cara untuk mendapatkan maxSlots dari targetList

        while (remainingToAdd > 0 && targetList.Count < maxSlots)
        {
            int amountForNewSlot = Mathf.Min(remainingToAdd, itemTemplate.maxStackCount);
            ItemData newSlot = new ItemData(itemTemplate.itemName, amountForNewSlot, itemToMove.quality, itemToMove.itemHealth);
            targetList.Add(newSlot);

            amountSuccessfullyMoved += amountForNewSlot;
            remainingToAdd -= amountForNewSlot;
        }


        if (amountSuccessfullyMoved > 0)
        {
            // Kurangi jumlah dari slot asal
            itemToMove.count -= amountSuccessfullyMoved;

            // Jika jumlahnya menjadi 0 atau kurang, hapus item dari list asal
            if (itemToMove.count <= 0)
            {
                sourceList.Remove(itemToMove);
            }
        }

        MechanicController.Instance.HandleUpdateInventory();
        //StorageUI.ClosePopUp();
        // --- FASE 3: REFRESH UI ---
        // Panggil fungsi refresh UI Anda di sini setelah semua operasi selesai
        // Contoh:
        // StorageUI.Instance.RefreshUI();
        // PlayerInventory.Instance.OnInventoryUpdated?.Invoke();
    }

    public void HandleOpenInventory()
    {
        //GameController.Instance.PindahKeScene("Village");
        Debug.Log("buka inventory");
        if (InventoryUI !=null)
        {
            InventoryUI.OpenInventory();
        }else
        {
            Debug.Log("inventory ui kosong bro");
        }
    }

    public void HandleCloseInventory()
    {
        InventoryUI.CloseInventory();
    }
    public void HandleOpenStorage(StorageInteractable storage)
    {
        Debug.Log("membuka storage");
        //GameController.Instance.PindahKeScene("Village");
        QuestManager.Instance.DeleteSaveData();
        StorageUI.OpenStorage(storage);
    }

    public void HandleUpdateInventory()
    {
        InventoryUI.SetInventory();
    }

    public void HandleOpenCrafting()
    {
        CraftingUI.OpenUI();
    }

    public void HandleSwapItems (int sourceIndex, int destinationIndex)
    {
        InventoryUI.SwapItems(sourceIndex, destinationIndex);
    }

    public void HandleInitiateDrag(ItemInteraction itemToDrag)
    {
        // Logika untuk memulai drag item
        // Misalnya, Anda bisa mengaktifkan mode drag pada InventoryUI
        InventoryUI.InitiateDrag(itemToDrag);
    }

    public void HandleSetDescription(ItemData itemData)
    {
        // Logika untuk mengatur deskripsi item
        // Misalnya, Anda bisa mengupdate UI deskripsi di InventoryUI
        InventoryUI.SetDescription(itemData);
    }

    public void HandleCancelDrag()
    {
        // Logika untuk membatalkan drag item
        // Misalnya, Anda bisa menonaktifkan mode drag pada InventoryUI
        InventoryUI.CancelDrag();
    }

    public ItemInteraction HandleGetHeldItem()
    {
        ItemInteraction heldItem = InventoryUI.GetHeldItem();
        return heldItem;
    }

    public void HandleSuccessfulDrop(int targetIndex)
    {
        // Logika untuk menangani drop item yang berhasil
        // Misalnya, Anda bisa memperbarui UI atau menyimpan perubahan ke database
        InventoryUI.SuccessfulDrop(targetIndex);
    }
    public void HandleUpdateDragPosition(Vector2 position)
    {
        // Logika untuk memperbarui posisi item yang sedang di-drag
        // Misalnya, Anda bisa mengupdate posisi item di InventoryUI
        InventoryUI.UpdateDragPosition(position);
    }

    public void HandleDropItemFromInventory(int itemIndex, int quantityToRemove)
    {
        Debug.Log("HandleDropItemFromInventory called with itemIndex: " + itemIndex + " and quantityToRemove: " + quantityToRemove);
        InventoryUI.DropItemFromInventory(itemIndex, quantityToRemove);
    }

    public void HandleUpdateMenuInventory(int targetIndex)
    {
        InventoryUI.ChangeMenu(targetIndex);
    }

    public void HandleOpenShop(TypeShop typeShop, List<ItemData> itemsForSale, List<ItemData> itemSell, ShopInteractable shopInteractable)
    {
        Debug.Log("membuka shop");
        //GameController.Instance.PindahKeScene("Village");
        ShopUI.OpenShop(typeShop, itemsForSale, itemSell, shopInteractable );
    }
}
