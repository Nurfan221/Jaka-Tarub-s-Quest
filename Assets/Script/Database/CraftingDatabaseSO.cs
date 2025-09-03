using System.Collections.Generic;
using UnityEngine;
using static DatabaseManager;

[CreateAssetMenu(fileName = "CraftingDatabase", menuName = "Database/Crafting Recipe Database")]
public class CraftingDatabaseSO : ScriptableObject
{
    // Gunakan CraftingRecipeSO jika Anda sudah memisahkannya, atau class biasa
    public List<CraftRecipe> craftRecipes;
}