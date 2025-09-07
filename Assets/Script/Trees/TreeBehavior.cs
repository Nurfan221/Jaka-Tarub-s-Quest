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
    public ItemData kayu;
    public ItemData daun;
    public ItemData getah;
    public ItemData benih;

    [Header("Logika Tanam Pohon")]
    public GrowthTree currentStage = GrowthTree.Seed;
    public GameObject[] growthObject; // Gambar untuk tiap tahap pertumbuhan
    public float growthTime; // Waktu total untuk mencapai tahap akhir
    public string nameEnvironment;
    public GameObject tumbangSprite;
    public GameObject akarPohonPrefab;
    public bool isRubuh;

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

    

    [Header("Keperluan")]
    public Transform plantsContainer;
    


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();



    }






    public void PertumbuhanPohon()
    {
        daysSincePlanting++;  // Meningkatkan hari
        Debug.Log($"Pertumbuhan pohon dipanggil. Hari ke-{daysSincePlanting} sejak penanaman.");



        // Cek apakah daysSincePlanting telah mencapai growthSpeed
        if (daysSincePlanting % growthSpeed == 0)
        {
            //Debug.Log("fungsi AdvanceGrowthStage di jalankan");
            AdvanceGrowthStage(); // Maju ke tahap berikutnya
        }

        if (daysSincePlanting >= growthTime)
        {
            currentStage = GrowthTree.MaturePlant;
            //Debug.Log("Pohon siap dipanen!");
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
        PlantContainer plantContainerScript = plantsContainer.GetComponent<PlantContainer>();
        int stageIndex = (int)currentStage;
        if (stageIndex >= 0 && stageIndex < growthObject.Length)
        {
            Vector2 posisiPohon = transform.position;

            GameObject objectPohon = growthObject[stageIndex];
            GameObject pohonBaru = Instantiate(objectPohon, posisiPohon, Quaternion.identity);
            TreeBehavior treeBehavior = pohonBaru.GetComponent<TreeBehavior>();

            //masukan nilai yang di butukan ke pohon baru 
            treeBehavior.plantsContainer = plantsContainer;
            treeBehavior.growthSpeed = growthSpeed;
            treeBehavior.growthObject = growthObject;
            pohonBaru.transform.SetParent(plantsContainer);

            // Cari dan ganti objek ini di dalam list plantObject
            
            for (int i = 0; i < plantContainerScript.plantObject.Count; i++)
            {
                if (plantContainerScript.plantObject[i] == this.gameObject)
                {
                    plantContainerScript.plantObject[i] = pohonBaru;
                    break;
                }
            }

            Destroy(gameObject);

            Debug.Log("Sprite diperbarui ke tahap: " + currentStage);
        }
        else
        {
            Debug.LogError($"Gambar untuk tahap {currentStage} tidak ditemukan!");
        }
    }


    public void TakeDamage(int damage)
    {
        if (!isRubuh)
        {
            health -= Mathf.Min(damage, health);
            Debug.Log($"Pohon terkena damage. Sisa HP: {health}");

            if (health <= 0)
            {
                StartCoroutine(FellTree());
            }
        }
    }

    private IEnumerator TreeFallAnimation(Transform tumbangTransform, GameObject pohonAsli)
    {
        Debug.Log("Animasi pohon dijalankan");

        int duration = 5;
        int elapsed = 0;

        float startZ = 0f;
        float tahap2Z = -20f;
        float tahap3Z = -45f;
        float tahap4Z = -60f;
        float endZ = -90f;

        while (elapsed < duration)
        {
            switch (elapsed)
            {
                case 0:
                    Debug.Log("Tahap 1");
                    tumbangTransform.rotation = Quaternion.Euler(0, 0, startZ);
                    break;
                case 1:
                    Debug.Log("Tahap 2");
                    tumbangTransform.rotation = Quaternion.Euler(0, 0, tahap2Z);
                    break;
                case 2:
                    Debug.Log("Tahap 3");
                    tumbangTransform.rotation = Quaternion.Euler(0, 0, tahap3Z);
                    break;
                case 3:
                    Debug.Log("Tahap 4");
                    tumbangTransform.rotation = Quaternion.Euler(0, 0, tahap4Z);
                    break;
                case 4:
                    Debug.Log("Tahap 5");
                    tumbangTransform.rotation = Quaternion.Euler(0, 0, endZ);
                    break;
            }

            elapsed += 1;
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Rotasi selesai");

        // Setelah animasi selesai, baru hancurkan pohon lama
        Destroy(pohonAsli);
    }




    private IEnumerator FellTree()
    {
        isRubuh = true;
        Vector3 posisiPohon = transform.position;

        //Spawn akar di posisi pohon
        if (akarPohonPrefab != null)
        {
            GameObject akar = Instantiate(akarPohonPrefab, posisiPohon, Quaternion.identity, plantsContainer);
            //GameObject akar = Instantiate(batangPohon, posisiPohon, Quaternion.identity);
            SpriteRenderer akarSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            akarSpriteRenderer.sprite = null;
        }

        //Spawn batang tumbang (di atas akar)
        if (tumbangSprite != null)
        {
            GameObject batang = Instantiate(tumbangSprite, posisiPohon, Quaternion.identity);
            batang.transform.SetParent(gameObject.transform); 
            yield return StartCoroutine(RotateFalling(batang.transform));
        }

        EnvironmentManager environmentManager = plantsContainer.gameObject.GetComponent<EnvironmentManager>();

        if (environmentManager != null)
        {
            foreach (var cekLokasiObjek in environmentManager.environmentList)
            {
                if (gameObject.transform.position == cekLokasiObjek.objectPosition && nameEnvironment == cekLokasiObjek.prefabName)
                {
                    cekLokasiObjek.isGrowing = false;
                }
            }
        }
        //Hancurkan pohon asli
        Destroy(gameObject);

        //(Opsional) Drop item kayu, getah, dll.
        DropResources(posisiPohon);
    }

    private IEnumerator RotateFalling(Transform target)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        float startZ = 0f;
        float endZ = -90f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float z = Mathf.Lerp(startZ, endZ, elapsed / duration);
            target.rotation = Quaternion.Euler(0, 0, z);
            yield return null;
        }

        target.rotation = Quaternion.Euler(0, 0, endZ);
    }

    private void DropResources(Vector3 posisi)
    {
        // Hitung jumlah random untuk masing-masing item
        int woodCount = Random.Range(minWood, maxWood + 1);
        int sapCount = Random.Range(minSap, maxSap + 1);
        int leafCount = Random.Range(minLeaf, maxLeaf + 1);
        int seedCount = Random.Range(minLeaf, maxLeaf + 1);

        Debug.Log($"Menjatuhkan item: Kayu={woodCount}, Getah={sapCount}, Daun={leafCount}, Seed={seedCount}");

        // Drop kayu
        for (int i = 0; i < woodCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            if (kayu != null)
            {
                ItemPool.Instance.DropItem(kayu.itemName,kayu.itemHealth, kayu.quality, posisi + offset   );
            }
        }

        // Drop getah
        for (int i = 0; i < sapCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            if (getah != null)
            {
                ItemPool.Instance.DropItem(getah.itemName, getah.itemHealth, getah.quality, posisi + offset);
            }
        }

        // Drop daun
        for (int i = 0; i < leafCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            if (daun != null)
            {
                ItemPool.Instance.DropItem(daun.itemName, daun.itemHealth, daun.quality, posisi + offset);
            }
        }

        // Drop benih
        for (int i = 0; i < seedCount; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            if (benih != null)
            {
                ItemPool.Instance.DropItem(benih.itemName, benih.itemHealth, benih.quality, posisi + offset);
            }
        }

        isRubuh = false;
    }




}
