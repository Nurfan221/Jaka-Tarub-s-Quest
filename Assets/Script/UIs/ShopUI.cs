using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] TMP_Text currentMoney;

    [SerializeField] Image itemSprite;
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemDesc;

    [Header("BUY UI")]
    [SerializeField] Transform itemsToBuyContainer;
    [SerializeField] Transform itemsToBuyTemplate;
    List<Item> itemsToBuy;
    [Serializable]
    struct ItemListing
    {
        public string tag;
        public int amount;
        public Item Item;
        public ItemListing(int amount, Item Item, string tag)
        {
            this.amount = amount;
            this.Item = Item;
            this.tag = tag;
        }
        public ItemListing(ItemListing itemListing)
        {
            this.amount = itemListing.amount;
            this.Item = itemListing.Item;
            this.tag = itemListing.tag;
        }
    }
    [SerializeField] List<ItemListing> listings;


    [Header("SELL UI")]
    [SerializeField] Transform itemsToSellContainer;
    [SerializeField] Transform itemsToSellTemplate;
    public List<Item> itemsToSell;

    [Header("UPGRADE UI")]
    [SerializeField] Transform itemsToUpgradeContainer;
    [SerializeField] Transform itemsToUpgradeTemplate;
    public List<Item> itemsToUpgrade;

    private void Awake()
    {
        List<ItemListing> temp = new(listings);
        itemsToBuy = new List<Item>();
        itemsToBuy.Clear();
        foreach (ItemListing item in temp)
        {
            Item thisItem = ItemPool.Instance.GetItem(item.Item.itemName);
            thisItem.stackCount = item.amount;
            AddToStore(thisItem);
        }
    }

    private void Update()
    {
        // Close Shop
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("Click");
            gameObject.SetActive(false);
            GameController.Instance.ShowPersistentUI(true);
        }
    }

    public void OpenShop()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        currentMoney.text = "$" + GameEconomy.Instance.Money;

        itemsToSell = Player_Inventory.Instance.itemList;
        itemsToUpgrade.Clear();
        foreach (Item item in itemsToSell)
        {
            switch (item.type)
            {
                case ItemType.Melee_Combat:
                case ItemType.Ranged_Combat:
                    itemsToUpgrade.Add(item);
                    break;
                default: break;
            }
        }

        List<Item> temp = new(itemsToBuy);
        itemsToBuy.Clear();
        foreach (Item item in temp)
        {
            AddToStore(item);
        }

        RefreshListUI(itemsToBuy, itemsToBuyContainer, itemsToBuyTemplate, 1);
        RefreshListUI(itemsToSell, itemsToSellContainer, itemsToSellTemplate, 2);
        RefreshListUI(itemsToUpgrade, itemsToUpgradeContainer, itemsToUpgradeTemplate, 3);
    }


    void RefreshListUI(List<Item> Items, Transform container, Transform template, int buySellOrUpgrade = 1)
    {
        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }

        foreach (Item item in Items)
        {
            Transform itemInInventory = Instantiate(template, container);
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.name = item.itemName;
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(item));

            if (buySellOrUpgrade == 1)
            {
                itemInInventory.GetChild(3).GetComponent<TMP_Text>().text = "$ " + item.BuyValue;

                if (item.stackCount == 0)
                    itemInInventory.GetChild(2).GetComponent<Button>().interactable = false;
            }

            if (buySellOrUpgrade == 2)
            {
                itemInInventory.GetChild(3).GetComponent<TMP_Text>().text = "$ " + item.SellValue;
            }

            if (buySellOrUpgrade == 3)
            {
                if (item.Level < item.MaxLevel)
                {
                    itemInInventory.GetChild(2).GetComponent<Button>().interactable = true;
                    itemInInventory.GetChild(2).GetComponentInChildren<TMP_Text>().text = "UPGRADE";
                }
                else
                {
                    itemInInventory.GetChild(2).GetComponent<Button>().interactable = false;
                    itemInInventory.GetChild(2).GetComponentInChildren<TMP_Text>().text = "MAXED";
                }

                itemInInventory.GetChild(4).GetComponent<TMP_Text>().text = "$ " + (item.UpgradeCost);

                Transform upgradesContainer = itemInInventory.GetChild(3);
                Transform levelTemplate = itemInInventory.GetChild(3).GetChild(0);
                for (int i = 0; i < item.MaxLevel; i++)
                {
                    Transform level = Instantiate(levelTemplate, upgradesContainer);
                    level.gameObject.SetActive(true);
                    if (i < item.Level)
                        level.GetChild(0).gameObject.SetActive(true);
                }
            }

            Button theButton = itemInInventory.GetChild(2).GetComponent<Button>();
            theButton.onClick.RemoveAllListeners();
            switch (buySellOrUpgrade)
            {
                case 1:
                    theButton.onClick.AddListener(() => BuyItem(item));
                    break;
                case 2:
                    theButton.onClick.AddListener(() => SellItem(item));
                    break;
                case 3:
                    theButton.onClick.AddListener(() => UpgradeItem(item));
                    break;
            }
        }
    }

    void AddToStore(Item item)
    {
        item = Instantiate(item);
        if (itemsToBuy.Exists(x => x.itemName == item.itemName))
        {
            if (item.isStackable)
                itemsToBuy.Find(x => x.itemName == item.itemName).stackCount++;
            else if (!item.isStackable && itemsToBuy.Find(x => x.itemName == item.itemName).stackCount == 0)
                itemsToBuy.Find(x => x.itemName == item.itemName).stackCount++;
            else
                itemsToBuy.Add(item);
        }
        else
            itemsToBuy.Add(item);
    }


    public void SetDescription(Item item)
    {
        // Set item's texts
        itemSprite.sprite = item.sprite;
        itemName.text = item.itemName;
        itemDesc.text = item.itemDescription;
    }


    // Usable Function for buying, selling, and upgrading items
    void BuyItem(Item item)
    {
        // Check if can buy then subtract money
        if (GameEconomy.Instance.SpendMoney(item.BuyValue))
        {
            // Add Item to inventory
            Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(item.itemName, 1));
            // Remove/Disable from shop
            item.stackCount--;
        }
        OpenShop();
    }
    void SellItem(Item item)
    {
        // Add money
        GameEconomy.Instance.GainMoney(item.SellValue);
        // Sell how many
        // Remove from item
        Player_Inventory.Instance.RemoveItem(item);
        // Add item to shop stock ???
        if (item.stackCount <= 0) // sold item count will be zero
            item.stackCount = 1; // so make it 1 in the store
        AddToStore(item);
        OpenShop();
    }
    void UpgradeItem(Item item)
    {
        if (item.Level < item.MaxLevel)
        {
            // Check if money enough then subtract
            if (GameEconomy.Instance.SpendMoney(item.UpgradeCost))
                item.Level++; // Add item level
            OpenShop();
        }
    }
}
