using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

// Definisikan HoedTileData di sini agar bisa diakses
[System.Serializable]
public class HoedTileData
{
    public Vector3Int tilePosition;
    public int hoedTime;
    public bool watered;
    public bool isFarm;
    public string plantedItemName; // Menyimpan nama item benihnya
    //public GameObject plantStatus; // Hati-hati dengan referensi GameObject di SO

    public HoedTileData(Vector3Int pos, int time)
    {
        tilePosition = pos;
        hoedTime = time;
        watered = false;
        isFarm = false;
        plantedItemName = null;
        //plantStatus = null;

    }
}

[CreateAssetMenu(fileName = "New Farm Data", menuName = "JakaTarub/Farm Data")]
public class FarmData_SO : ScriptableObject
{
    //public Tilemap tilemap;
    public Tile hoeedTile; // Tile untuk hasil cangkul
    public Tile wateredTile; // Tile untuk hasil cangkul
    public Tile emptySoilTile; // Tile tanah kosong (boleh dicangkul)
    public Tile grassTile; // tile grass

    // Pindahkan list Anda ke sini
    public List<HoedTileData> hoedTilesList = new List<HoedTileData>();

    // Tambahkan fungsi untuk mereset data saat memulai game baru
    public void ClearData()
    {
        hoedTilesList.Clear();
    }
}