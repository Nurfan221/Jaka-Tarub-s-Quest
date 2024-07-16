using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageUI : MonoBehaviour
{
    /// <summary>
    /// Store, take, storage limit
    /// </summary>

    StorageInteractable theStorage;
    List<Item> Items = new();

    [Header("Slots")]
    [SerializeField] Transform StorageContainer;
    [SerializeField] Transform InventoryContainer;
    [SerializeField] Transform itemSlotTemplate;

    [Header("Item Description")]
    [SerializeField] Image itemSprite;
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemDesc;
    [SerializeField] Button itemAction;

    // Need to refresh both inventory and storage slots

    private void Update()
    {
        // Close
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("Click");
            gameObject.SetActive(false);
            GameController.Instance.ShowPersistentUI(true);
            theStorage.Items = Items;
        }
    }

    public void OpenStorage(StorageInteractable theStorage, List<Item> Items)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

        this.theStorage = theStorage;
        this.Items = new();
        foreach (Item item in Items)
        {
            this.Items.Add(item);
        }
        RefreshInventoryItems();
    }

    void RefreshInventoryItems()
    {
        foreach (Transform child in StorageContainer)
        {
            if (child == itemSlotTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in InventoryContainer)
        {
            if (child == itemSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        // Set storage slots
        foreach (Item item in Items)
        {
            Transform itemInInventory = Instantiate(itemSlotTemplate, StorageContainer);
            itemInInventory.name = item.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(item, false));
        }

        // Set Inventory Slots
        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            Transform itemInInventory = Instantiate(itemSlotTemplate, InventoryContainer);
            itemInInventory.name = item.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(item, true));
        }
    }

    public void SetDescription(Item item, bool storeOrTake)
    {
        // Set item's texts
        itemSprite.sprite = item.sprite;
        itemName.text = item.itemName;
        itemDesc.text = item.itemDescription;

        // Set the button functionality
        itemAction.onClick.RemoveAllListeners();
        if (storeOrTake)
        {
            itemAction.onClick.AddListener(() => StoreItem(item));
            itemAction.GetComponentInChildren<TMP_Text>().text = "Store";
        }
        else
        {
            itemAction.onClick.AddListener(() => TakeItem(item));
            itemAction.GetComponentInChildren<TMP_Text>().text = "Take";
        }
        itemAction.onClick.AddListener(() => RefreshInventoryItems());

        if (item.stackCount <= 1)
        {
            if (storeOrTake)
                itemAction.onClick.AddListener(() => SetDescription(Items[0], false));
            else
                itemAction.onClick.AddListener(() => SetDescription(Player_Inventory.Instance.itemList[0], true));
        }
    }

    void StoreItem(Item item)
    {
        item = ItemPool.Instance.GetItem(item.itemName);

        // Remove item from inventory
        Player_Inventory.Instance.RemoveItem(item);

        // Add item to Storage
        if (item.isStackable && Items.Exists(x => x.itemName == item.itemName))
        {
            Items.Find(x => x.itemName == item.itemName).stackCount++;
        }
        else
        {
            item.stackCount = 1;
            Items.Add(item);
        }
    }

    void TakeItem(Item item)
    {
        item = ItemPool.Instance.GetItem(item.itemName);

        // Add to player inventory
        Player_Inventory.Instance.AddItem(item);

        // Remove from storage
        item = Items.Find(x => x.itemName == item.itemName);
        if (item.isStackable)
        {
            Items.Find(x => x.itemName == item.itemName).stackCount--;
            if (Items.Find(x => x.itemName == item.itemName).stackCount <= 0)
                Items.Remove(Items.Find(x => x.itemName == item.itemName));
        }
        else
            Items.Remove(Items.Find(x => x.itemName == item.itemName));
    }
}
