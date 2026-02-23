using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FarmTemplateDatabaseSO", menuName = "Database/FarmTemplateDatabaseSO")]
public class FarmTemplateDatabaseSO : ScriptableObject
{
    public List<HoedTileData> hoedTilesList = new List<HoedTileData>();
}
