using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CookingDatabase", menuName = "Database/Cooking Recipe Database")]
public class CookingDatabaseSO : ScriptableObject
{
    public List<Recipe> cookRecipes;
}