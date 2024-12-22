using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CookIngredient : MonoBehaviour
{

    [SerializeField] private RecipeDatabase recipeDatabaseInstance;
    [SerializeField] Transform ContentGO;
    [SerializeField] Transform SlotTemplate; // Parent untuk menempatkan hasil resep
    public bool checkRecipes = false;

    //gameobjek untuk menampilkan detail recipe
    public GameObject ItemCook;
    public GameObject ItemResult;



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
        foreach (Recipe recipe in recipeDatabaseInstance.cookRecipes)
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
                    DestroyCraftItems();
                    checkRecipes = false; // Set checkRecipes kembali menjadi false setelah menghapus objek
                }
            });

        }
    }


    private void CheckIngredients(string nameResult)
    {
        // Iterasi melalui semua resep
        foreach (Recipe recipe in recipeDatabaseInstance.cookRecipes)
        {
            // Pastikan resep yang dimaksud adalah resep yang sesuai dengan nameResult
            if (recipe.result.name == nameResult)
            {

                // Tampilkan hasil crafting di ItemResult
                DisplayResultInSlot(ItemResult, recipe.result, 1);  // Menampilkan hasil dengan jumlah 1 (default)
                                                                    // Loop untuk menampilkan bahan-bahan yang diperlukan
                DisplayIngredientInSlot(ItemCook, recipe.ingredient, recipe.ingredientCount);


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



    private void DestroyCraftItems()
    {
        // Menghapus item yang ada di setiap ItemCraft slot
        DestroyItemInSlot(ItemCook);
       
        DestroyItemInSlot(ItemResult);

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
