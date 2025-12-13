using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DatabaseManager;

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

    public void OpenUI()
    {
        gameObject.SetActive(true);
        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();
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
        // Bersihkan daftar lama sebelum menambahkan yang baru
        // agar tidak terjadi duplikasi saat OpenUI() dipanggil lagi.
        foreach (Transform child in recipeListContent)
        {
            if (child != recipeSlotTemplate) // Jangan hancurkan template
            {
                Destroy(child.gameObject);
            }
        }

        foreach (CraftRecipe recipe in DatabaseManager.Instance.craftingDatabase.craftRecipes)
        {
            CraftRecipe localRecipe = recipe;
            Item itemUse = ItemPool.Instance.GetItemWithQuality(localRecipe.result.itemName, localRecipe.result.quality);
            Transform recipeSlotGO = Instantiate(recipeSlotTemplate, recipeListContent);
            recipeSlotGO.gameObject.SetActive(true);

            // 'GetChild(0)' ini "rapuh". Jika Anda mengubah urutan child di prefab, 
            // kode ini akan rusak. Metode 'RecipeSlotUI.cs' yang kita diskusikan sebelumnya
            // jauh lebih aman.
            Image itemImage = recipeSlotGO.GetChild(0).GetComponent<Image>();
            itemImage.gameObject.SetActive(true);

            if (itemImage != null)
            {
                itemImage.sprite = itemUse.sprite;
            }

            recipeSlotGO.GetComponent<Button>().onClick.RemoveAllListeners();
            recipeSlotGO.GetComponent<Button>().onClick.AddListener(() => SelectRecipe(localRecipe));
        }
    }

    private void SelectRecipe(CraftRecipe recipe)
    {
        Debug.Log($"Memilih resep: {recipe.result.itemName}");
        selectedRecipe = recipe;
        selectedResultItem = ItemPool.Instance.GetItem(recipe.result.itemName);
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

            if (i < selectedRecipe.ingredients.Count)
            {
                ItemData ingredientData = selectedRecipe.ingredients[i];
                Item itemObject = ItemPool.Instance.GetItem(ingredientData.itemName);

                if (itemObject != null)
                {
                    // Tampilkan slot
                    currentSlot.slotTransform.gameObject.SetActive(true);

                    currentSlot.slotImage.sprite = itemObject.sprite;
                    currentSlot.slotImage.gameObject.SetActive(true);

                    int requiredCount = ingredientData.count * selectedCraftAmount;
                    currentSlot.slotRecipeCount.text = requiredCount.ToString();
                    currentSlot.slotRecipeCount.gameObject.SetActive(true);

                    int ownedCount = CountItemsInInventory(itemObject);
                    currentSlot.slotItemCount.text = $"({ownedCount})";
                    currentSlot.slotItemCount.gameObject.SetActive(true);

                    // Cek apakah bahan INI cukup
                    bool hasEnoughThisItem = ownedCount >= requiredCount;

                    // Update Visual Text
                    currentSlot.slotItemCount.color = hasEnoughThisItem ? Color.white : Color.red;
                    currentSlot.slotRecipeCount.color = hasEnoughThisItem ? Color.white : Color.red;

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

        ItemData resultData = selectedRecipe.result;
        Item resultItem = ItemPool.Instance.GetItem(resultData.itemName);

        if (resultItem != null)
        {
            resultSlot.slotTransform.gameObject.SetActive(true);
            resultSlot.slotImage.sprite = resultItem.sprite;
            resultSlot.slotImage.gameObject.SetActive(true);

            int resultCount = resultData.count * selectedCraftAmount;
            resultSlot.slotRecipeCount.text = resultCount.ToString();
            resultSlot.slotRecipeCount.gameObject.SetActive(true);
        }

        if (craftButton != null)
        {
            craftButton.interactable = canCraft;
            Debug.Log("Tombol craft diatur ke: " + (canCraft ? "INTERACTABLE" : "NON-INTERACTABLE"));
        }

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

        int maxAmount = int.MaxValue; // Mulai dengan angka tak terbatas

        // Cek setiap bahan
        foreach (ItemData ingredient in selectedRecipe.ingredients)
        {
            Item item = ItemPool.Instance.GetItem(ingredient.itemName);
            if (item == null) return 0; // Jika ada item resep yg tidak valid

            int ownedCount = CountItemsInInventory(item);
            int requiredCount = ingredient.count;

            // Jika butuh 0 (aneh) atau pemain tidak punya itemnya
            if (requiredCount <= 0 || ownedCount == 0)
            {
                return 0; // Tidak bisa membuat sama sekali
            }

            // Berapa banyak yang bisa dibuat HANYA dengan item ini
            int possibleWithThisItem = ownedCount / requiredCount;

            // Kita dibatasi oleh item yang paling sedikit kita miliki
            if (possibleWithThisItem < maxAmount)
            {
                maxAmount = possibleWithThisItem;
            }
        }

        // Jika maxAmount tidak pernah berubah (misal resep tidak butuh bahan)
        // atau jika pemain punya semua bahan, kembalikan nilainya.
        // Jika tidak punya bahan, maxAmount akan menjadi 0.
        return maxAmount == int.MaxValue ? 0 : maxAmount;
    }

    private void ExecuteCraft()
    {
        // Pastikan resep & jumlah valid
        if (selectedRecipe == null || selectedCraftAmount <= 0)
        {
            Debug.LogError("Gagal craft: Resep tidak valid atau jumlah 0.");
            return;
        }

        //  Cek apakah bahan cukup
        if (!CheckIfHasEnoughIngredients())
        {
            Debug.LogWarning("Gagal craft: Bahan tidak cukup.");
            // (Opsional) Panggil efek shake tombol di sini
            return;
        }

        //  Siapkan Data Item Hasil 
        ItemData recipeResult = selectedRecipe.result;
        int totalResultCount = recipeResult.count * selectedCraftAmount;

        // Asumsi Anda punya constructor: new ItemData(nama, jumlah, kualitas)
        // Jika tidak, sesuaikan dengan cara Anda membuat ItemData baru
        ItemData finalItemToAdd = new ItemData(recipeResult.itemName, totalResultCount, recipeResult.quality, recipeResult.itemHealth);


        // Kita kurangi dulu agar jika tas 20/20 dan resep butuh 1 item, slotnya jadi kosong untuk hasil.
        Debug.Log("Mengurangi bahan...");
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            int totalToRemove = ingredient.count * selectedCraftAmount;
            RemoveItems(ingredient.itemName, totalToRemove);
        }

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

            // Karena kita sudah menghapusnya di langkah no. 4
            foreach (var ingredient in selectedRecipe.ingredients)
            {
                int totalToRefund = ingredient.count * selectedCraftAmount;

                // Buat data refund
                ItemData refundItem = new ItemData(ingredient.itemName, totalToRefund, ingredient.quality, ingredient.itemHealth);

                // Di sini kita asumsi AddItem pasti berhasil untuk refund karena kita baru saja menghapusnya
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

        foreach (var ingredient in selectedRecipe.ingredients)
        {
            Item item = ItemPool.Instance.GetItem(ingredient.itemName);
            int requiredAmount = ingredient.count * selectedCraftAmount;

            if (CountItemsInInventory(item) < requiredAmount)
            {
                return false; // Langsung gagal jika satu bahan saja kurang
            }
        }
        return true; // Semua bahan ada
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