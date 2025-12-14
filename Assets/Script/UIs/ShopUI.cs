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

    [Header("Daftar Hubungan")]
    public ShopInteractable shopInteractable;

    [Header("Daftar item list")]
    public List<ItemData> currentSeasonItems; // List yang sedang aktif
    public List<ItemData> itemToSell;




    [Header("Logika Shop")]
    public Transform contentSellUI;
    public Transform templateSellUI;
    public Transform contentBuyUI;
    public Transform templateBuyUI;
    public TypeShop currentTypeShop;
    public ItemCategory categoryItemShop;


    [Header("Daftar Button dan UI")]
    public Button btnSell;
    public Button btnBuy;
    public Transform buyUI;
    public Transform sellUI;
    public Button btnClose;
    public Transform deskripsiUI;
    public String errorMessage;

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

      

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(CloseShop);

    }

    public void OpenShop(TypeShop typeShop, List<ItemData> itemsToDisplay, List<ItemData> itemSell, ShopInteractable interactable)
    {
        currentTypeShop = typeShop;
        if (typeShop == TypeShop.ItemShop)
        {
            categoryItemShop = ItemCategory.PlantSeed;
        }else if(typeShop == TypeShop.FoodShop)
        {
            categoryItemShop = ItemCategory.Food;
        }
            // hubungkan interactable ke ui 
            shopInteractable = interactable; 
        itemToSell = new List<ItemData>(itemSell);
        currentSeasonItems.Clear();
        currentSeasonItems = new List<ItemData>(itemsToDisplay);
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        //GameController.Instance.PauseGame();
        gameObject.SetActive(true);
        RefreshShopUI(currentSeasonItems);
    }

    private void CloseShop()
    {
        shopInteractable.itemToSell.Clear();
        shopInteractable.itemToSell = new List<ItemData>(itemToSell);
        itemToSell.Clear();
        shopInteractable = null;
        GameController.Instance.ResumeGame();
        // Tutup UI Storage
        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
    }





    public void UpdateSingleItemStock(string itemName, int newStockCount)
    {
        // Fungsi .Find() sangat cepat untuk mencari direct child
        Transform itemSlot = contentBuyUI.Find(itemName);

        if (itemSlot != null)
        {
            Debug.Log($"objek ui dengan nama {itemName} ditemukan");

            // Contoh: Jika Text ada di dalam child bernama "StockCountText"
            TMP_Text stackCountText = itemSlot.GetChild(0).GetChild(1).GetComponent<TMP_Text>();


            if (stackCountText != null)
            {
                stackCountText.gameObject.SetActive(true);
                stackCountText.text = newStockCount.ToString();
            }

           

            // Jika stok 0, apakah mau langsung dihapus atau dimatikan?
            if (newStockCount <= 0)
            {
                

                Destroy(itemSlot.gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"Item UI dengan nama {itemName} tidak ditemukan di dalam contentBuyUI!");
        }
    }



    private void RefreshShopUI(List<ItemData> items)
    {
        ClearChildUI(contentBuyUI, templateBuyUI);
        ClearChildUI(contentSellUI, templateSellUI);




        //Tampilkan item di inventory untuk dijual
        foreach (ItemData itemData in stats.inventory)
        {

            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
            if (item.categories == categoryItemShop)
            {
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
            Button btnDeskripsi = itemSlot.GetComponent<Button>();

            Button buy = itemSlot.GetChild(4).GetComponent<Button>();
            buy.onClick.RemoveAllListeners();
            buy.onClick.AddListener(() =>
            {
                BuyItem(itemShop, countText, itemBuyCounts, btnDeskripsi);
            });

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
        foreach (ItemData itemShop in itemToSell)
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
            Button btnDeskripsi = itemSlot.GetComponent<Button>();

            Button buy = itemSlot.GetChild(4).GetComponent<Button>();
            buy.onClick.RemoveAllListeners();
            buy.onClick.AddListener(() =>
            {
                BuyItem(itemShop, countText, itemBuyCounts, btnDeskripsi);
            });

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

    private void BuyItem(ItemData selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts, Button templateBuyImage)
    {
        // Setup Data Awal
        int amountToBuy = itemCounts[selectedItem.itemName]; // Simpan jumlah asli yang ingin dibeli
        int remainingToProcess = amountToBuy;                // Variabel untuk diproses di logic inventory (akan berkurang jadi 0)

        Item itemReference = ItemPool.Instance.GetItemWithQuality(selectedItem.itemName, selectedItem.quality);

        // Cek Validasi (Uang Cukup & Stok Toko Cukup)
        if (GameEconomy.Instance.SpendMoney(amountToBuy * itemReference.BuyValue) && amountToBuy <= selectedItem.count)
        {
            Debug.Log($"Pembelian Berhasil: {selectedItem.itemName} x{amountToBuy}");

          
            //  Cek apakah bisa ditumpuk (Stacking) ke slot yang sudah ada
            ItemData existingItem = stats.inventory.Find(x => x.itemName == selectedItem.itemName && x.count < itemReference.maxStackCount);

            if (existingItem != null)
            {
                int availableSpace = itemReference.maxStackCount - existingItem.count;
                int amountToAdd = Mathf.Min(availableSpace, remainingToProcess);

                existingItem.count += amountToAdd;
                remainingToProcess -= amountToAdd;
            }

            // Jika masih ada sisa, buat slot baru (New Stack)
            while (remainingToProcess > 0)
            {
                // Buat data item baru
                ItemData newItem = new ItemData(selectedItem.itemName, 1, selectedItem.quality, selectedItem.itemHealth);

                int amountToTake = Mathf.Min(remainingToProcess, itemReference.maxStackCount);
                newItem.count = amountToTake;
                remainingToProcess -= amountToTake;

                // Coba masukkan ke tas
                bool isSuccess = ItemPool.Instance.AddItem(newItem);

                if (isSuccess)
                {
                    Debug.Log($"Masuk tas: {newItem.itemName} x{newItem.count}");
                }
                else
                {
                    Debug.LogWarning($"Tas penuh! Menjatuhkan {newItem.itemName} x{newItem.count}");

                    string errorMsg = $"Tas penuh!! Drop Item {newItem.itemName} sejumlah {newItem.count}";
                    PlayerUI.Instance.ShowErrorUI(errorMsg);

                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                    Vector3 playerPosition = PlayerUI.Instance.player.transform.position;

                    ItemPool.Instance.DropItem(
                        newItem.itemName,
                        newItem.itemHealth,
                        newItem.quality,
                        playerPosition + offset,
                        newItem.count 
                    );
                }
            }

         
            int currentShopStock = selectedItem.count - amountToBuy;

           
            UpdateSingleItemStock(selectedItem.itemName, Mathf.Max(0, currentShopStock));

            if (amountToBuy > 0)
            {
                DeleteItemFromShop(selectedItem, amountToBuy);
            }

            itemBuyCounts[selectedItem.itemName] = 1;
            UpdateCountText(selectedItem.itemName, countText, itemBuyCounts);

            MechanicController.Instance.HandleUpdateInventory();

        }
        else
        {
         
            Debug.Log("Gagal Beli: Uang kurang atau stok habis.");

            // Reset Counter
            itemBuyCounts[selectedItem.itemName] = 1;
            UpdateCountText(selectedItem.itemName, countText, itemBuyCounts);

            // Efek Guncangan (Shake)
            if (templateBuyImage != null)
            {
                var shaker = templateBuyImage.GetComponent<UIShaker>();
                if (shaker != null) shaker.Shake();
            }

            PlayerUI.Instance.ShowErrorUI("Gagal melakukan transaksi Uang kurang atau stok habis. ");
        }
    }




    private void SellItem(ItemData selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        Item item = ItemPool.Instance.GetItemWithQuality(selectedItem.itemName, selectedItem.quality);

        if (item.categories == categoryItemShop)
        {
            int remainingToStore = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dipindahkan

            // Cek apakah item sudah ada di currentSeasonItems
            ItemData existingItem = itemToSell.Find(x => x.itemName == selectedItem.itemName);

            if (existingItem != null)
            {
                Debug.Log("Ada item yang sama di storage");
                existingItem.count += remainingToStore;
            }
            else
            {

                ItemData newItem = new ItemData(selectedItem.itemName, 1, selectedItem.quality, selectedItem.itemHealth);
                newItem.count = remainingToStore;
                itemToSell.Add(newItem);
            }

            GameEconomy.Instance.GainMoney((item.SellValue * remainingToStore));
            DeleteItemFromInventory(selectedItem, remainingToStore);
            RefreshShopUI(currentSeasonItems);
        }else
        {
            Debug.LogError("anda menjual item yang salah");
        }
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

        for (int i = itemToSell.Count - 1; i >= 0; i--)
        {
            ItemData itemData = itemToSell[i];
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
                    itemToSell.RemoveAt(i);

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
   
}
