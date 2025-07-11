using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RecipeDatabase;

public class CraftUI : MonoBehaviour
{
    #region UI References & Dependencies
    [Header("Dependencies")]
    [SerializeField] private PlayerData_SO playerData;
    [SerializeField] private InventoryUI inventoryUI; // Hubungkan jika perlu untuk refresh

    [Header("Panel Utama")]
    [SerializeField] private Button closeButton;

    [Header("Daftar Resep (Sebelah Kiri)")]
    [SerializeField] private Transform recipeListContent;
    [SerializeField] private GameObject recipeSlotTemplate;

    [Header("Detail Resep (Sebelah Kanan)")]
    [SerializeField] private GameObject[] ingredientSlots;
    [SerializeField] private GameObject resultSlot;
    [SerializeField] private Button craftButton;

    [Header("Popup Kuantitas")]
    [SerializeField] private GameObject quantityPopup;
    [SerializeField] private Image popupItemImage;
    [SerializeField] private TMP_Text popupItemCountText;
    [SerializeField] private Button plusButton, minusButton, maxButton, confirmButton, cancelButton;
    #endregion

    #region Private State
    // Variabel state dibuat private untuk keamanan dan enkapsulasi
    private CraftRecipe selectedRecipe;
    private ItemData itemObjectRemoved;
    private Item selectedResultItem; // Simpan item hasil untuk efisiensi
    private List<ItemData> selectedIngredients = new List<ItemData>(); // Simpan item bahan untuk efisiensi
    private int craftAmount;
    #endregion

    #region Initialization & UI Binding
    private void Awake()
    {
        // Pastikan playerData didapat dari PlayerController jika tidak di-set di Inspector
        if (playerData == null && PlayerController.Instance != null)
        {
            playerData = PlayerController.Instance.playerData;
        }
    }

    private void Start()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData_SO tidak ditemukan! Crafting tidak akan berfungsi.");
            return;
        }

        // Binding semua listener tombol
        closeButton.onClick.AddListener(CloseUI);
        craftButton.onClick.AddListener(ExecuteCraft);

        plusButton.onClick.AddListener(() => UpdateCraftAmount(1));
        minusButton.onClick.AddListener(() => UpdateCraftAmount(-1));
        maxButton.onClick.AddListener(SetMaxCraftAmount);
        confirmButton.onClick.AddListener(ConfirmRecipeSelection);
        cancelButton.onClick.AddListener(() => quantityPopup.SetActive(false));

        gameObject.SetActive(false);
        quantityPopup.SetActive(false);
    }
    #endregion

    #region UI Flow & Display Logic
    public void OpenUI()
    {
        Debug.Log("Membuka UI Crafting...");
        gameObject.SetActive(true);
        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
        RefreshRecipeList();
        ClearRecipeDetails();
    }

    private void CloseUI()
    {
        gameObject.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
    }

    private void RefreshRecipeList()
    {
        if (!recipeListContent.gameObject.activeInHierarchy)
        {
            Debug.LogError("recipeListContent (parent) tidak aktif!");
            return;
        }

        foreach (Transform child in recipeListContent)
        {
            if (child.gameObject != recipeSlotTemplate)
            {
                Destroy(child.gameObject);
            }
        }
        recipeSlotTemplate.SetActive(false);

        foreach (CraftRecipe recipe in RecipeDatabase.Instance.craftRecipes)
        {
            GameObject recipeSlotGO = Instantiate(recipeSlotTemplate, recipeListContent);
            recipeSlotGO.SetActive(true);

            Item itemResult = ItemPool.Instance.GetItem(recipe.result.itemName);

            // PERBAIKAN: Gunakan nama child yang benar, misal "ItemSlotTemplate"
            Transform imageTransform = recipeSlotGO.transform.Find("ItemSlotTemplate");
            if (imageTransform != null && itemResult != null)
            {
                imageTransform.GetComponent<Image>().sprite = itemResult.sprite;
            }

            Button recipeButton = recipeSlotGO.GetComponent<Button>();
            if (recipeButton != null)
            {
                recipeButton.onClick.AddListener(() => SelectRecipe(recipe));
            }
        }
    }

    private void SelectRecipe(CraftRecipe recipe)
    {
        selectedRecipe = recipe;
        craftAmount = 1;

        // Ambil dan simpan referensi item sekali saja untuk efisiensi
        selectedResultItem = ItemPool.Instance.GetItem(recipe.result.itemName);
        selectedIngredients.Clear();
        foreach (var ingredientData in recipe.ingredients)
        {
            selectedIngredients.Add(ingredientData);
        }

        if (selectedResultItem == null)
        {
            Debug.LogError($"Item hasil '{recipe.result.itemName}' tidak ditemukan di ItemPool!");
            return;
        }

        quantityPopup.SetActive(true);
        popupItemImage.sprite = selectedResultItem.sprite;
        popupItemCountText.text = craftAmount.ToString();
    }

    private void ConfirmRecipeSelection()
    {
        quantityPopup.SetActive(false);
        DisplaySelectedRecipe();
    }

    private void DisplaySelectedRecipe()
    {
        if (selectedRecipe == null) return;

        // Tampilkan bahan-bahan yang dibutuhkan
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            // Jika ada bahan untuk slot ini, tampilkan
            if (i < selectedRecipe.ingredients.Count)
            {
                ItemData ingredientData = selectedRecipe.ingredients[i];
                Item itemObject = ItemPool.Instance.GetItem(selectedIngredients[i].itemName);
                UpdateSlotUI(ingredientSlots[i], itemObject, ingredientData.count * craftAmount);
            }
            // Jika tidak, bersihkan slotnya (sembunyikan gambar & teks)
            else
            {
                ClearSlotUI(ingredientSlots[i]);
            }
        }

        // Tampilkan hasil
        UpdateSlotUI(resultSlot, selectedResultItem, selectedRecipe.result.count * craftAmount);
        CheckIngredientAvailability();
    }

    private void ClearRecipeDetails()
    {
        // Panggil ClearSlotUI untuk semua slot
        foreach (var slot in ingredientSlots)
        {
            ClearSlotUI(slot);
        }
        ClearSlotUI(resultSlot);

        craftButton.interactable = false;
        selectedRecipe = null;
    }
    #endregion

    #region Quantity Popup Logic
    private void UpdateCraftAmount(int change)
    {
        craftAmount = Mathf.Max(1, craftAmount + change);
        popupItemCountText.text = craftAmount.ToString();
        // Update tampilan resep setiap kali jumlah diubah
        DisplaySelectedRecipe();
    }

    private void SetMaxCraftAmount()
    {
        if (selectedRecipe == null) return;

        int maxPossible = int.MaxValue;
        for (int i = 0; i < selectedRecipe.ingredients.Count; i++)
        {
            ItemData ingredientData = selectedRecipe.ingredients[i];
            Item itemObject = ItemPool.Instance.GetItem(selectedIngredients[i].itemName);
            int owned = CountItemsInInventory(itemObject);
            int canMake = owned / ingredientData.count;
            if (canMake < maxPossible)
            {
                maxPossible = canMake;
            }
        }
        craftAmount = Mathf.Max(1, maxPossible);
        popupItemCountText.text = craftAmount.ToString();
        DisplaySelectedRecipe();
    }
    #endregion

    #region Core Crafting Logic
    private void CheckIngredientAvailability()
    {
        if (selectedRecipe == null)
        {
            craftButton.interactable = false;
            return;
        }

        bool canCraft = true;
        for (int i = 0; i < selectedRecipe.ingredients.Count; i++)
        {
            ItemData ingredientData = selectedRecipe.ingredients[i];
            Item itemObject = ItemPool.Instance.GetItem(selectedIngredients[i].itemName);
            if (CountItemsInInventory(itemObject) < ingredientData.count * craftAmount)
            {
                canCraft = false;
                break;
            }
        }
        craftButton.interactable = canCraft;
    }

    private void ExecuteCraft()
    {
        if (selectedRecipe == null || !craftButton.interactable) return;

        // Kurangi bahan dari inventory
        for (int i = 0; i < selectedRecipe.ingredients.Count; i++)
        {
            itemObjectRemoved = selectedIngredients[i];
            int countToRemove = selectedRecipe.ingredients[i].count * craftAmount;
            itemObjectRemoved.count = countToRemove;
            ItemPool.Instance.RemoveItemsFromInventory(itemObjectRemoved);
        }

        // Tambahkan hasil ke inventory
        ItemData resultData = new ItemData(
            selectedResultItem.itemName,
            selectedRecipe.result.count * craftAmount,
            selectedResultItem.quality // Asumsi Item punya quality
        );
        ItemPool.Instance.AddItem(resultData);

        Debug.Log($"Berhasil crafting {resultData.itemName} x{resultData.count}");

        // Refresh UI Inventory dan tutup jendela crafting
        MechanicController.Instance.HandleUpdateInventory();
        CloseUI();
    }
    #endregion

    #region Helper & Utility Functions
    // Di dalam CraftUI.cs

    private void UpdateSlotUI(GameObject slot, Item item, int requiredCount)
    {
        // --- Bagian yang sudah ada: Menampilkan Gambar ---
        Transform imageTransform = slot.transform.Find("ItemImage");
        if (imageTransform != null && item != null)
        {
            imageTransform.gameObject.SetActive(true);
            imageTransform.GetComponent<Image>().sprite = item.sprite;
        }

        if (slot == resultSlot)
        {
            Transform resultdTextTransform = slot.transform.Find("itemCount");
            if (resultdTextTransform != null)
            {
                resultdTextTransform.gameObject.SetActive(true);
                resultdTextTransform.GetComponent<TMP_Text>().text = popupItemCountText.text;
            }
            
        }

        // --- Bagian yang sudah ada: Menampilkan Jumlah yang DIBUTUHKAN ---
        Transform requiredTextTransform = slot.transform.Find("IngridientCount");
        if (requiredTextTransform != null)
        {
            requiredTextTransform.gameObject.SetActive(true);
            requiredTextTransform.GetComponent<TMP_Text>().text = requiredCount.ToString();
        }

        // =================================================================
        // --- LOGIKA BARU: Menampilkan Jumlah yang DIMILIKI ---
        // =================================================================

        // 1. Dapatkan jumlah item yang dimiliki pemain saat ini
        int ownedCount = CountItemsInInventory(item);

        // 2. Tentukan warna teks berdasarkan ketersediaan bahan
        //    Jika jumlah yang dimiliki >= jumlah yang dibutuhkan, warna putih. Jika tidak, warna merah.
        Color availabilityColor = (ownedCount >= requiredCount) ? Color.white : Color.red;

        // 3. Cari komponen teks "ItemInInventory"
        Transform ownedTextTransform = slot.transform.Find("ItemInInventory");
        if (ownedTextTransform != null)
        {
            TMP_Text ownedText = ownedTextTransform.GetComponent<TMP_Text>();

            // 4. Atur teks dan warnanya, lalu aktifkan
            ownedText.text = $"{ownedCount}"; // Formatnya menjadi "(15)" agar mudah dibedakan
            ownedText.color = availabilityColor;
            ownedText.gameObject.SetActive(true);
        }
    }



    // Fungsi-fungsi manajemen inventory
    private int CountItemsInInventory(Item itemToCount)
    {
        int totalCount = 0;
        foreach (ItemData slot in playerData.inventory)
        {
            if (slot.itemName == itemToCount.itemName)
            {
                totalCount += slot.count;
            }
        }
        return totalCount;
    }
    private void ClearSlotUI(GameObject slot)
    {
        // Cari anak bernama "ItemImage" lalu matikan
        Transform imageTransform = slot.transform.Find("ItemImage");
        if (imageTransform) imageTransform.gameObject.SetActive(false);

        // Cari anak bernama "IngridientCount" lalu matikan
        Transform requiredTextTransform = slot.transform.Find("IngridientCount");
        if (requiredTextTransform) requiredTextTransform.gameObject.SetActive(false);

        // LOGIKA BARU: Matikan juga teks ItemInInventory
        Transform ownedTextTransform = slot.transform.Find("ItemInInventory");
        if (ownedTextTransform) ownedTextTransform.gameObject.SetActive(false);
    }

    #endregion
}