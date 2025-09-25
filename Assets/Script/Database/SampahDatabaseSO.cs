using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SampahDatabaseSO", menuName = "Database/SampahDatabaseSO")]
public class SampahDatabaseSO : ScriptableObject
{
    public List<SampahDatabase> listSampah = new List<SampahDatabase>();
}
