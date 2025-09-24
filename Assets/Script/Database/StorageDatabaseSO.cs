using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageDatabaseSO", menuName = "Database/StorageDatabaseSO")]
public class StorageDatabaseSO : ScriptableObject
{
    public List<StorageSaveData> savedStorages = new List<StorageSaveData>();
}
