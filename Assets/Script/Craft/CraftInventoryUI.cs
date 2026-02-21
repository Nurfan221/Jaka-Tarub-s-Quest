using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftInventoryUI : MonoBehaviour
{
    #region UI References & Dependencies
    [Header("Dependencies")]
    public PlayerController playerData;

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
            playerData = PlayerController.Instance;
        }
    }
    private void Start()
    {
        //if (playerData == null)
        //{
        //    // ... (logika null check playerData Anda sudah benar) ...
        //    return;
        //}

        closeButton.onClick.AddListener(CloseUI);
        craftButton.onClick.AddListener(ExecuteCraft);

        // Lakukan null check sebelum menambahkan listener
        if (QuantityPopupUI.Instance != null)
        {
            // Gunakan nama fungsi yang benar: HandlePopupConfirm dan HandlePopupCancel
            QuantityPopupUI.Instance.onConfirm.AddListener(HandlePopupConfirm);
            //QuantityPopupUI.Instance.onCancel.AddListener(HandlePopupCancel);
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
        //GameController.Instance.ShowPersistentUI(false);
        //GameController.Instance.PauseGame();
        RefreshRecipeList();
        ClearRecipeDetails();
    }

    public void CloseUI()
    {
        gameObject.SetActive(false);
        Debug.Log("CraftInventoryUI: CloseUI called");
        //GameController.Instance.ShowPersistentUI(true);
        //GameController.Instance.ResumeGame();
    }

    private void RefreshRecipeList()
    {
        Debug.Log("Menyegarkan daftar resep...");
        foreach (Transform child in recipeListContent)
        {
            if (child.gameObject != recipeSlotTemplate) Destroy(child.gameObject);
        }
        recipeSlotTemplate.SetActive(false);

        foreach (CraftRecipe recipe in DatabaseManager.Instance.craftingDatabase.craftRecipes)
        {
            if (recipe.craftIngredient.Count > 2)
            {
                continue; // Lewati resep dengan lebih dari 2 bahan
            }
            CraftRecipe localRecipe = recipe;

            GameObject recipeSlotGO = Instantiate(recipeSlotTemplate, recipeListContent);
            recipeSlotGO.SetActive(true);

            //Cek apakah resep ini bisa dibuat (minimal 1)
            bool canCraft = IsRecipeCraftable(localRecipe);

            // Dapatkan komponen Image dari slot resep
            Image itemImage = recipeSlotGO.transform.Find("ItemImage").GetComponent<Image>();
            itemImage.gameObject.SetActive(true); // Pastikan gambar diaktifkan
            if (itemImage == null )
            {
                Debug.Log("Item Image tidak ditemukan");
            }
            else
            {
                Debug.Log("Item Image ditemukan");
                itemImage.sprite = recipe.result.sprite; // Pastikan sprite hasil ditampilkan
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
        foreach (var ingredient in recipe.craftIngredient)
        {
            if (CountItemsInInventory(ingredient.ingredientItem) < ingredient.ingredientCount)
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

        QuantityPopupUI.Instance.Show(selectedResultItem, 1, maxPossible);
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
            if (i < selectedRecipe.craftIngredient.Count)
            {
                // Menggunakan selectedCraftAmount
                int requiredCount = selectedRecipe.craftIngredient[i].ingredientCount * selectedCraftAmount;
                UpdateSlotUI(ingredientSlots[i], selectedRecipe.craftIngredient[i].ingredientItem, requiredCount);
            }
            else
            {
                ClearSlotUI(ingredientSlots[i]);
            }
        }

        // Tampilkan hasil
        // Menggunakan selectedCraftAmount
        int resultCount = selectedRecipe.resultCount * selectedCraftAmount;
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
        foreach (var ingredient in selectedRecipe.craftIngredient)
        {
            if (CountItemsInInventory(ingredient.ingredientItem) < ingredient.ingredientCount * selectedCraftAmount)
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
        foreach (var ingredient in selectedRecipe.craftIngredient)
        {
            int countToRemove = ingredient.ingredientCount * selectedCraftAmount;
            ItemData itemToRemove = new ItemData(ingredient.ingredientItem.itemName, countToRemove, ItemQuality.Normal, ingredient.ingredientItem.health);
            ItemPool.Instance.RemoveItemsFromInventory(itemToRemove);
        }

        // Tambahkan hasil ke inventory
        ItemData resultData = new ItemData(
            selectedResultItem.itemName,
            selectedRecipe.resultCount * selectedCraftAmount,
            selectedResultItem.quality,
            selectedResultItem.health
        );

        bool isSuccess = ItemPool.Instance.AddItem(resultData);

        if (isSuccess)
        {
            // Hapus item dari Craft
            Debug.Log($"Berhasil crafting {resultData.itemName} x{resultData.count}");
            MechanicController.Instance.HandleUpdateInventory();

            MechanicController.Instance.HandleUpdateMenuInventory(0);
        }
        else
        {
            // Jangan hapus, biarkan di tungku
            Debug.Log("Tas penuh, item tetap di tungku.");
            // Opsional: Munculkan teks "Tas Penuh!"
        }

        Debug.Log($"Berhasil crafting {resultData.itemName} x{resultData.count}");
        MechanicController.Instance.HandleUpdateInventory();

        MechanicController.Instance.HandleUpdateMenuInventory(0);
    }
    #endregion

    #region Helper & Utility Functions
    private void UpdateSlotUI(GameObject slot, Item item, int requiredCount)
    {
        if (item == null || slot == null) return;

        //  Menampilkan Gambar Item 
        Transform imageTransform = slot.transform.Find("ItemImage"); // Asumsi nama child tetap "ItemImage"
        imageTransform.gameObject.SetActive(true);
        imageTransform.GetComponent<Image>().sprite = item.sprite;

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
            // Hitung jumlah yang dimiliki
            int ownedCount = CountItemsInInventory(item);

            // Tentukan warna berdasarkan kecukupan
            //Color availabilityColor = (ownedCount >= requiredCount) ? Color.green : Color.red;

            // Cari dan perbarui teks jumlah yang dimiliki
            Transform ownedTextTransform = slot.transform.Find("ItemInInventory"); // Ganti "ItemInInventory" dengan nama child Anda, misal "ItemInInventory"
            if (ownedTextTransform != null)
            {
                TMP_Text ownedText = ownedTextTransform.GetComponent<TMP_Text>();
                ownedText.gameObject.SetActive(true);
                ownedText.text = ownedCount.ToString(); // Formatnya menjadi (10)
                //ownedText.color = availabilityColor;
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

        // Loop menggunakan struktur baru craftIngredient
        foreach (var ingredient in selectedRecipe.craftIngredient)
        {
            // Tidak perlu ItemPool lagi, itemnya sudah nempel di resep
            Item itemObject = ingredient.ingredientItem;

            int owned = CountItemsInInventory(itemObject);

            // Hindari pembagian nol
            if (ingredient.ingredientCount <= 0) continue;

            // Hitung rasio
            int canMake = owned / ingredient.ingredientCount;

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