using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CookIngredient : MonoBehaviour
{

    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate; // Parent untuk menempatkan hasil resep
    public bool checkRecipes = false;





    public void Start()
    {
        RefreshRecipe();
    }

    public void RefreshRecipe()
    {
        // Hapus semua child sebelumnya di ContentGO kecuali SlotTemplate
        foreach (Transform child in ContentGO)
        {
            if (child == SlotTemplate) continue; // Abaikan SlotTemplate
            Destroy(child.gameObject); // Hapus child lainnya
        }

        // Perulangan untuk setiap resep di database
        foreach (RecipeCooking recipe in DatabaseManager.Instance.cookingDatabase.cookRecipes)
        {
            // Instansiasi SlotTemplate untuk setiap resep
            Transform recipeSlot = Instantiate(SlotTemplate, ContentGO);
            recipeSlot.gameObject.SetActive(true); // Mengaktifkan objek SlotTemplate yang baru

            // Set sprite gambar hasil resep
            recipeSlot.GetChild(0).GetComponent<Image>().sprite = recipe.result.sprite;

            // Set jumlah hasil (biasanya hanya 1)
            recipeSlot.GetChild(1).GetComponent<TMP_Text>().text = "1"; // Menampilkan 1 karena hasil craft biasanya 1 item

            // Set nama slot berdasarkan nama hasil resep
            recipeSlot.name = recipe.result.name; // Mengatur nama SlotTemplate sesuai dengan nama hasil resep

            // Pastikan nama tidak memiliki akhiran "(Clone)" setelah instansiasi
            recipeSlot.name = recipeSlot.name.Replace("(Clone)", "");

            // Menambahkan listener untuk mendeskripsikan item jika perlu
            Button button = recipeSlot.GetComponent<Button>();
            // Update listener untuk button
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (checkRecipes == false)
                {
                    // Jika checkRecipes false, jalankan CheckIngredients
                    CheckIngredients(recipeSlot.name);
                    checkRecipes = true; // Set checkRecipes menjadi true setelah CheckIngredients dipanggil
                }
                else
                {
                    // Jika checkRecipes true, lakukan "destroy" pada objek yang ditampilkan
                    if (!CookUI.Instance.isCookReady)
                    {
                        CookUI.Instance.DestroyCraftItems();
                        checkRecipes = false; // Set checkRecipes kembali menjadi false setelah menghapus objek
                    }
                }
            });

        }
    }


    private void CheckIngredients(string nameResult)
    {
        // Iterasi melalui semua resep
        foreach (RecipeCooking recipe in DatabaseManager.Instance.cookingDatabase.cookRecipes)
        {
            // Pastikan resep yang dimaksud adalah resep yang sesuai dengan nameResult
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
