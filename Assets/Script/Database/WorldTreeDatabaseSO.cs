using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WorldTreeDatabase", menuName = "Database/World Tree Placement Database")]
public class WorldTreeDatabaseSO : ScriptableObject
{
    public List<TreePlacementData> initialTreePlacements;
}