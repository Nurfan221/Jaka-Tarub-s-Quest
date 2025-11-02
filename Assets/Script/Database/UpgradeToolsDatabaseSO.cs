using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeToolsDatabaseSO", menuName = "Database/UpgradeToolsDatabaseSO")]
public class UpgradeToolsDatabaseSO : ScriptableObject
{
    public List<UpgradeToolsDatabase> upgradeTools = new List<UpgradeToolsDatabase>();
}
