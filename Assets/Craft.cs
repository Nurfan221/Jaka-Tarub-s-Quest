using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Craft : MonoBehaviour
{
    [System.Serializable]
    public class CraftRecipe
    {
        public List<Item> ingredients;        // Daftar item yang dibutuhkan untuk resep
        public List<int> ingredientsCount;    // Jumlah item yang dibutuhkan untuk setiap ingredient
        public Item result;                   // Item hasil craft
    }

    public List<CraftRecipe> recipes;

    [Header("Inventory Container")]
    [SerializeField] Transform itemSlotContainer;
    [SerializeField] Transform itemSlotTemplate;

    [Header("Button Container")]
    public Button buttonClose;

    [Header("Craft container")]
    public List<Item> itemsInCraft = new List<Item>(); // menyimpan item

    public GameObject ItemCraft1;
    public GameObject ItemCraft2;
    public GameObject ItemCraft3;
    public GameObject ItemCraft4;

    public GameObject hasilCraft;
    public bool ItemCraft1Value = false;
    public bool ItemCraft2Value = false;
    public bool ItemCraft3Value = false;
    public bool ItemCraft4Value = false;
    public bool ItemCraft5Value = false;
    public bool hasilCraftValue = false;

    void Start()
    {
        if (buttonClose != null)
        {
            buttonClose.onClick.AddListener(CloseCraftUI);
            Debug.Log("Tombol close listener added.");
        }
        else
        {
            Debug.Log("Tombol close belum terhubung");
        }
    }

    void Update() {}

    public void OpenCraft()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

        RefreshSlots();
    }

    public void RefreshSlots()
    {
        foreach (Transform child in itemSlotContainer)
        {
            if (child == itemSlotTemplate)
                continue;
            Destroy(child.gameObject);
        }

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            Transform theItem = Instantiate(itemSlotTemplate, itemSlotContainer);
            theItem.name = item.itemName;
            theItem.gameObject.SetActive(true);
            theItem.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            theItem.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            theItem.GetComponent<DragCook>().itemName = item.itemName;

            if (item.category == ItemCategory.Fruit || 
                item.category == ItemCategory.Meat || 
                item.category == ItemCategory.Vegetable ||
                item.category == ItemCategory.Crafting_Material)
            {
                theItem.GetComponent<Button>().onClick.AddListener(() => MoveToCraft(item));
            }
        }
    }

    private void CloseCraftUI()
    {
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
        Debug.Log("Crafting ditutup");

        RefreshSlots();
    }

    public void MoveToCraft(Item item)
    {
        Debug.Log("MoveToCraft successful");

        item = ItemPool.Instance.GetItem(item.itemName);
        Player_Inventory.Instance.RemoveItem(item);

        Item existingItem = itemsInCraft.FirstOrDefault(i => i.itemName == item.itemName);

        if (existingItem != null)
        {
            existingItem.stackCount++;
            Debug.Log("Item ada lebih dari 1");
        }
        else
        {
            item.stackCount = 1;
            itemsInCraft.Add(item);
        }

        UpdateCraft();
        RefreshSlots();
    }

    public void UpdateCraft()
    {
        ClearSlot(ItemCraft1);
        ClearSlot(ItemCraft2);
        ClearSlot(ItemCraft3);
        ClearSlot(ItemCraft4);
        ClearSlot(hasilCraft);

        if (itemsInCraft.Count > 0)
        {
            UpdateSlot(ItemCraft1, itemsInCraft[0]);
            StartCrafting();
            ItemCraft1Value = true;
        }

        if (itemsInCraft.Count > 1)
        {
            UpdateSlot(ItemCraft2, itemsInCraft[1]);
            StartCrafting();
            ItemCraft2Value = true;
        }

        if (itemsInCraft.Count > 2)
        {
            UpdateSlot(ItemCraft3, itemsInCraft[2]);
            StartCrafting();
            ItemCraft3Value = true;
        }

        if (itemsInCraft.Count > 3)
        {
            UpdateSlot(ItemCraft4, itemsInCraft[3]);
            StartCrafting();
            ItemCraft4Value = true;
        }
    }

    private void ClearSlot(GameObject itemCraftSlot)
    {
        foreach (Transform child in itemCraftSlot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateSlot(GameObject itemCraftSlot, Item item)
    {
        Transform theItem = Instantiate(itemSlotTemplate, itemCraftSlot.transform);
        theItem.name = item.itemName;
        theItem.gameObject.SetActive(true);

        theItem.GetChild(0).GetComponent<Image>().sprite = item.sprite;
        theItem.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();

        theItem.GetComponent<DragCook>().itemName = item.itemName;
        theItem.GetComponent<Button>().onClick.AddListener(() => ReturnItemToInventory(item));
    }

    public void ReturnItemToInventory(Item item)
    {
        Player_Inventory.Instance.AddItem(item);
        itemsInCraft.Remove(item);

        UpdateCraft();
        RefreshSlots();
        ItemCraft1Value = false;
    }

    public void StartCrafting()
    {
        ClearSlot(hasilCraft);
        hasilCraftValue = true;

        HashSet<string> craftedItems = new HashSet<string>();

        foreach (CraftRecipe recipe in recipes)
        {
            if (itemsInCraft.Count != recipe.ingredients.Count)
            {
                Debug.Log("Jumlah item dalam craft tidak sesuai dengan resep.");
                continue;
            }

            bool isRecipeMatch = true;
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                Item ingredient = recipe.ingredients[i];
                int requiredCount = recipe.ingredientsCount[i];

                Item craftedItem = itemsInCraft.FirstOrDefault(item => item.itemName == ingredient.itemName);

                if (craftedItem == null || craftedItem.stackCount < requiredCount)
                {
                    isRecipeMatch = false;
                    Debug.Log("Item atau jumlah item tidak sesuai dengan resep. " + i);
                    break;
                }
            }

            if (isRecipeMatch && hasilCraftValue)
            {
                hasilCraftValue = true;
                if (hasilCraftValue)
                {
                    UpdateCraftResultUI(recipe.result);
                }
            }
        }

        Debug.Log("Tidak ada resep yang cocok.");
    }

    public void UpdateCraftResultUI(Item result)
    {
        hasilCraftValue = false;

        Transform theItem = Instantiate(itemSlotTemplate, hasilCraft.transform);
        theItem.name = result.itemName;
        theItem.gameObject.SetActive(true);

        theItem.GetChild(0).GetComponent<Image>().sprite = result.sprite;
        theItem.GetChild(1).GetComponent<TMP_Text>().text = result.stackCount.ToString();

        theItem.GetComponent<DragCook>().itemName = result.itemName;
        theItem.GetComponent<Button>().onClick.AddListener(() => ReturnItemCraftToInventory(result));
    }

    public void ReturnItemCraftToInventory(Item result)
    {
        hasilCraftValue = false;

        foreach (CraftRecipe recipe in recipes)
        {
            if (recipe.result.itemName == result.itemName)
            {
                for (int i = 0; i < recipe.ingredients.Count; i++)
                {
                    Item ingredient = recipe.ingredients[i];
                    int requiredCount = recipe.ingredientsCount[i];

                    Item craftedItem = itemsInCraft.FirstOrDefault(item => item.itemName == ingredient.itemName);

                    if (craftedItem != null)
                    {
                        craftedItem.stackCount -= requiredCount;

                        if (craftedItem.stackCount <= 0)
                        {
                            itemsInCraft.Remove(craftedItem);
                        }
                    }
                }
            }
        }

        Debug.Log("ReturnItemCraft");

        Player_Inventory.Instance.AddItem(result);

        UpdateCraft();
        RefreshSlots();
        ItemCraft1Value = false;
    }
}
