using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : MonoBehaviour
{
    [Header("Referensi UI & Data")]
    [SerializeField] private PlayerController stats;
    [SerializeField] private StorageInteractable theStorage;
    [SerializeField] private Transform StorageContainer;
    [SerializeField] private Transform InventoryContainer;
    [SerializeField] private Transform itemSlotTemplate;

    [Header("Tombol Aksi")]
    [SerializeField] private Button closeStorageButton;
    [SerializeField] private Button storeAllButton;
    [SerializeField] private Button takeAllButton;

    // Menyimpan item yang sedang dalam proses untuk dipindahkan.
    private ItemData currentItemForPopup;
    // Menyimpan mode operasi (mengambil atau menyimpan).
    private bool isTakingFromStorage;


    #region Inisialisasi

    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }

    private void Start()
    {
        if (stats == null && PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
        }

        if (stats == null)
        {
            Debug.LogError("PlayerData_SO tidak ditemukan! UI Storage tidak akan berfungsi.");
            return;
        }

        // --- Pendaftaran Listener ---
        closeStorageButton.onClick.AddListener(CloseStorage);
        storeAllButton.onClick.AddListener(StoreAllItems);
        takeAllButton.onClick.AddListener(TakeAllItems);

        // Daftarkan listener ke event popup SEKALI SAJA.
        if (QuantityPopupUI.Instance != null)
        {
            QuantityPopupUI.Instance.onConfirm.AddListener(HandlePopupConfirmation);
            QuantityPopupUI.Instance.onCancel.AddListener(HandlePopupCancellation);
        }
        else
        {
            Debug.LogError("QuantityPopupUI.Instance tidak ditemukan. Pastikan objek popup ada di scene dan aktif.");
        }

        //gameObject.SetActive(false);
    }
    #endregion

    #region Alur Buka & Tutup UI
    public void OpenStorage(StorageInteractable storage)
    {

        Debug.Log("Membuka Storage UI untuk: " + storage.name);
        this.theStorage = storage;

        if (SoundManager.Instance != null)
            //SoundManager.Instance.PlaySound("Click");

        //GameController.Instance.ShowPersistentUI(false);
        //GameController.Instance.PauseGame();
        gameObject.SetActive(true);

        RefreshAllItems();
    }

    private void CloseStorage()
    {
        if (theStorage != null)
        {
            theStorage.StartAnimationClose();
        }

        //GameController.Instance.ResumeGame();
        //GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
    }
    #endregion

    #region Logika Utama
    private void RefreshAllItems()
    {
        // Bersihkan semua slot sebelum menggambar ulang
        foreach (Transform child in StorageContainer)
        {
            if (child != itemSlotTemplate) Destroy(child.gameObject);
        }
        foreach (Transform child in InventoryContainer)
        {
            if (child != itemSlotTemplate) Destroy(child.gameObject);
        }

        // Tampilkan item dari inventaris pemain
        foreach (ItemData itemData in stats.inventory)
        {
            CreateItemSlot(itemData, InventoryContainer, false);
        }

        // Tampilkan item dari storage
        foreach (ItemData itemData in theStorage.storage)
        {
            CreateItemSlot(itemData, StorageContainer, true);
        }

        // Perbarui visibilitas tombol "Take All"
        takeAllButton.gameObject.SetActive(theStorage.storage.Count > 0);
    }

    private void CreateItemSlot(ItemData data, Transform parent, bool isFromStorage)
    {
        if (data == null) return;
        Item itemSO = ItemPool.Instance.GetItemWithQuality(data.itemName, data.quality);
        if (itemSO == null) return;

        Transform itemSlot = Instantiate(itemSlotTemplate, parent);
        itemSlot.name = itemSO.itemName;
        itemSlot.gameObject.SetActive(true);
        itemSlot.GetChild(0).GetComponent<Image>().sprite = itemSO.sprite;
        itemSlot.GetChild(1).GetComponent<TMP_Text>().text = data.count.ToString();

        // Tambahkan listener dengan lambda untuk menangkap data yang benar
        itemSlot.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (isFromStorage)
            {
                OnStorageItemClick(data);
            }
            else
            {
                OnInventoryItemClick(data);
            }
        });
    }

    public void OnStorageItemClick(ItemData data)
    {
        currentItemForPopup = data;
        isTakingFromStorage = true;
        Item itemSO = ItemPool.Instance.GetItemWithQuality(data.itemName, data.quality);
        if (itemSO == null) return;

        QuantityPopupUI.Instance.Show(itemSO.sprite, 1, data.count);
    }

    public void OnInventoryItemClick(ItemData data)
    {
        currentItemForPopup = data;
        isTakingFromStorage = false;
        Item itemSO = ItemPool.Instance.GetItemWithQuality(data.itemName, data.quality);
        if (itemSO == null) return;

        QuantityPopupUI.Instance.Show(itemSO.sprite, 1, data.count);
    }

    private void HandlePopupConfirmation(int selectedAmount)
    {
        if (currentItemForPopup == null) return;

        List<ItemData> sourceList = isTakingFromStorage ? theStorage.storage : stats.inventory;
        List<ItemData> destinationList = isTakingFromStorage ? stats.inventory : theStorage.storage;

        MechanicController.Instance.MoveItem(sourceList, destinationList, currentItemForPopup, selectedAmount);

        currentItemForPopup = null;
        RefreshAllItems(); // Refresh UI setelah item dipindahkan
    }

    private void HandlePopupCancellation()
    {
        Debug.Log("Operasi popup dibatalkan.");
        currentItemForPopup = null;
    }
    #endregion

    #region Aksi Tombol "All"
    public void StoreAllItems()
    {
        // Buat salinan list untuk di-loop, agar aman saat memodifikasi list asli
        List<ItemData> itemsToMove = new List<ItemData>(stats.inventory);
        foreach (ItemData itemData in itemsToMove)
        {
            MechanicController.Instance.MoveItem(stats.inventory, theStorage.storage, itemData, itemData.count);
        }
        RefreshAllItems();
    }

    public void TakeAllItems()
    {
        List<ItemData> itemsToMove = new List<ItemData>(theStorage.storage);
        foreach (ItemData itemData in itemsToMove)
        {
            MechanicController.Instance.MoveItem(theStorage.storage, stats.inventory, itemData, itemData.count);
        }
        RefreshAllItems();
    }
    #endregion
}