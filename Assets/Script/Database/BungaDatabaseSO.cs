using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentDatabaseSO", menuName = "Database/EnvironmentDatabaseSO")]
public class EnvironmentDatabaseSO : ScriptableObject
{
    public List<EnvironmentSaveData> FlowerSaveData = new List<EnvironmentSaveData>();
    public List<EnvironmentDatabase> flowerDatabases = new List<EnvironmentDatabase>();

    
    public List<EnvironmentSaveData> jamurSaveData = new List<EnvironmentSaveData>();
    public List<EnvironmentDatabase> jamurDatabases = new List<EnvironmentDatabase>();
}
