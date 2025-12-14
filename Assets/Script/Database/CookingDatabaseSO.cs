using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CookingDatabase", menuName = "Database/Cooking Recipe Database")]
public class CookingDatabaseSO : ScriptableObject
{
    public List<RecipeCooking> cookRecipes;
    public List<RecipeCooking> smeltRecipes;
}