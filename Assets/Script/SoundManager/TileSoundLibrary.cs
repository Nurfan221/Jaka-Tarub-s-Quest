using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TileSoundLibrary", menuName = "Audio/Tile Sound Library")]
public class TileSoundLibrary : ScriptableObject
{
    // Class kecil untuk menyimpan pasangan Tile + Suara
    [System.Serializable]
    public class TileData
    {
        public TileBase tileAsset; // Masukkan Tile lamamu ke sini
        public SurfaceType surfaceType; // Pilih jenis suaranya
    }

    [Header("Daftarkan Tile Disini")]
    public List<TileData> tileList;

    // Dictionary untuk pencarian cepat (Optimasi)
    private Dictionary<TileBase, SurfaceType> tileDictionary;

    // Fungsi ini mengubah List jadi Dictionary saat game mulai (agar cepat)
    public void Initialize()
    {
        tileDictionary = new Dictionary<TileBase, SurfaceType>();
        foreach (var item in tileList)
        {
            if (item.tileAsset != null && !tileDictionary.ContainsKey(item.tileAsset))
            {
                tileDictionary.Add(item.tileAsset, item.surfaceType);
            }
        }
    }

    // Fungsi untuk ditanya oleh Player
    public SurfaceType GetSurfaceType(TileBase tileToCheck)
    {
        if (tileDictionary == null) Initialize();

        if (tileToCheck != null && tileDictionary.ContainsKey(tileToCheck))
        {
            return tileDictionary[tileToCheck];
        }

        // Kalau tile tidak terdaftar, anggap saja tanah biasa (Default)
        return SurfaceType.Dirt;
    }
}