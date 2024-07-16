using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropCookSlot : MonoBehaviour, IDropHandler
{
    public GameObject item; // Item yang saat ini ada di slot
    public string slotName; // Nama slot, misalnya "SlotCook1" atau "SlotCook2"

    public GameObject hasilCook; // Referensi ke GameObject HasilCook

    [SerializeField] CookUI cookUI;
    [SerializeField] CookIngredients cookIngredients;

    private Button hasilCookButton;

    private void Start()
    {
        // Menetapkan inventorySlots dari CookUI jika belum dilakukan
        if (cookUI == null || cookUI.inventorySlots == null)
        {
            Debug.LogError("CookUI or inventorySlots in CookUI is not assigned!");
        }

        // Mendapatkan komponen Button dari hasilCook
        hasilCookButton = hasilCook.GetComponent<Button>();
        if (hasilCookButton == null)
        {
            Debug.LogError("Button component on hasilCook is not assigned!");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedItem = eventData.pointerDrag;
        DragCook dragCook = draggedItem.GetComponent<DragCook>();

        if (dragCook != null)
        {
            if (!item)
            {
                cookIngredients.DestroyRecipeInHasilCook();
                RemoveFromPreviousSlot(draggedItem);
                SetItemToSlot(draggedItem);
                CreateCook();
            }
            else
            {
                dragCook.ResetPosition();
            }
        }
    }

    private void RemoveFromPreviousSlot(GameObject draggedItem)
    {
        DropCookSlot[] slots = transform.parent.GetComponentsInChildren<DropCookSlot>();
        foreach (DropCookSlot slot in slots)
        {
            if (slot.item == draggedItem)
            {
                slot.item = null;
            }
        }
    }

    private void SetItemToSlot(GameObject draggedItem)
    {
        RectTransform itemRectTransform = draggedItem.GetComponent<RectTransform>();
        itemRectTransform.SetParent(transform);
        itemRectTransform.anchoredPosition = Vector3.zero;
        item = draggedItem;
    }

    public void CreateCook()
    {
        bool recipeFound = false;

        DragCook item1 = cookUI.slotCook1.item != null ? cookUI.slotCook1.item.GetComponent<DragCook>() : null;
        DragCook item2 = cookUI.slotCook2.item != null ? cookUI.slotCook2.item.GetComponent<DragCook>() : null;

        Debug.Log($"Item1: {item1?.itemName}, Item2: {item2?.itemName}");

        foreach (var recipe in cookUI.recipes)
        {
            Debug.Log($"Checking recipe for result: {recipe.result.itemName}");

            if (recipe.ingredients.Count == 1)
            {
                if (item1 != null && item1.itemName == recipe.ingredients[0].itemName && item2 == null)
                {
                    recipeFound = true;
                    DisplayCookResult(recipe);
                    break; // Exit the loop once a matching recipe is found
                }
            }
            else if (recipe.ingredients.Count == 2)
            {
                if (item1 != null && item2 != null &&
                    ((item1.itemName == recipe.ingredients[0].itemName && item2.itemName == recipe.ingredients[1].itemName) ||
                     (item2.itemName == recipe.ingredients[0].itemName && item1.itemName == recipe.ingredients[1].itemName)))
                {
                    recipeFound = true;
                    DisplayCookResult(recipe);
                    break; // Exit the loop once a matching recipe is found
                }
            }
        }

        if (!recipeFound)
        {
            Debug.Log("No matching recipe found. Cancel cooking.");
            CancelCook();
        }
    }

    public void DisplayCookResult(CookUI.CookRecipe recipe)
    {
        CancelCook();
        string resultDetails = $"Result: {recipe.result.itemName}";
        Debug.Log("Displaying result: " + resultDetails);

        GameObject resultItem = new GameObject(recipe.result.itemName);
        Image imageComponent = resultItem.AddComponent<Image>();
        imageComponent.sprite = recipe.result.sprite;
        resultItem.transform.SetParent(hasilCook.transform, false);
        RectTransform resultRectTransform = resultItem.GetComponent<RectTransform>();
        resultRectTransform.sizeDelta = new Vector2(270, 270);
        resultRectTransform.anchoredPosition = Vector3.zero;
        resultRectTransform.rotation = Quaternion.identity;

        hasilCookButton.onClick.RemoveAllListeners();
        hasilCookButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("Pick");
            Item resultItemInstance = ItemPool.Instance.GetItem(recipe.result.itemName);

            if (resultItemInstance != null)
            {
                Player_Inventory.Instance.AddItem(resultItemInstance);
                Destroy(resultItem);
                RemoveItemsFromSlots();
            }
            else
            {
                Debug.LogError("Item hasil masakan tidak ditemukan di ItemPool!");
            }
        });
    }

    public void CancelCook()
    {
        if (hasilCook.transform.childCount > 0)
        {
            Destroy(hasilCook.transform.GetChild(0).gameObject);
        }
        hasilCookButton.onClick.RemoveAllListeners();
    }

    private void RemoveItemsFromSlots()
    {
        if (cookUI.slotCook1.item != null)
        {
            Player_Inventory.Instance.RemoveItem(ItemPool.Instance.GetItem(cookUI.slotCook1.item.GetComponent<DragCook>().itemName));
            Destroy(cookUI.slotCook1.item);
            cookUI.slotCook1.item = null;
        }

        if (cookUI.slotCook2.item != null)
        {
            Player_Inventory.Instance.RemoveItem(ItemPool.Instance.GetItem(cookUI.slotCook2.item.GetComponent<DragCook>().itemName));
            Destroy(cookUI.slotCook2.item);
            cookUI.slotCook2.item = null;
        }

        cookUI.RefreshSlots();
        CreateCook();
    }

    public void ReturnToInventory()
    {
        if (item != null)
        {
            item.transform.SetParent(cookUI.inventorySlots.transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            item = null;

            CreateCook();
        }
    }
}
