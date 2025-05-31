using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum GrowthStage
{
    Seed,
    Sprout,
    YoungPlant,
    MaturePlant,
    ReadyToHarvest
}
public class PlantSeed : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    public ParticleSystem water;
    public ParticleSystem grow;
    public ParticleSystem insect;

    public GrowthStage currentStage = GrowthStage.Seed; // Tahap awal
    public bool siram = false;
    public bool isReadyToHarvest = false;
    public bool isInsect = false;

    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    public float growthTime; // Waktu sedd tumbuh maksimal
    public float growthSpeed = 0f; // waktu jeda pertumbuhan
    public int insectTime = 0;
    public float growthTimer; // Timer untuk menghitung waktu pertumbuhan
    public string namaSeed;
    public GameObject dropItem;

    public float timeSaatIni;
     private TimeManager timeManager;
     private FarmTile farmTile;
    public Vector3 plantLocation;



     private void Start()
    {
        // Misalnya mencari komponen timeManager dari objek Tilemap
        timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
        farmTile = GameObject.Find("Tilemap").GetComponent<FarmTile>();

        timeSaatIni = timeManager.date;
        
        if (currentStage == GrowthStage.Seed)
        {
            growthTimer = 0;
        }
        // Hitung growthSpeed berdasarkan panjang growthImages
        growthSpeed = growthTime / growthImages.Length; // Menghitung waktu jeda pertumbuhan

        Debug.Log("nilai growthTime saat Start : " + growthTime);

        ParticleEffect();
    }

    private void Update()
    {
        // Logika untuk menambahkan growthTimer setiap hari
        // Misalnya, jika Anda memiliki mekanisme untuk melacak hari, panggil fungsi ini
        // Contoh: Setiap kali satu hari berlalu, panggil fungsi UpdateGrowth()
        // Di sini kita hanya akan panggil fungsi ini langsung untuk contoh
        // if (Input.GetKeyDown(KeyCode.Space)) // Contoh: tekan Spasi untuk menambahkan satu hari
        // {
        //     AddOneDay(); // Tambah satu hari dan perbarui pertumbuhan
        // }



  




    }


    

    public void AdvanceGrowthStage()
    {
        // Tingkatkan tahap pertumbuhan ke tahap berikutnya
        if (currentStage < GrowthStage.ReadyToHarvest) // Pastikan tidak melewati tahap terakhir
        {
            currentStage++; // Maju ke tahap berikutnya
            UpdateSprite(); // Ganti sprite sesuai dengan tahap pertumbuhan saat ini

            // Log untuk debugging
            Debug.Log("Tahap pertumbuhan tanaman: " + currentStage);
        }
    }

    private void UpdateSprite()
    {
        int stageIndex = (int)currentStage; // Dapatkan indeks dari enum
        if (stageIndex >= 0 && stageIndex < growthImages.Length)
        {
            GetComponent<SpriteRenderer>().sprite = growthImages[stageIndex]; // Ganti sprite
        }
    }

    

   public void Harvest()
    {
        if (isReadyToHarvest)
        {
            Debug.Log("Biji dipanen!");
            
            // Men-drop item menggunakan ItemPool
            ItemPool.Instance.DropItem(dropItem.name, transform.position + new Vector3(0, 0.5f, 0), dropItem);
            isReadyToHarvest = false;

            // Hapus objek setelah efek partikel
            Destroy(gameObject, 0.5f); // Menghapus objek setelah 0.5 detik untuk memberi waktu partikel bermain
        }
        else if (siram)
        {
            Debug.Log("Siram dulu bos");
        }
        else
        {
            Debug.Log("Biji belum siap dipanen bos");
        }
    }


    public void ParticleEffect()
    {
        if (grow == null || water == null || insect == null)
        {
            Debug.LogWarning("Salah satu Particle System belum di-assign!");
            return;
        }

        // Aktifkan semua GameObject partikel
        grow.gameObject.SetActive(true);
        water.gameObject.SetActive(true);
        insect.gameObject.SetActive(true);

        if (isInsect)
        {
            grow.Stop();
            water.Stop();
            insect.Play();
        }
        else if (siram)
        {
            grow.Play();
            water.Stop();
            insect.Stop();
            Debug.Log("Menjalankan animasi pertumbuhan");
        }
        else if(isReadyToHarvest)
        {
            grow.Stop();
            water.Play();
            insect.Stop();
        }    
        else
        {
            grow.Stop();
            water.Play();
            insect.Stop();
        }
    }

    public void PlantsTerinfeksi()
    {
        if (isInsect &&insectTime > 2)
        {
            Destroy(gameObject);
        }
        else
        {
            insectTime++;
        }
    }

    public void DestroyPlant()
    {
        Destroy(gameObject);
    }
}
