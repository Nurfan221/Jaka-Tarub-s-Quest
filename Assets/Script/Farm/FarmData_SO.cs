using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;


[System.Serializable]
public class HoedTileData
{
    public Vector3Int tilePosition;
    public int hoedTime;
    public bool watered;
    public bool isPlanted;
    public int growthProgress;
    public string plantedItemName;
    public GrowthStage currentStage;

    public HoedTileData(Vector3Int pos, int time)
    {
        tilePosition = pos;
        hoedTime = time;
        watered = false;
        isPlanted = false;
        growthProgress = 0;
        plantedItemName = null;
        currentStage = GrowthStage.none; // Inisialisasi dengan tahap awal
    }
}


[CreateAssetMenu(fileName = "New Farm Data", menuName = "JakaTarub/Farm Data")]
public class FarmData_SO : ScriptableObject
{
    [Header("Tile Assets")]
    public Tile hoeedTile;     // Tile untuk tanah yang baru dicangkul
    public Tile wateredTile;   // Tile untuk tanah yang sudah disiram
    public Tile emptySoilTile; // Tile tanah kosong (bisa dicangkul)
    public Tile grassTile;     // Tile rumput

    [Header("Farm State Data")]
    public List<HoedTileData> hoedTilesList = new List<HoedTileData>();


    public void ClearData()
    {
        hoedTilesList.Clear();
    }
}