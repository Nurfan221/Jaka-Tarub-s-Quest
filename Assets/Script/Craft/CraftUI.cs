using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RecipeDatabase;

public class CraftUI : MonoBehaviour
{
    #region UI References & Dependencies
    [Header("Dependencies")]
    public PlayerData_SO playerData;

    [Header("Panel Utama")]
    public Button closeButton;

    [Header("Daftar Resep")]
    public Transform recipeListContent;
    public GameObject recipeSlotTemplate;

    [Header("Detail Resep")]
    public GameObject[] ingredientSlots;
    public GameObject resultSlot;
    public Button craftButton;

    [Header("Popup Kuantitas")]
    //[SerializeField] private QuantityPopupUI quantityPopup;
    #endregion

    #region Private State
    private CraftRecipe selectedRecipe;
    private Item selectedResultItem;
    private int selectedCraftAmount;
    #endregion

    #region Initialization & UI Binding

    private void Awake()
    {

        if (playerData == null && PlayerController.Instance != null)
        {
            playerData = PlayerController.Instance.playerData;
        }
    }
    private void Start()
    {
        if (playerData == null)
        {
            // ... (logika null check playerData Anda sudah benar) ...
            return;
        }

        closeButton.onClick.AddListener(CloseUI);
        craftButton.onClick.AddListener(ExecuteCraft);

        // Lakukan null check sebelum menambahkan listener
        if (QuantityPopupUI.Instance != null)
        {
            // Gunakan nama fungsi yang benar: HandlePopupConfirm dan HandlePopupCancel
            QuantityPopupUI.Instance.onConfirm.AddListener(HandlePopupConfirm);
            QuantityPopupUI.Instance.onCancel.AddListener(HandlePopupCancel);
        }
        else
        {
            Debug.LogError("Referensi ke QuantityPopupUI.Instance adalah null saat Start(). Pastikan objek popup ada di scene.");
        }
      
    }
    #endregion

    #region UI Flow & Display Logic
    public void OpenUI()
    {
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
        Debug.Log("Menyegarkan daftar resep...");
        foreach (Transform child in recipeListContent)
        {
            if (child.gameObject != recipeSlotTemplate) Destroy(child.gameObject);
        }
        recipeSlotTemplate.SetActive(false);

        foreach (CraftRecipe recipe in RecipeDatabase.Instance.craftRecipes)
        {
            CraftRecipe localRecipe = recipe;
            Item itemUse = ItemPool.Instance.GetItemWithQuality(localRecipe.result.itemName, localRecipe.result.quality);
            GameObject recipeSlotGO = Instantiate(recipeSlotTemplate, recipeListContent);
            recipeSlotGO.SetActive(true);

            //Cek apakah resep ini bisa dibuat (minimal 1)
            bool canCraft = IsRecipeCraftable(localRecipe);

            // Dapatkan komponen Image dari slot resep
            //Image slotImage = recipeSlotGO.GetComponent<Image>();
            Image itemImage = recipeSlotGO.transform.Find("ItemImage").GetComponent<Image>();
            if (itemImage == null)
            {
                Debug.Log("Item Image tidak ditemukan");
            }else
            {
                Debug.Log("Item Image ditemukan");
                itemImage.sprite = itemUse.sprite; // Pastikan sprite hasil ditampilkan
            }


            //Atur warnanya berdasarkan ketersediaan
            if (canCraft)
            {
                // Warna normal jika bisa dibuat
                recipeSlotGO.GetComponent<Button>().interactable = true; // Aktifkan interaksi
            }
            else
            {
                // Warna redup/abu-abu jika tidak bisa
                recipeSlotGO.GetComponent<Button>().interactable = false; // Nonaktifkan interaksi
                                                                          //recipeSlotGO.interactab
            }

            // ... (sisa kode untuk menampilkan sprite dan listener) ...
            recipeSlotGO.GetComponent<Button>().onClick.AddListener(() => OnRecipeSlotClicked(localRecipe, canCraft));
        }
    }
    // Buat metode baru ini
    private void OnRecipeSlotClicked(CraftRecipe recipe, bool isCraftable)
    {
        if (isCraftable)
        {
            // Jika bisa dibuat, lanjutkan ke pemilihan jumlah
            SelectRecipe(recipe);
        }
        else
        {
            // Jika tidak, beri feedback ke pemain
            Debug.Log("Bahan tidak cukup untuk membuat item ini!");
            // Di sini Anda bisa memunculkan pesan di UI atau memutar suara error
            // Contoh: UIMessageManager.Instance.ShowMessage("Bahan tidak cukup!");
        }
    }
    private bool IsRecipeCraftable(CraftRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (CountItemsInInventory(ItemPool.Instance.GetItem(ingredient.itemName)) < ingredient.count)
            {
                return false; // Jika ada satu saja bahan yang kurang, langsung return false
            }
        }
        return true;
    }

    private void SelectRecipe(CraftRecipe recipe)
    {
        Debug.Log($"Memilih resep: {recipe.result.itemName}");
        selectedRecipe = recipe;
        selectedResultItem = ItemPool.Instance.GetItem(recipe.result.itemName);
        if (selectedResultItem == null) return;

        int maxPossible = CalculateMaxCraftAmount();
        if (maxPossible <= 0)
        {
            Debug.Log("Tidak memiliki bahan yang cukup untuk membuka popup.");
            // Di sini Anda bisa memberikan feedback ke pemain, misal tombol resep berwarna merah
            return;
        }

        QuantityPopupUI.Instance.Show(selectedResultItem.sprite, 1, maxPossible);
    }

    public void HandlePopupConfirm(int amount)
    {
        selectedCraftAmount = amount;
        DisplaySelectedRecipe();
    }

    public void HandlePopupCancel()
    {
        ClearRecipeDetails();
    }

    private void DisplaySelectedRecipe()
    {
        if (selectedRecipe == null) return;

        // Tampilkan bahan-bahan yang dibutuhkan
        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            if (i < selectedRecipe.ingredients.Count)
            {
                ItemData ingredientData = selectedRecipe.ingredients[i];
                Item itemObject = ItemPool.Instance.GetItem(ingredientData.itemName);
                // --- PERBAIKAN --- Menggunakan selectedCraftAmount
                int requiredCount = ingredientData.count * selectedCraftAmount;
                UpdateSlotUI(ingredientSlots[i], itemObject, requiredCount);
            }
            else
            {
                ClearSlotUI(ingredientSlots[i]);
            }
        }

        // Tampilkan hasil
        // --- PERBAIKAN --- Menggunakan selectedCraftAmount
        int resultCount = selectedRecipe.result.count * selectedCraftAmount;
        UpdateSlotUI(resultSlot, selectedResultItem, resultCount);
        CheckIngredientAvailability();
    }

    private void ClearRecipeDetails()
    {
        foreach (var slot in ingredientSlots) ClearSlotUI(slot);
        ClearSlotUI(resultSlot);
        craftButton.interactable = false;
        selectedRecipe = null;
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
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            if (CountItemsInInventory(ItemPool.Instance.GetItem(ingredient.itemName)) < ingredient.count * selectedCraftAmount)
            {
                canCraft = false;
                break;
            }
        }
        craftButton.interactable = canCraft; // Ini baris kuncinya
    }

    private void ExecuteCraft()
    {
        if (selectedRecipe == null || !craftButton.interactable) return;

        // Kurangi bahan dari inventory
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            // --- PERBAIKAN --- Menggunakan selectedCraftAmount
            int countToRemove = ingredient.count * selectedCraftAmount;
            ItemData itemToRemove = new ItemData(ingredient.itemName, countToRemove, ingredient.quality);
            ItemPool.Instance.RemoveItemsFromInventory(itemToRemove);
        }

        // Tambahkan hasil ke inventory
        ItemData resultData = new ItemData(
            selectedResultItem.itemName,
            selectedRecipe.result.count * selectedCraftAmount,
            selectedResultItem.quality
        );
        ItemPool.Instance.AddItem(resultData);

        Debug.Log($"Berhasil crafting {resultData.itemName} x{resultData.count}");
        MechanicController.Instance.HandleUpdateInventory();
        CloseUI();
    }
    #endregion

    #region Helper & Utility Functions
    private void UpdateSlotUI(GameObject slot, Item item, int requiredCount)
    {
        if (item == null || slot == null) return;

        // --- Menampilkan Gambar Item ---
        Transform imageTransform = slot.transform.Find("ItemImage"); // Asumsi nama child tetap "ItemImage"
        imageTransform.gameObject.SetActive(true);
        imageTransform.GetComponent<Image>().sprite = item.sprite;

        // --- Menampilkan Jumlah (Baik untuk Hasil maupun Bahan) ---
        // Mari kita gunakan satu nama yang konsisten, misalnya "IngridientCount"
        Transform countTextTransform = slot.transform.Find("IngridientCount");
        if (countTextTransform != null)
        {
            if (requiredCount > 0)
            {
                countTextTransform.gameObject.SetActive(true);
                countTextTransform.GetComponent<TMP_Text>().text = requiredCount.ToString();
            }
            else
            {
                countTextTransform.gameObject.SetActive(false);
            }
        }

        // --- LOGIKA KHUSUS UNTUK SLOT BAHAN (TIDAK BERJALAN UNTUK RESULT SLOT) ---
        if (slot != resultSlot)
        {
            // 1. Hitung jumlah yang dimiliki
            int ownedCount = CountItemsInInventory(item);

            // 2. Tentukan warna berdasarkan kecukupan
            Color availabilityColor = (ownedCount >= requiredCount) ? Color.white : Color.red;

            // 3. Cari dan perbarui teks jumlah yang dimiliki
            Transform ownedTextTransform = slot.transform.Find("ItemInInventory"); // Ganti "ItemInInventory" dengan nama child Anda, misal "ItemInInventory"
            if (ownedTextTransform != null)
            {
                TMP_Text ownedText = ownedTextTransform.GetComponent<TMP_Text>();
                ownedText.gameObject.SetActive(true);
                ownedText.text = ownedCount.ToString(); // Formatnya menjadi (10)
                ownedText.color = availabilityColor;
            }
        }
    }

    private int CountItemsInInventory(Item itemToCount)
    {
        if (itemToCount == null) return 0;
        int totalCount = 0;
        foreach (ItemData slot in playerData.inventory)
        {
            if (slot != null && slot.itemName == itemToCount.itemName)
            {
                totalCount += slot.count;
            }
        }
        return totalCount;
    }

    private int CalculateMaxCraftAmount()
    {
        if (selectedRecipe == null) return 0;
        int maxPossible = int.MaxValue;
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            Item itemObject = ItemPool.Instance.GetItem(ingredient.itemName);
            int owned = CountItemsInInventory(itemObject);
            if (ingredient.count <= 0) continue;
            int canMake = owned / ingredient.count;
            if (canMake < maxPossible)
            {
                maxPossible = canMake;
            }
        }
        return maxPossible;
    }

    private void ClearSlotUI(GameObject slot)
    {
        slot.transform.Find("ItemImage").gameObject.SetActive(false);
        Debug.Log("CraftUI: ClearSlotUI called for slot: " + slot.name);
        Transform countText = slot.transform.Find("IngridientCount");
        if (countText) countText.gameObject.SetActive(false);
        Transform ownedText = slot.transform.Find("ItemInInventory");
        if (ownedText) ownedText.gameObject.SetActive(false);
    }
    #endregion
}