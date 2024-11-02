    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class FarmTile : MonoBehaviour
    {
        [SerializeField] public Tilemap tilemap;
        [SerializeField] public Tile hoeedTile; // Tile untuk hasil cangkul
        [SerializeField] public Tile wateredTile; // Tile untuk hasil cangkul
        [SerializeField] private Tile emptySoilTile; // Tile tanah kosong (boleh dicangkul)

        public bool siram = false;

        private List<Vector3Int> wateredTiles = new List<Vector3Int>(); // Menyimpan posisi tile yang disiram
        private List<Vector3Int> hoedTilesList = new List<Vector3Int>(); // Menyimpan posisi tile yang dicangkul

        public float time; // Simulasi waktu
        private float waktuSaatIni;

        public void start() 
        {
            waktuSaatIni = time;
        }

        
        void Update()
        {
            // Simulasikan waktu berjalan
            // time += Time.deltaTime;

            // Ketika time mencapai 1, kembalikan tile yang sudah disiram ke hoeedTile
            if (time == waktuSaatIni +1)
            {
                waktuSaatIni = time;
                ResetWateredTiles();
                ResetHoedTilesList();
            }
        }

        public void HoeTile(Vector3 playerPosition, Vector3 faceDirection)
        {
            // Konversi posisi pemain ke cell tilemap
            Vector3Int playerTilePos = tilemap.WorldToCell(playerPosition);

            // Menentukan tile berdasarkan arah face
            Vector3Int tileToHoe = playerTilePos + new Vector3Int(Mathf.RoundToInt(faceDirection.x), Mathf.RoundToInt(faceDirection.y), 0);

            // Mendapatkan tile yang ada di posisi tersebut
            TileBase currentTile = tilemap.GetTile(tileToHoe);

            // Lakukan pengecekan dan penggantian tile seperti biasa
            if (currentTile == emptySoilTile)
            {
                tilemap.SetTile(tileToHoe, hoeedTile);
                Debug.Log($"Tile at {tileToHoe} has been changed to hoeedTile.");
                  // Simpan posisi tile yang sudah disiram
                if (!hoedTilesList.Contains(tileToHoe))
                {
                    hoedTilesList.Add(tileToHoe);
                }
            }
            else
            {
                Debug.Log("Cannot hoe this tile, it's not empty soil.");
            }
        }
   // Fungsi untuk menyiram tile
    public void WaterTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        Vector3Int playerTilePos = tilemap.WorldToCell(playerPosition);
        Vector3Int tileToWater = playerTilePos + new Vector3Int(Mathf.RoundToInt(faceDirection.x), Mathf.RoundToInt(faceDirection.y), 0);

        TileBase currentTile = tilemap.GetTile(tileToWater);

        if (currentTile == hoeedTile)
        {
            siram = true;
            tilemap.SetTile(tileToWater, wateredTile);
            Debug.Log($"Tile at {tileToWater} has been changed to wateredTile.");

            // Simpan posisi tile yang sudah disiram
            if (!wateredTiles.Contains(tileToWater))
            {
                wateredTiles.Add(tileToWater);
            }
        }
        else
        {
            Debug.Log("Cannot water this tile, it's not hoeedTile.");
        }
    }
    // Fungsi untuk mengembalikan tile yang sudah disiram ke hoeedTile
    private void ResetWateredTiles()
    {
        foreach (var tilePos in wateredTiles)
        {
            siram = false;
            // Ganti kembali ke hoeedTile
            tilemap.SetTile(tilePos, hoeedTile);
            Debug.Log($"Tile at {tilePos} has been reset to hoeedTile.");
        }

        // Bersihkan daftar tile yang sudah di-reset
        wateredTiles.Clear();
    }
    private void ResetHoedTilesList()
    {
        Debug.Log("fungsi reset hoe tile list di panggil ");
        foreach (var tilePos in hoedTilesList)
        {
            // Ganti kembali ke hoeedTile
            tilemap.SetTile(tilePos, emptySoilTile);
            Debug.Log($"Tile at {tilePos} has been reset to hoeedTile.");
        }

        // Bersihkan daftar tile yang sudah di-reset
        hoedTilesList.Clear();
    }


    }
