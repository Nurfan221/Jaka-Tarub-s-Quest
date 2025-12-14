using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CookIngredient : MonoBehaviour
{

    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate; // Parent untuk menempatkan hasil resep
    public bool checkRecipes = false;
    public TypeCooking typeCooking;





    public void Start()
    {
    }

    public void RefreshRecipe(TypeCooking typeCooking)
    {
        this.typeCooking = typeCooking;
        // Hapus semua child sebelumnya di ContentGO kecuali SlotTemplate
        foreach (Transform child in ContentGO)
        {
            if (child == SlotTemplate) continue;
            Destroy(child.gameObject);
        }

        // Pilih list resep sesuai tipe tungku
        List<RecipeCooking> recipes = typeCooking == TypeCooking.FoodCook
            ? DatabaseManager.Instance.cookingDatabase.cookRecipes
            : DatabaseManager.Instance.cookingDatabase.smeltRecipes;

        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("[RefreshRecipe] Tidak ada resep ditemukan untuk tipe ini: " + typeCooking);
            return;
        }

        foreach (RecipeCooking recipe in recipes)
        {
            if (recipe == null || recipe.result == null)
            {
                Debug.LogWarning("[RefreshRecipe] Resep atau hasil resep null, dilewati.");
                continue;
            }

            // Instansiasi slot resep baru
            Transform recipeSlot = Instantiate(SlotTemplate, ContentGO);
            recipeSlot.gameObject.SetActive(true);

            // Set data visual
            Image icon = recipeSlot.GetChild(0).GetComponent<Image>();
            TMP_Text countText = recipeSlot.GetChild(1).GetComponent<TMP_Text>();
            Button button = recipeSlot.GetComponent<Button>();

            icon.sprite = recipe.result.sprite;
            countText.text = "1";
            recipeSlot.name = recipe.result.itemName; // gunakan itemName untuk konsistensi

            // Hapus listener lama dan tambahkan baru
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {

                // Toggle mode lihat resep
                if (!checkRecipes)
                {
                    Debug.Log($"[RefreshRecipe] Memeriksa bahan untuk resep: {recipeSlot.name}");
                    CheckIngredients(recipeSlot.name);
                    checkRecipes = true;
                }
                else
                {
                    Debug.Log($"[RefreshRecipe] Membatalkan mode lihat resep untuk: {recipeSlot.name}");
                    // Jika UI belum siap masak, artinya masih dalam mode lihat resep
                    if (!CookUI.Instance.isCookReady)
                    {
                        CookUI.Instance.DestroyCraftItems();
                        checkRecipes = false;
                    }
                }
            });
        }

        Debug.Log($"[RefreshRecipe] {recipes.Count} resep berhasil dimuat untuk {typeCooking}");
    }


    private void CheckIngredients(string nameResult)
    {

        List<RecipeCooking> recipes = typeCooking == TypeCooking.FoodCook
           ? DatabaseManager.Instance.cookingDatabase.cookRecipes
           : DatabaseManager.Instance.cookingDatabase.smeltRecipes;

        if (recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("[RefreshRecipe] Tidak ada resep ditemukan untuk tipe ini: " + typeCooking);
            return;
        }

        foreach (RecipeCooking recipe in recipes)
        {
            if (recipe == null || recipe.result == null)
            {
                Debug.LogWarning("[RefreshRecipe] Resep atau hasil resep null, dilewati.");
                continue;
            }

            if (recipe.result.name == nameResult && !CookUI.Instance.isCookReady)
            {

                // Tampilkan hasil crafting di ItemResult
                CookUI.Instance.isIngredientAdded = true;
                CookUI.Instance.currentIngredient = recipe.ingredient;
                CookUI.Instance.resultIngredient = recipe.result;
                CookUI.Instance.currentIngredientCount = recipe.ingredientCount;
                CookUI.Instance.resultIngredientCount = 1; // Asumsikan hasil crafting selalu 1

                CookUI.Instance.CekIngredient();


            }
        }

    }

    private void DisplayIngredientInSlot(GameObject itemCraftSlot, Item ingredient, float count)
    {
        // Pastikan itemCraftSlot bukan null
        if (itemCraftSlot != null)
        {
            // Instansiasi slot template untuk ingredient
            Transform ingredientSlot = Instantiate(SlotTemplate, itemCraftSlot.transform);
            ingredientSlot.gameObject.SetActive(true);

            // Set nama item (untuk debugging atau keperluan lain)
            ingredientSlot.gameObject.name = ingredient.name;

            // Set sprite untuk ingredient
            ingredientSlot.GetChild(0).GetComponent<Image>().sprite = ingredient.sprite;

            // Set jumlah ingredient
            ingredientSlot.GetChild(1).GetComponent<TMP_Text>().text = count.ToString();

            // Menambahkan listener untuk menampilkan deskripsi bahan saat diklik
            Button button = ingredientSlot.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                //button.onClick.AddListener(() => SetDescription(ingredient));
            }
        }
    }

    private void DisplayResultInSlot(GameObject itemResultSlot, Item result, int count)
    {
        // Pastikan itemResultSlot bukan null
        if (itemResultSlot != null)
        {
            // Instansiasi slot template untuk result
            Transform resultSlot = Instantiate(SlotTemplate, itemResultSlot.transform);
            resultSlot.gameObject.SetActive(true);

            // Set nama item (untuk debugging atau keperluan lain)
            resultSlot.gameObject.name = result.name;

            // Set sprite untuk result
            resultSlot.GetChild(0).GetComponent<Image>().sprite = result.sprite;

            // Set jumlah result
            resultSlot.GetChild(1).GetComponent<TMP_Text>().text = count.ToString();

            // Jika perlu, Anda bisa menambahkan listener untuk deskripsi atau interaksi lainnya
            Button button = resultSlot.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                // button.onClick.AddListener(() => SetDescription(result)); // Misalnya untuk deskripsi lebih lanjut
            }
        }
    }





    private void DestroyItemInSlot(GameObject itemCraftSlot)
    {
        // Hapus semua child di dalam slot (item yang ada)
        foreach (Transform child in itemCraftSlot.transform)
        {
            Destroy(child.gameObject);
        }
    }



}
