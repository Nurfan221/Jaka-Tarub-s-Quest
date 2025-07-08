using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmTile : MonoBehaviour
{
    public static FarmTile Instance { get; private set; }

    // Referensi ke Tilemap di scene ini
    public Tilemap tilemap;

    public bool siram = false;

    [Header("Data Ladang (Aset)")]
    private List<Vector3Int> wateredTiles = new List<Vector3Int>(); // Menyimpan posisi tile yang disiram
    public Dictionary<Vector3Int, GameObject> activePlants = new Dictionary<Vector3Int, GameObject>();
    public FarmData_SO farmData; // DRAG & DROP aset "LadangJaka_Data" ke sini

    //public List<GameObject> plantStatus;

    [SerializeField] private TimeManager timeManager;  // Referensi ke timeManager.dateManager


    private void Awake()
    {
        // Logika Singleton standar, tapi tanpa DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Fungsi untuk mengubah tile sekarang bisa diakses dari mana saja
    public void ChangeTile(Vector3Int position, Tile newTile)
    {
        tilemap.SetTile(position, newTile);
    }
    public void Start()
    {

        RestoreFarmStateFromData();
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
        //     // ResetfarmData.hoedTilesList();
        // }
        // if (timeManager.date == waktuKetikaCangkul +4)
        // {
        //     Debug.Log("set tile kembali ke tanah di pacul");
        //     // ResetWateredTiles();
        //     ResetfarmData.hoedTilesList();
        // }

        // foreach (var hoedTile in farmData.hoedTilesList)
        // {
        //     // Jika waktu sekarang >= waktu cangkul + 4 hari, reset tile
        //     if (timeManager.date >= hoedTile.hoedTime + 4)
        //     {
        //         Debug.Log($"Resetting tile at {hoedTile.tilePosition}, time passed: {timeManager.date - hoedTile.hoedTime} days.");
        //         ResetfarmData.hoedTilesList(hoedTile.tilePosition); // Panggil reset untuk tile yang sudah lebih dari 4 hari
        //     }
        // }

    }
    public void RestoreFarmStateFromData()
    {
        if (farmData == null || tilemap == null)
        {
            Debug.LogError("FarmData atau tilemap belum di-set di Inspector FarmTile!");
            return;
        }

        Debug.Log($"Memuat kondisi ladang. Ditemukan {farmData.hoedTilesList.Count} data tile yang dicangkul.");

        // Loop melalui setiap data tile yang tersimpan di "Buku Catatan"
        foreach (HoedTileData tileData in farmData.hoedTilesList)
        {
            // --- Langkah 1: Kembalikan Tampilan Tile ---
            Tile targetTile = tileData.watered ? farmData.wateredTile : farmData.hoeedTile;
            tilemap.SetTile(tileData.tilePosition, targetTile);

            // --- Langkah 2: Munculkan Kembali Tanaman (Jika Ada) ---
            if (tileData.isFarm && !string.IsNullOrEmpty(tileData.plantedItemName))
            {
                // Dapatkan template item dari database
                Item itemTemplate = ItemPool.Instance.GetItem(tileData.plantedItemName);
                if (itemTemplate != null && itemTemplate.prefabItem != null)
                {
                    // Tentukan posisi spawn di tengah tile
                    Vector3 spawnPosition = tilemap.GetCellCenterWorld(tileData.tilePosition);

                    // Buat instance GameObject tanaman
                    GameObject plantObject = Instantiate(itemTemplate.prefabItem, spawnPosition, Quaternion.identity);
                    

                    // Daftarkan tanaman yang baru "hidup" ini ke dictionary pelacak
                    activePlants[tileData.tilePosition] = plantObject;

                    // Anda bisa menambahkan logika untuk mengembalikan state tanaman (misal: tahap pertumbuhan) di sini
                    // Contoh:
                    // PlantSeed plantSeed = plantObject.GetComponent<PlantSeed>();
                    // if (plantSeed != null)
                    // {
                    //     plantSeed.LoadStateFromData(tileData.plantGrowthProgress);
                    // }
                }
            }
        }
        Debug.Log("Kondisi ladang berhasil dimuat!");
    }

    public GameObject GetPlantAtPosition(Vector3Int tilePosition)
    {
        if (activePlants.ContainsKey(tilePosition))
        {
            return activePlants[tilePosition];
        }
        return null;
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
        if (currentTile == farmData.emptySoilTile)
        {
            tilemap.SetTile(tileToHoe, farmData.hoeedTile);
            Debug.Log($"Tile at {tileToHoe} has been changed to hoeedTile.");

            // Simpan posisi tile yang dicangkul dan waktu cangkul
            HoedTileData newHoedTile = new HoedTileData(tileToHoe, timeManager.date);
            farmData.hoedTilesList.Add(newHoedTile);
            Debug.Log($"Tile at {tileToHoe} hoed at time: {timeManager.date}");
        }
        else
        {
            Debug.Log("Cannot hoe this tile, it's not empty soil.");
        }
    }



    //// Fungsi untuk mengecek apakah tile sudah dicangkul sebelumnya
    //private bool TileAlreadyHoed(Vector3Int tilePos)
    //{
    //    foreach (HoedTileData data in farmData.hoedTilesList)
    //    {
    //        if (data.tilePosition == tilePos)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    // Fungsi untuk menyiram tile
    public void WaterTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        // Konversi posisi pemain ke cell farmData.tilemap
        Vector3Int playerTilePos = tilemap.WorldToCell(playerPosition);

        // Menentukan tile berdasarkan arah face
        Vector3Int tilesiram = playerTilePos + new Vector3Int(Mathf.RoundToInt(faceDirection.x), Mathf.RoundToInt(faceDirection.y), 0);

        // Cari tile yang dicangkul pada posisi ini di farmData.hoedTilesList
        HoedTileData hoedTile = farmData.hoedTilesList.Find(tile => tile.tilePosition == tilesiram);

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
        if (currentTile == farmData.hoeedTile)
        {
            // Cek apakah ada objek dengan script SeedManager di posisi tile yang akan disiram
            Vector3 worldPosition = tilemap.GetCellCenterWorld(tilesiram);
            Collider2D collider = Physics2D.OverlapPoint(worldPosition);

            if (collider != null)
            {
                Debug.Log("Collider found on object: " + collider.gameObject.name);

                // Cek apakah objek yang ditemukan memiliki komponen SeedManager
                PlantSeed plantSeed = collider.GetComponent<PlantSeed>();
                if (plantSeed != null && !plantSeed.isInsect && !plantSeed.isReadyToHarvest)
                {
                    Debug.Log("Found an object with SeedManager at this tile position.");

                    // Memanggil metode untuk menyiram tanaman
                    plantSeed.siram = true;
                    plantSeed.ParticleEffect();

                    // Ubah tile ke wateredTile dan tandai tile sebagai sudah disiram
                    tilemap.SetTile(tilesiram, farmData.wateredTile);
                    hoedTile.watered = true;
                    Debug.Log("tanggal di cangkul : " + hoedTile.hoedTime);
                    hoedTile.hoedTime = hoedTile.hoedTime + 1;
                    Debug.Log("tanggal di cangkul : " + hoedTile.hoedTime);
                    Debug.Log($"Tile at {tilesiram} has been changed to wateredTile.");
                }
                else
                {
                    Debug.Log("No SeedManager component found on this object.");
                    Debug.Log("lokasi tile yang di aksi" + worldPosition);
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
        for (int i = 0; i < farmData.hoedTilesList.Count; i++)
        {
            HoedTileData tileData = farmData.hoedTilesList[i];
            Debug.Log($"Tile Position: {tileData.tilePosition}, Hoe Time: {tileData.hoedTime}, Watered: {tileData.watered}");

            // Cek apakah waktu saat ini adalah waktu cangkul dan tile dalam keadaan disiram
            if (timeManager.date + 1 == tileData.hoedTime && tileData.watered)
            {
                // Set nilai watered menjadi false
                tileData.watered = false;

                // Reset tile ke hoeedTile di farmData.tilemap
                tilemap.SetTile(tileData.tilePosition, farmData.hoeedTile);
                GameObject tanaman = GetPlantAtPosition(tileData.tilePosition);
                PlantSeed plantSeed = tanaman.gameObject.GetComponent<PlantSeed>();
                plantSeed.siram = false;
                plantSeed.PlantsTerinfeksi();
                if (plantSeed.currentStage != GrowthStage.ReadyToHarvest)
                {
                    plantSeed.ParticleEffect();
                }
            }
        }

        // Bersihkan daftar tile yang sudah di-reset
        wateredTiles.Clear();
        RandomHama();
    }


    // private void ResetfarmData.hoedTilesList()
    // {
    //     Debug.Log("fungsi reset hoe tile list di panggil ");
    //     foreach (var tilePos in farmData.hoedTilesList)
    //     {
    //         // Ganti kembali ke hoeedTile
    //         farmData.tilemap.SetTile(tilePos, emptySoilTile);
    //         Debug.Log($"Tile at {tilePos} has been reset to hoeedTile.");
    //     }

    //     // Bersihkan daftar tile yang sudah di-reset
    //     hoedTilesList.Clear();
    // }

    private void ResetHoedTilesList(Vector3Int tilePosition)
    {
        // Reset tile ke keadaan awal (misalnya kembali ke emptySoilTile)
        tilemap.SetTile(tilePosition, farmData.emptySoilTile);
        Debug.Log($"Tile at {tilePosition} has been reset to empty soil.");

        // Hapus dari daftar hoedTilesList



        foreach (var lokasiTarget in farmData.hoedTilesList)
        {
            if (lokasiTarget.tilePosition == tilePosition)
            {
                if (lokasiTarget.isFarm == true)
                {
                    GameObject tanaman = GetPlantAtPosition(lokasiTarget.tilePosition);
                    //Destroy(lokasiTarget.plantStatus);
                    PlantSeed plantSeed = tanaman.gameObject.GetComponent<PlantSeed>();
                    plantSeed.DestroyPlant();
                }
            }
        }
        farmData.hoedTilesList.RemoveAll(item => item.tilePosition == tilePosition);

    }


    public void CheckTile()
    {
        for (int i = 0; i < farmData.hoedTilesList.Count; i++)
        {
            HoedTileData tileData = farmData.hoedTilesList[i];
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
        Debug.Log("memanggil fungsi siram");
        // Pengecekan Keamanan #1: Pastikan data ladang ada.
        if (farmData == null || farmData.hoedTilesList == null)
        {
            Debug.LogWarning("FarmData belum siap, fungsi Siram dibatalkan.");
            return;
        }
        foreach (var hoedlist in farmData.hoedTilesList)
        {
            if (hoedlist.isFarm == true)
            {
                GameObject tanaman = GetPlantAtPosition(hoedlist.tilePosition);
                PlantSeed plantSeed = tanaman.GetComponent<PlantSeed>();
                if (plantSeed.siram)
                {
                    plantSeed.growthTimer++; // Tambahkan satu hari ke growthTimer

                    Debug.Log("tambahkan growt timer");
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

    public void RandomHama()
    {

        foreach (var hoedlist in farmData.hoedTilesList)
        {
            int randomValue = Random.Range(0, 1);
            GameObject tanaman = GetPlantAtPosition(hoedlist.tilePosition);
            PlantSeed plantSeed = tanaman.GetComponent<PlantSeed>();
            if (Random.value < 0.2f && !plantSeed.isReadyToHarvest)
            {
                plantSeed.isInsect = true;
                plantSeed.siram = false;
                plantSeed.ParticleEffect();
                plantSeed.insectTime = 1;
            }

        }
    }




    public void DiSiramHujan()
    {
        foreach (var lokasiTarget in farmData.hoedTilesList)
        {
            // Mendapatkan tile yang ada di posisi tersebut
            TileBase currentTile = tilemap.GetTile(lokasiTarget.tilePosition);

            // Lakukan pengecekan dan penggantian tile jika tile dalam keadaan hoeedTile
            if (currentTile == farmData.hoeedTile)
            {
                // Pastikan plantStatus tidak null sebelum mengakses komponen PlantSeed
                if (lokasiTarget.isFarm == true)
                {
                    GameObject tanaman = GetPlantAtPosition(lokasiTarget.tilePosition);
                    PlantSeed plantSeed = tanaman.GetComponent<PlantSeed>();

                    if (plantSeed != null && !plantSeed.isInsect && !plantSeed.isReadyToHarvest)
                    {
                        Debug.Log("Found an object with SeedManager at this tile position.");

                        // Memanggil metode untuk menyiram tanaman
                        plantSeed.siram = true;
                        plantSeed.ParticleEffect();

                        // Ubah tile ke wateredTile dan tandai tile sebagai sudah disiram
                        tilemap.SetTile(lokasiTarget.tilePosition, farmData.wateredTile);
                        lokasiTarget.watered = true;

                        // Tambah waktu cangkul
                        lokasiTarget.hoedTime += 1;
                        Debug.Log($"Tile at {lokasiTarget.tilePosition} has been changed to wateredTile.");
                    }
                    else
                    {
                        Debug.Log("No SeedManager component found on this object or plant is ready to harvest/infected.");
                    }
                }
                else
                {
                    Debug.Log("plantStatus is null at this tile position.");
                }
            }
            else
            {
                Debug.Log("Cannot water this tile, it's not hoeedTile.");
            }
        }
    }


}