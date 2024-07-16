using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour // Attach this to the InventoryUI prefab
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

    private void Start()
    {
        
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
        Item item = Player_Inventory.Instance.equippedCombat[0];
        equippedItem1.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        equippedItem1.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();

        item = Player_Inventory.Instance.equippedCombat[1];
        equippedItem2.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        equippedItem2.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();

        item = Player_Inventory.Instance.quickSlots[0];
        quickSlot1.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        quickSlot1.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();

        item = Player_Inventory.Instance.quickSlots[1];
        quickSlot2.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        quickSlot2.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount <= 0 ? "" : item.stackCount.ToString();
    }

    // Handle opened inventory with its items
    public void SetInventory(List<Item> Items)
    {
        this.Items = Items;
        RefreshInventoryItems();
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

    public void SetDescription(Item item)
    {
        // Set item's texts
        itemSprite.sprite = item.sprite;
        itemName.text = item.itemName;
        itemDesc.text = item.itemDescription;

        // Set the "equip" button functionality
        itemAction.onClick.RemoveAllListeners();
        switch (item.type)
        {
            case ItemType.Melee_Combat:
                itemAction.onClick.AddListener(() =>
                {
                    Player_Inventory.Instance.EquipItem(item, 0);
                    SoundManager.Instance.PlaySound("PickUp");
                });
                break;
            case ItemType.Ranged_Combat:
                itemAction.onClick.AddListener(() =>
                {
                    Player_Inventory.Instance.EquipItem(item, 1);
                    SoundManager.Instance.PlaySound("PickUp");
                });
                break;
            case ItemType.Heal:
                itemAction.onClick.AddListener(() =>
                {
                    Player_Inventory.Instance.AddQuickSlot(item, 0);
                    SoundManager.Instance.PlaySound("PickUp");
                });
                break;
            case ItemType.Buff:
                itemAction.onClick.AddListener(() =>
                {
                    Player_Inventory.Instance.AddQuickSlot(item, 1);
                    SoundManager.Instance.PlaySound("PickUp");
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
