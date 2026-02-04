using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingDatabase", menuName = "Database/Crafting Recipe Database")]
public class CraftingDatabaseSO : ScriptableObject
{
    // Gunakan CraftingRecipeSO jika Anda sudah memisahkannya, atau class biasa
    public List<CraftRecipe> craftRecipes;
    public List<CraftRecipe> craftFoodRecipe;
}