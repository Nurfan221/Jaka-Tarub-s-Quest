using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;





[CreateAssetMenu(fileName = "New Farm Data", menuName = "JakaTarub/Farm Data")]
public class FarmData_SO : ScriptableObject
{
    [Header("Tile Assets")]
    public Tile hoeedTile;     // Tile untuk tanah yang baru dicangkul
    public Tile wateredTile;   // Tile untuk tanah yang sudah disiram
    public Tile emptySoilTile; // Tile tanah kosong (bisa dicangkul)
    public Tile grassTile;     // Tile rumput
    public Tile fertilizerTile; // Tile pupuk yang belum disiram
    public Tile fertilezerTileWatered; // Tile pupuk yang sudah disiram

    [Header("Farm State Data")]
    public List<HoedTileData> hoedTilesList = new List<HoedTileData>();


    public void ClearData()
    {
        hoedTilesList.Clear();
    }
}