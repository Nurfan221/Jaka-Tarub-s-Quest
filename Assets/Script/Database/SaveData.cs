using UnityEngine;
using System.Collections.Generic;

// Atribut ini PENTING! Ia memberitahu Unity bahwa class ini bisa diubah
// menjadi format lain (seperti JSON) untuk disimpan.
[System.Serializable]
public class TreeSaveData
{
    public string treeName;
    public Vector3 position;
    public GrowthTree currentGrowthStage;
    public float growthTimer;
    public bool sudahTumbang;
}

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
    public List<SaveableEntityData> savedEntities = new List<SaveableEntityData>();
}

[System.Serializable]
public class SaveableEntityData
{
    public string id;       // ID unik dari objek (misal: dari UniqueID.cs)
    public object state;    // Data yang disimpan (misal: TreeSaveData)
}

