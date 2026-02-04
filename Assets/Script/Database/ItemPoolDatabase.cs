using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPoolDatabase", menuName = "Database/ItemPool Database")]
public class ItemPoolDatabase : ScriptableObject
{
    // Pastikan class 'Item' Anda sudah [System.Serializable]
    public List<Item> items = new List<Item>();
}
