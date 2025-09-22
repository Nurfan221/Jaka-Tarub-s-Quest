using UnityEngine;
using System.Collections.Generic;

// Atribut ini PENTING! Ia memberitahu Unity bahwa class ini bisa diubah
// menjadi format lain (seperti JSON) untuk disimpan.


[System.Serializable]
public class PlayerSaveData
{
    // Data Posisi
    public Vector2 position;

    // Data Health & Stamina
    public int health;
    public int currentHealthCap;
    public float stamina;
    public float currentStaminaCap;

    // Data Status Emosional
    public bool isInGrief;
    public float currentGriefPenalty;
    public int healingQuestsCompleted;
    public float currentFatiguePenalty;

    // Data Inventory & Equipment (menjawab pertanyaan Anda)
    public List<ItemData> inventory;
    public List<ItemData> equippedItemData;
    public List<ItemData> itemUseData;
    public bool equipped1;
    public bool itemUse1;
}

[System.Serializable]
public class StorageSaveData
{
    public string id; // ID unik dari peti ini
    public Vector2 storagePosition;
    public List<ItemData> itemsInStorage; // Daftar item di dalamnya
}

[System.Serializable]
public class StoneRespawnSaveData
{
    public string id; // ID unik dari batu ini
    public Vector2 stonePosition;
    public int dayToRespawn;
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
    public List<TreePlacementData> savedTrees = new List<TreePlacementData>();
    public List<PlayerSaveData> savedPlayerData = new List<PlayerSaveData>();
    public List<StorageSaveData> savedStorages = new List<StorageSaveData>();
    public List<StoneRespawnSaveData> queueRespownStone = new List<StoneRespawnSaveData>();
    public TimeSaveData timeSaveData;
}

[System.Serializable]
public class SaveableEntityData
{
    public string id;       // ID unik dari objek (misal: dari UniqueID.cs)
    public object state;    // Data yang disimpan (misal: TreeSaveData)
}

