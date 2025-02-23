using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class Craft : MonoBehaviour
{
    [Header("Database Crafting")]
    [SerializeField] private RecipeDatabase recipeDatabaseInstance;

    [Header("Kategori Item yang Valid")]
    public ItemCategory[] validCategories = {
        ItemCategory.Fruit,
        ItemCategory.Meat,
        ItemCategory.Vegetable,
        ItemCategory.Crafting_Material
    };

    [Header("Inventory Container")]
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private Transform itemSlotTemplate;

    [Header("Button Container")]
    [SerializeField] private Button buttonClose;

    [Header("Craft container")]
    public List<Item> itemsInCraft = new List<Item>(); // Item yang dimasukkan ke crafting slot
    public List<GameObject> itemCraftSlots = new List<GameObject>(); // Slot UI untuk crafting
    public GameObject hasilCraftSlot;

    // Menambahkan UI untuk slot crafting
    public GameObject ItemCraft1;
    public GameObject ItemCraft2;
    public GameObject ItemCraft3;
    public GameObject ItemCraft4;

    private bool hasilCraftValue = false; // Variabel untuk status crafting

    void Start()
    {
        if (buttonClose != null)
        {
            buttonClose.onClick.AddListener(CloseCraftUI);
            Debug.Log("Tombol close listener added.");
        }
        else
        {
            Debug.LogError("Tombol close belum terhubung");
        }
    }



    public void OpenCraft()
    {
        Debug.Log("craft active");
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

            if (validCategories.Any(category => item.IsInCategory(category)))
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
        item = ItemPool.Instance.GetItem(item.itemName);
        Player_Inventory.Instance.RemoveItem(item);

        Item existingItem = itemsInCraft.FirstOrDefault(i => i.itemName == item.itemName);
        if (existingItem != null)
        {
            existingItem.stackCount++;
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
        // Bersihkan slot crafting
        ClearSlot(ItemCraft1);
        ClearSlot(ItemCraft2);
        ClearSlot(ItemCraft3);
        ClearSlot(ItemCraft4);
        ClearSlot(hasilCraftSlot);

        // Tempatkan item ke slot crafting berdasarkan urutan
        if (itemsInCraft.Count > 0) UpdateSlot(ItemCraft1, itemsInCraft[0]);
        if (itemsInCraft.Count > 1) UpdateSlot(ItemCraft2, itemsInCraft[1]);
        if (itemsInCraft.Count > 2) UpdateSlot(ItemCraft3, itemsInCraft[2]);
        if (itemsInCraft.Count > 3) UpdateSlot(ItemCraft4, itemsInCraft[3]);

        StartCrafting();
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
    }

    public void StartCrafting()
    {
        ClearSlot(hasilCraftSlot);
        hasilCraftValue = false;

        foreach (RecipeDatabase.CraftRecipe recipe in recipeDatabaseInstance.craftRecipes)
        {
            if (SomeMethod(recipe))
            {
                UpdateCraftResultUI(recipe.result);
                break; // Selesai crafting jika sudah menemukan resep yang cocok
            }
        }

        if (!hasilCraftValue)
        {
            Debug.Log("Tidak ada resep yang cocok.");
        }
    }

    public bool SomeMethod(RecipeDatabase.CraftRecipe recipe)
    {
        if (itemsInCraft.Count != recipe.ingredients.Count)
            return false;

        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            Item requiredItem = recipe.ingredients[i];
            int requiredCount = recipe.ingredientsCount[i];

            Item itemInCraft = itemsInCraft.FirstOrDefault(item => item.itemName == requiredItem.itemName);
            if (itemInCraft == null || itemInCraft.stackCount < requiredCount)
                return false;
        }

        return true;
    }

    public void UpdateCraftResultUI(Item result)
    {
        Transform theItem = Instantiate(itemSlotTemplate, hasilCraftSlot.transform);
        theItem.name = result.itemName;
        theItem.gameObject.SetActive(true);

        theItem.GetChild(0).GetComponent<Image>().sprite = result.sprite;
        theItem.GetChild(1).GetComponent<TMP_Text>().text = result.stackCount.ToString();

        theItem.GetComponent<DragCook>().itemName = result.itemName;
        theItem.GetComponent<Button>().onClick.AddListener(() => ReturnItemCraftToInventory(result));
    }

    public void ReturnItemCraftToInventory(Item result)
    {
        foreach (RecipeDatabase.CraftRecipe recipe in recipeDatabaseInstance.craftRecipes)
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

        Player_Inventory.Instance.AddItem(result);
        UpdateCraft();
        RefreshSlots();
    }
}

