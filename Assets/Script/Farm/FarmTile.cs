using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmTile : MonoBehaviour
{
    public static FarmTile Instance { get; private set; }

    [Header("Komponen & Referensi")]
    public Tilemap tilemap;
    public Transform plantsContainer;
    [SerializeField] private TimeManager timeManager;

    [Header("Data Ladang (Aset)")]
    public FarmData_SO farmData;

    // Dictionary untuk melacak GameObject tanaman aktif berdasarkan posisi tile
    public Dictionary<Vector3Int, GameObject> activePlants = new Dictionary<Vector3Int, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        RestoreFarmStateFromData();
    }

    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
    }


    private void HandleNewDay()
    {
        Debug.Log("WeatherManager menerima sinyal hari baru!");

        // Pastikan timeData_SO di TimeManager bersifat public atau memiliki getter.
        bool isRaining = TimeManager.Instance.timeData_SO.isRain;
        AdvanceDay(isRaining);
    }


    /// Fungsi utama yang dieksekusi setiap kali hari berganti.
    public void AdvanceDay(bool isRaining = false)
    {
        //Debug.Log("--- Memulai Hari Baru (Tanggal: " + timeManager.date + ")Hujan: " + isRaining);
        if (farmData == null || farmData.hoedTilesList == null) return;

        // Jika hujan, siram semua tile terlebih dahulu
        if (isRaining)
        {
            WaterAllHoedTiles();
        }

        // Looping mundur agar aman saat menghapus item dari list
        for (int i = farmData.hoedTilesList.Count - 1; i >= 0; i--)
        {
            HoedTileData tileData = farmData.hoedTilesList[i];
            PlantSeed plant = GetPlantAtPosition(tileData.tilePosition)?.GetComponent<PlantSeed>();
            //plant.tilePosition = tileData.tilePosition;

            //Proses Pertumbuhan Tanaman (jika kemarin disiram)
            if (tileData.isPlanted && plant != null)
            {
                Debug.Log($"[DEBUG] Memproses pertumbuhan tanaman di {tileData.tilePosition} ({plant.namaSeed})");
                ProcessPlantGrowth(tileData, plant);
            }

            // Ini memaksa pemain untuk menyiram lagi setiap hari.
            if (!isRaining)
            {
                DryOutWateredTile(tileData, plant);
            }

            // Reset Tanah Cangkulan yang Terlantar (jika tidak ditanami)
            if (!tileData.isPlanted && timeManager.timeData_SO.date >= tileData.hoedTime + 3)
            {
                RevertTileToSoil(tileData.tilePosition);
            }
        }

        // Munculkan Hama Secara Acak (jika tidak hujan)
        if (!isRaining)
        {
            TrySpawningPests();
        }
    }
  

    public void HoeTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        Vector3Int tileToHoe = tilemap.WorldToCell(playerPosition + faceDirection);
        TileBase currentTile = tilemap.GetTile(tileToHoe);

        if (currentTile == farmData.emptySoilTile)
        {
            tilemap.SetTile(tileToHoe, farmData.hoeedTile);
            HoedTileData newHoedTile = new HoedTileData(tileToHoe, timeManager.timeData_SO.date);
            farmData.hoedTilesList.Add(newHoedTile);
        }

        if (TimeManager.Instance.timeData_SO.isRain)
        {
            WaterTile(playerPosition, faceDirection);
        }
    }

    public void WaterTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        Vector3Int tileToWater = tilemap.WorldToCell(playerPosition + faceDirection);
        HoedTileData hoedTile = farmData.hoedTilesList.Find(t => t.tilePosition == tileToWater);


        if (hoedTile == null)
        {
            Debug.Log($"[DEBUG] Gagal menyiram di {tileToWater}: Tile ini tidak pernah dicangkul.");
            return;
        }
        if (hoedTile.watered)
        {
            Debug.Log($"[DEBUG] Gagal menyiram di {tileToWater}: Tile ini SUDAH dalam keadaan basah.");
            return;
        }


        PlantSeed plant = GetPlantAtPosition(tileToWater)?.GetComponent<PlantSeed>();

        if (plant == null || (!plant.isInfected && !plant.isReadyToHarvest))
        {
            tilemap.SetTile(tileToWater, farmData.wateredTile);
            hoedTile.watered = true;
            // Catat kapan terakhir disiram
            hoedTile.hoedTime = timeManager.timeData_SO.date;

            if (plant != null)
            {
                plant.isWatered = true;
                plant.UpdateParticleEffect();
            }
            Debug.Log($"[DEBUG] Berhasil menyiram tile di {tileToWater}.");
        }
        else
        {
            if (plant != null)
            {
                Debug.Log($"[DEBUG] Gagal menyiram di {tileToWater}: Tanaman terinfeksi (isInfected: {plant.isInfected}) atau siap panen (isReadyToHarvest: {plant.isReadyToHarvest}).");
            }
        }
    }

    private void DryOutWateredTile(HoedTileData tileData, PlantSeed plant)
    {
        // Jika tile basah dan sudah lewat satu hari sejak disiram
        if (tileData.watered && timeManager.timeData_SO.date + 1 > tileData.hoedTime)
        {
            tileData.watered = false;
            tilemap.SetTile(tileData.tilePosition, farmData.hoeedTile);
            Debug.Log($"[DEBUG] Memeriksa tile di {tileData.tilePosition} untuk mengeringkan...");

            if (plant != null)
            {
                Debug.Log($"[DEBUG] Mengeringkan tanaman {plant.namaSeed} di {tileData.tilePosition}.");
                plant.isWatered = false;
                plant.UpdateParticleEffect();
            }
        }
    }

    private void WaterAllHoedTiles()
    {
        Debug.Log("Hujan turun! Menyiram semua tanaman...");
        foreach (var tileData in farmData.hoedTilesList)
        {
            if (!tileData.watered)
            {
                tilemap.SetTile(tileData.tilePosition, farmData.wateredTile);
                tileData.watered = true;
                tileData.hoedTime = timeManager.timeData_SO.date; // Update waktu siram

                PlantSeed plant = GetPlantAtPosition(tileData.tilePosition)?.GetComponent<PlantSeed>();
                if (plant != null && !plant.isInfected && !plant.isReadyToHarvest)
                {
                    plant.isWatered = true;
                    plant.UpdateParticleEffect();
                }
            }
        }
    }

 
    private void ProcessPlantGrowth(HoedTileData tileData, PlantSeed plant)
    {
        // Hanya tumbuh jika disiram
        if (!plant.isWatered) return;

        // Jangan tumbuh lagi jika sudah siap panen
        if (plant.isReadyToHarvest) return;

        tileData.growthProgress++;
        plant.growthTimer = tileData.growthProgress;
        Debug.Log($"[DEBUG] {plant.namaSeed} di {tileData.tilePosition}: Progress tumbuh menjadi {tileData.growthProgress}, Butuh {plant.growthTime} total.");

        // Cek apakah sudah waktunya panen
        if (plant.growthTimer >= plant.growthTime)
        {
            plant.currentStage = GrowthStage.ReadyToHarvest;
            tileData.currentStage = GrowthStage.ReadyToHarvest;
            plant.isReadyToHarvest = true;
            tileData.isReadyToHarvest = true;
            Debug.Log($"[DEBUG] {plant.namaSeed} SIAP PANEN!");
            plant.UpdateSprite(); // Pastikan sprite terakhir (panen) juga di-update
            plant.UpdateParticleEffect();
        }
        // Cek apakah sudah waktunya ganti tahap pertumbuhan
        // Pastikan growthSpeed tidak nol untuk menghindari error pembagian
        else if (plant.growthSpeed > 0 && tileData.growthProgress % plant.growthSpeed == 0)
        {
            plant.AdvanceGrowthStage();
            tileData.currentStage = plant.currentStage; // Update tahap pertumbuhan di data tile
        }
    }



    private void TrySpawningPests()
    {
        foreach (var tileData in farmData.hoedTilesList)
        {
            if (tileData.isPlanted && Random.value < 0.1f) // 10% kemungkinan
            {
                PlantSeed plant = GetPlantAtPosition(tileData.tilePosition)?.GetComponent<PlantSeed>();
                if (plant != null && !plant.isReadyToHarvest && !plant.isInfected)
                {
                    plant.isInfected = true;
                    plant.UpdateParticleEffect();
                    Debug.Log("Hama muncul di tanaman: " + plant.namaSeed);
                }
            }
        }
    }

    private void RevertTileToSoil(Vector3Int tilePosition)
    {
        tilemap.SetTile(tilePosition, farmData.emptySoilTile);

        // Hapus juga tanaman yang ada di atasnya
        GameObject plantToDestroy = GetPlantAtPosition(tilePosition);
        if (plantToDestroy != null)
        {
            Destroy(plantToDestroy);
            activePlants.Remove(tilePosition);
        }

        // Hapus data tile dari list
        farmData.hoedTilesList.RemoveAll(item => item.tilePosition == tilePosition);
    }

    public GameObject GetPlantAtPosition(Vector3Int tilePosition)
    {
        activePlants.TryGetValue(tilePosition, out GameObject plant);
        return plant;
    }

    public void RestoreFarmStateFromData()
    {
        if (farmData == null || tilemap == null) return;

        foreach (HoedTileData tileData in farmData.hoedTilesList)
        {
            Tile targetTile = tileData.watered ? farmData.wateredTile : farmData.hoeedTile;
            tilemap.SetTile(tileData.tilePosition, targetTile);

            if (tileData.isPlanted && !string.IsNullOrEmpty(tileData.plantedItemName))
            {
                Item itemTemplate = ItemPool.Instance.GetItem(tileData.plantedItemName);
                if (itemTemplate != null && itemTemplate.prefabItem != null)
                {
                    Vector3 spawnPosition = tilemap.GetCellCenterWorld(tileData.tilePosition);
                    GameObject plantObject = Instantiate(itemTemplate.prefabItem, spawnPosition, Quaternion.identity, plantsContainer);

                    PlantSeed seedComponent = plantObject.GetComponent<PlantSeed>();
                    if (seedComponent != null)
                    {
                        // Setup data tanaman
                        seedComponent.namaSeed = tileData.plantedItemName;
                        seedComponent.dropItem = itemTemplate.dropItem;
                        seedComponent.growthImages = itemTemplate.growthImages;
                        seedComponent.growthTime = itemTemplate.growthTime;
                        seedComponent.plantLocation = spawnPosition;
                        seedComponent.tilePosition = tileData.tilePosition; 
                        seedComponent.isWatered = tileData.watered;
                        seedComponent.growthTimer = tileData.growthProgress;
                        seedComponent.currentStage = tileData.currentStage;
                        seedComponent.isReadyToHarvest = tileData.isReadyToHarvest;

                        // Panggil Initialize untuk final setup
                        seedComponent.Initialize();
                    }

                    activePlants[tileData.tilePosition] = plantObject;
                }
            }
        }
    }

    public void OnPlantHarvested(Vector3Int tilePosition)
    {
        // 1. Cari data tile yang sesuai di dalam list
        HoedTileData tileData = farmData.hoedTilesList.Find(t => t.tilePosition == tilePosition);

        if (tileData != null)
        {
            // 2. Update data: tandai bahwa tile sudah tidak ditanami lagi
            tileData.isPlanted = false;

            // 3. (Opsional tapi direkomendasikan) Reset data tanaman lainnya untuk kebersihan
            tileData.plantedItemName = null;
            tileData.growthProgress = 0;
            tileData.currentStage = GrowthStage.Seed;
            tileData.isReadyToHarvest = false;

            Debug.Log($"Tanaman di posisi {tilePosition} telah dipanen. Tile sekarang bisa ditanami kembali.");
        }

        // 4. Hapus referensi tanaman dari dictionary tanaman aktif
        if (activePlants.ContainsKey(tilePosition))
        {
            activePlants.Remove(tilePosition);
        }
    }

    //hanya contoh bisa di gunakan nanti jika panjul berubah pikiran
    void SaveFarmData()
    {
        foreach (var tileData in farmData.hoedTilesList)
        {
            if (tileData.isPlanted)
            {
                GameObject plantObject = GetPlantAtPosition(tileData.tilePosition);
                if (plantObject != null)
                {
                    PlantSeed seed = plantObject.GetComponent<PlantSeed>();
                    // Simpan state terakhir tanaman ke dalam data
                    tileData.currentStage = seed.currentStage;
                    tileData.isReadyToHarvest = seed.isReadyToHarvest;
                }
            }
        }
    }
}