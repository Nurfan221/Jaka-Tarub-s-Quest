using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DatabaseManager;

public class CraftUI : MonoBehaviour
{
    #region UI References & Dependencies
    [Header("Dependencies")]
    public PlayerController playerData;

    [Header("Panel Utama")]
    public Button closeButton;

    [Header("Daftar Resep")]
    public Transform recipeListContent;
    public Transform recipeSlotTemplate;

    [Header("Detail Resep")]
    public Transform[] ingredientSlots;
    public Transform resultSlot;
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
            playerData = PlayerController.Instance;
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
        recipeSlotTemplate.gameObject.SetActive(false);

        foreach (CraftRecipe recipe in DatabaseManager.Instance.craftingDatabase.craftRecipes)
        {
            CraftRecipe localRecipe = recipe;
            Item itemUse = ItemPool.Instance.GetItemWithQuality(localRecipe.result.itemName, localRecipe.result.quality);
            Transform recipeSlotGO = Instantiate(recipeSlotTemplate, recipeListContent);
            recipeSlotGO.gameObject.SetActive(true);

            //Cek apakah resep ini bisa dibuat (minimal 1)
            bool canCraft = IsRecipeCraftable(localRecipe);

            // Dapatkan komponen Image dari slot resep
            //Image slotImage = recipeSlotGO.GetComponent<Image>();
            Image itemImage = recipeSlotGO.transform.Find("ItemImage").GetComponent<Image>();
            itemImage.gameObject.SetActive(true);
            if (itemImage == null)
            {
                Debug.Log("Item Image tidak ditemukan");
            }
            else
            {
                Debug.Log("Item Image ditemukan");
                itemImage.sprite = itemUse.sprite; // Pastikan sprite hasil ditampilkan
            }


            //Atur warnanya berdasarkan ketersediaan
            //if (canCraft)
            //{
            //    // Warna normal jika bisa dibuat
            //    recipeSlotGO.GetComponent<Button>().interactable = true; // Aktifkan interaksi
            //}
            //else
            //{
            //    // Warna redup/abu-abu jika tidak bisa
            //    recipeSlotGO.GetComponent<Button>().interactable = false; // Nonaktifkan interaksi
            //                                                              //recipeSlotGO.interactab
            //}

            recipeSlotGO.GetComponent<Button>().onClick.AddListener(() => SelectRecipe(localRecipe));
        }
    }
    // Buat metode baru ini
    private void OnRecipeSlotClicked(CraftRecipe recipe)
    {

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

        int maxPossible = 10;

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
                int requiredCount = ingredientData.count * selectedCraftAmount;
                UpdateSlotUI(ingredientSlots[i], itemObject, requiredCount);
            }
            else
            {
                ClearSlotUI(ingredientSlots[i]);
            }
        }

        // Tampilkan hasil

        int resultCount = selectedRecipe.result.count * selectedCraftAmount;
        UpdateSlotUI(resultSlot, selectedResultItem, resultCount);
        CheckIngredientAvailability();
    }

    private void ClearRecipeDetails()
    {
        // --- DEBUG TAMBAHAN 1 ---
        if (ingredientSlots == null || ingredientSlots.Length == 0)
        {
            Debug.LogError("[CraftUI] GAGAL! Array 'ingredientSlots' di Inspector MASIH KOSONG!");
            return; // Hentikan fungsi di sini
        }

        foreach (var slot in ingredientSlots)
        {
            // --- DEBUG TAMBAHAN 2 ---
            if (slot == null)
            {
                Debug.LogError("[CraftUI] GAGAL! Salah satu elemen di dalam 'ingredientSlots' ternyata NULL/MISSING!");
                continue; // Lanjutkan ke slot berikutnya
            }

            // Jika lolos dari 2 debug di atas, baru jalankan ClearSlotUI
            ClearSlotUI(slot);
        }

        // Ini juga harus dicek
        if (resultSlot != null)
        {
            ClearSlotUI(resultSlot);
        }
        else
        {
            Debug.LogError("[CraftUI] GAGAL! 'resultSlot' di Inspector juga KOSONG!");
        }

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
            int countToRemove = ingredient.count * selectedCraftAmount;
            ItemData itemToRemove = new ItemData(ingredient.itemName, countToRemove, ingredient.quality, ingredient.itemHealth);
            ItemPool.Instance.RemoveItemsFromInventory(itemToRemove);
        }

        // Tambahkan hasil ke inventory
        ItemData resultData = new ItemData(
            selectedResultItem.itemName,
            selectedRecipe.result.count * selectedCraftAmount,
            selectedResultItem.quality,
            selectedResultItem.health
        );
        ItemPool.Instance.AddItem(resultData);

        Debug.Log($"Berhasil crafting {resultData.itemName} x{resultData.count}");
        MechanicController.Instance.HandleUpdateInventory();
        CloseUI();
    }
    #endregion

    #region Helper & Utility Functions
    private void UpdateSlotUI(Transform slot, Item item, int requiredCount)
    {
        if (item == null || slot == null) return;

        // Menampilkan Gambar Item
        Transform imageTransform = slot.transform.Find("ItemImage"); // Asumsi nama child tetap "ItemImage"
        imageTransform.gameObject.SetActive(true);
        imageTransform.GetComponent<Image>().sprite = item.sprite;

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

        if (slot != resultSlot)
        {
            //  Hitung jumlah yang dimiliki
            int ownedCount = CountItemsInInventory(item);

            // Tentukan warna berdasarkan kecukupan
            Color availabilityColor = (ownedCount >= requiredCount) ? Color.white : Color.red;

            //  Cari dan perbarui teks jumlah yang dimiliki
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

    private void ClearSlotUI(Transform slot)
    {
        if (slot == null)
        {
            Debug.LogError("eh  kenapa ga bisa ");

            // TAMBAHKAN INI!
            return; // Hentikan fungsi di sini agar tidak crash
        }
        else
        {
            Debug.LogError("bisa deng  " + slot.name);
        }

        // Kode di bawah ini sekarang aman
        Transform slotImage = slot.transform.Find("ItemImage");
        if (slotImage) slotImage.gameObject.SetActive(false);

        // Pindahkan log ini ke dalam 'else' agar aman
        // Debug.Log("CraftUI: ClearSlotUI called for slot: " + slot.name); 

        Transform countText = slot.transform.Find("IngridientCount");
        if (countText) countText.gameObject.SetActive(false);

        Transform ownedText = slot.transform.Find("ItemInInventory");
        if (ownedText) ownedText.gameObject.SetActive(false);
    }
    #endregion
}