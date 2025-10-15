using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmTile : MonoBehaviour, ISaveable
{
    public static FarmTile Instance { get; private set; }

    [Header("Komponen & Referensi")]
    public Tilemap tilemap;
    public Transform plantsContainer;
    [SerializeField] private TimeManager timeManager;
    public GameObject plantPrefab;

    //[Header("Data Ladang (Aset)")]
    public FarmData_SO databaseManager;

    public List<HoedTileData> hoedTilesList = new List<HoedTileData>();
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

        if (DatabaseManager.Instance != null)
        {
            databaseManager = DatabaseManager.Instance.farmData_SO; 
        }else
        {
            Debug.LogError("DatabaseManager Instance is null! Please ensure DatabaseManager is initialized before FarmTile.");
        }
        plantPrefab = DatabaseManager.Instance.plantWorldPrefab;


    }

    private void Start()
    {

        //RestoreFarmStateFromData();
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

    public object CaptureState()
    {
        Debug.Log("[SAVE] Menangkap data antrian respawn pohon...");

        // Buat list baru dengan format yang siap disimpan
        var savePlant = new List<HoedTileData>();

        // Konversi setiap item di antrian runtime ke format save
        foreach (var respawnItem in hoedTilesList)
        {
            savePlant.Add(new HoedTileData
            {
                tilePosition = respawnItem.tilePosition,
                hoedTime = respawnItem.hoedTime,
                isPlanted = respawnItem.isPlanted,
                plantID = respawnItem.plantID,
                plantSeedItem = respawnItem.plantSeedItem,
                isInfected = respawnItem.isInfected,
                growthProgress = respawnItem.growthProgress,
                currentStage = respawnItem.currentStage,
                isReadyToHarvest = respawnItem.isReadyToHarvest,
            });
        }

        // Kembalikan SELURUH LIST yang sudah siap disimpan
        return savePlant;
    }

    public void RestoreState(object state)
    {
        HoedTileData data = (HoedTileData)state;
        hoedTilesList.Add(new HoedTileData
        {
            tilePosition = data.tilePosition,
            hoedTime = data.hoedTime,
            isPlanted = data.isPlanted,
            plantID = data.plantID,
            plantSeedItem = data.plantSeedItem,
            growthProgress = data.growthProgress,
            currentStage = data.currentStage,
            isReadyToHarvest = data.isReadyToHarvest,

        });
        Debug.Log("[LOAD] Merestorasi data antrian respawn tanaman..." + hoedTilesList.Count);

    }

    private void HandleNewDay()
    {
        Debug.Log("WeatherManager menerima sinyal hari baru!");

        // Pastikan timeData_SO di TimeManager bersifat public atau memiliki getter.
        bool isRaining = TimeManager.Instance.isRain;
        AdvanceDay(isRaining);
    }


    /// Fungsi utama yang dieksekusi setiap kali hari berganti.
    public void AdvanceDay(bool isRaining)
    {
        // Munculkan Hama Secara Acak (jika tidak hujan)
       TrySpawningPests();


        //Debug.Log("--- Memulai Hari Baru (Tanggal: " + timeManager.date + ")Hujan: " + isRaining);
        if (hoedTilesList == null) return;
       
        // Jika hujan, siram semua tile terlebih dahulu
        if (isRaining)
        {
            Debug.Log("Hujan turun! Menyiram semua tanaman...");
            WaterAllHoedTiles();
        }else
        {
            Debug.Log("Hari cerah, tidak ada penyiraman otomatis.");
        }

        // Looping mundur agar aman saat menghapus item dari list
        for (int i = hoedTilesList.Count - 1; i >= 0; i--)
        {
            HoedTileData tileData = hoedTilesList[i];
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
            if (!tileData.isPlanted && timeManager.date >= tileData.hoedTime + 3)
            {
                RevertTileToSoil(tileData.tilePosition);
            }
        }

  
    }


    public void HoeTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        Vector3Int tileToHoe = tilemap.WorldToCell(playerPosition + faceDirection);
        TileBase currentTile = tilemap.GetTile(tileToHoe);

        if (currentTile == databaseManager.emptySoilTile)
        {
            tilemap.SetTile(tileToHoe, databaseManager.hoeedTile);
            HoedTileData newHoedTile = new HoedTileData();
            newHoedTile.tilePosition = tileToHoe;
            newHoedTile.hoedTime = timeManager.date;
            hoedTilesList.Add(newHoedTile);
        }

        if (TimeManager.Instance.isRain)
        {
            WaterTile(playerPosition, faceDirection);
        }
    }

    public void WaterTile(Vector3 playerPosition, Vector3 faceDirection)
    {
        Vector3Int tileToWater = tilemap.WorldToCell(playerPosition + faceDirection);
        HoedTileData hoedTile = hoedTilesList.Find(t => t.tilePosition == tileToWater);


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
            tilemap.SetTile(tileToWater, databaseManager.wateredTile);
            hoedTile.watered = true;
            // Catat kapan terakhir disiram
            hoedTile.hoedTime = timeManager.date;

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
        if (tileData.watered && timeManager.date + 1 > tileData.hoedTime)
        {
            tileData.watered = false;
            tilemap.SetTile(tileData.tilePosition, databaseManager.hoeedTile);
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
        foreach (var tileData in hoedTilesList)
        {
            if (!tileData.watered && !tileData.isInfected)
            {
                tilemap.SetTile(tileData.tilePosition, databaseManager.wateredTile);
                tileData.watered = true;
                tileData.hoedTime = timeManager.date; // Update waktu siram

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
        if (plant.isReadyToHarvest)
        {
            PlantInteractable plantInteractable = plant.GetComponent<PlantInteractable>();
            plantInteractable.promptMessage = "Panen"+ tileData.plantID;
            return;
        }
        

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


    //berikan tanamanam hama dengan nilai random

    private void TrySpawningPests()
    {
        foreach (var tileData in hoedTilesList)
        {
            if (tileData.isPlanted && Random.value < 0.1f) // 10% kemungkinan
            {
                PlantSeed plant = GetPlantAtPosition(tileData.tilePosition)?.GetComponent<PlantSeed>();
                if (plant != null && !plant.isReadyToHarvest && !plant.isInfected)
                {
                    plant.isInfected = true;
                    tileData.isInfected = true;
                    plant.UpdateParticleEffect();
                    Debug.Log("Hama muncul di tanaman: " + plant.namaSeed);
                }
            }
        }
    }

    private void RevertTileToSoil(Vector3Int tilePosition)
    {
        tilemap.SetTile(tilePosition, databaseManager.emptySoilTile);

        // Hapus juga tanaman yang ada di atasnya
        GameObject plantToDestroy = GetPlantAtPosition(tilePosition);
        if (plantToDestroy != null)
        {
            Destroy(plantToDestroy);
            activePlants.Remove(tilePosition);
        }

        // Hapus data tile dari list
        hoedTilesList.RemoveAll(item => item.tilePosition == tilePosition);
    }

    public GameObject GetPlantAtPosition(Vector3Int tilePosition)
    {
        activePlants.TryGetValue(tilePosition, out GameObject plant);
        return plant;
    }

    

    public void OnPlantHarvested(string plantTargetID)
    {
        // Cari data tile yang sesuai di dalam list
        HoedTileData tileData = hoedTilesList.Find(t => t.plantID == plantTargetID);

        if (tileData != null)
        {
            // Update data: tandai bahwa tile sudah tidak ditanami lagi
            tileData.isPlanted = false;

            // (Opsional tapi direkomendasikan) Reset data tanaman lainnya untuk kebersihan
            tileData.plantID = null;
            tileData.growthProgress = 0;
            tileData.currentStage = GrowthStage.Seed;
            tileData.isReadyToHarvest = false;
            tileData.isInfected = false;
            tileData.plantSeedItem = null;
            Debug.Log($"Tanaman dengan id {plantTargetID} telah dipanen. Tile sekarang bisa ditanami kembali.");
        }

        //Hapus referensi tanaman dari dictionary tanaman aktif
        if (activePlants.ContainsKey(tileData.tilePosition))
        {

            activePlants.Remove(tileData.tilePosition);
        }
    }

    public void RestoreFarmStateList()
    {
        if (hoedTilesList == null || tilemap == null) return;

        foreach (HoedTileData tileData in hoedTilesList)
        {
            Tile targetTile = tileData.watered ? databaseManager.wateredTile : databaseManager.hoeedTile;
            tilemap.SetTile(tileData.tilePosition, targetTile);

            if (tileData.isPlanted && !string.IsNullOrEmpty(tileData.plantID))
            {
                Debug.Log($"[LOAD] Menemukan tanaman yang ditanam: {tileData.plantID} di {tileData.tilePosition}.");
                Item itemTemplate = tileData.plantSeedItem;
                if (itemTemplate != null && plantPrefab != null)
                {
                    Debug.Log($"[LOAD] Merestorasi tanaman {tileData.plantID} di {tileData.tilePosition}.");
                    Vector3 spawnPosition = tilemap.GetCellCenterWorld(tileData.tilePosition);
                    GameObject plantObject = Instantiate(plantPrefab, spawnPosition, Quaternion.identity, MainEnvironmentManager.Instance.plantContainer.transform);
                    plantObject.name = "Tanaman " + itemTemplate.itemDropName; // Memberi nama pada objek tanaman yang diinst
                    PlantInteractable plantInteractable = plantObject.GetComponent<PlantInteractable>();
                    plantInteractable.promptMessage = tileData.plantID;
                    PlantSeed seedComponent = plantObject.GetComponent<PlantSeed>();
                    if (seedComponent != null)
                    {
                        // Setup data tanaman
                        seedComponent.namaSeed = tileData.plantID;
                        seedComponent.dropItem = itemTemplate.itemDropName;
                        seedComponent.growthImages = itemTemplate.growthImages;
                        seedComponent.growthTime = itemTemplate.growthTime;
                        seedComponent.plantLocation = spawnPosition;
                        seedComponent.tilePosition = tileData.tilePosition;
                        seedComponent.isWatered = tileData.watered;
                        seedComponent.isInfected = tileData.isInfected;
                        seedComponent.growthTimer = tileData.growthProgress;
                        seedComponent.currentStage = tileData.currentStage;
                        seedComponent.plantSeedItem = tileData.plantSeedItem;
                        seedComponent.isReadyToHarvest = tileData.isReadyToHarvest;

                        // Panggil Initialize untuk final setup
                        seedComponent.Initialize();
                    }

                    if (tileData.currentStage == GrowthStage.ReadyToHarvest)
                    {
                        plantInteractable.promptMessage = "Panen " + tileData.plantID;
                    }

                    activePlants[tileData.tilePosition] = plantObject;
                }
            }
        }
    }
}