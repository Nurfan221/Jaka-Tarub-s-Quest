using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class Checkingredients : MonoBehaviour
{
    [SerializeField] private RecipeDatabase recipeDatabaseInstance;
    [SerializeField] Craft craftScript;
    [SerializeField] Transform ContentGO; 
    [SerializeField] Transform SlotTemplate; // Parent untuk menempatkan hasil resep
    public GameObject popUp;
    public Image imagePopUp;
    public TextMeshProUGUI itemCount;
    public bool checkRecipes = false;
    public string itemActive;
    public Item selectedItem;

    //gameobjek untuk menampilkan detail recipe
    public GameObject ItemCraft1;
    public GameObject ItemCraft2;
    public GameObject ItemCraft3;
    public GameObject ItemCraft4;
    public GameObject ItemResult;
    public int resultCount;

    [Header("Button Action")]

    // [SerializeField] Button itemAction;
    public Button plusItem;
    public Button minusItem;
    public Button confirm;
    public Button cancel;
    public Button maxResult;
    public Button minResult;



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
        foreach (RecipeDatabase.CraftRecipe recipe in recipeDatabaseInstance.craftRecipes)
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
                PopUpCraftResult(recipe.result);
                //DisplayResultInSlot(ItemResult, recipe.result, 1);

                //if (checkRecipes == false )
                //{
                //    // Jika checkRecipes false, jalankan CheckIngredients
                //    CheckIngredients(recipeSlot.name);
                //    checkRecipes = true; // Set checkRecipes menjadi true setelah CheckIngredients dipanggil
                //    itemActive = recipe.result.name;
                //}
                //else if (checkRecipes && itemActive == recipe.result.name)
                //{
                //    // Jika checkRecipes true, lakukan "destroy" pada objek yang ditampilkan
                //    DestroyCraftItems();
                //    checkRecipes = false; // Set checkRecipes kembali menjadi false setelah menghapus objek
                //}
                //else if (checkRecipes && itemActive != recipe.result.name)
                //{
                //    // Jika checkRecipes false, jalankan CheckIngredients
                //    CheckIngredients(recipeSlot.name);
                //    checkRecipes = true; // Set checkRecipes menjadi true setelah CheckIngredients dipanggil
                //    itemActive = recipe.result.name;
                //}
            });

        }
    }


    private void CheckIngredients(string nameResult)
    {
        // Reset slot sebelum menampilkan bahan baru
        ResetIngredientSlots();

        // Iterasi melalui semua resep
        foreach (RecipeDatabase.CraftRecipe recipe in recipeDatabaseInstance.craftRecipes)
        {
            // Pastikan resep yang dimaksud adalah resep yang sesuai dengan nameResult
            if (recipe.result.name == nameResult)
            {
                // Loop untuk menampilkan bahan-bahan yang diperlukan
                for (int i = 0; i < recipe.ingredients.Count; i++)
                {
                    Debug.Log("recipe yang diperlukan: " + recipe.ingredients[i].name);
                    Debug.Log("jumlah yang diperlukan: " + recipe.ingredientsCount[i]);

                    // Tentukan slot yang akan digunakan untuk menampilkan ingredient berdasarkan case
                    switch (i)
                    {
                        case 0:
                            DisplayIngredientInSlot(ItemCraft1, recipe.ingredients[i], recipe.ingredientsCount[i]);
                            craftScript.CheckItemtoCraft(1);
                            break;
                        case 1:
                            DisplayIngredientInSlot(ItemCraft2, recipe.ingredients[i], recipe.ingredientsCount[i]);
                            craftScript.CheckItemtoCraft(2);
                            break;
                        case 2:
                            DisplayIngredientInSlot(ItemCraft3, recipe.ingredients[i], recipe.ingredientsCount[i]);
                            craftScript.CheckItemtoCraft(3);
                            break;
                        case 3:
                            DisplayIngredientInSlot(ItemCraft4, recipe.ingredients[i], recipe.ingredientsCount[i]);
                            craftScript.CheckItemtoCraft(4);
                            break;
                        default:
                            Debug.LogWarning("Bahan lebih dari 4 tidak ditampilkan.");
                            break;
                    }
                }
            }
        }
    }

    private void ResetIngredientSlots()
    {
        // Reset list bahan crafting
        craftScript.ingredientItemList.Clear();

        // Nonaktifkan semua slot ingredient
        GameObject[] ingredientSlots = { ItemCraft1, ItemCraft2, ItemCraft3, ItemCraft4 };

        foreach (GameObject slot in ingredientSlots)
        {
            ResetChildVisibility(slot, "itemImage");
            ResetChildVisibility(slot, "ItemInInventory");
            ResetChildVisibility(slot, "IngridientCount");
        }
    }
    private void ResetChildVisibility(GameObject parent, string childName)
    {
        Transform childTransform = parent.transform.Find(childName);
        if (childTransform != null)
        {
            childTransform.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"'{childName}' tidak ditemukan dalam {parent.name}!");
        }
    }


    private void DisplayIngredientInSlot(GameObject itemCraftSlot, Item ingredient, int count)
    {
        // Pastikan itemCraftSlot bukan null
        if (itemCraftSlot != null)
        {


            // Instansiasi slot template untuk ingredient
            GameObject craftSlot = itemCraftSlot;
            craftSlot.gameObject.SetActive(true);

            // Set nama item (untuk debugging atau keperluan lain)
            craftSlot.gameObject.name = ingredient.name;

            // Buat Clone dari Item Agar Tidak Mengubah Data Asli
            Item ingredientClone = Instantiate(ingredient);
            ingredientClone.stackCount = (resultCount * count);  // Set jumlah bahan yang diperlukan
            craftScript.ingredientItemList.Add(ingredientClone); // Tambahkan ke List

            // Set sprite untuk ingredient
            Transform imageTransform = craftSlot.transform.Find("itemImage");
            if (imageTransform != null)
            {
                imageTransform.gameObject.SetActive(true);
                Image targetImage = imageTransform.GetComponent<Image>();
                targetImage.sprite = ingredient.sprite;
            }
            else
            {
                Debug.LogWarning("Image untuk item tidak ditemukan di dalam slot!");
            }
            



            // Set jumlah ingredient
                Transform TextTransform = craftSlot.transform.Find("IngridientCount");
            if (TextTransform != null)
            {
                TextTransform.gameObject.SetActive(true);
                TMP_Text targetText = craftSlot.GetComponentInChildren<TMP_Text>();
                targetText.text = (resultCount * count).ToString();
            }
            else
            {
                Debug.LogWarning("text untuk item tidak ditemukan di dalam slot!");
            }
      

            // Menambahkan listener untuk menampilkan deskripsi bahan saat diklik
            Button button = craftSlot.GetComponent<Button>();
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
            GameObject resultSlot = itemResultSlot;
            resultSlot.gameObject.SetActive(true);



            // Set nama item (untuk debugging atau keperluan lain)
            resultSlot.gameObject.name = result.name;

            // Set sprite untuk ingredient
            Transform imageTransform = resultSlot.transform.Find("itemImage");
            if (imageTransform != null)
            {
                imageTransform.gameObject.SetActive(true);
                Image targetImage = imageTransform.GetComponent<Image>();
                targetImage.sprite = result.sprite;
            }
            else
            {
                Debug.LogWarning("Image untuk item tidak ditemukan di dalam slot!");
            }

            // Set jumlah ingredient
            Transform textTransform = resultSlot.transform.Find("itemCount");
            if (textTransform != null)
            {
                textTransform.gameObject.SetActive(true);
                TMP_Text targetText = textTransform.GetComponent<TMP_Text>();
                targetText.text = count.ToString();
            }
            else
            {
                Debug.LogWarning("Text untuk item tidak ditemukan di dalam slot!");
            }

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
        popUp.gameObject.SetActive(false);
        // Menghapus item yang ada di setiap ItemCraft slot
        DestroyItemInSlot(ItemCraft1);
        DestroyItemInSlot(ItemCraft2);
        DestroyItemInSlot(ItemCraft3);
        DestroyItemInSlot(ItemCraft4);
        DestroyItemInSlot(ItemResult);

    }

    private void DestroyItemInSlot(GameObject itemCraftSlot)
    {
        // Hapus semua child di dalam slot (item yang ada)
        foreach (Transform child in itemCraftSlot.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void PopUpCraftResult(Item item)
    {
        popUp.gameObject.SetActive(true);
        selectedItem = item;
        resultCount = 1; // Default ke 1

        // Tampilkan gambar dan jumlah item
        imagePopUp.sprite = item.sprite;
        itemCount.text = resultCount.ToString(); // Pastikan UI diperbarui langsung

        // Reset event listener agar tidak bertumpuk
        minResult.onClick.RemoveAllListeners();
        plusItem.onClick.RemoveAllListeners();
        maxResult.onClick.RemoveAllListeners();
        confirm.onClick.RemoveAllListeners();
        cancel.onClick.RemoveAllListeners();

        // Tambahkan fungsi ke tombol UI
        plusItem.onClick.AddListener(IncreaseItemCount);
        minusItem.onClick.AddListener(DecreaseItemCount);
        minResult.onClick.AddListener(MinimizeItemCount);
        confirm.onClick.AddListener(() => ConfirmResultCraft(selectedItem, resultCount)); // Panggil setelah konfirmasi

        cancel.onClick.AddListener(() =>
        {
            popUp.gameObject.SetActive(false);
        });
    }


    private void IncreaseItemCount()
    {
        resultCount++;
        itemCount.text = resultCount.ToString();


    }

    private void DecreaseItemCount()
    {
        if (resultCount > 1) // Tidak boleh kurang dari 1
        {
            resultCount--;
            itemCount.text = resultCount.ToString();
        }
    }

    // Maksimalkan jumlah item yang bisa dipilih
    //private void MaximizeItemCount()
    //{
    //    resultCount = selectedItem.stackCount;
    //    itemCount.text = resultCount.ToString();
    //}

    // Kembalikan jumlah item ke 1
    private void MinimizeItemCount()
    {
        resultCount = 1;
        itemCount.text = resultCount.ToString();
    }

    private void ConfirmResultCraft(Item result, int count)
    {
        craftScript.hasilCraftItem = result;
        craftScript.hasilCraftItem.stackCount = count;

        // Cek bahan sebelum crafting
        CheckIngredients(result.name);

        // Menampilkan hasil crafting di slot hasil
        DisplayResultInSlot(ItemResult, result, count);

        // Pastikan buttonCraft tetap memiliki listener setelah konfirmasi
        craftScript.buttonCraft.onClick.RemoveAllListeners();
        craftScript.buttonCraft.onClick.AddListener(craftScript.Crafting);

        // Tutup pop-up setelah crafting dikonfirmasi
        popUp.gameObject.SetActive(false);
    }



}
