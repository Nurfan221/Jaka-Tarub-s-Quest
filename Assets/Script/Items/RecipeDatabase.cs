using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Recipe
{
    public string recipeName; // Nama resep
    public GameObject[] ingredients; // Bahan-bahan yang dibutuhkan
    public GameObject result; // Hasil dari resep ini
}

public class RecipeDatabase : MonoBehaviour
    {


        [System.Serializable]
        public class CraftRecipe
        {
            public List<Item> ingredients;        // Daftar item yang dibutuhkan untuk resep
            public List<int> ingredientsCount;    // Jumlah item yang dibutuhkan untuk setiap ingredient
            public Item result;                   // Item hasil craft
        }

    [Header("Daftar Semua Resep")]
    public List<CraftRecipe> craftRecipes;

   


    [Header("Daftar Resep Makanan")]
    public Recipe[] foodRecipes; // Array yang menyimpan semua resep makanan

    [Header("Daftar Resep Khusus (Opsional)")]
    public Recipe[] specialRecipes; // Resep khusus atau lainnya
}