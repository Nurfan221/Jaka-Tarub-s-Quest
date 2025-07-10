using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class Checkingredients : MonoBehaviour
{
    [SerializeField] Craft craftScript;
    [SerializeField] Transform ContentGO; 
    [SerializeField] Transform SlotTemplate; // Parent untuk menempatkan hasil resep
    public GameObject popUp;
    public Image imagePopUp;
    public TextMeshProUGUI itemCount;

    [Header("Deskripsi Craft")]
    public bool twoIngredients;
    public bool checkRecipes = false;
    public int recipeCount;
    public string itemActive;
    public Item selectedItem;

    // Menambahkan UI untuk slot crafting
    public GameObject itemCraft1;
    public GameObject itemCraft2;
    public GameObject itemCraft3;
    public GameObject itemCraft4;
    public GameObject itemResult;
    public int resultCount;
    //Loop hanya sesuai jumlah bahan
    public GameObject[] craftSlots;

    [Header("Button Action")]

    // [SerializeField] Button itemAction;
    public Button plusItem;
    public Button minusItem;
    public Button confirm;
    public Button cancel;
    public Button maxResult;
    public Button minResult;

    private RecipeDatabase database;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (RecipeDatabase.Instance != null)
        {
            database = RecipeDatabase.Instance;
        }
        else
        {
            Debug.LogError("RecipeDatabase.Instance tidak ditemukan saat Awake!");
        }
    }

    public void Start()
    {
        RefreshRecipe();
        if (twoIngredients)
        {
            recipeCount = 2;
        }else
        {
            recipeCount = 4;
        }

        if (itemCraft1 == null || itemCraft2 == null || itemCraft3 == null || itemCraft4 == null)
        {
            Debug.Log("itemCraft ada yang belum terhubung");
        }

        craftSlots = new GameObject[] { itemCraft1, itemCraft2, itemCraft3, itemCraft4 };
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
        foreach (RecipeDatabase.CraftRecipe recipe in database.craftRecipes)
        {
            int ingredientCount = recipe.ingredients.Count; // Hitung jumlah bahan dalam resep

            // Filter berdasarkan jumlah ingredients
            if ((twoIngredients && ingredientCount == 2) || (!twoIngredients && ingredientCount > 2))
            {
                // Instansiasi SlotTemplate untuk setiap resep yang sesuai dengan kondisi
                Transform recipeSlot = Instantiate(SlotTemplate, ContentGO);
                recipeSlot.gameObject.SetActive(true);

                Item resultItem = ItemPool.Instance.GetItem(recipe.result.itemName);
                // Set sprite gambar hasil resep
                recipeSlot.GetChild(0).GetComponent<Image>().sprite = resultItem.sprite;

                // Set jumlah hasil (biasanya hanya 1)
                recipeSlot.GetChild(1).GetComponent<TMP_Text>().text = "1";

                // Set nama slot berdasarkan nama hasil resep
                recipeSlot.name = recipe.result.itemName;
                recipeSlot.name = recipeSlot.name.Replace("(Clone)", ""); // Hilangkan "(Clone)"

                // Tambahkan listener untuk menampilkan popup crafting
                Button button = recipeSlot.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    PopUpCraftResult(resultItem);
                });
            }
        }
    }



    private void CheckIngredients(string nameResult)
    {
        // Reset SEMUA slot sebelum memproses resep baru
        ResetIngredientSlots(4); // Reset sampai 4 agar item sebelumnya tidak tersisa

        foreach (RecipeDatabase.CraftRecipe recipe in database.craftRecipes)
        {
            if (recipe.result.itemName == nameResult)
            {
                if ((twoIngredients && recipe.ingredients.Count > 2) || (!twoIngredients && recipe.ingredients.Count <= 2))
                {
                    Debug.LogWarning($"Resep {nameResult} tidak cocok dengan mode crafting.");
                    return;
                }

                recipeCount = recipe.ingredients.Count;
                ResetIngredientSlots(recipeCount);

                // Loop menggunakan struktur ItemData yang baru
                for (int i = 0; i < recipeCount; i++)
                {
                    ItemData ingredientData = recipe.ingredients[i];
                    // Kirim seluruh ItemData ke fungsi Display
                    DisplayIngredientInSlot(craftSlots[i], ingredientData);
                    craftScript.CheckItemtoCraft(i + 1);
                }
            }
        }
    }



    private void ResetIngredientSlots(int countRecipe)
    {
        // Reset list bahan crafting
        craftScript.ingredientItemList.Clear();
         
        // Nonaktifkan semua slot ingredient


        if (twoIngredients)
        {

            for(int i = 0; i<recipeCount; i++)
            {
                ResetChildVisibility(craftSlots[i], "itemImage");
                ResetChildVisibility(craftSlots[i], "ItemInInventory");
                ResetChildVisibility(craftSlots[i], "IngridientCount");
            }
        }
        else
        {
            for (int i = 0; i < countRecipe; i++)
            {
                ResetChildVisibility(craftSlots[i], "itemImage");
                ResetChildVisibility(craftSlots[i], "ItemInInventory");
                ResetChildVisibility(craftSlots[i], "IngridientCount");
            }
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


    private void DisplayIngredientInSlot(GameObject itemCraftSlot, ItemData ingredientData)
    {
        if (itemCraftSlot != null)
        {
            // Ambil data dari ItemData
            Item ingredientItem = ItemPool.Instance.GetItem(ingredientData.itemName);
            int requiredCount = ingredientData.count;

            GameObject craftSlot = itemCraftSlot;
            craftSlot.gameObject.SetActive(true);
            craftSlot.gameObject.name = ingredientItem.name;

            // Tambahkan ke list bahan crafting di Craft.cs
            // Kita tidak perlu clone, cukup tambahkan ItemData-nya langsung
            craftScript.ingredientItemList.Add(ingredientData);

            // Set sprite
            Transform imageTransform = craftSlot.transform.Find("itemImage");
            if (imageTransform != null)
            {
                imageTransform.gameObject.SetActive(true);
                imageTransform.GetComponent<Image>().sprite = ingredientItem.sprite;
            }

            // Set jumlah bahan yang dibutuhkan
            Transform TextTransform = craftSlot.transform.Find("IngridientCount");
            if (TextTransform != null)
            {
                TextTransform.gameObject.SetActive(true);
                // Jumlah total yang dibutuhkan = jumlah per resep * jumlah hasil yang ingin dibuat
                int totalRequired = requiredCount * resultCount;
                TextTransform.GetComponent<TMP_Text>().text = totalRequired.ToString();
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
        DestroyItemInSlot(itemCraft1);
        DestroyItemInSlot(itemCraft2);
        DestroyItemInSlot(itemCraft3);
        DestroyItemInSlot(itemCraft4);
        DestroyItemInSlot(itemResult);

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
        //craftScript.hasilCraftItem.stackCount = count;

        // Cek bahan sebelum crafting
        CheckIngredients(result.name);

        // Menampilkan hasil crafting di slot hasil
        DisplayResultInSlot(itemResult, result, count);

        // Pastikan buttonCraft tetap memiliki listener setelah konfirmasi
        craftScript.buttonCraft.onClick.RemoveAllListeners();
        craftScript.buttonCraft.onClick.AddListener(craftScript.Crafting);

        // Tutup pop-up setelah crafting dikonfirmasi
        popUp.gameObject.SetActive(false);
    }



}
