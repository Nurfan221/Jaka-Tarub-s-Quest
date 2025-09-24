using UnityEngine;
using System.Collections.Generic;

// Atribut ini PENTING! Ia memberitahu Unity bahwa class ini bisa diubah
// menjadi format lain (seperti JSON) untuk disimpan.


// Anda bisa menambahkan class data save lain di file yang sama untuk kerapian
// [System.Serializable]
// public class ChestSaveData 
// {
//     public string chestID;
//     public List<ItemData> items;
// }

// Ini adalah wadah utama yang akan kita ubah menjadi JSON
[System.Serializable]
public class GameSaveData
{
    // Wadah utama kita sekarang adalah sebuah LIST, bukan Dictionary
    public List<TreePlacementData> savedTrees = new List<TreePlacementData>();
    public List<PlayerSaveData> savedPlayerData = new List<PlayerSaveData>();
    public List<StorageSaveData> savedStorages = new List<StorageSaveData>();
    public List<StoneRespawnSaveData> queueRespownStone = new List<StoneRespawnSaveData>();
    public List<HoedTileData> savedHoedTilesList = new List<HoedTileData>();
    public TimeSaveData timeSaveData;
}

[System.Serializable]
public class SaveableEntityData
{
    public string id;       // ID unik dari objek (misal: dari UniqueID.cs)
    public object state;    // Data yang disimpan (misal: TreeSaveData)
}

