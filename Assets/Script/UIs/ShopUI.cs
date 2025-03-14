using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class ShopUI : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    public Player_Inventory player_Inventory;
    public GameEconomy gameEconomy;
    public List<Item> listItemShop = new();

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
    public Transform GagalUI;

    private Dictionary<string, int> itemSellCounts = new(); // Menyimpan jumlah item yang akan dibeli
    private Dictionary<string , int> itemBuyCounts = new();

    private void Start()
    {
        sellUI.gameObject.SetActive(false);

        btnSell.onClick.RemoveAllListeners();
        btnBuy.onClick.RemoveAllListeners();

        btnSell.onClick.AddListener(() =>
        {
            buyUI.gameObject.SetActive(false);
            sellUI.gameObject.SetActive(true);
            RefreshItemShop();
        });

        btnBuy.onClick.AddListener(() =>
        {
            buyUI.gameObject.SetActive(true);
            sellUI.gameObject.SetActive(false);
            RefreshItemShop();
        });

        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(CloseShop);
    }

    public void OpenShop()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        RefreshItemShop();
    }

    private void CloseShop()
    {
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
    }

    private void RefreshItemShop()
    {
        ClearChildUI(contentBuyUI, templateBuyUI);
        ClearChildUI(contentSellUI, templateSellUI);

        //Tampilkan item di inventory untuk dijual
        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            Transform itemSlot = Instantiate(templateSellUI, contentSellUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = item.itemName;

            itemSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.itemName;
            itemSlot.GetChild(2).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemSlot.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + item.SellValue;

            //Inisialisasi jumlah item yang akan dijual
            if (!itemSellCounts.ContainsKey(item.itemName))
                itemSellCounts[item.itemName] = 1;

            TMP_Text countText = itemSlot.GetChild(5).GetComponent<TMP_Text>();
            UpdateCountText(item.itemName, countText, itemSellCounts);

            Button btnPlus = itemSlot.GetChild(6).GetComponent<Button>();
            Button btnMinus = itemSlot.GetChild(7).GetComponent<Button>();

            btnPlus.onClick.RemoveAllListeners();
            btnMinus.onClick.RemoveAllListeners();

            btnPlus.onClick.AddListener(() =>
            {
                if (itemSellCounts[item.itemName] < item.stackCount)
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
                SellItem(item, countText, itemSellCounts);
            });
        }

        //Tampilkan item yang bisa dibeli dari shop
        foreach (Item itemShop in listItemShop)
        {
            Transform itemSlot = Instantiate(templateBuyUI, contentBuyUI);
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = itemShop.itemName;

            itemSlot.GetChild(0).GetComponent<Image>().sprite = itemShop.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = itemShop.itemName;
            itemSlot.GetChild(2).GetComponent<TMP_Text>().text = "Rp." + itemShop.SellValue;

            //Inisialisasi jumlah item yang akan dibeli
            if (!itemSellCounts.ContainsKey(itemShop.itemName))
                itemSellCounts[itemShop.itemName] = 1;

            TMP_Text countText = itemSlot.GetChild(4).GetComponent<TMP_Text>();
            UpdateCountText(itemShop.itemName, countText, itemSellCounts);

            Button btnPlus = itemSlot.GetChild(5).GetComponent<Button>();
            Button btnMinus = itemSlot.GetChild(6).GetComponent<Button>();


            btnPlus.onClick.RemoveAllListeners();
            btnMinus.onClick.RemoveAllListeners();

            btnPlus.onClick.AddListener(() =>
            {
                itemSellCounts[itemShop.itemName]++;
                UpdateCountText(itemShop.itemName, countText, itemSellCounts);

            });

            btnMinus.onClick.AddListener(() =>
            {
                if (itemSellCounts[itemShop.itemName] > 1)
                {
                    itemSellCounts[itemShop.itemName]--;
                    UpdateCountText(itemShop.itemName, countText, itemSellCounts);
                }
            });

            Button buy = itemSlot.GetChild(3).GetComponent<Button>();
            buy.onClick.RemoveAllListeners();
            buy.onClick.AddListener(() =>
            {
                BuyItem(itemShop, countText, itemSellCounts);
            });
        }
    }

    private void BuyItem(Item selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        int remainingToBuy = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dibeli

        if (gameEconomy.SpendMoney(remainingToBuy * selectedItem.BuyValue)&& remainingToBuy <= selectedItem.stackCount)
        {
            // Cek apakah item sudah ada di inventory
            Item inventoryItem = Player_Inventory.Instance.itemList.Find(x => x.itemName == selectedItem.itemName);

            if (inventoryItem != null && inventoryItem.stackCount < inventoryItem.maxStackCount)
            {
                int availableSpace = inventoryItem.maxStackCount - inventoryItem.stackCount;
                int amountToAdd = Mathf.Min(availableSpace, remainingToBuy);

                inventoryItem.stackCount += amountToAdd;
                remainingToBuy -= amountToAdd;
            }

            // Jika masih ada sisa item, buat stack baru di inventory
            while (remainingToBuy > 0 && Player_Inventory.Instance.itemList.Count < Player_Inventory.Instance.maxItem)
            {
                Item newItem = Instantiate(selectedItem);
                int amountToTake = Mathf.Min(remainingToBuy, newItem.maxStackCount);
                newItem.stackCount = amountToTake;
                remainingToBuy -= amountToTake;

                newItem.isStackable = newItem.stackCount < newItem.maxStackCount;

                Player_Inventory.Instance.itemList.Add(newItem);
            }

            Debug.Log("Nama item: " + selectedItem.itemName + " | Jumlah: " + itemCounts[selectedItem.itemName]);
            Debug.Log("Total harga: " + (itemCounts[selectedItem.itemName] * selectedItem.BuyValue)); // Gunakan BuyValue!

            //Pastikan item hanya dihapus jika ada yang tersisa untuk dihapus
            if (itemCounts[selectedItem.itemName] > 0)
            {
                DeleteItemFromShop(selectedItem, itemCounts[selectedItem.itemName]);
            }

            itemSellCounts[selectedItem.itemName] = 1;
            UpdateCountText(selectedItem.itemName, countText, itemSellCounts);


            RefreshItemShop();
        }
        else
        {
            itemSellCounts[selectedItem.itemName] = 1;
            UpdateCountText(selectedItem.itemName, countText, itemSellCounts);
            StartCoroutine(StartUIGagal());
        }
    }



    private void SellItem(Item selectedItem, TMP_Text countText, Dictionary<string, int> itemCounts)
    {
        int remainingToStore = itemCounts[selectedItem.itemName]; // Jumlah item yang ingin dipindahkan

        // Cek apakah item sudah ada di listItemShop
        Item existingItem = listItemShop.Find(x => x.itemName == selectedItem.itemName);

        if (existingItem != null)
        {
            Debug.Log("Ada item yang sama di storage");
            existingItem.stackCount += remainingToStore;
        }
        else
        {
            Item newItem = Instantiate(selectedItem);
            newItem.stackCount = remainingToStore;
            listItemShop.Add(newItem);
        }

        gameEconomy.GainMoney((selectedItem.SellValue * remainingToStore));
        DeleteItemFromInventory(selectedItem, remainingToStore);
        RefreshItemShop();
    }

    private void DeleteItemFromInventory(Item selectedItem,int selectedItemCount)
    {
        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = Player_Inventory.Instance.itemList.Count - 1; i >= 0; i--)
        {
            Item item = Player_Inventory.Instance.itemList[i];

            if (selectedItem.itemName == item.itemName)
            {
                if (item.stackCount > remainingToRemove)
                {
                    item.stackCount -= remainingToRemove;
                    item.isStackable = item.stackCount < item.maxStackCount;
                    return;
                }
                else
                {
                    remainingToRemove -= item.stackCount;
                    Player_Inventory.Instance.itemList.RemoveAt(i);

                    if (remainingToRemove <= 0)
                        return;
                }
            }
        }
    }

    private void DeleteItemFromShop(Item selectedItem, int selectedItemCount)
    {
        if (selectedItemCount <= 0) return; // **Cegah penghapusan jika jumlahnya 0 atau negatif**

        int remainingToRemove = selectedItemCount; // Jumlah yang ingin dihapus

        for (int i = listItemShop.Count - 1; i >= 0; i--)
        {
            Item item = listItemShop[i];

            if (selectedItem.itemName == item.itemName)
            {
                if (item.stackCount > remainingToRemove)
                {
                    item.stackCount -= remainingToRemove;
                    item.isStackable = item.stackCount < item.maxStackCount;
                    return;
                }
                else
                {
                    remainingToRemove -= item.stackCount;
                    listItemShop.RemoveAt(i);

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

    private void SellItem(Item itemToSell)
    {
        if (Player_Inventory.Instance.itemList.Contains(itemToSell))
        {
            Player_Inventory.Instance.itemList.Remove(itemToSell);
            RefreshItemShop();
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
        GagalUI.gameObject.SetActive(true); // Tampilkan UI
        yield return new WaitForSeconds(1f); // Tunggu 1 detik
        GagalUI.gameObject.SetActive(false); // Sembunyikan UI
    }
}
