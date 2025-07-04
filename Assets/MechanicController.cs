using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class MechanicController : MonoBehaviour
{
    public static MechanicController Instance { get; private set; }

    public StorageUI StorageUI {  get; private set; }
    public InventoryUI InventoryUI { get; private set; }

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterStorage(StorageUI storage)
    {
        this.StorageUI = storage;
        //Debug.Log($"StorageController: Paket Storage '{st.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterStorage(StorageUI storage)
    {
        if (this.StorageUI == storage)
        {
            this.StorageUI = null;
        }
    }
    public void RegisterInventory(InventoryUI inventory)
    {
        this.InventoryUI = inventory;
        //Debug.Log($"InventoryController: Paket Inventory '{st.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterInventory(InventoryUI inventory)
    {
        if (this.InventoryUI == inventory)
        {
            this.InventoryUI = null;
        }
    }

    public void HandleRefreshInventoryUI()
    {
        StorageUI.RefreshInventoryItems();
        InventoryUI.UpdateSixItemDisplay();
    }
    public void MoveItem(List<ItemData> sourceList, List<ItemData> targetList, ItemData itemToMove, int amountToMove)
    {
        // Dapatkan data template dari database
        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToMove.itemName, itemToMove.quality); ; ;
        if (itemTemplate == null || amountToMove <= 0) return;

        // Pastikan kita tidak memindahkan lebih dari yang kita miliki
        amountToMove = Mathf.Min(amountToMove, itemToMove.count);

        int amountSuccessfullyMoved = 0;

        // --- FASE 1: TAMBAHKAN ITEM KE LIST TUJUAN ---

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

        // --- FASE 2: KURANGI ITEM DARI LIST ASAL ---

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
}
