using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory Instance; // Access this class by its Instance
    Player_Action pA;

    public List<Item> itemList;

    KeyCode switchWeapon = KeyCode.R;
    [HideInInspector] public bool meleeOrRanged = true;

    [Header("UI ELEMENTS")]
    public Item emptyItem;
    [HideInInspector] public List<Item> equippedCombat = new List<Item>(2);
    [HideInInspector] public Item equippedWeapon;
    [HideInInspector] public List<Item> quickSlots = new List<Item>(2);

    InventoryUI inventoryUI;
    [HideInInspector] public bool inventoryOpened;
    [HideInInspector] public bool inventoryClosed;

    [SerializeField] ParticleSystem healParticle;
    public Button inventoryButton;  // Drag and drop the button in the inspector
    public Button closeInventoryButton;  // Drag and drop the close button in the inspector

    private void Awake()
    {
        Instance = this;
        pA = GetComponent<Player_Action>();

        // Making sure everything in it is a clone
        List<Item> newList = new List<Item>();
        foreach (Item item in itemList)
        {
            newList.Add(Instantiate(item));
        }
        itemList.Clear();
        itemList = newList;

        emptyItem = Instantiate(emptyItem);
    }

    private void Start()
    {
        inventoryUI = PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>();
        inventoryButton.onClick.AddListener(ToggleInventory); // Add listener to button
        closeInventoryButton.onClick.AddListener(CloseInventory); // Add listener to close button
    }

   private void Update()
    {
        // Key to switch weapon
        if (GameController.Instance.enablePlayerInput)
        {
            if (Input.GetKeyDown(switchWeapon))
            {
                meleeOrRanged = !meleeOrRanged;
            }
        }

        // Use of different weapon
        if (meleeOrRanged && equippedCombat[0] != null)
        {
            equippedWeapon = equippedCombat[0];
            if (equippedWeapon != null && PlayerUI.Instance != null && PlayerUI.Instance.equippedUI != null)
            {
                PlayerUI.Instance.equippedUI.sprite = equippedWeapon.sprite;
            }
        }
        else if (equippedCombat[1] != null)
        {
            equippedWeapon = equippedCombat[1];
            if (equippedWeapon != null && PlayerUI.Instance != null && PlayerUI.Instance.equippedUI != null)
            {
                PlayerUI.Instance.equippedUI.sprite = equippedWeapon.sprite;
            }
        }
    }


    private void ToggleInventory()
{
    if (SoundManager.Instance != null)
        SoundManager.Instance.PlaySound("Click");

    inventoryOpened = !inventoryOpened;
    inventoryClosed = !inventoryOpened; // Update inventoryClosed
    PlayerUI.Instance.inventoryUI.SetActive(inventoryOpened);

    GameController.Instance.ShowPersistentUI(!inventoryOpened);

    if (inventoryOpened)
    {
        GameController.Instance.PauseGame();
        Instance.AddItem(ItemPool.Instance.GetItem("BuahCabai"));
        inventoryUI.UpdateInventoryUI(); // Update UI when inventory is opened
    }
    else
    {
        GameController.Instance.ResumeGame();
    }
}


    private void CloseInventory()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        inventoryOpened = false;
        inventoryClosed = true;
        PlayerUI.Instance.inventoryUI.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
    }

    public void AddItem(Item item)
    {
        item = Instantiate(item);
        if (item.isStackable && itemList.Exists(x => x.itemName == item.itemName))
        {
            itemList.Find(x => x.itemName == item.itemName).stackCount++;
        }
        else
        {
            itemList.Add(item);
        }

        print(item.itemName + " added to inventory");
         // Update the inventory UI
    PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>().UpdateInventoryUI();
    }

    public void RemoveItem(Item item)
    {
        item = Instantiate(item);
        if (item.isStackable)
        {
            itemList.Find(x => x.itemName == item.itemName).stackCount--;
            if (itemList.Find(x => x.itemName == item.itemName).stackCount <= 0)
                itemList.Remove(itemList.Find(x => x.itemName == item.itemName));
        }
        else
            itemList.Remove(itemList.Find(x => x.itemName == item.itemName));
    }

    public Item FindItemInInventory(string name)
    {
        if (itemList.Exists(x => x.itemName == name))
        {
            return itemList.Find(x => x.itemName == name);
        }
        return ItemPool.Instance.GetItem("Empty");
    }

    public void EquipItem(Item item, int index)
    {
        if (!itemList.Exists(x => x.itemName == item.itemName) && item.itemName != "Empty")
            return;

        equippedCombat[index] = item;
        PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>().SetActiveItem(index, item);
        print(item.itemName + " equipped");
    }

    // Add item to quick slot according index (0,1)
    public void AddQuickSlot(Item item, int index)
    {
        if (!itemList.Exists(x => x.itemName == item.itemName) && item.itemName != "Empty")
            return;

        switch (item.type)
        {
            case ItemType.Heal:
            case ItemType.Buff:
                break;
            default: break;
        }

        quickSlots[index] = item;
        PlayerUI.Instance.quickSlotsUI_Inventory[index].sprite = item.sprite;
        PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>().SetActiveItem(index + 2, item);
    }

    // Use which quick slot (1,2)
    public void UseQuickSlot(int which)
    {
        // Making sure there is an item in the quick slot
        Item item = quickSlots[which - 1];
        if (item == null || item.itemName == "Empty")
        {
            print("No item bish");
            return;
        }

        // Using them from inventory
        print("using quick slot " + (which - 1));
        item = itemList.Find(x => x.itemName == item.itemName);
        item.stackCount--;
        if (item.stackCount <= 0)
        {
            SoundManager.Instance.PlaySound("Eat");

            itemList.Remove(item);
            AddQuickSlot(emptyItem, which - 1);
        }

        // Have its effect
        switch (item.type)
        {
            case ItemType.Heal:
                // Heal Player
                print("HEALED");
                healParticle.Play();
                Player_Health.Instance.Heal(10);
                break;

            case ItemType.Buff:
                // Buff player
                print("BUFFED");
                break;

            default: break;
        }
    }
}
