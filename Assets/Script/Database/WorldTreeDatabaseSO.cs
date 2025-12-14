using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldTreeDatabase", menuName = "Database/World Tree Placement Database")]
public class WorldTreeDatabaseSO : ScriptableObject
{
    public List<TreePlacementData> initialTreePlacements;
}