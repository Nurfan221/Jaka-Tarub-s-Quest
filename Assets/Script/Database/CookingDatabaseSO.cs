using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CookingDatabase", menuName = "Database/Cooking Recipe Database")]
public class CookingDatabaseSO : ScriptableObject
{
    public List<RecipeCooking> cookRecipes;
    public List<RecipeCooking> smeltRecipes;
}