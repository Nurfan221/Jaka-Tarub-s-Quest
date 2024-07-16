using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CraftUI : MonoBehaviour
{
    [System.Serializable]
    public class CraftRecipe
    {
        public Item result;
        public int resultCount = 1;
        public List<Item> ingredients;
        public List<int> ingredientsCount;
    }

    [SerializeField] List<CraftRecipe> recipes;

    [Header("Crafting List")]
    [SerializeField] Transform craftSlotContainer;
    [SerializeField] Transform craftSlotTemplate;

    [Header("Recipe Description")]
    [SerializeField] Transform resultTransform;
    [SerializeField] Button craftingButton;

    private void Start()
    {
        SetRecipeDescription(recipes[0], CanCraft(recipes[0]));
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

    public void OpenCraft()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

        RefreshRecipeItems();
    }

    void RefreshRecipeItems()
    {
        foreach (Transform child in craftSlotContainer)
        {
            if (child == craftSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        // Logic for flexible UI sizing
        Vector2 itemSlotCellSize = craftSlotTemplate.GetComponent<RectTransform>().sizeDelta;
        Vector2 correction = itemSlotCellSize / 2;
        int x = 0, y = 0;
        // ----------------------------

        foreach (CraftRecipe recipe in recipes)
        {
            Transform itemInInventory = Instantiate(craftSlotTemplate, craftSlotContainer);
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.name = recipe.result.itemName;

            RectTransform itemSlotRectTransform = itemInInventory.GetComponent<RectTransform>();
            itemSlotRectTransform.GetChild(0).GetComponent<Image>().sprite = recipe.result.sprite;
            for (int i = 0; i < 3; i++)
            {
                Image theImage = itemSlotRectTransform.GetChild(i + 1).GetComponent<Image>();
                TMP_Text theAmount = theImage.GetComponentInChildren<TMP_Text>();
                // Set ingredients UI accordingly, or empty
                if (recipe.ingredients.Count > i)
                {
                    theImage.sprite = recipe.ingredients[i].sprite;
                    theAmount.text = "X" + recipe.ingredientsCount[i];

                    // Set sprite opacity low if player don't have the ingredient
                    theImage.color = new(1, 1, 1,
                        Player_Inventory.Instance.itemList.Exists(x => x.itemName == recipe.ingredients[i].itemName) ? 1 : .3f);
                }
                else
                {
                    theImage.sprite = ItemPool.Instance.GetItem("Empty").sprite;
                    theAmount.text = string.Empty;
                }
            }

            // Set position of each item
            itemSlotRectTransform.anchoredPosition =
                new((x * itemSlotCellSize.x) + correction.x, (y * -itemSlotCellSize.y) - correction.y);

            itemInInventory.GetComponent<Button>().onClick.RemoveAllListeners();
            itemInInventory.GetComponent<Button>().onClick.AddListener(() => SetRecipeDescription(recipe, CanCraft(recipe)));
            y++;
        }
    }

    // Check if a recipe can be crafted
    bool CanCraft(CraftRecipe recipe)
    {
        // Variable helper to determine if all ingredients are in player's inventory
        int ingre = recipe.ingredients.Count;
        int match = 0;
        List<Item> playerInv = Player_Inventory.Instance.itemList;
        for (int i = 0; i < recipe.ingredients.Count; i++)
        {
            // Check if player have it and the amount is greater than needed
            if (playerInv.Exists(x => x.itemName == recipe.ingredients[i].itemName) &&
                playerInv.Find(x => x.itemName == recipe.ingredients[i].itemName).stackCount >= recipe.ingredientsCount[i])
            {
                // Increment match value if it is
                match++;
            }
        }
        // Return true if all ingredients are in player's inventory
        return match == ingre;
    }

    // Actually crafting the damn thing
    void Craft(CraftRecipe recipe)
    {
        SoundManager.Instance.PlaySound("Pick");
        // Add recipe result to player's inventory
        Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(recipe.result.itemName, recipe.resultCount));
        print("crafted " + recipe.result.itemName);
        // Remove ingredients from player's inventory
        int index = 0;
        foreach (Item item in recipe.ingredients)
        {
            for (int i = 0; i < recipe.ingredientsCount[index]; i++)
            {
                Player_Inventory.Instance.RemoveItem(item);
            }
            index++;
            print(recipe.ingredientsCount[index] + " " + item.itemName + " removed from inventory");
        }
        // Refresh the menu and set description to last crafted
        OpenCraft();
        SetRecipeDescription(recipe, CanCraft(recipe));
    }

    // To set recipe description UI
    void SetRecipeDescription(CraftRecipe recipe, bool canCraft)
    {
        resultTransform.GetChild(0).GetComponent<Image>().sprite = recipe.result.sprite;
        resultTransform.GetChild(1).GetComponent<TMP_Text>().text = recipe.result.itemName;
        resultTransform.GetChild(2).GetComponent<TMP_Text>().text = recipe.result.itemDescription;


        // Set the Craft button
        craftingButton.onClick.RemoveAllListeners();
        if (canCraft)
        {
            craftingButton.interactable = true;
            craftingButton.GetComponentInChildren<TMP_Text>().text = "CRAFT";
            craftingButton.onClick.AddListener(() => Craft(recipe));
        }
        else
        {
            craftingButton.interactable = false;
            craftingButton.GetComponentInChildren<TMP_Text>().text = "CAN'T CRAFT";
        }
    }

}
