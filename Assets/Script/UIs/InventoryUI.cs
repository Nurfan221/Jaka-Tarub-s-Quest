using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventoryUI : MonoBehaviour
{
    [Header("Daftar hubungan Script")]
    [SerializeField] NPCListUI npcListUI;
    public bool isInventoryOpen;



    [Header("Active Slot")]
    public Transform equippedItem1;
    public Transform equippedItem2;
    public Transform quickSlot1;
    public TMP_Text jumlahQuickItem1;
    public Transform quickSlot2;
    public TMP_Text jumlahQuickItem2;

    [Header("UI STUFF")]
    public Transform ContentGO;
    public Transform SlotTemplate;

    [Header("Button")]
    public Button openCraft;
    public Button openInventory;
    public Button btnHapus;

    [Header("Item Description")]
    public GameObject frontSide; // Drag the front side GameObject here
    public GameObject backSide;  // Drag the back side GameObject here
    public bool Description = false;
    public Image itemSprite;
    public TMP_Text itemName;
    public TMP_Text itemDesc;
    public Button itemAction;

    [Header("Six Item Display")]
    public Transform ContentGO6; // New UI Content for 6 items
    public Transform SlotTemplate6; // New UI Slot Template for 6 items



    [System.Serializable]
    public class MenuPanel
    {
        public string menuName;
        public GameObject panelInventory;
        public GameObject panelMenu;
    }

    [Header("UI Elements")]
    public MenuPanel[] menuPanels;
    public Button[] btnMenu;

    // [Header("item active in display")]
    // [SerializeField] private Image attackHUDImage;



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
    private void Start()
    {
        PlayerUI.Instance.RegisterInventoryUI(this);
        MechanicController.Instance.RegisterInventory(this);

        //Loop untuk Mengatur Tombol Menu Secara Dinamis
        for (int i = 0; i < btnMenu.Length; i++)
        {
            int index = i;
            btnMenu[i].onClick.RemoveAllListeners();
            btnMenu[i].onClick.AddListener(() => ChangeMenu(index));
        }

        //impan Tombol dalam Array untuk Menghindari Pengulangan Kode
        Button[] equippedButtons = {
        equippedItem1.GetComponent<Button>(),
        equippedItem2.GetComponent<Button>()
        };

        Button[] quickSlotButtons = {
        quickSlot1.GetComponent<Button>(),
        quickSlot2.GetComponent<Button>()
        };

        //Loop untuk Menetapkan EventListener pada Equipped Item
        for (int i = 0; i < equippedButtons.Length; i++)
        {
            int index = i;
            equippedButtons[i].onClick.RemoveAllListeners();
            equippedButtons[i].onClick.AddListener(() => ShowDeleteButton(() => RisetEquippedUse(index)));
        }

        //Loop untuk Menetapkan EventListener pada Quick Slots
        for (int i = 0; i < quickSlotButtons.Length; i++)
        {
            int index = i;
            quickSlotButtons[i].onClick.RemoveAllListeners();
            quickSlotButtons[i].onClick.AddListener(() => ShowDeleteButton(() => RisetQuickSlot(index)));
        }

        CloseInventory();
    }

    private void OnDestroy()
    {
        if (MechanicController.Instance != null)
        {
            MechanicController.Instance.UnregisterInventory(this);
        }
    }

    public void OpenInventory()
    {
        Debug.Log("membuka inventory");
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        isInventoryOpen = true;
        IfClose();


        if (isInventoryOpen)
        {
            GameController.Instance.PauseGame();
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("Padi"));






            UpdateInventoryUI(); // Update UI when inventory is opened
        }
        else
        {
            GameController.Instance.ResumeGame();
        }
    }

    public void CloseInventory()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        isInventoryOpen = false;

        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
    }

    // private void update()
    // {
    //     UpdateSixItemDisplay();
    // }

    private void OnEnable()
    {
        // Pastikan referensi ke ContohFlipCard diatur saat InventoryUI diaktifkan


    }

    // Handle equipped items
    public void SetActiveItem(int slot, ItemData item)
    {
        Item itemUse = ItemPool.Instance.GetItemWithQuality(item.itemName, item.quality);
        Transform pickedSlot;
        switch (slot)
        {
            case 0: pickedSlot = equippedItem1; break;
            case 1: pickedSlot = equippedItem2; break;
            case 2:
                pickedSlot = quickSlot1;
                //jumlahQuickItem1.text = item.stackCount.ToString(); 
                jumlahQuickItem1.gameObject.SetActive(true);
                break;
            case 3:
                pickedSlot = quickSlot2;
                //jumlahQuickItem2.text = item.stackCount.ToString();
                jumlahQuickItem2.gameObject.SetActive(true);
                break;
            default: pickedSlot = equippedItem1; break;
        }

        pickedSlot.gameObject.GetComponentInChildren<Image>().sprite = itemUse.sprite;


        pickedSlot.gameObject.SetActive(true);
        //pickedSlot.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();

    }

    public void UpdateInventoryUI()
    {
        SetInventory();
        if (stats.inventory.Count > 0)
        {
            SetDescription(stats.inventory[0]);
        }
        else
        {
            SetDescription(stats.emptyItemTemplate);
        }
        Debug.Log("Inventory has running");
    }

    void RefreshActiveItems()
    {
        // Refresh equipped items
        RefreshItemSlot(equippedItem1, stats.equippedItemData[0]);
        RefreshItemSlot(equippedItem2, stats.equippedItemData[1]);

        // Refresh quick slots
        RefreshItemSlot(quickSlot1, stats.itemUseData[0]);
        RefreshItemSlot(quickSlot2, stats.itemUseData[1]);
    }

    void RefreshItemSlot(Transform slot, ItemData item)
    {

        Item itemuse = ItemPool.Instance.GetItemWithQuality(item.itemName, item.quality);
        if (itemuse.sprite != null)
        {
            Debug.Log("item equipped inventory di refresh");
            slot.gameObject.GetComponentInChildren<Image>().sprite = itemuse.sprite;
            slot.gameObject.SetActive(true);
        }

        //slot.GetChild(1).GetComponent<TMP_Text>().text = item.count <= 0 ? "" : item.count.ToString();
    }

    public void SetInventory()
    {

        // Jika item kosong, tidak perlu lanjutkan refresh
        if (stats.inventory == null || stats.inventory.Count == 0)
        {
            Debug.Log("No items to display in the inventory");
            UpdateSixItemDisplay();  // Tetap update untuk bersihkan display jika kosong
            return;
        }

        RefreshInventoryItems();
        UpdateSixItemDisplay();
    }

    public void RefreshInventoryItems()
    {
        Debug.Log("item inventory di refresh");
        //error
        RefreshActiveItems();
        foreach (Transform child in ContentGO)
        {
            if (child == SlotTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < stats.inventory.Count; i++)
        {
            ItemData currentItemData = stats.inventory[i];
            Item item = ItemPool.Instance.GetItemWithQuality(stats.inventory[i].itemName, stats.inventory[i].quality);
            Transform itemInInventory = Instantiate(SlotTemplate, ContentGO);
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.gameObject.name = item.itemName;

            // Set sprite dan stack count
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = stats.inventory[i].count.ToString();

            // Mengatur itemID berdasarkan indeks
            ItemDragandDrop itemDragAndDrop = itemInInventory.GetComponent<ItemDragandDrop>();
            if (itemDragAndDrop != null)
            {
                itemDragAndDrop.itemID = i; // Set itemID dengan indeks item
            }

            // Menambahkan listener untuk deskripsi item
            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(currentItemData));
        }
    }

    public void ShowDescription()
    {
        Debug.Log("ShowDescription method called.");

        if (Description == false)
        {
            frontSide.SetActive(false);
            backSide.SetActive(true);
            Description = true;

        }
        else
        {
            frontSide.SetActive(true);
            backSide.SetActive(false);
            Description = false;
        }

    }
    public void IfClose()
    {
        frontSide.SetActive(true);
        backSide.SetActive(false);
        Description = false;
    }

    public void UpdateSixItemDisplay()
    {

        // Clear existing items in the 6-item display
        foreach (Transform child in ContentGO6)
        {
            if (child == SlotTemplate6) continue;
            Destroy(child.gameObject);
        }

        // Cek apakah item kosong atau tidak
        if (stats.inventory == null || stats.inventory.Count == 0)
        {
            // Debug.Log("No items to display");
            return;
        }

        if (stats.inventory.Count == 0)
        {
            return;
        }
        else
        {
            int itemCount = Mathf.Min(6, stats.inventory.Count);
            for (int i = 0; i < itemCount; i++)
            {
                Item item = ItemPool.Instance.GetItemWithQuality(stats.inventory[i].itemName, stats.inventory[i].quality);
                if (item == null) continue; // Jika item null, skip

                Transform itemInDisplay = Instantiate(SlotTemplate6, ContentGO6);
                itemInDisplay.gameObject.SetActive(true);
                itemInDisplay.gameObject.name = item.itemName;

                // Cek jika sprite item tidak null
                if (item.sprite != null)
                {
                    itemInDisplay.GetChild(0).GetComponent<Image>().sprite = item.sprite;
                }

                // Cek jika stackCount tidak null
                //itemInDisplay.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();


            }
        }
    }

    public void SetDescription(ItemData item)
    {
        ShowDescription();
        Item getItem = ItemPool.Instance.GetItemWithQuality(item.itemName, item.quality);
        // Set item's texts
        itemSprite.sprite = getItem.sprite;
        itemName.text = getItem.itemName;
        itemDesc.text = getItem.itemDescription;

        // Set the "equip" button functionality
        itemAction.onClick.RemoveAllListeners();
        itemAction.onClick.AddListener(() =>
        {
            Debug.Log("itemAction clicked");
            ShowDescription();
        });

        switch (getItem.types)
        {
            case ItemType.Melee_Combat:
                itemAction.onClick.AddListener(() =>
                {
                    PlayerController.Instance.HandleEquipItem(item);
                    // SoundManager.Instance.PlaySound("PickUp");
                });
                break;

            case ItemType.Heal:
                itemAction.onClick.AddListener(() =>
                {
                    //Player_Inventory.Instance.AddQuickSlot(item, 0);
                    // SoundManager.Instance.PlaySound("PickUp");
                });
                break;
            case ItemType.Buff:
                itemAction.onClick.AddListener(() =>
                {
                    //Player_Inventory.Instance.AddQuickSlot(item, 1);
                    // SoundManager.Instance.PlaySound("PickUp");
                });
                break;
            default:
                Debug.Log("item tidak sesuai");
                break;
        }

        // Set the "Equip" button according to item's type
        string itemUses;
        if (getItem.types == ItemType.Item)
        {
            itemUses = "CAN'T EQUIP";
            itemAction.interactable = false;
        }
        else
        {
            itemUses = "EQUIP";
            itemAction.interactable = true;
        }
        itemAction.GetComponentInChildren<TMP_Text>().text = itemUses;
    }

    public void ChangeMenu(int menu)
    {
        for (int i = 0; i < menuPanels.Length; i++)
        {
            bool isActive = (i == menu);
            menuPanels[i].panelInventory.SetActive(isActive);
            menuPanels[i].panelMenu.SetActive(isActive);

            string nameMenuPanel = menuPanels[i].panelInventory.name;
            if (nameMenuPanel != null && nameMenuPanel == "NPCList")
            {
                npcListUI.RefreshNPCList();
            }
        }

        Debug.Log($"Menu {menu} aktif");
    }

    //Fungsi untuk Menampilkan Tombol Hapus dan Mengatur Listener
    private void ShowDeleteButton(System.Action resetAction)
    {
        btnHapus.gameObject.SetActive(true);
        btnHapus.onClick.RemoveAllListeners();
        btnHapus.onClick.AddListener(() => {
            resetAction();
            btnHapus.gameObject.SetActive(false); // Sembunyikan tombol setelah reset
        });
    }

    //Fungsi untuk Mereset Equipped Use
    public void RisetEquippedUse(int index)
    {
        Debug.Log("Equipped item di-reset: " + index);
        stats.inventory.Add(stats.equippedItemData[index]);
        //player_Inventory.equippedCombat[index] = stats.emptyItem;

        Image itemImage = (index == 0) ?
            equippedItem1.GetComponentInChildren<Image>() :
            equippedItem2.GetComponentInChildren<Image>();

        itemImage.sprite = null; // Hapus sprite
        itemImage.gameObject.SetActive(false);
        RefreshInventoryItems();
        UpdateSixItemDisplay();
    }

    //Fungsi untuk Mereset Quick Slot
    public void RisetQuickSlot(int index)
    {
        Debug.Log("Quick Slot di-reset: " + index);
        stats.inventory.Add(stats.itemUseData[index]);
        stats.quickSlots[index] = stats.emptyItem;

        Image itemImage = (index == 0) ?
            quickSlot1.GetComponentInChildren<Image>() :
            quickSlot2.GetComponentInChildren<Image>();

        itemImage.sprite = null; // Hapus sprite
        if (index == 0)
        {
            jumlahQuickItem1.gameObject.SetActive(false);
        }
        else
        {
            jumlahQuickItem2.gameObject.SetActive(false);
        }
        itemImage.gameObject.SetActive(false);
        RefreshInventoryItems();
        UpdateSixItemDisplay();
    }

}