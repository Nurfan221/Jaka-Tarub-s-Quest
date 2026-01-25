using UnityEngine;

public enum GrowthStage
{
    Seed,
    YoungPlant,
    MaturePlant,
    ReadyToHarvest
}

public class PlantSeed : UniqueIdentifiableObject
{
    [Header("Particle Effects")]
    public ParticleSystem waterEffect; // Efek "perlu disiram" atau "siap panen"
    public ParticleSystem harvestParticle;
    public ParticleSystem growEffect;  // Efek saat tumbuh setelah disiram
    public ParticleSystem insectEffect; // Efek saat terserang hama

    [Header("Plant State")]
    public GrowthStage currentStage = GrowthStage.Seed;
    public bool isWatered = false;
    public bool isReadyToHarvest = false;
    public bool isInfected = false;
    public bool hasFertilizer = false;
    public bool isWithered = false; // Dicek dari timer telat panen
    public bool isRegrow = false;

    [Header("Plant Attributes")]
    public Sprite[] growthImages;
    public float growthTime; // Total hari untuk tumbuh maksimal
    public float growthSpeed; // Jeda hari antar tahap pertumbuhan
    public string namaSeed;
    public SeedType seedType;
    public ItemRarity rarity;
    public EnvironmentHardnessLevel hardnessLevel;
    public string dropItem;
    public Item plantSeedItem; // Referensi ke item benih

    // Data Internal
    public SpriteRenderer spriteRenderer; // Referensi ke komponen SpriteRenderer
    public float growthTimer = 0; // Menghitung progres pertumbuhan (dalam hari)
    public float reGrowTimer = 0; // menghitung progres pertumbuhan setelah dipanen
    public int insectTime = 0; // Menghitung berapa lama tanaman terinfeksi

    public Vector3 plantLocation;
    public Vector3Int tilePosition;


    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return seedType.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return hardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        return seedType.ToString();

    }

    public override string GetVariantName()
    {
        return currentStage.ToString();

    }

    #endregion

    private void Awake()
    {
       
    }
    private void Start()
    {
      

        Transform waterTransform = transform.Find("WaterParticle");
        if (waterTransform != null)
        {
            waterEffect = waterTransform.GetComponent<ParticleSystem>();
        }

        Transform growTransform = transform.Find("GrowingParticle");
        if (growTransform != null)
        {
            growEffect = growTransform.GetComponent<ParticleSystem>();
        }

        Transform insectTransform = transform.Find("InsectParticle");
        if (insectTransform != null)
        {
            insectEffect = insectTransform.GetComponent<ParticleSystem>();
        }

        Transform harvestTransform = transform.Find("HarvestParticle");
        if (harvestTransform != null)
        {
            harvestParticle = harvestTransform.GetComponent<ParticleSystem>();
        }

        Transform visualChild = transform.Find("Visual");

        if (visualChild != null)
        {
            // Ambil komponen dari anak tersebut
            spriteRenderer = visualChild.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Gawat! Tidak ada anak bernama 'Visual' di objek ini!");
        }
        // Hitung waktu jeda pertumbuhan jika belum di-set
        if (growthSpeed <= 0 && growthImages.Length > 0)
        {
            float speedGrowth = growthTime / growthImages.Length;
            growthSpeed = Mathf.Max(1f, speedGrowth);
        }
               
        UpdateParticleEffect();
    }
    public void Initialize()
    {
        //timeSaatIni = timeManager.date; // Jika masih diperlukan



        UpdateParticleEffect();
        UpdateSprite();
    }

    public void AdvanceGrowthStage()
    {
        if (currentStage < GrowthStage.ReadyToHarvest)
        {
            currentStage++;
            Debug.Log($"[DEBUG] {namaSeed} di {plantLocation} BERTUMBUH ke tahap: {currentStage}. Memanggil UpdateSprite().");
            UpdateSprite(); // Panggilan ini sudah benar
        }

   
    }


    public void UpdateSprite(int spriteIndex)
    {
        // Validasi Index
        if (spriteIndex >= 0 && spriteIndex < growthImages.Length) // Pakai '<' bukan '<=' karena array mulai dari 0
        {
            // Debug.Log($"[DEBUG] Mengganti sprite ke index: {spriteIndex}");

            Transform visualChild = transform.Find("Visual");

            if (visualChild != null)
            {
                spriteRenderer = visualChild.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = growthImages[spriteIndex];
                }
            }
            else
            {
                Debug.LogError("Gawat! Tidak ada anak bernama 'Visual'!");
            }
        }
        else
        {
            Debug.LogError($"[DEBUG] UpdateSprite GAGAL: Index {spriteIndex} di luar batas array images.");
        }
    }

    public void UpdateSprite()
    {
        // Kita lempar nilai enum yang dikonversi ke int ke fungsi utama di atas
        UpdateSprite((int)currentStage);
    }


    public void UpdateParticleEffect()
    {
        // Safety Check
        if (growEffect == null || waterEffect == null || insectEffect == null || harvestParticle == null) return;

        growEffect.Stop();
        waterEffect.Stop();
        insectEffect.Stop();
        harvestParticle.Stop(); 

        if (isReadyToHarvest)
        {
            harvestParticle.Play();
        }
        else if (isInfected)
        {
            insectEffect.Play();
        }
        else if (isWatered)
        {
            growEffect.Play();
        }
        else // Kering
        {
            waterEffect.Play();
        }
    }
    // Logika harian untuk tanaman yang terinfeksi. Jika lebih dari 2 hari, tanaman mati.

    public void UpdateInfection()
    {
        if (!isInfected) return;

        insectTime++;
        if (insectTime > 2)
        {
            // FarmTile akan menghandle penghancuran object
            Debug.Log("Tanaman " + namaSeed + " mati karena infeksi.");
        }
    }

    public void Harvest()
    {
        Item itemDrop = ItemPool.Instance.GetItem(dropItem);
        if (itemDrop == null)
        {
            Debug.LogError($"Harvest Gagal: Item '{dropItem}' tidak ditemukan di ItemPool!");
            return;
        }

        if (isReadyToHarvest)
        {
            Debug.Log($"Tanaman {plantSeedItem.itemName} dipanen!");

            // Jika min=1, max=3 -> Range(1, 4) -> Hasil: 1, 2, atau 3.
            int itemToDrop = GetWeightedDropCount(plantSeedItem.minDropHarvest, plantSeedItem.maxDropHarvest);
            // Drop Item
            ItemPool.Instance.DropItem(itemDrop.itemName, itemDrop.health, itemDrop.quality, transform.position + new Vector3(0, 0.5f, 0), itemToDrop);

            if (plantSeedItem.canRegrow)
            {
                // Fungsi ini akan mereset visual dan memundurkan timer secara otomatis.
                PlantReGrow();

                // Biarkan PlantReGrow yang mengatur timer.
            }
            else
            {
                // Lapor dulu ke Tile bahwa tanaman ini hilang (kosongkan data tile)
                FarmTile.Instance.OnPlantHarvested(this.UniqueID);

                // Baru hancurkan objek visualnya
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Tanaman belum siap dipanen bos.");
        }
    }


    public void PlantReGrow()
    {
        if (!isReadyToHarvest) return;

        Debug.Log("Memulai proses Regrow...");

        isReadyToHarvest = false;

        // Jika hujan, otomatis ter-siram untuk besok. Jika tidak, kering.
        isWatered = TimeManager.Instance.isRain;


        // Rumus Aman: Timer = Total Waktu Tumbuh - Waktu Regrow
        float timeToRewind = plantSeedItem.regrowTime;


        growthTimer = plantSeedItem.growthTime - timeToRewind;

        // Safety clamp
        growthTimer = Mathf.Max(0f, growthTimer);


        float originalTotalTime = plantSeedItem.growthTime;
        int totalImages = growthImages.Length;

        float currentProgressPercent = growthTimer / originalTotalTime;
        int targetSpriteIndex = Mathf.FloorToInt(currentProgressPercent * totalImages);

        // Safety: Pastikan index minimal 1 (Tanaman muda), jangan 0 (Biji)
        targetSpriteIndex = Mathf.Clamp(targetSpriteIndex, 1, totalImages - 1);

        UpdateSprite(targetSpriteIndex);

        if (targetSpriteIndex >= totalImages - 1)
        {
            currentStage = GrowthStage.ReadyToHarvest;
        }
        else if (targetSpriteIndex > 1)
        {
            currentStage = GrowthStage.MaturePlant;
        }
        else
        {
            currentStage = GrowthStage.YoungPlant;
        }

        UpdateParticleEffect();

        // Gunakan ini HANYA jika Anda ingin membatasi berapa kali tanaman bisa panen.
        if (!isRegrow)
        {
            isRegrow = true;
            reGrowTimer = 0; // Inisialisasi counter panen
        }
        reGrowTimer++; // Tambah 1 kali panen


        FarmTile.Instance.OnRegrowHarvested(this);

        Debug.Log($"Regrow Selesai. Timer: {growthTimer}. Stage: {currentStage}. Panen ke-{reGrowTimer}");
    }

    public int GetWeightedDropCount(int min, int max)
    {
        if (min >= max) return min;

        // Random.Range(0, 100) akan menghasilkan angka 0 s/d 99.
        int roll = Random.Range(0, 100);


        // Mendapatkan nilai MINIMUM
        if (roll < 70)
        {
            return min;
        }

     
        else if (roll < 90 && (max - min) >= 2)
        {
           
            return Random.Range(min + 1, max);
        }

       
        else
        {
            return max;
        }
    }
    public void CureInfection()
    {
        isInfected = false;
        insectTime = 0;
        UpdateParticleEffect();
    }
}