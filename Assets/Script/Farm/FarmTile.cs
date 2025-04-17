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
        [SerializeField] public Tile grassTile; // tile grass

        public bool siram = false;

        private List<Vector3Int> wateredTiles = new List<Vector3Int>(); // Menyimpan posisi tile yang disiram
        public List<HoedTileData> hoedTilesList = new List<HoedTileData>();
        public List<GameObject> plantStatus;

    [SerializeField] private TimeManager timeManager;  // Referensi ke timeManager.dateManager

      

        public void start() 
        {
           
            
        }

        
        void Update()
        {
            // Simulasikan waktu berjalan
            // timeManager.date += timeManager.date.deltatimeManager.date;

            // Ketika timeManager.date mencapai 1, kembalikan tile yang sudah disiram ke hoeedTile
            // if (timeManager.date == waktuKetikaSiram +1)
            // {
            //     Debug.Log("set tile kembali ke tanah di pacul");
            //     waktuKetikaSiram = timeManager.date;
            //     ResetWateredTiles();
            //     // ResetHoedTilesList();
            // }
            // if (timeManager.date == waktuKetikaCangkul +4)
            // {
            //     Debug.Log("set tile kembali ke tanah di pacul");
            //     // ResetWateredTiles();
            //     ResetHoedTilesList();
            // }

            // foreach (var hoedTile in hoedTilesList)
            // {
            //     // Jika waktu sekarang >= waktu cangkul + 4 hari, reset tile
            //     if (timeManager.date >= hoedTile.hoedTime + 4)
            //     {
            //         Debug.Log($"Resetting tile at {hoedTile.tilePosition}, time passed: {timeManager.date - hoedTile.hoedTime} days.");
            //         ResetHoedTilesList(hoedTile.tilePosition); // Panggil reset untuk tile yang sudah lebih dari 4 hari
            //     }
            // }

        }

        public void HoeTile(Vector3 playerPosition, Vector3 faceDirection)
        {
            // Konversi posisi pemain ke cell tilemap
            Vector3Int playerTilePos = tilemap.WorldToCell(playerPosition);

            // Menentukan tile berdasarkan arah face
            Vector3Int tileToHoe = playerTilePos + new Vector3Int(Mathf.RoundToInt(faceDirection.x), Mathf.RoundToInt(faceDirection.y), 0);

            // Mendapatkan tile yang ada di posisi tersebut
            TileBase currentTile = tilemap.GetTile(tileToHoe);

            // Lakukan pengecekan dan penggantian tile
            if (currentTile == emptySoilTile)
            {
                tilemap.SetTile(tileToHoe, hoeedTile);
                Debug.Log($"Tile at {tileToHoe} has been changed to hoeedTile.");

                // Simpan posisi tile yang dicangkul dan waktu cangkul
                HoedTileData newHoedTile = new HoedTileData(tileToHoe, timeManager.date);
                hoedTilesList.Add(newHoedTile);
                Debug.Log($"Tile at {tileToHoe} hoed at time: {timeManager.date}");
            }
            else
            {
                Debug.Log("Cannot hoe this tile, it's not empty soil.");
            }
        }



         // Fungsi untuk mengecek apakah tile sudah dicangkul sebelumnya
    private bool TileAlreadyHoed(Vector3Int tilePos)
    {
        foreach (HoedTileData data in hoedTilesList)
        {
            if (data.tilePosition == tilePos)
            {
                return true;
            }
        }
        return false;
    }

    // Fungsi untuk menyiram tile
    public void WaterTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        // Konversi posisi pemain ke cell tilemap
        Vector3Int playerTilePos = tilemap.WorldToCell(playerPosition);

        // Menentukan tile berdasarkan arah face
        Vector3Int tilesiram = playerTilePos + new Vector3Int(Mathf.RoundToInt(faceDirection.x), Mathf.RoundToInt(faceDirection.y), 0);

        // Cari tile yang dicangkul pada posisi ini di hoedTilesList
        HoedTileData hoedTile = hoedTilesList.Find(tile => tile.tilePosition == tilesiram);

        // Jika tidak ada data tile atau tile sudah disiram, keluar dari fungsi
        if (hoedTile == null)
        {
            Debug.Log("Tile belum dicangkul, tidak bisa disiram.");
            return;
        }
        if (hoedTile.watered)
        {
            Debug.Log("Tile sudah disiram. Tidak perlu menyiram lagi.");
            return;
        }

        // Mendapatkan tile yang ada di posisi tersebut
        TileBase currentTile = tilemap.GetTile(tilesiram);

        // Lakukan pengecekan dan penggantian tile jika tile dalam keadaan hoeedTile
        if (currentTile == hoeedTile)
        {
            // Cek apakah ada objek dengan script SeedManager di posisi tile yang akan disiram
            Vector3 worldPosition = tilemap.GetCellCenterWorld(tilesiram);
            Collider2D collider = Physics2D.OverlapPoint(worldPosition);

            if (collider != null)
            {
                Debug.Log("Collider found on object: " + collider.gameObject.name);

                // Cek apakah objek yang ditemukan memiliki komponen SeedManager
                PlantSeed PlantSeed = collider.GetComponent<PlantSeed>();
                if (PlantSeed != null)
                {
                    Debug.Log("Found an object with SeedManager at this tile position.");

                    // Memanggil metode untuk menyiram tanaman
                    PlantSeed.siram = true;

                    // Ubah tile ke wateredTile dan tandai tile sebagai sudah disiram
                    tilemap.SetTile(tilesiram, wateredTile);
                    hoedTile.watered = true;
                    Debug.Log("tanggal di cangkul : " + hoedTile.hoedTime);
                    hoedTile.hoedTime = hoedTile.hoedTime + 1;
                    Debug.Log("tanggal di cangkul : " + hoedTile.hoedTime);
                    Debug.Log($"Tile at {tilesiram} has been changed to wateredTile.");
                }
                else
                {
                    Debug.Log("No SeedManager component found on this object.");
                }
            }
            else
            {
                Debug.Log("No collider found at this position.");
            }
        }
        else
        {
            Debug.Log("Cannot water this tile, it's not hoeedTile.");
        }
    }




    // Fungsi untuk mengembalikan tile yang sudah disiram ke hoeedTile
    public void ResetWateredTiles()
    {
        for (int i = 0; i < hoedTilesList.Count; i++)
        {
            HoedTileData tileData = hoedTilesList[i];
            Debug.Log($"Tile Position: {tileData.tilePosition}, Hoe Time: {tileData.hoedTime}, Watered: {tileData.watered}");
            
            // Cek apakah waktu saat ini adalah waktu cangkul dan tile dalam keadaan disiram
            if (timeManager.date + 1 == tileData.hoedTime && tileData.watered)
            {
                // Set nilai watered menjadi false
                tileData.watered = false;

                // Reset tile ke hoeedTile di tilemap
                tilemap.SetTile(tileData.tilePosition, hoeedTile);
                foreach (var plant in plantStatus)
                {
                    PlantSeed plantSeed = plant.gameObject.GetComponent<PlantSeed>();
                    plantSeed.siram = false;
                }
            }
        }

        // Bersihkan daftar tile yang sudah di-reset
        wateredTiles.Clear();
    }


    // private void ResetHoedTilesList()
    // {
    //     Debug.Log("fungsi reset hoe tile list di panggil ");
    //     foreach (var tilePos in hoedTilesList)
    //     {
    //         // Ganti kembali ke hoeedTile
    //         tilemap.SetTile(tilePos, emptySoilTile);
    //         Debug.Log($"Tile at {tilePos} has been reset to hoeedTile.");
    //     }

    //     // Bersihkan daftar tile yang sudah di-reset
    //     hoedTilesList.Clear();
    // }

    private void ResetHoedTilesList(Vector3Int tilePosition)
    {
        // Reset tile ke keadaan awal (misalnya kembali ke emptySoilTile)
        tilemap.SetTile(tilePosition, emptySoilTile);
        Debug.Log($"Tile at {tilePosition} has been reset to empty soil.");

        // Hapus dari daftar hoedTilesList
        hoedTilesList.RemoveAll(item => item.tilePosition == tilePosition);


    }


    public void CheckTile()
    {
        for (int i = 0; i < hoedTilesList.Count; i++)
        {
            HoedTileData tileData = hoedTilesList[i];
            Debug.Log($"Tile Position: {tileData.tilePosition}, Hoe Time: {tileData.hoedTime}, Watered: {tileData.watered}");
            
            // Contoh modifikasi data
            if (timeManager.date == tileData.hoedTime + 1 && tileData.watered == false)
            {
                ResetHoedTilesList(tileData.tilePosition);
            }
           
        }

    }

    public void Siram()
    {
        foreach (var plant in plantStatus)
        {
            PlantSeed plantSeed = plant.GetComponent<PlantSeed>();
            if (plantSeed.siram)
            {
                plantSeed.growthTimer++; // Tambahkan satu hari ke growthTimer
                Debug.Log("Fungsi AddOnDan di update");
                Debug.Log("nilai dari growthtime saat fungsi AddOneDay di jalankan" + plantSeed.growthTime);
                Debug.Log("nilai dari growtimer " + plantSeed.growthTimer);

                // Cek apakah growthTimer telah mencapai growthSpeed
                if (plantSeed.growthTimer % plantSeed.growthSpeed == 0)
                {
                    Debug.Log("fungsi AdvanceGrowthStage di jalankan");
                    plantSeed.AdvanceGrowthStage(); // Maju ke tahap berikutnya
                }

                // Cek apakah growthTimer telah mencapai growthTime
                if (plantSeed.growthTimer >= plantSeed.growthTime)
                {
                    plantSeed.currentStage = GrowthStage.ReadyToHarvest; // Set tahap akhir
                    Debug.Log("Tanaman siap dipanen!");
                    plantSeed.isReadyToHarvest = true;
                }
            }
        }
    }
}
