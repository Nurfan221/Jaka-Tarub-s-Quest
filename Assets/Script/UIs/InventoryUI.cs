using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Daftar hubungan Script")]
    public TutorialList tutorialList;
    [SerializeField] CraftInventoryUI craftInventoryUI;
    public SpriteImageTemplate spriteImageTemplate;
    public bool isInventoryOpen;
    [Header("Komponen untuk Interaksi Mobile")]
    public Image dragIcon; // Ikon yang mengikuti sentuhan
    public ItemInteraction heldItem; // Ganti state dari bool menjadi referensi skrip
    //public Item contohItem;



    [Header("Active Slot")]
    public Transform equippedItem1;
    public Transform equippedItem2;
    public Transform quickSlot1;
    public TMP_Text jumlahQuickItem1;
    public Transform quickSlot2;
    public TMP_Text jumlahQuickItem2;
    public Transform tongSampah;

    [Header("UI STUFF")]
    public Transform ContentGO;
    public Transform SlotTemplate;

    [Header("Button")]
  
    public Button btnHapus;
    public Button closeInventoryButton;  // Drag and drop the close button in the inspector


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



    private PlayerController stats;
    private PlayerData_SO playerData;


    private void Awake()
    {



        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
            playerData = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
        dragIcon.gameObject.SetActive(false); // Pastikan ikon nonaktif di awal
    }

    private void Start()
    {
        //PlayerUI.Instance.RegisterInventoryUI(this);


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

        if (closeInventoryButton != null)
        {
            closeInventoryButton.onClick.AddListener(MechanicController.Instance.HandleCloseInventory);
        }


        spriteImageTemplate = DatabaseManager.Instance.GetSpriteTempalte("HealthItemUI");

        CloseInventory();
        SetInventory();
    }


    private void Update()
    {

    }
    public void OpenInventory()
    {

        Debug.Log("membuka inventory");
        //if (SoundManager.Instance != null)
        //    SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        gameObject.SetActive(true);
        //IfClose();
        Description = false;
        SetInventory(); // Update UI when inventory is opened



    }

    public void CloseInventory()
    {
        if (SoundManager.Instance != null)
            //SoundManager.Instance.PlaySound("Click");

        isInventoryOpen = false;

        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
    }



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



    // Ganti nama fungsi utama agar lebih jelas
    void RefreshAllActiveSlots()
    {

        // Panggil fungsi spesialis untuk setiap slot equipment
        RefreshSingleSlot(equippedItem1, stats.equippedItemData.Count > 0 ? stats.equippedItemData[0] : null);
        RefreshSingleSlot(equippedItem2, stats.equippedItemData.Count > 1 ? stats.equippedItemData[1] : null);

        //Refresh Quick Slots 
        // Panggil fungsi spesialis yang berbeda untuk quick slot karena mereka punya teks jumlah
        RefreshQuickSlot(quickSlot1, jumlahQuickItem1, stats.itemUseData.Count > 0 ? stats.itemUseData[0] : null);
        RefreshQuickSlot(quickSlot2, jumlahQuickItem2, stats.itemUseData.Count > 1 ? stats.itemUseData[1] : null);
    }

    // Fungsi ini sekarang hanya untuk slot biasa (tanpa teks jumlah)
    void RefreshSingleSlot(Transform slotTransform, ItemData itemData)
    {
        // Cek keamanan jika slot atau data tidak ada
        if (slotTransform == null) return;
        if (itemData == null || itemData.itemName == "Empty")
        {
            slotTransform.gameObject.SetActive(false); // Sembunyikan slot jika kosong
            return;
        }
        Item itemSO = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
        if (itemSO == null)
        {
            slotTransform.gameObject.SetActive(false);
            return;
        }

        // Update sprite dan aktifkan slot
        slotTransform.GetComponentInChildren<Image>().sprite = itemSO.sprite;
        slotTransform.gameObject.SetActive(true);
    }

    // Fungsi BARU yang khusus untuk Quick Slot (dengan teks jumlah)
    void RefreshQuickSlot(Transform slotTransform, TMP_Text countText, ItemData itemData)
    {
        // Cek keamanan
        if (slotTransform == null || countText == null) return;
        if (itemData == null || itemData.itemName == "Empty")
        {
            slotTransform.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            return;
        }

        Item itemSO = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
        if (itemSO == null)
        {
            slotTransform.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            return;
        }

        // Update sprite dan aktifkan slot
        slotTransform.GetComponentInChildren<Image>().sprite = itemSO.sprite;
        slotTransform.gameObject.SetActive(true);

        // Update teks jumlah HANYA untuk slot ini
        if (itemData.count > 0)
        {
            countText.text = itemData.count.ToString();
            countText.gameObject.SetActive(true);
        }
        else
        {
            countText.gameObject.SetActive(false);
        }
    }

    public void SetInventory()
    {
        if (stats.inventory.Count > 0)
        {
            SetDescription(stats.inventory[0]);
        }
        else
        {
            SetDescription(playerData.emptyItemTemplate);
        }
        //// Jika item kosong, tidak perlu lanjutkan refresh
        //if (stats.inventory == null || stats.inventory.Count == 0)
        //{
        //    Debug.Log("No items to display in the inventory");
        //    UpdateSixItemDisplay();  // Tetap update untuk bersihkan display jika kosong
        //    return;
        //}

        RefreshInventoryItems();
        UpdateSixItemDisplay();
    }

    public void RefreshInventoryItems()
    {
        Debug.Log("item inventory di refresh");
        //error
        RefreshAllActiveSlots();
        //foreach (Transform child in ContentGO)
        //{
        //    if (child == SlotTemplate) continue;
        //    Destroy(child.gameObject);
        //}

        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in ContentGO)
        {
            // Pastikan kita tidak menghapus Template jika dia ada di dalam ContentGO
            if (child.gameObject == SlotTemplate) continue;

            toDestroy.Add(child.gameObject);
        }

        foreach (GameObject go in toDestroy)
        {
            // Hapus gameobject
            Destroy(go);
        }
        if (stats.inventory.Count == 0)
        {
            Debug.Log("Inventory Kosong. UI harusnya bersih.");
            return; // Jangan lanjut loop jika kosong
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
            ItemInteraction itemInteraction = itemInInventory.GetComponent<ItemInteraction>();
            if (itemInteraction != null)
            {
                itemInteraction.index = i; // Set itemID dengan indeks item
            }

            Button button = itemInInventory.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    Debug.Log($"Item {item.itemName} clicked");
                    SetDescription(currentItemData);
                    ShowDescription();
                });
            }
            else
            {
                Debug.LogWarning("Button component not found on itemInInventory");
            }

            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = stats.inventory[i].count.ToString();
            Image healthIndicatorImage = itemInInventory.GetChild(2).GetComponent<Image>();
            if (item.isItemCombat)
            {
                Debug.Log($"Item adalah item combat Checking health for item: {item.itemName} with health {stats.inventory[i].itemHealth}/{item.maxhealth}");
                float percentage = ((float)stats.inventory[i].itemHealth / item.maxhealth) * 100f;


                // Gunakan if-else if untuk rentang nilai
                if (percentage > 75) // Termasuk 100%
                {
                    healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[0].sprites; // Set sprite indikator kesehatan
                    Debug.Log($"Item health is high: {percentage}%");
                }
                else if (percentage > 50) // Rentang 51% - 75%
                {
                    healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[1].sprites; // Set sprite indikator kesehatan
                    Debug.Log($"Item health is medium: {percentage}%");
                }
                else if (percentage > 25) // Rentang 26% - 50%
                {
                    healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[2].sprites; // Set sprite indikator kesehatan
                    Debug.Log($"Item health is low: {percentage}%");
                }
                else if (percentage > 10) // Rentang 11% - 25%
                {
                    healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[3].sprites; // Set sprite indikator kesehatan   
                }
                else // Rentang 0% - 25%
                {
                    healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[4].sprites; // Set sprite indikator kesehatan
                    Debug.Log($"Item health is critical: {percentage}%");
                }
            }
            else
            {
                healthIndicatorImage.gameObject.SetActive(false);
            }

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
                itemInDisplay.GetChild(1).GetComponent<TMP_Text>().text = stats.inventory[i].count.ToString();
                Image healthIndicatorImage = itemInDisplay.GetChild(2).GetComponent<Image>();
                Debug.Log($"Item isItemCombat: {item.isItemCombat} for item {item.itemName}");

                if (item.isItemCombat)
                {
                    Debug.Log($"Item adalah item combat Checking health for item: {item.itemName} with health {stats.inventory[i].itemHealth}/{item.maxhealth}");
                    float percentage = ((float)stats.inventory[i].itemHealth / item.maxhealth) * 100f;


                    // Gunakan if-else if untuk rentang nilai
                    if (percentage > 75) // Termasuk 100%
                    {
                        healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[0].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is high: {percentage}%");
                    }
                    else if (percentage > 50) // Rentang 51% - 75%
                    {
                        healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[1].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is medium: {percentage}%");
                    }
                    else if (percentage > 25) // Rentang 26% - 50%
                    {
                        healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[2].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is low: {percentage}%");
                    }
                    else if (percentage > 10) // Rentang 11% - 25%
                    {
                        healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[3].sprites; // Set sprite indikator kesehatan   
                    }
                    else // Rentang 0% - 25%
                    {
                        healthIndicatorImage.sprite = spriteImageTemplate.imagePersens[4].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is critical: {percentage}%");
                    }
                }
                else
                {
                    healthIndicatorImage.gameObject.SetActive(false);
                }


                Button button = itemInDisplay.GetComponent<Button>();
                if (button != null)
                {
                    ItemData currentItemData = stats.inventory[i];
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log($"Item {item.itemName} clicked in 6-item display");
                        //SetDescription(currentItemData);
                        //ShowDescription();
                        SimpleUIEquppedToggle.Instance.TekanTombol(currentItemData);
                    });
                }
            }
        }
    }

    public void SetDescription(ItemData item)
    {
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

            PlayerController.Instance.HandleEquipItem(item);


            ShowDescription();
        });


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
        // Cek untuk menghindari error jika menu di luar batas array
        if (menu < 0 || menu >= menuPanels.Length)
        {
            Debug.LogError($"Indeks menu {menu} tidak valid.");
            return;
        }

        //Langkah Nonaktifkan semua panel 
        for (int i = 0; i < menuPanels.Length; i++)
        {
            menuPanels[i].panelInventory.SetActive(false);
            menuPanels[i].panelMenu.SetActive(false);
        }

        //Langkah Aktifkan panel yang benar dan jalankan logika spesifik 
        bool isActive = true;
        menuPanels[menu].panelInventory.SetActive(isActive);
        menuPanels[menu].panelMenu.SetActive(isActive);

        string nameMenuPanel = menuPanels[menu].panelInventory.name;
        Debug.Log($"Menu panel yang diaktifkan: {nameMenuPanel}");

        switch (nameMenuPanel)
        {
            case "TutorialList":
                tutorialList = menuPanels[menu].panelInventory.GetComponent<TutorialList>();

                Debug.Log("button tutorial di tekan");
                if (tutorialList != null)
                {
                    tutorialList.RefreshTutorialList();
                }
                //if (craftInventoryUI != null)
                //{
                //    craftInventoryUI.CloseUI();
                //}
                break;

            case "Craft":
                Debug.Log("Craft Inventory UI is being opened");
                if (craftInventoryUI != null)
                {
                    craftInventoryUI.OpenUI();
                }
                //if (npcListUI != null)
                //{
                //    // Jika perlu, tutup NPC list saat membuka Crafting
                //    npcListUI.CloseUI();
                //}
                break;
            case "QuestInfo":
                QuestInfoUI questInfoUI = menuPanels[menu].panelInventory.GetComponent<QuestInfoUI>();
                foreach (var quest in QuestManager.Instance.questActive)
                {
                    //questInfoUI.DisplayActiveQuest(quest.sideQuests);
                }
                questInfoUI.RefreshActiveQuest();
                break;

            default:
                // Logika default jika tidak ada panel yang cocok
                Debug.Log("Menu bukan NPCList atau Craft. Menutup UI khusus.");
                if (craftInventoryUI != null)
                {
                    craftInventoryUI.CloseUI();
                }
                //if (npcListUI != null)
                //{
                //    npcListUI.CloseUI();
                //}
                break;
        }

        Debug.Log($"Menu {menu} ({nameMenuPanel}) diaktifkan.");
    }

    //Fungsi untuk Menampilkan Tombol Hapus dan Mengatur Listener
    private void ShowDeleteButton(System.Action resetAction)
    {
        btnHapus.gameObject.SetActive(true);
        btnHapus.onClick.RemoveAllListeners();
        btnHapus.onClick.AddListener(() =>
        {
            resetAction();
            btnHapus.gameObject.SetActive(false); // Sembunyikan tombol setelah reset
        });
    }

    //Fungsi untuk Mereset Equipped Use
    public void RisetEquippedUse(int index)
    {
        Debug.Log("Equipped item di-reset: " + index);
        if (stats.equippedItemData[index].itemName != playerData.emptyItemTemplate.itemName)
        {
            // Di dalam CookUI / Result Button Listener
            bool isSuccess = ItemPool.Instance.AddItem(stats.equippedItemData[index]);

            if (isSuccess)
            {
                // Hapus item dari tungku
                stats.equippedItemData[index] = playerData.emptyItemTemplate;

            }
            else
            {
                // Jangan hapus, biarkan di tungku
                Debug.Log("Tas penuh, item tetap di tungku.");
                // Opsional: Munculkan teks "Tas Penuh!"
            }
        }
        //player_Inventory.equippedCombat[index] = stats.emptyItem;

        Image itemImage = (index == 0) ?
            equippedItem1.GetComponentInChildren<Image>() :
            equippedItem2.GetComponentInChildren<Image>();

        itemImage.sprite = null; // Hapus sprite
        itemImage.gameObject.SetActive(false);
        RefreshInventoryItems();
        UpdateSixItemDisplay();
        PlayerUI.Instance.UpdateEquippedWeaponUI();
    }

    //Fungsi untuk Mereset Quick Slot
    public void RisetQuickSlot(int index)
    {
        Debug.Log("Quick Slot di-reset: " + index);
        ItemPool.Instance.AddItem(stats.itemUseData[index]);
        // Di dalam CookUI / Result Button Listener
        bool isSuccess = ItemPool.Instance.AddItem(stats.itemUseData[index]);

        if (isSuccess)
        {
            // Hapus item dari tungku
            stats.itemUseData[index] = playerData.emptyItemTemplate;
            //stats.equippedItemData[index].count = 0;

            Image itemImage = (index == 0) ?
                quickSlot1.GetComponentInChildren<Image>() :
                quickSlot2.GetComponentInChildren<Image>();

            itemImage.sprite = null; // Hapus sprite
            itemImage.gameObject.SetActive(false);
            if (index == 0)
            {
                jumlahQuickItem1.gameObject.SetActive(false);
            }
            else
            {
                jumlahQuickItem2.gameObject.SetActive(false);
            }
        }
        else
        {
            // Jangan hapus, biarkan di tungku
            Debug.Log("Tas penuh, item tetap di tungku.");
            // Opsional: Munculkan teks "Tas Penuh!"
        }

        //itemImage.gameObject.SetActive(false);
        RefreshInventoryItems();
        UpdateSixItemDisplay();
        PlayerUI.Instance.UpdateItemUseUI();

    }

    //public void SwapItems(int id1, int id2)
    //{
    //    if (id1 < 0 || id1 >= stats.inventory.Count || id2 < 0 || id2 >= stats.inventory.Count)
    //        return; // Pastikan ID valid

    //    ItemData tempItem = stats.inventory[id1];
    //    stats.inventory[id1] = stats.inventory[id2];
    //    stats.inventory[id2] = tempItem;

    //    // Opsional: Anda bisa menambahkan logika untuk mengupdate status item jika diperlukan
    //}

    //logika drag item dan swap item
    // Dipanggil oleh ItemInteraction setelah press and hold berhasil
    // Dipanggil oleh OnBeginDrag pada ItemInteraction
    public void InitiateDrag(ItemInteraction itemToDrag)
    {
        // Jangan mulai drag baru jika sudah ada yang di-drag
        if (heldItem != null) return;

        // Cek apakah item yang di-drag punya data
        ItemData itemData = PlayerController.Instance.HandleGetItem(itemToDrag.index);
        Item itemUse = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
        if (itemData == null) return; // Jangan drag slot kosong

        // Simpan item yang di-drag
        heldItem = itemToDrag;

        // Atur dan tampilkan ikon drag
        dragIcon.sprite = itemUse.sprite; // Ambil sprite dari data
        dragIcon.gameObject.SetActive(true);

        // Sembunyikan item asli di grid
        heldItem.SetAlpha(0f);
    }

    // Dipanggil oleh OnDrag pada ItemInteraction
    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (heldItem != null)
        {
            dragIcon.transform.position = screenPosition;
        }
    }

    // Dipanggil oleh OnDrop pada item target atau TrashZone
    public void SuccessfulDrop(int targetIndex)
    {
        Debug.Log($"Drop berhasil pada index {targetIndex}");
        if (heldItem != null)
        {
            // Jika targetIndex valid (>= 0), lakukan swap
            if (targetIndex >= 0)
            {
                SwapItems(heldItem.index, targetIndex);
            }
            // Jika tidak, berarti item dibuang (logika RemoveItem sudah dipanggil oleh TrashZone)

            // Sembunyikan ikon dan reset state
            dragIcon.gameObject.SetActive(false);
            heldItem = null;

            // Selalu refresh UI untuk menampilkan hasil akhir
            RefreshInventoryItems();
        }
    }

    // Dipanggil oleh OnEndDrag jika drop tidak valid
    public void CancelDrag()
    {
        Debug.Log("Drag dibatalkan");
        if (heldItem != null)
        {
            // Tampilkan kembali item asli
            heldItem.SetAlpha(1f);

            // Sembunyikan ikon dan reset state
            dragIcon.gameObject.SetActive(false);
            heldItem = null;
        }
    }

    // Helper untuk mendapatkan item yang dipegang
    public ItemInteraction GetHeldItem()
    {
        return heldItem;
    }
    public void SwapItems(int indexA, int indexB)
    {
        Debug.Log($"Mencoba menukar item pada index {indexA} dan {indexB}");
        // Pengecekan keamanan untuk memastikan kedua index valid
        if (indexA < 0 || indexA >= stats.inventory.Count || indexB < 0 || indexB >= stats.inventory.Count)
        {
            Debug.LogWarning("Swap Gagal: Index di luar jangkauan.");
            return;
        }

        // Logika swapping standar menggunakan variabel temporary
        ItemData temp = stats.inventory[indexA];
        stats.inventory[indexA] = stats.inventory[indexB];
        stats.inventory[indexB] = temp;

        RefreshInventoryItems();
        UpdateSixItemDisplay();
        Debug.Log($"Data pada index {indexA} dan {indexB} berhasil ditukar.");
    }

    public void DropItemFromInventory(int itemIndex, int quantityToRemove)
    {
        //Pengecekan Awal (Sudah Benar) 
        if (itemIndex < 0 || itemIndex >= stats.inventory.Count)
        {
            Debug.LogWarning("Index item untuk di-drop tidak valid.");
            return;
        }

        ItemData itemData = stats.inventory[itemIndex];
        if (itemData == null) return;

        Item itemUse = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
        //contohItem = itemUse; // Simpan contoh item untuk referensi
        if (itemUse == null)
        {
            if (itemUse.itemDropName == null)
            {
                Debug.LogError($"Item '{itemData.itemName}' tidak memiliki prefab untuk di-drop.");
                return;
            }
            Debug.LogWarning($"Item '{itemData.itemName}' tidak ditemukan dalam database.");
            return;
        }


        int actualDropCount = Mathf.Min(quantityToRemove, itemData.count);

        // Simpan posisi player sekali saja
        Vector3 playerPosition = PlayerController.Instance.HandleGetPlayerPosition();

        Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), 0.5f, UnityEngine.Random.Range(-0.2f, 0.2f));
        ItemPool.Instance.DropItem(itemData.itemName, itemData.itemHealth, itemData.quality, playerPosition + offset, actualDropCount);

        // Gunakan actualDropCount untuk perbandingan yang lebih aman.
        if (actualDropCount >= itemData.count)
        {
            Debug.Log($"Membuang semua ({actualDropCount} buah) {itemData.itemName}.");
            stats.inventory.RemoveAt(itemIndex);
        }
        else
        {
            itemData.count -= actualDropCount;
            Debug.Log($"Membuang {actualDropCount} dari {itemData.itemName}. Sisa: {itemData.count}");
        }

        // Jangan lupa refresh UI setelah ada perubahan data
        MechanicController.Instance.HandleUpdateInventory();
    }

    //kumpulan animasi button 
    // Fungsi publik yang akan dipanggil oleh tombol


}