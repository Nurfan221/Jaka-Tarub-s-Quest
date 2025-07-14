using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static TimeManager;
using static UnityEditor.Progress;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class ShopUI : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    //public Player_Inventory player_Inventory;
    public GameEconomy gameEconomy;
    public InventoryUI inventoryUI;

    [Header("Daftar item list")]
    public List<Item> rainSeasonShop = new();
    public List<Item> drySeasonShop = new();
    public List<Item> currentSeasonItems; // List yang sedang aktif
    public List<Item> ItemToSell;
    public int itemCountInShop;

   

    [Header("Logika Shop")]
    public Transform contentSellUI;
    public Transform templateSellUI;
    public Transform contentBuyUI;
    public Transform templateBuyUI;

    [Header("Daftar Button dan UI")]
    public Button btnSell;
    public Button btnBuy;
    public Transform buyUI;
    public Transform sellUI;
    public Button btnClose;
    public Button gagalUI;
    public Transform deskripsiUI;
    private Season currenSeason;

    private Dictionary<string, int> itemSellCounts = new(); // Menyimpan jumlah item yang akan dibeli
    private Dictionary<string , int> itemBuyCounts = new();

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

    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
        TimeManager.OnSeasonChanged += HandleNewSeason;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
        TimeManager.OnSeasonChanged -= HandleNewSeason;
    }

    public void HandleNewDay()
    {
        //RestockDaily(currentSeason)
        currenSeason = TimeManager.Instance.GetCurrentSeason();
        UpdateShopBySeason(currenSeason);
    }

    public void HandleNewSeason()
    {
        currenSeason = TimeManager.Instance.GetCurrentSeason();
        UpdateShopBySeason(currenSeason);

    }

    private void Start()
    {
        sellUI.gameObject.SetActive(false);

        btnSell.onClick.RemoveAllListeners();
        btnBuy.onClick.RemoveAllListeners();

        btnSell.onClick.AddListener(() =>
        {
            buyUI.gameObject.SetActive(false);
            sellUI.gameObject.SetActive(true);
            RefreshShopUI(currentSeasonItems);
        });

        btnBuy.onClick.AddListener(() =>
        {
            buyUI.gameObject.SetActive(true);
            sellUI.gameObject.SetActive(false);
            RefreshShopUI(currentSeasonItems);
        });

        gagalUI.onClick.AddListener(() =>
        {
            gagalUI.gameObject.SetActive(false);
        });

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(CloseShop);
    }

    public void OpenShop()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        gameObject.SetActive(true);
        RefreshShopUI(currentSeasonItems);
    }

    private void CloseShop()
    {
        
        GameController.Instance.ResumeGame();
        // Tutup UI Storage
        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
        gagalUI.gameObject.SetActive(false); // Sembunyikan UI
    }
    public void UpdateShopBySeason(Season season)
    {
        switch (season)
        {
            case Season.Rain:
                UpdateItemInShop(Season.Rain);
                break;
            case Season.Dry:
                UpdateItemInShop(Season.Dry);
                break;
        }

        
    }
    public void AddItemToList(List<Item> Items)
    {
        // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama
        List<Item> newItemList = new List<Item>();

        // Menambahkan item baru ke dalam itemQuests berdasarkan countItem
        for (int i = 0; i < Items.Count; i++)
        {
            // Memanggil AddNewItem untuk membuat item baru dan menambahkannya ke dalam newItemList
            Item newItem = ItemPool.Instance.GetItemWithQuality(Items[i].name, Items[i].quality);

            // Menambahkan item salinan baru ke dalam newItemList
            newItemList.Add(newItem);
        }

        // Menambahkan item baru dari newItemList ke currentSeasonItems
        foreach (var item in newItemList)
        {
            currentSeasonItems.Add(item);
            //Debug.Log($"Item: {item.itemName}, Jumlah: {item.stackCount}");
        }
    }



    public void UpdateItemInShop(Season season)
    {
        if (season == Season.Rain)
        {
            AddItemToList(rainSeasonShop);
        }else if (season == Season.Dry)
        {
            AddItemToList(drySeasonShop);
        }
    }

   

    private void RefreshShopUI(List<Item> items)
    {
        ClearChildUI(contentBuyUI, templateBuyUI);
        ClearChildUI(contentSellUI, templateSellUI);


        //Tampilkan item di inventory untuk dijual
        foreach (Item item in stats.itemList)
        {
            Transform itemSlot = Instantiate(templateSellUI, contentSellUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = item.itemName;

            itemSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.itemName;
            //itemSlot.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + item.SellValue;

            //Inisialisasi jumlah item yang akan dijual
            if (!itemSellCounts.ContainsKey(item.itemName))
                itemSellCounts[item.itemName] = 1;

            TMP_Text countText = itemSlot.GetChild(2).GetComponent<TMP_Text>();
            UpdateCountText(item.itemName, countText, itemSellCounts);

            Button btnPlus = itemSlot.GetChild(4).GetComponent<Button>();
            Button btnMinus = itemSlot.GetChild(5).GetComponent<Button>();

            btnPlus.onClick.RemoveAllListeners();
            btnMinus.onClick.RemoveAllListeners();

            btnPlus.onClick.AddListener(() =>
            {
                //if (itemSellCounts[item.itemName] < item.stackCount)
                //{
                //    itemSellCounts[item.itemName]++;
                //    UpdateCountText(item.itemName, countText, itemSellCounts);
                //}
            });

            btnMinus.onClick.AddListener(() =>
            {
                if (itemSellCounts[item.itemName] > 1)
                {
                    itemSellCounts[item.itemName]--;
                    UpdateCountText(item.itemName, countText, itemSellCounts);
                }
            });

            Button sell = itemSlot.GetChild(6).GetComponent<Button>();
            sell.onClick.RemoveAllListeners();
            sell.onClick.AddListener(() =>
            {
                SellItem(item, countText, itemSellCounts);
            });

            Button btnDeskripsi = templateSellUI.GetComponent<Button>();
            btnDeskripsi.onClick.RemoveAllListeners();
            btnDeskripsi.onClick.AddListener(() =>
            {
                Image imageDeskripsi = templateSellUI.GetChild(0).GetComponent<Image>();
                imageDeskripsi.gameObject.SetActive(true);
                imageDeskripsi.sprite = item.sprite;

                TMP_Text namaItem = templateSellUI.GetChild(1).GetComponent<TMP_Text>();
                imageDeskripsi.gameObject.SetActive(true);
                namaItem.text = item.itemName;

                TMP_Text deskripsiItem = templateSellUI.GetChild(2).GetComponent<TMP_Text>();
                imageDeskripsi.gameObject.SetActive(true);
                deskripsiItem.text = item.itemDescription;
            });
        }

        //Tampilkan item yang bisa dibeli dari shop
        foreach (Item itemShop in currentSeasonItems)
        {
            Transform itemSlot = Instantiate(templateBuyUI, contentBuyUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = itemShop.itemName;

            itemSlot.GetChild(0).GetComponent<Image>().sprite = itemShop.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = itemShop.itemName;
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + itemShop.BuyValue;

            //Inisialisasi jumlah item yang akan dibeli
            if (!itemBuyCounts.ContainsKey(itemShop.itemName))
                itemBuyCounts[itemShop.itemName] = 1;
            
            TMP_Text countText = itemSlot.GetChild(2).GetComponent<TMP_Text>();
            UpdateCountText(itemShop.itemName, countText, itemBuyCounts);

            Button btnPlus = itemSlot.GetChild(5).GetComponent<Button>();
            Button btnMinus = itemSlot.GetChild(6).GetComponent<Button>();


            btnPlus.onClick.RemoveAllListeners();
            btnMinus.onClick.RemoveAllListeners();



            btnPlus.onClick.AddListener(() =>
            {
                //if (itemBuyCounts[itemShop.itemName] < itemShop.stackCount)
                //{
                //    itemBuyCounts[itemShop.itemName]++;
                //    UpdateCountText(itemShop.itemName, countText, itemBuyCounts);
                //}
            });

            btnMinus.onClick.AddListener(() =>
            {
                if (itemBuyCounts[itemShop.itemName] > 1)
                {
                    itemBuyCounts[itemShop.itemName]--;
                    UpdateCountText(itemShop.itemName, countText, itemBuyCounts);
                }
            });

            Button buy = itemSlot.GetChild(4).GetComponent<Button>();
            buy.onClick.RemoveAllListeners();
            buy.onClick.AddListener(() =>
            {
                BuyItem(itemShop, countText, itemBuyCounts);
            });

            Button btnDeskripsi = itemSlot.GetComponent<Button>();
            btnDeskripsi.onClick.RemoveAllListeners();
            btnDeskripsi.onClick.AddListener(() =>
            {
                Debug.Log("tampilkan deskripsi");
                Image imageDeskripsi = deskripsiUI.GetChild(0).GetComponent<Image>();
                imageDeskripsi.gameObject.SetActive(true);
                imageDeskripsi.sprite = itemShop.sprite;

                TMP_Text namaItem = deskripsiUI.GetChild(1).GetComponent<TMP_Text>();
                namaItem.gameObject.SetActive(true);
                namaItem.text = itemShop.itemName;

                TMP_Text deskripsiItem = deskripsiUI.GetChild(2).GetComponent<TMP_Text>();
                deskripsiItem.gameObject.SetActive(true);
                deskripsiItem.text = itemShop.itemDescription;
            });
        }

        //Item yang di beli npc 
        foreach (Item itemShop in ItemToSell)
        {
            Transform itemSlot = Instantiate(templateBuyUI, contentBuyUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = itemShop.itemName;

            itemSlot.GetChild(0).GetComponent<Image>().sprite = itemShop.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = itemShop.itemName;
            itemSlot.GetChild(0).GetChild(0).GetComponent<TMP_Text>().gameObject.SetActive(true);
            //itemSlot.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = itemShop.stackCount.ToString();
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + itemShop.BuyValue;

            //Inisialisasi jumlah item yang akan dibeli
            if (!itemBuyCounts.ContainsKey(itemShop.itemName))
                itemBuyCounts[itemShop.itemName] = 1;

            TMP_Text countText = itemSlot.GetChild(2).GetComponent<TMP_Text>();
            UpdateCountText(itemShop.itemName, countText, itemBuyCounts);

            Button btnPlus = itemSlot.GetChild(5).GetComponent<Button>();
            Button btnMinus = itemSlot.GetChild(6).GetComponent<Button>();


            btnPlus.onClick.RemoveAllListeners();
            btnMinus.onClick.RemoveAllListeners();

            btnPlus.onClick.AddListener(() =>
            {
                //if (itemBuyCounts[itemShop.itemName] < itemShop.stackCount)
                //{
                //    itemBuyCounts[itemShop.itemName]++;
                //    UpdateCountText(itemShop.itemName, countText, itemBuyCounts);
                //}
            });

            btnMinus.onClick.AddListener(() =>
            {
                if (itemBuyCounts[itemShop.itemName] > 1)
                {
                    itemBuyCounts[itemShop.itemName]--;
                    UpdateCountText(itemShop.itemName, countText, itemBuyCounts);
                }
            });

            Button buy = itemSlot.GetChild(4).GetComponent<Button>();
            buy.onClick.RemoveAllListeners();
            buy.onClick.AddListener(() =>
            {
                BuyItem(itemShop, countText, itemBuyCounts);
            });

            Button btnDeskripsi = itemSlot.GetComponent<Button>();
            btnDeskripsi.onClick.RemoveAllListeners();
            btnDeskripsi.onClick.AddListener(() =>
            {
                Debug.Log("tampilkan deskripsi");
                Image imageDeskripsi = deskripsiUI.GetChild(0).GetComponent<Image>();
                imageDeskripsi.gameObject.SetActive(true);
                imageDeskripsi.sprite = itemShop.sprite;

                TMP_Text namaItem = deskripsiUI.GetChild(1).GetComponent<TMP_Text>();
                namaItem.gameObject.SetActive(true);
                namaItem.text = itemShop.itemName;

                TMP_Text deskripsiItem = deskripsiUI.GetChild(2).GetComponent<TMP_Text>();
                deskripsiItem.gameObject.SetActive(true);
                deskripsiItem.text = itemShop.itemDescription;
            });
        }
    }

    private void BuyItem(Item selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        int remainingToBuy = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dibeli

        //if (gameEconomy.SpendMoney(remainingToBuy * selectedItem.BuyValue)&& remainingToBuy <= selectedItem.stackCount)
        //{
        //    // Cek apakah item sudah ada di inventory
        //    Item inventoryItem = stats.itemList.Find(x => x.itemName == selectedItem.itemName);

        //    if (inventoryItem != null && inventoryItem.stackCount < inventoryItem.maxStackCount)
        //    {
        //        int availableSpace = inventoryItem.maxStackCount - inventoryItem.stackCount;
        //        int amountToAdd = Mathf.Min(availableSpace, remainingToBuy);

        //        inventoryItem.stackCount += amountToAdd;
        //        remainingToBuy -= amountToAdd;
        //    }

        //    // Jika masih ada sisa item, buat stack baru di inventory
        //    while (remainingToBuy > 0 && stats.itemList.Count < stats.maxItem)
        //    {
        //        Item newItem = Instantiate(selectedItem);
        //        int amountToTake = Mathf.Min(remainingToBuy, newItem.maxStackCount);
        //        newItem.stackCount = amountToTake;
        //        remainingToBuy -= amountToTake;

        //        newItem.isStackable = newItem.stackCount < newItem.maxStackCount;

        //        stats.itemList.Add(newItem);
        //    }

        //    Debug.Log("Nama item: " + selectedItem.itemName + " | Jumlah: " + itemCounts[selectedItem.itemName]);
        //    Debug.Log("Total harga: " + (itemCounts[selectedItem.itemName] * selectedItem.BuyValue)); // Gunakan BuyValue!

        //    //Pastikan item hanya dihapus jika ada yang tersisa untuk dihapus
        //    if (itemCounts[selectedItem.itemName] > 0)
        //    {
        //        DeleteItemFromShop(selectedItem, itemCounts[selectedItem.itemName]);
        //    }

        //    itemBuyCounts[selectedItem.itemName] = 1;
        //    UpdateCountText(selectedItem.itemName, countText, itemBuyCounts);


        //    RefreshShopUI(currentSeasonItems);
        //}
        //else
        //{
        //    itemBuyCounts[selectedItem.itemName] = 1;
        //    UpdateCountText(selectedItem.itemName, countText, itemBuyCounts);
        //    StartCoroutine(StartUIGagal());
        //}

        inventoryUI.RefreshInventoryItems();
        inventoryUI.UpdateSixItemDisplay();
    }



    private void SellItem(Item selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        int remainingToStore = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dipindahkan

        // Cek apakah item sudah ada di currentSeasonItems
        Item existingItem = ItemToSell.Find(x => x.itemName == selectedItem.itemName);

        //if (existingItem != null)
        //{
        //    Debug.Log("Ada item yang sama di storage");
        //    existingItem.stackCount += remainingToStore;
        //}
        //else
        //{
        //    Item newItem = Instantiate(selectedItem);
        //    newItem.stackCount = remainingToStore;
        //    ItemToSell.Add(newItem);
        //}

        gameEconomy.GainMoney((selectedItem.SellValue * remainingToStore));
        DeleteItemFromInventory(selectedItem, remainingToStore);
        RefreshShopUI(currentSeasonItems);
    }

    private void DeleteItemFromInventory(Item selectedItem,int selectedItemCount)
    {
        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = stats.itemList.Count - 1; i >= 0; i--)
        {
            Item item = stats.itemList[i];

            if (selectedItem.itemName == item.itemName)
            {
                //if (item.stackCount > remainingToRemove)
                //{
                //    item.stackCount -= remainingToRemove;
                //    item.isStackable = item.stackCount < item.maxStackCount;
                //    return;
                //}
                //else
                //{
                //    remainingToRemove -= item.stackCount;
                //    stats.itemList.RemoveAt(i);

                //    if (remainingToRemove <= 0)
                //        return;
                //}
            }
        }
    }

    private void DeleteItemFromShop(Item selectedItem, int selectedItemCount)
    {
        if (selectedItemCount <= 0) return; // **Cegah penghapusan jika jumlahnya 0 atau negatif**

        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = currentSeasonItems.Count - 1; i >= 0; i--)
        {
            Item item = currentSeasonItems[i];

            if (selectedItem.itemName == item.itemName)
            {
                //if (item.stackCount > remainingToRemove)
                //{
                //    item.stackCount -= remainingToRemove;
                //    item.isStackable = item.stackCount < item.maxStackCount;
                //    return;
                //}
                //else
                //{
                //    remainingToRemove -= item.stackCount;
                //    currentSeasonItems.RemoveAt(i);

                //    if (remainingToRemove <= 0)
                //        return;
                //}
            }
        }

        for (int i = ItemToSell.Count - 1; i >= 0; i--)
        {
            Item item = ItemToSell[i];

            if (selectedItem.itemName == item.itemName)
            {
                //if (item.stackCount > remainingToRemove)
                //{
                //    item.stackCount -= remainingToRemove;
                //    item.isStackable = item.stackCount < item.maxStackCount;
                //    return;
                //}
                //else
                //{
                //    remainingToRemove -= item.stackCount;
                //    ItemToSell.RemoveAt(i);

                //    if (remainingToRemove <= 0)
                //        return;
                //}
            }
        }
    }


    private void UpdateCountText(string itemName, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        countText.text = itemCounts[itemName].ToString();
    }

    private void SellItem(Item itemToSell)
    {
        if (stats.itemList.Contains(itemToSell))
        {
            stats.itemList.Remove(itemToSell);
            RefreshShopUI(currentSeasonItems);
        }
    }

    private void ClearChildUI(Transform parent, Transform template)
    {
        foreach (Transform child in parent)
        {
            if (child != template)
                Destroy(child.gameObject);
        }
    }
    private IEnumerator StartUIGagal()
    {
        gagalUI.gameObject.SetActive(true); // Tampilkan UI
        yield return new WaitForSeconds(1f); // Tunggu 1 detik
        gagalUI.gameObject.SetActive(false); // Sembunyikan UI
        Button gagalbtn = gagalUI.GetComponent<Button>();

        gagalbtn.onClick.AddListener(() =>
        {
            gagalUI.gameObject.SetActive(false);
        });
    }
}
