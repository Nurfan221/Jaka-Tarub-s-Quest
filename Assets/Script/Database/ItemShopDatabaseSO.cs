using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemShopDatabaseSO", menuName = "Database/ItemShopDatabaseSO")]
public class ItemShopDatabaseSO : ScriptableObject
{
    public List<ShopTypeDatabase> shopTypeDatabases;
    public List<ItemShopSaveData> itemShopSaveData;
    //public List<ItemShopDatabase> itemShopDatabases;
    public List<Item> itemWajib; // List item wajib yang harus ada di toko
}
