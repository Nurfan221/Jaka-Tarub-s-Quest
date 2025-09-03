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

[System.Serializable]
public class  EmoticonTemplate
{
    public string emoticonName;
    public Sprite emoticonSprite;
}

[System.Serializable]
public class CraftRecipe
{
    public List<ItemData> ingredients; // Satu list untuk bahan dan jumlahnya
    public ItemData result;            // Satu object untuk hasil dan jumlahnya
}


public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    // Seret aset-aset database Anda ke sini di Inspector
    public CraftingDatabaseSO craftingDatabase;
    public CookingDatabaseSO cookingDatabase;
    public EmoticonDatabaseSO emoticonDatabase;

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
}