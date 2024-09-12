using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    List<Item> Items;

    [Header("Active Slot")]
    [SerializeField] Transform equippedItem1;
    [SerializeField] Transform equippedItem2;
    [SerializeField] Transform quickSlot1;
    [SerializeField] Transform quickSlot2;

    [Header("UI STUFF")]
    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate;

    [Header("Item Description")]
    [SerializeField] Image itemSprite;
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemDesc;
    [SerializeField] Button itemAction;

    [Header("Six Item Display")]
    [SerializeField] Transform ContentGO6; // New UI Content for 6 items
    [SerializeField] Transform SlotTemplate6; // New UI Slot Template for 6 items

    [SerializeField] private ContohFlipCard contohFlipCard;

    // [Header("item active in display")]
    // [SerializeField] private Image attackHUDImage;



    

    private void Start()
    {
        // Any initial setup can be added here
         
    }

    private void OnEnable()
    {
        // Pastikan referensi ke ContohFlipCard diatur saat InventoryUI diaktifkan
        contohFlipCard = FindObjectOfType<ContohFlipCard>();
        if (contohFlipCard == null)
        {
            Debug.LogError("ContohFlipCard tidak ditemukan di scene!");
        }
    }

    // Handle equipped items
    public void SetActiveItem(int slot, Item item)
    {
        Transform pickedSlot;
        switch (slot)
        {
            case 0: pickedSlot = equippedItem1; break;
            case 1: pickedSlot = equippedItem2; break;
            case 2: pickedSlot = quickSlot1; break;
            case 3: pickedSlot = quickSlot2; break;
            default: pickedSlot = equippedItem1; break;
        }

        pickedSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        pickedSlot.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();
      
    }

    public void UpdateInventoryUI()
    {
        SetInventory(Player_Inventory.Instance.itemList);
        if (Player_Inventory.Instance.itemList.Count > 0)
        {
            SetDescription(Player_Inventory.Instance.itemList[0]);
        }
        else
        {
            SetDescription(Player_Inventory.Instance.emptyItem);
        }
    }

    void RefreshActiveItems()
    {
        // Refresh equipped items
        RefreshItemSlot(equippedItem1, Player_Inventory.Instance.equippedCombat[0]);
        RefreshItemSlot(equippedItem2, Player_Inventory.Instance.equippedCombat[1]);

        // Refresh quick slots
        RefreshItemSlot(quickSlot1, Player_Inventory.Instance.quickSlots[0]);
        RefreshItemSlot(quickSlot2, Player_Inventory.Instance.quickSlots[1]);
    }

    void RefreshItemSlot(Transform slot, Item item)
    {
        slot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        slot.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();
    }

    public void SetInventory(List<Item> items)
    {
        this.Items = items;
        RefreshInventoryItems();
        UpdateSixItemDisplay();
    }

    void RefreshInventoryItems()
    {
        RefreshActiveItems();
        foreach (Transform child in ContentGO)
        {
            if (child == SlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Item item in Items)
        {
            Transform itemInInventory = Instantiate(SlotTemplate, ContentGO);
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.gameObject.name = item.itemName;
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetDescription(item));
        }
    }

    void UpdateSixItemDisplay()
    {
        // Clear existing items in the 6-item display
        foreach (Transform child in ContentGO6)
        {
            if (child == SlotTemplate6) continue;
            Destroy(child.gameObject);
        }

        int itemCount = Mathf.Min(6, Items.Count);
        for (int i = 0; i < itemCount; i++)
        {
            Item item = Items[i];
            Transform itemInDisplay = Instantiate(SlotTemplate6, ContentGO6);
            itemInDisplay.gameObject.SetActive(true);
            itemInDisplay.gameObject.name = item.itemName;
            itemInDisplay.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInDisplay.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

            itemInDisplay.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInDisplay.GetComponent<Button>().onClick.AddListener(() => SetDescription(item));
        }
    }

    public void SetDescription(Item item)
{
    // Set item's texts
    itemSprite.sprite = item.sprite;
    itemName.text = item.itemName;
    itemDesc.text = item.itemDescription;

    // Set the "equip" button functionality
     itemAction.onClick.RemoveAllListeners();
    itemAction.onClick.AddListener(() =>
    {
        Debug.Log("itemAction clicked");
        if (contohFlipCard != null)
        {
            Debug.Log("yeeeayy kesambung");
            contohFlipCard.ShowDescription();
        }
        else
        {
            Debug.LogError("ga kesambung kontol !!!!");
        }
    });

    switch (item.type)
    {
        case ItemType.Melee_Combat:
            itemAction.onClick.AddListener(() =>
            {
                Player_Inventory.Instance.EquipItem(item, 0);
                // SoundManager.Instance.PlaySound("PickUp");
            });
            break;
        case ItemType.Ranged_Combat:
            itemAction.onClick.AddListener(() =>
            {
                Player_Inventory.Instance.EquipItem(item, 1);
                // SoundManager.Instance.PlaySound("PickUp");
            });
            break;
        case ItemType.Heal:
            itemAction.onClick.AddListener(() =>
            {
                Player_Inventory.Instance.AddQuickSlot(item, 0);
                // SoundManager.Instance.PlaySound("PickUp");
            });
            break;
        case ItemType.Buff:
            itemAction.onClick.AddListener(() =>
            {
                Player_Inventory.Instance.AddQuickSlot(item, 1);
                // SoundManager.Instance.PlaySound("PickUp");
            });
            break;
        default:
            break;
    }

    // Set the "Equip" button according to item's type
    string itemUses;
    if (item.type == ItemType.Item)
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

}
