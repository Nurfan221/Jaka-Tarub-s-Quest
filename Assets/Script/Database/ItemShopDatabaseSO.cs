using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemShopDatabaseSO", menuName = "Database/ItemShopDatabaseSO")]
public class ItemShopDatabaseSO : ScriptableObject
{
    public List<ShopTypeDatabase> shopTypeDatabases;
    public List<ShopTypeDatabase> itemShopSaveData;
    //public List<ItemShopDatabase> itemShopDatabases;
}
