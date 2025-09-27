using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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


    [Header("Daftar item list")]

    public List<ItemData> currentSeasonItems; // List yang sedang aktif
    public List<ItemData> ItemToSell;
    public int minItemShop;
    public int maxItemShop;



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

    private PlayerController stats;
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
        UpdateItemInShop(currenSeason);
    }

    public void HandleNewSeason()
    {
        currenSeason = TimeManager.Instance.GetCurrentSeason();
        UpdateItemInShop(currenSeason);
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

        HandleNewDay();
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

    public void AddItemToList(List<Item> Items)
    {
        // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama

       foreach (var item in Items)
        {
            //Debug.Log($"Sebelum Clear Item: {item.itemName}, Jumlah: {item.stackCount}");
            ItemData itemData = new ItemData(item.itemName, 1, item.quality, item.health);
            int randomCount = UnityEngine.Random.Range(minItemShop, maxItemShop + 1);
            itemData.count = randomCount;
            currentSeasonItems.Add(itemData);

        }

        foreach (var item in DatabaseManager.Instance.itemShopDatabase.itemWajib)
        {
            ItemData itemData = new ItemData(item.itemName, 1, item.quality, item.health);
            int randomCount = UnityEngine.Random.Range(minItemShop, maxItemShop + 1);
            itemData.count = randomCount;
            currentSeasonItems.Add(itemData);
        }


    }



    public void UpdateItemInShop(Season season)
    {
        ItemShopDatabase itemShopDatabase = DatabaseManager.Instance.GetCurrentItemShopDatabase(season);


        AddItemToList(itemShopDatabase.itemsForSale);
        RefreshShopUI(currentSeasonItems);
    }

   

    private void RefreshShopUI(List<ItemData> items)
    {
        ClearChildUI(contentBuyUI, templateBuyUI);
        ClearChildUI(contentSellUI, templateSellUI);




        //Tampilkan item di inventory untuk dijual
        foreach (ItemData itemData in stats.inventory)
        {
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
            Transform itemSlot = Instantiate(templateSellUI, contentSellUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = item.itemName;

            Image imageItem = itemSlot.GetChild(0).GetChild(0).GetComponent<Image>();
            TMP_Text stackCountText = itemSlot.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
            stackCountText.gameObject.SetActive(true);
            stackCountText.text = itemData.count.ToString();
            imageItem.sprite = item.sprite;

            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.itemName;
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + item.SellValue;

            //Inisialisasi jumlah item yang akan dijual
            if (!itemSellCounts.ContainsKey(item.itemName))
                itemSellCounts[item.itemName] = 1;

            TMP_Text countText = itemSlot.GetChild(2).GetComponent<TMP_Text>();
            UpdateCountText(item.itemName, countText, itemSellCounts);

            Button btnPlus = itemSlot.GetChild(5).GetComponent<Button>();
            Button btnMinus = itemSlot.GetChild(6).GetComponent<Button>();

            btnPlus.onClick.RemoveAllListeners();
            btnMinus.onClick.RemoveAllListeners();

            btnPlus.onClick.AddListener(() =>
            {
                if (itemSellCounts[item.itemName] < itemData.count)
                {
                    itemSellCounts[item.itemName]++;
                    UpdateCountText(item.itemName, countText, itemSellCounts);
                }
            });

            btnMinus.onClick.AddListener(() =>
            {
                if (itemSellCounts[item.itemName] > 1)
                {
                    itemSellCounts[item.itemName]--;
                    UpdateCountText(item.itemName, countText, itemSellCounts);
                }
            });

            Button sell = itemSlot.GetChild(4).GetComponent<Button>();
            sell.onClick.RemoveAllListeners();
            sell.onClick.AddListener(() =>
            {
                SellItem(itemData, countText, itemSellCounts);
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
        foreach (ItemData itemShop in currentSeasonItems)
        {
            Transform itemSlot = Instantiate(templateBuyUI, contentBuyUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = itemShop.itemName;

            Item item = ItemPool.Instance.GetItemWithQuality(itemShop.itemName, itemShop.quality);
            Image imageItem = itemSlot.GetChild(0).GetChild(0).GetComponent<Image>();
            TMP_Text stackCountText = itemSlot.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
            stackCountText.gameObject.SetActive(true);
            stackCountText.text = itemShop.count.ToString();
            imageItem.sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.itemName;
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + item.BuyValue;

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
                if (itemBuyCounts[itemShop.itemName] < itemShop.count)
                {
                    itemBuyCounts[itemShop.itemName]++;
                    UpdateCountText(itemShop.itemName, countText, itemBuyCounts);
                }
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
                imageDeskripsi.sprite = item.sprite;

                TMP_Text namaItem = deskripsiUI.GetChild(1).GetComponent<TMP_Text>();
                namaItem.gameObject.SetActive(true);
                namaItem.text = itemShop.itemName;

                TMP_Text deskripsiItem = deskripsiUI.GetChild(2).GetComponent<TMP_Text>();
                deskripsiItem.gameObject.SetActive(true);
                deskripsiItem.text = item.itemDescription;
            });
        }

        //Item yang di beli npc 
        foreach (ItemData itemShop in ItemToSell)
        {
            Transform itemSlot = Instantiate(templateBuyUI, contentBuyUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = itemShop.itemName;
            Item item = ItemPool.Instance.GetItemWithQuality(itemShop.itemName, itemShop.quality);

            //itemSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.itemName;
            Image imageItem = itemSlot.GetChild(0).GetChild(0).GetComponent<Image>();
            TMP_Text stackCountText = itemSlot.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
            stackCountText.text = itemShop.count.ToString();
            imageItem.sprite = item.sprite;
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + item.BuyValue;

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
                Debug.Log("button pluss di tekan");

                if (itemBuyCounts[itemShop.itemName] < itemShop.count)
                {
                    itemBuyCounts[itemShop.itemName]++;
                    UpdateCountText(itemShop.itemName, countText, itemBuyCounts);
                }
            });

            btnMinus.onClick.AddListener(() =>
            {
                Debug.Log("button Minuss di tekan");

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
                imageDeskripsi.sprite = item.sprite;

                TMP_Text namaItem = deskripsiUI.GetChild(1).GetComponent<TMP_Text>();
                namaItem.gameObject.SetActive(true);
                namaItem.text = itemShop.itemName;

                TMP_Text deskripsiItem = deskripsiUI.GetChild(2).GetComponent<TMP_Text>();
                deskripsiItem.gameObject.SetActive(true);
                deskripsiItem.text = item.itemDescription;
            });
        }
    }

    private void BuyItem(ItemData selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        Debug.Log("button beli item di tekan");
        int remainingToBuy = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dibeli
        Item item = ItemPool.Instance.GetItemWithQuality(selectedItem.itemName, selectedItem.quality);
        if (GameEconomy.Instance.SpendMoney(remainingToBuy * item.BuyValue) && remainingToBuy <= selectedItem.count)
        {
            // Cek apakah item sudah ada di inventory
            ItemData inventoryItem = stats.inventory.Find(x => x.itemName == selectedItem.itemName);

            if (inventoryItem != null && inventoryItem.count < item.maxStackCount)
            {
                int availableSpace = item.maxStackCount - inventoryItem.count;
                int amountToAdd = Mathf.Min(availableSpace, remainingToBuy);

                inventoryItem.count += amountToAdd;
                remainingToBuy -= amountToAdd;
            }

            // Jika masih ada sisa item, buat stack baru di inventory
            while (remainingToBuy > 0 && stats.inventory.Count < stats.playerData.maxItem)
            {
                //Item newItem = Instantiate(selectedItem);
                ItemData newItem = new ItemData(selectedItem.itemName, 1, selectedItem.quality, selectedItem.itemHealth);
                int amountToTake = Mathf.Min(remainingToBuy, item.maxStackCount);
                newItem.count = amountToTake;
                remainingToBuy -= amountToTake;

                item.isStackable = newItem.count < item.maxStackCount;

                ItemPool.Instance.AddItem(newItem);
                //stats.inventory.Add(newItem);
            }

            Debug.Log("Nama item: " + selectedItem.itemName + " | Jumlah: " + itemCounts[selectedItem.itemName]);
            Debug.Log("Total harga: " + (itemCounts[selectedItem.itemName] * item.BuyValue)); // Gunakan BuyValue!

            //Pastikan item hanya dihapus jika ada yang tersisa untuk dihapus
            if (itemCounts[selectedItem.itemName] > 0)
            {
                DeleteItemFromShop(selectedItem, itemCounts[selectedItem.itemName]);
            }

            itemBuyCounts[selectedItem.itemName] = 1;
            UpdateCountText(selectedItem.itemName, countText, itemBuyCounts);


            RefreshShopUI(currentSeasonItems);
        }
        else
        {
            itemBuyCounts[selectedItem.itemName] = 1;
            UpdateCountText(selectedItem.itemName, countText, itemBuyCounts);
            StartCoroutine(StartUIGagal());
        }

        MechanicController.Instance.HandleUpdateInventory();
        //inventoryUI.UpdateSixItemDisplay();
    }




    private void SellItem(ItemData selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        int remainingToStore = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dipindahkan

        // Cek apakah item sudah ada di currentSeasonItems
        ItemData existingItem = ItemToSell.Find(x => x.itemName == selectedItem.itemName);
        Item item = ItemPool.Instance.GetItemWithQuality(selectedItem.itemName, selectedItem.quality);

        if (existingItem != null)
        {
            Debug.Log("Ada item yang sama di storage");
            existingItem.count += remainingToStore;
        }
        else
        {

            ItemData newItem = new ItemData(selectedItem.itemName, 1, selectedItem.quality, selectedItem.itemHealth);
            newItem.count = remainingToStore;
            ItemToSell.Add(newItem);
        }

        GameEconomy.Instance.GainMoney((item.SellValue * remainingToStore));
        DeleteItemFromInventory(selectedItem, remainingToStore);
        RefreshShopUI(currentSeasonItems);
    }

    private void DeleteItemFromInventory(ItemData selectedItem,int selectedItemCount)
    {
        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = stats.inventory.Count - 1; i >= 0; i--)
        {
            ItemData itemData = stats.inventory[i];
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            if (selectedItem.itemName == itemData.itemName)
            {
                if (itemData.count > remainingToRemove)
                {
                    itemData.count -= remainingToRemove;
                    item.isStackable = itemData.count < item.maxStackCount;
                    return;
                }
                else
                {
                    remainingToRemove -= itemData.count;
                    stats.inventory.RemoveAt(i);

                    if (remainingToRemove <= 0)
                        return;
                }
            }
        }
    }

    private void DeleteItemFromShop(ItemData selectedItem, int selectedItemCount)
    {
        if (selectedItemCount <= 0) return; // **Cegah penghapusan jika jumlahnya 0 atau negatif**

        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = currentSeasonItems.Count - 1; i >= 0; i--)
        {
            ItemData itemData = currentSeasonItems[i];
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            if (selectedItem.itemName == itemData.itemName)
            {
                if (itemData.count > remainingToRemove)
                {
                    itemData.count -= remainingToRemove;
                    item.isStackable = itemData.count < item.maxStackCount;
                    return;
                }
                else
                {
                    remainingToRemove -= itemData.count;
                    currentSeasonItems.RemoveAt(i);

                    if (remainingToRemove <= 0)
                        return;
                }
            }
        }

        for (int i = ItemToSell.Count - 1; i >= 0; i--)
        {
            ItemData itemData = ItemToSell[i];
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
            if (selectedItem.itemName == item.itemName)
            {
                if (itemData.count > remainingToRemove)
                {
                    itemData.count -= remainingToRemove;
                    item.isStackable = itemData.count < item.maxStackCount;
                    return;
                }
                else
                {
                    remainingToRemove -= itemData.count;
                    ItemToSell.RemoveAt(i);

                    if (remainingToRemove <= 0)
                        return;
                }
            }
        }
    }


    private void UpdateCountText(string itemName, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        countText.text = itemCounts[itemName].ToString();
    }

    private void SellItem(ItemData itemToSell)
    {
        if (stats.inventory.Contains(itemToSell))
        {
            stats.inventory.Remove(itemToSell);
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
