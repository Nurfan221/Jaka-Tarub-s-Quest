using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Recipe
{
    public string recipeName; // Nama resep
    public Item ingredient;    // Bahan-bahan yang dibutuhkan
    public float ingredientCount; // jumlah bahan yang di perlukan
    public Item result;    // Hasil dari resep ini
}

public class RecipeDatabase : MonoBehaviour
{
    public static RecipeDatabase Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    [System.Serializable]
    public class CraftRecipe
    {
        // --- SEBELUMNYA ---
        // public List<Item> ingredients;
        // public List<int> ingredientsCount;
        // public Item result;

        // +++ MENJADI +++
        public List<ItemData> ingredients; // Satu list untuk bahan dan jumlahnya
        public ItemData result;            // Satu object untuk hasil dan jumlahnya
    }

    [Header("Daftar Semua Resep")]
    public List<CraftRecipe> craftRecipes;




    [Header("Daftar Resep Makanan")]
    public Recipe[] cookRecipes; // Array yang menyimpan semua resep makanan

    [Header("Daftar Resep Khusus (Opsional)")]
    public Recipe[] specialRecipes; // Resep khusus atau lainnya

 
}
