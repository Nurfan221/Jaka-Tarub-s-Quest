using UnityEngine;

public enum GrowthStage
{
    none, // Tahap ini tidak digunakan, hanya untuk keamanan
    Seed,
    Sprout,
    YoungPlant,
    MaturePlant,
    ReadyToHarvest
}

public class PlantSeed : MonoBehaviour
{
    [Header("Particle Effects")]
    public ParticleSystem waterEffect; // Efek "perlu disiram" atau "siap panen"
    public ParticleSystem growEffect;  // Efek saat tumbuh setelah disiram
    public ParticleSystem insectEffect; // Efek saat terserang hama

    [Header("Plant State")]
    public GrowthStage currentStage = GrowthStage.Seed;
    public bool isWatered = false;
    public bool isReadyToHarvest = false;
    public bool isInfected = false;

    [Header("Plant Attributes")]
    public Sprite[] growthImages;
    public float growthTime; // Total hari untuk tumbuh maksimal
    public float growthSpeed; // Jeda hari antar tahap pertumbuhan
    public string namaSeed;
    public GameObject dropItem;

    // Data Internal
    public float growthTimer = 0; // Menghitung progres pertumbuhan (dalam hari)
    public int insectTime = 0; // Menghitung berapa lama tanaman terinfeksi

    public Vector3 plantLocation;

   
    private void Start()
    {
        // Cari GameObject anak berdasarkan NAMA, lalu ambil komponen ParticleSystem-nya.
        // Pastikan nama "WaterParticle", "GrowParticle", dan "InsectParticle" 
        // sama persis dengan nama GameObject anak di dalam prefab PlantSeed.

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

        // Hitung waktu jeda pertumbuhan jika belum di-set
        if (growthSpeed <= 0 && growthImages.Length > 0)
        {
            growthSpeed = growthTime / growthImages.Length;
        }
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


    public void UpdateSprite()
    {
        int stageIndex = (int)currentStage;
        if (stageIndex >= 0 && stageIndex < growthImages.Length)
        {
            // DEBUGGING LOG
            Debug.Log($"[DEBUG] UpdateSprite: Mengganti sprite ke growthImages[{stageIndex}]. Nama Sprite: {growthImages[stageIndex].name}");
            // AKHIR DEBUGGING
            GetComponent<SpriteRenderer>().sprite = growthImages[stageIndex];
        }
        else
        {
            // DEBUGGING LOG
            Debug.LogError($"[DEBUG] UpdateSprite GAGAL: Index {stageIndex} di luar jangkauan array growthImages (panjang: {growthImages.Length}).");
            // AKHIR DEBUGGING
        }
    }

  
    public void UpdateParticleEffect()
    {
        // Pastikan semua particle system ada
        if (growEffect == null || waterEffect == null || insectEffect == null) return;

        // Nonaktifkan semua dulu untuk kebersihan
        growEffect.Stop();
        waterEffect.Stop();
        insectEffect.Stop();

        if (isReadyToHarvest)
        {
            // Disarankan menggunakan efek berbeda untuk siap panen (misal: berkilau)
            // Untuk sementara, kita gunakan waterEffect sebagai tanda.
            //waterEffect.Play();
        }
        
        else if (isInfected)
        {
            insectEffect.Play();
        }
        else if (isWatered)
        {
            growEffect.Play();
        }
        else // Kering dan belum siap panen
        {
            // Menampilkan efek "butuh air"
            waterEffect.Play();
        }
    }

    /// <summary>
    /// Logika harian untuk tanaman yang terinfeksi. Jika lebih dari 2 hari, tanaman mati.
    /// </summary>
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
        if (isReadyToHarvest)
        {
            ItemPool.Instance.DropItem(dropItem.name, transform.position + new Vector3(0, 0.5f, 0), dropItem);
            // Penghancuran akan di-handle oleh FarmTile setelah tile di-reset
            Destroy(gameObject);
        }
    }

    public void CureInfection()
    {
        isInfected = false;
        insectTime = 0;
        UpdateParticleEffect();
    }
}