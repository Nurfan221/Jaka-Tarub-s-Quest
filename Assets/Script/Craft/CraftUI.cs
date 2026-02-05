using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class IngredientSlots
{
    public string slotName;
    public Transform slotTransform;
    public Image slotImage;
    public TMP_Text slotRecipeCount;
    public TMP_Text slotItemCount;
    public CanvasGroup canvasGroup;
}

public class CraftUI : MonoBehaviour
{
    #region UI References & Dependencies
    [Header("Dependencies")]
    public PlayerController playerData;
    public string errorMessage = "Bahan yang di perlukan tidak cukup";

    [Header("Panel Utama")]
    public Button closeButton;

    [Header("Daftar Resep")]
    public Transform recipeListContent;
    public Transform recipeSlotTemplate;

    [Header("Detail Resep")]
    // Mengganti nama 'slots' menjadi 'ingredientSlots' agar lebih jelas 
    // dan konsisten dengan penggunaan di dalam fungsi.
    public IngredientSlots[] ingredientSlots;

    // 'resultSlot' harus bertipe 'IngredientSlots' agar konsisten 
    // dan memiliki referensi ke 'slotImage', 'slotRecipeCount', dll.
    public IngredientSlots resultSlot;
    public Button craftButton;
    #endregion

    #region Private State
    private CraftRecipe selectedRecipe;
    private Item selectedResultItem;
    private int selectedCraftAmount;
    private bool isCraftFood;
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
        closeButton.onClick.AddListener(CloseUI);
        craftButton.onClick.AddListener(ExecuteCraft);

        if (QuantityPopupUI.Instance != null)
        {
            QuantityPopupUI.Instance.onConfirm.AddListener(HandlePopupConfirm);
            QuantityPopupUI.Instance.onCancel.AddListener(HandlePopupCancel);
        }
        else
        {
            Debug.LogError("Referensi ke QuantityPopupUI.Instance adalah null saat Start().");
        }

    }
    #endregion

    public void OpenUI(bool valueObject)
    {
        isCraftFood = valueObject;
        gameObject.SetActive(true);
        GameController.Instance.ShowPersistentUI(false);
        //GameController.Instance.PauseGame();
        RefreshRecipeList();

        // Bersihkan detail resep saat pertama kali membuka UI
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

        // Bersihkan UI lama
        foreach (Transform child in recipeListContent)
        {
            if (child != recipeSlotTemplate) Destroy(child.gameObject);
        }

        // Kita buat variabel list sementara.
        List<CraftRecipe> recipesToDisplay;

        if (isCraftFood)
        {
            recipesToDisplay = DatabaseManager.Instance.craftingDatabase.craftFoodRecipe;

        }
        else
        {
            recipesToDisplay = DatabaseManager.Instance.craftingDatabase.craftRecipes;


        }

        foreach (CraftRecipe recipe in recipesToDisplay)
        {
            CraftRecipe localRecipe = recipe;

            // PERUBAHAN: Langsung ambil sprite dari result (Item SO)
            // Tidak perlu GetItemWithQuality lagi
            Item itemUse = localRecipe.result;

            Transform recipeSlotGO = Instantiate(recipeSlotTemplate, recipeListContent);
            recipeSlotGO.gameObject.SetActive(true);

            Image itemImage = recipeSlotGO.GetChild(0).GetComponent<Image>();
            itemImage.gameObject.SetActive(true);

            if (itemImage != null && itemUse != null)
            {
                itemImage.sprite = itemUse.sprite;
            }

            Button btn = recipeSlotGO.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectRecipe(localRecipe));
        }
    }

    private void SelectRecipe(CraftRecipe recipe)
    {
        Debug.Log($"Memilih resep: {recipe.result.itemName}");
        selectedRecipe = recipe;
        selectedResultItem = selectedRecipe.result;
        if (selectedResultItem == null) return;

        // Menggunakan fungsi kalkulasi yang sebenarnya, bukan 'maxPossible = 10'
        int maxPossible = CalculateMaxCraftAmount();

        // Popup mungkin tidak bisa menangani max = 0, jadi atur minimum ke 1
        if (maxPossible == 0)
        {
            maxPossible = 1;
        }

        // Tambahkan null check untuk menghindari error jika popup tidak ada
        if (QuantityPopupUI.Instance != null)
        {
            QuantityPopupUI.Instance.Show(selectedResultItem.sprite, 1, maxPossible);
        }
        else
        {
            Debug.LogError("QuantityPopupUI.Instance is null. Tidak dapat menampilkan popup.");
            // Fallback: langsung tampilkan untuk 1 item jika popup gagal
            HandlePopupConfirm(1);
        }
    }

    public void HandlePopupConfirm(int amount)
    {
        selectedCraftAmount = amount;
        DisplaySelectedRecipe();
    }

    public void HandlePopupCancel()
    {
        // Panggil ClearRecipeDetails agar UI bersih saat popup dibatalkan
        ClearRecipeDetails();
    }

    public void DisplaySelectedRecipe()
    {
        if (selectedRecipe == null)
        {
            Debug.LogWarning("Mencoba menampilkan resep, tetapi 'selectedRecipe' adalah null.");
            return;
        }

        bool canCraft = true;

        for (int i = 0; i < ingredientSlots.Length; i++)
        {
            IngredientSlots currentSlot = ingredientSlots[i];

            // PERUBAHAN: Cek count pada 'craftIngredient'
            if (i < selectedRecipe.craftIngredient.Count)
            {
                // Ambil data dari struktur baru
                IngredientCraft ingData = selectedRecipe.craftIngredient[i];
                Item itemObject = ingData.ingredientItem;

                if (itemObject != null)
                {
                    currentSlot.slotTransform.gameObject.SetActive(true);
                    currentSlot.slotImage.sprite = itemObject.sprite;
                    currentSlot.slotImage.gameObject.SetActive(true);

                    // Hitung kebutuhan
                    int requiredCount = ingData.ingredientCount * selectedCraftAmount;
                    currentSlot.slotRecipeCount.text = requiredCount.ToString();
                    currentSlot.slotRecipeCount.gameObject.SetActive(true);

                    // Cek Inventory
                    int ownedCount = CountItemsInInventory(itemObject);
                    currentSlot.slotItemCount.text = $"({ownedCount})";
                    currentSlot.slotItemCount.gameObject.SetActive(true);

                    bool hasEnoughThisItem = ownedCount >= requiredCount;

                    // Update Visual Text
                    currentSlot.slotItemCount.color = hasEnoughThisItem ? Color.white : Color.red;
                    currentSlot.slotRecipeCount.color = hasEnoughThisItem ? Color.white : Color.red;

                    if (!hasEnoughThisItem) canCraft = false;

                    // Update Visual Slot (Redup/Terang)
                    if (currentSlot.canvasGroup != null)
                    {
                        if (hasEnoughThisItem)
                        {
                            currentSlot.canvasGroup.alpha = 1.0f;
                            currentSlot.canvasGroup.interactable = true;
                        }
                        else
                        {
                            currentSlot.canvasGroup.alpha = 0.4f;
                            currentSlot.canvasGroup.interactable = false;

                            // Karena ada bahan yang KURANG.
                            canCraft = false;
                        }
                    }
                }
            }
            else
            {
                // JANGAN ubah nilai 'canCraft' di sini! Slot kosong tidak mempengaruhi crafting.
                if (currentSlot.slotTransform != null)
                {
                    //currentSlot.slotTransform.gameObject.SetActive(false);
                    currentSlot.canvasGroup.alpha = 0.4f;

                    currentSlot.canvasGroup.interactable = false;

                    currentSlot.slotImage.gameObject.SetActive(false);

                    currentSlot.slotRecipeCount.gameObject.SetActive(false);

                    currentSlot.slotItemCount.gameObject.SetActive(false);
                }
            }
        }

        Item resultItem = selectedRecipe.result;
        if (resultItem != null)
        {
            resultSlot.slotTransform.gameObject.SetActive(true);
            resultSlot.slotImage.sprite = resultItem.sprite;
            resultSlot.slotImage.gameObject.SetActive(true);

            // Gunakan variable resultCount yang kita tambahkan di langkah 1
            int resultCount = selectedRecipe.resultCount * selectedCraftAmount;
            resultSlot.slotRecipeCount.text = resultCount.ToString();
            resultSlot.slotRecipeCount.gameObject.SetActive(true);
        }

        if (craftButton != null) craftButton.interactable = canCraft;

        if (!canCraft)
        {
            resultSlot.canvasGroup.alpha = 0.4f;
        }
        else
        {
            resultSlot.canvasGroup.alpha = 1.0f;
        }
    }

    public void ClearRecipeDetails()
    {
        // Gunakan nama variabel 'ingredientSlots' yang benar
        foreach (var slot in ingredientSlots)
        {
            if (slot != null && slot.slotTransform != null)
            {
                slot.slotImage.gameObject.SetActive(false);
            }
        }

        // Gunakan struktur 'IngredientSlots' untuk resultSlot
        if (resultSlot != null && resultSlot.slotTransform != null)
        {
            resultSlot.slotImage.gameObject.SetActive(false);
        }

        // Reset state
        selectedRecipe = null;
        selectedResultItem = null;
        selectedCraftAmount = 0;
    }



    // Fungsi ini dipanggil di DisplaySelectedRecipe tetapi belum ada.
    private int CountItemsInInventory(Item itemToCount)
    {
        if (playerData == null || playerData.inventory == null || itemToCount == null)
        {
            return 0;
        }

        int totalCount = 0;
        foreach (ItemData slot in playerData.inventory)
        {
            // Cek null pada slot dan pastikan nama item cocok
            if (slot != null && !string.IsNullOrEmpty(slot.itemName) && slot.itemName == itemToCount.itemName)
            {
                totalCount += slot.count;
            }
        }
        return totalCount;
    }


    // Fungsi ini untuk memperbaiki logika 'maxPossible = 10' di SelectRecipe.
    private int CalculateMaxCraftAmount()
    {
        if (selectedRecipe == null) return 0;

        int maxAmount = int.MaxValue;

        // PERUBAHAN: Loop ke 'craftIngredient'
        foreach (IngredientCraft ingredient in selectedRecipe.craftIngredient)
        {
            // Langsung akses item dari class IngredientCraft
            Item item = ingredient.ingredientItem;

            if (item == null) return 0;

            int ownedCount = CountItemsInInventory(item);
            int requiredCount = ingredient.ingredientCount; // Ambil jumlah dari class baru

            if (requiredCount <= 0 || ownedCount == 0) return 0;

            int possibleWithThisItem = ownedCount / requiredCount;

            if (possibleWithThisItem < maxAmount)
            {
                maxAmount = possibleWithThisItem;
            }
        }

        return maxAmount == int.MaxValue ? 0 : maxAmount;
    }

    private void ExecuteCraft()
    {
        if (selectedRecipe == null || selectedCraftAmount <= 0) return;

        if (!CheckIfHasEnoughIngredients()) return;

        Debug.Log("Mengurangi bahan...");
        foreach (IngredientCraft ingredient in selectedRecipe.craftIngredient)
        {
            int totalToRemove = ingredient.ingredientCount * selectedCraftAmount;
            // Kita ambil nama dari ScriptableObject item
            RemoveItems(ingredient.ingredientItem.itemName, totalToRemove);
        }

        // Kita buat ItemData baru berdasarkan info dari ScriptableObject Result
        int totalResultCount = selectedRecipe.resultCount * selectedCraftAmount;

        ItemData finalItemToAdd = new ItemData(
            selectedRecipe.result.itemName, // Ambil nama dari SO
            totalResultCount,
            selectedRecipe.result.quality,  // Ambil default quality dari SO
            selectedRecipe.result.health    // Ambil default health dari SO
        );

        Debug.Log("Mencoba menambahkan item hasil...");
        bool isSuccess = ItemPool.Instance.AddItem(finalItemToAdd);

        if (isSuccess)
        {
            Debug.Log($"Berhasil craft {totalResultCount}x {finalItemToAdd.itemName}");

            // Hitung ulang berapa banyak lagi yang bisa dibuat untuk update UI
            int newMaxPossible = CalculateMaxCraftAmount();

            if (newMaxPossible > 0)
            {
                // Jika masih bisa buat, refresh popup
                if (QuantityPopupUI.Instance != null)
                {
                    QuantityPopupUI.Instance.Show(selectedResultItem.sprite, 1, newMaxPossible);
                }
            }
            else
            {
                // Jika bahan habis, bersihkan/tutup UI
                ClearRecipeDetails();
                if (craftButton != null) craftButton.interactable = false;
            }
        }
        else
        {
            Debug.LogError("CRITICAL: Gagal craft karena Inventaris Penuh! Melakukan REFUND bahan...");

            // LOGIKA REFUND (Penting diupdate juga)
            Debug.LogError("Inventory Penuh! Refund bahan...");
            foreach (IngredientCraft ingredient in selectedRecipe.craftIngredient)
            {
                int totalToRefund = ingredient.ingredientCount * selectedCraftAmount;

                // Buat ItemData dari IngredientCraft untuk dikembalikan
                ItemData refundItem = new ItemData(
                    ingredient.ingredientItem.itemName,
                    totalToRefund,
                    ingredient.ingredientItem.quality,
                    ingredient.ingredientItem.health
                );

                ItemPool.Instance.AddItem(refundItem);
            }

            // Tampilkan pesan error ke player
            PlayerUI.Instance.ShowErrorUI("Inventaris Penuh! Crafting Dibatalkan.");

            // (Opsional) Refresh UI untuk memastikan tampilan inventaris benar kembali
            RefreshRecipeList();
        }
    }

    private bool CheckIfHasEnoughIngredients()
    {
        if (selectedRecipe == null) return false;

        foreach (IngredientCraft ingredient in selectedRecipe.craftIngredient)
        {
            // Akses langsung
            Item item = ingredient.ingredientItem;
            int requiredAmount = ingredient.ingredientCount * selectedCraftAmount;

            if (CountItemsInInventory(item) < requiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    private void RemoveItems(string itemName, int amountToRemove)
    {
        // Loop dari BELAKANG ke depan saat menghapus item dari list
        for (int i = playerData.inventory.Count - 1; i >= 0; i--)
        {
            ItemData slot = playerData.inventory[i];

            // Hanya proses slot yang sesuai
            if (slot != null && slot.itemName == itemName)
            {
                if (slot.count > amountToRemove)
                {
                    // Stack ini punya lebih dari cukup, kurangi dan selesai
                    slot.count -= amountToRemove;
                    amountToRemove = 0;
                    return; // Selesai
                }
                else
                {
                    // Habiskan stack ini, kurangi jumlah yang perlu dihapus
                    amountToRemove -= slot.count;
                    playerData.inventory.RemoveAt(i); // Hapus stack
                }
            }

            if (amountToRemove == 0)
            {
                return; // Selesai
            }
        }
    }


}