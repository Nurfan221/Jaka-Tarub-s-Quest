using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    public Player_Inventory player_Inventory;
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
    }

    public void OpenShop()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        RefreshItemShop();
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
}
