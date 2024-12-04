using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrowthTree
{
    Seed,
    Sprout,
    YoungPlant,
    MaturePlant
}

public class TreeBehavior : MonoBehaviour
{
    [Header("Statistik Pohon")]
    public float health = 100f;
    public GameObject kayu;
    public GameObject daun;
    public GameObject getah;
    public GameObject benih;

    [Header("Logika Tanam Pohon")]
    public GrowthTree currentStage = GrowthTree.Seed;
    public Sprite[] growthImages; // Gambar untuk tiap tahap pertumbuhan
    public float growthTime; // Waktu total untuk mencapai tahap akhir
    public string namaSeed;

    public float growthSpeed; // Waktu jeda antar tahap pertumbuhan
    public int daysSincePlanting = 1; // Hari sejak pohon ditanam
    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer untuk mengganti gambar

    //logika menentukan jumlah minimal dan maksimal dari item yang akan di jatuhkan 
    public int minWood = 1;  // Jumlah minimum kayu
    public int maxWood = 5;  // Jumlah maksimum kayu
    public int minSap = 0;   // Jumlah minimum getah
    public int maxSap = 3;   // Jumlah maksimum getah
    public int minLeaf = 2;  // Jumlah minimum daun
    public int maxLeaf = 7;  // Jumlah maksimum daun

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Berlangganan ke event OnDayChanged di TimeManager
        TimeManager.OnDayChanged += OnDayChanged;

        growthSpeed = growthTime / growthImages.Length; // Hitung waktu jeda antar tahap
    }

    private void OnDestroy()
    {
        // Unsubscribe dari event OnDayChanged untuk mencegah error saat pohon dihancurkan
        TimeManager.OnDayChanged -= OnDayChanged;
    }

    private void OnDayChanged(int currentDay)
    {
        Debug.Log($"Pohon menerima perubahan hari, Hari ke-{currentDay}");
        PertumbuhanPohon();
        Debug.Log("pohon tumbuh");
    }


    public void PertumbuhanPohon()
    {
        daysSincePlanting++;  // Meningkatkan hari
        Debug.Log($"Pertumbuhan pohon dipanggil. Hari ke-{daysSincePlanting} sejak penanaman.");



        // Cek apakah daysSincePlanting telah mencapai growthSpeed
        if (daysSincePlanting % growthSpeed == 0)
        {
            Debug.Log("fungsi AdvanceGrowthStage di jalankan");
            AdvanceGrowthStage(); // Maju ke tahap berikutnya
        }

        if (daysSincePlanting >= growthTime)
        {
            currentStage = GrowthTree.MaturePlant;
            Debug.Log("Pohon siap dipanen!");
        }
    }




    private void AdvanceGrowthStage()
    {
        // Tingkatkan tahap pertumbuhan ke tahap berikutnya jika belum mencapai akhir
        if (currentStage < GrowthTree.MaturePlant)
        {
            currentStage++;
            UpdateSprite();
            Debug.Log("Tahap pertumbuhan: " + currentStage);
        }
    }

    private void UpdateSprite()
    {
        int stageIndex = (int)currentStage;
        if (stageIndex >= 0 && stageIndex < growthImages.Length)
        {
            spriteRenderer.sprite = growthImages[stageIndex];
            Debug.Log("Sprite diperbarui ke tahap: " + currentStage);
        }
        else
        {
            Debug.LogError($"Gambar untuk tahap {currentStage} tidak ditemukan!");
        }
    }

    public void TakeDamage(int damage)
    {
        health -= Mathf.Min(damage, health);
        Debug.Log($"Pohon terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            DestroyTree();
        }
    }

    private void DestroyTree()
    {
        Debug.Log("Pohon dihancurkan!");

        // Hitung jumlah acak untuk setiap jenis item
        int woodCount = (int)Random.Range(minWood, maxWood + 1);
        int sapCount = (int)Random.Range(minSap, maxSap + 1);
        int leafCount = (int)Random.Range(minLeaf, maxLeaf + 1);

        Debug.Log($"Jumlah kayu: {woodCount}, getah: {sapCount}, daun: {leafCount}");

        // Drop kayu
        for (int i = 0; i < woodCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            if (kayu != null)
                ItemPool.Instance.DropItem(kayu.name, transform.position + offset, kayu);
        }

        // Drop getah
        for (int i = 0; i < sapCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            if (getah != null)
                ItemPool.Instance.DropItem(getah.name, transform.position + offset, getah);
        }

        // Drop daun
        for (int i = 0; i < leafCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            if (daun != null)
                ItemPool.Instance.DropItem(daun.name, transform.position + offset, daun);
        }

        // Hancurkan pohon setelah menjatuhkan item
        Destroy(gameObject);
    }
}
