using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class MechanicController : MonoBehaviour
{
    public static MechanicController Instance { get; private set; }

    private StorageUI _storageUI;
    private InventoryUI _inventoryUI;

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
            ItemData newSlot = new ItemData(itemTemplate.itemName, amountForNewSlot, itemToMove.quality);
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

        StorageUI.RefreshInventoryItems();
        StorageUI.ClosePopUp();
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
        StorageUI.OpenStorage(storage);
    }

   public void HandleUpdateInventory()
    {
        InventoryUI.UpdateInventoryUI();
        InventoryUI.UpdateSixItemDisplay();
    }
}
