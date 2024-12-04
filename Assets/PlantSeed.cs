using System.Collections;
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
    public GrowthStage currentStage = GrowthStage.Seed; // Tahap awal
    public bool siram = false;
    public bool isReadyToHarvest = false;

    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    public float growthTime; // Waktu sedd tumbuh maksimal
    public float growthSpeed = 0f; // waktu jeda pertumbuhan
    public float growthTimer; // Timer untuk menghitung waktu pertumbuhan
    public string namaSeed;
    public GameObject dropItem;

    public float timeSaatIni;
     private TimeManager timeManager;
     private FarmTile farmTile;



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

        TanamanLayu();


    }

    public void CekFungsi()
    {
         growthTimer++; // Tambahkan satu hari ke growthTimer
        Debug.Log("Fungsi AddOnDan di update");
        Debug.Log("nilai dari growthtime saat fungsi AddOneDay di jalankan" + growthTime);
        Debug.Log("nilai dari growtimer " + growthTimer);
    }
    public void Siram()
    {
        growthTimer++; // Tambahkan satu hari ke growthTimer
        Debug.Log("Fungsi AddOnDan di update");
        Debug.Log("nilai dari growthtime saat fungsi AddOneDay di jalankan" + growthTime);
        Debug.Log("nilai dari growtimer " + growthTimer);

        // Cek apakah growthTimer telah mencapai growthSpeed
        if (growthTimer % growthSpeed == 0)
        {
            Debug.Log("fungsi AdvanceGrowthStage di jalankan");
            AdvanceGrowthStage(); // Maju ke tahap berikutnya
        }

        // Cek apakah growthTimer telah mencapai growthTime
        if (growthTimer >= growthTime)
        {
            currentStage = GrowthStage.ReadyToHarvest; // Set tahap akhir
            Debug.Log("Tanaman siap dipanen!");
            isReadyToHarvest = true;
        }
    }

    private void AdvanceGrowthStage()
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

            // Mainkan efek partikel sebelum menghapus objek
            transform.GetChild(0).GetComponent<ParticleSystem>().Play();

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

    public void TanamanLayu()
    {
        if (farmTile.siram == false && timeManager.date > timeSaatIni + 3)
        {
            Destroy(gameObject, 0.5f);
            Debug.Log("tanaman layu ");
        }else 
        {
            timeSaatIni = timeManager.date;
        }
    }

}
