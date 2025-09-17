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

public class TreeBehavior : UniqueIdentifiableObject
{


    //  Implementasi dari Kontrak IUniqueIdentifiable 
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;




    [Header("Statistik Pohon")]
    public float health = 100f;
    public ItemData kayu;
    public ItemData daun;
    public ItemData getah;
    public ItemData benih;
    

    [Header("Logika Tanam Pohon")]
    public ParticleSystem plantEffectPrefab; // Efek partikel saat menanam
    public GrowthTree currentStage = GrowthTree.Seed;
    public TypePlant typePlant; 
    public GameObject growthObject; // Gambar untuk tiap tahap pertumbuhan
    public float growthTime; // Waktu total untuk mencapai tahap akhir
    public string nameEnvironment;
    public string namaPohon;
    public bool isRubuh;
    public GameObject tumbangSprite; // Gambar batang pohon yang tumbang
    public GameObject akarPohonPrefab; // Prefab akar pohon yang muncul setelah pohon tumbang

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

    



    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return typeObject.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return hardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        return typePlant.ToString();
    }

    public override string GetVariantName()
    {
        return currentStage.ToString();
    }

    #endregion

      
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        //OnTreeChoppedDown();
        plantEffectPrefab= gameObject.GetComponentInChildren<ParticleSystem>();

    }

    // Fungsi untuk mengemas data pohon ini
    //public object CaptureState()
    //{
    //    // Buat sebuah objek data save baru, isi dengan nilai saat ini, lalu kembalikan.
    //    return new TreePlacementData
    //    {
    //        TreeID = this.uniqueID,
    //        position = transform.position,
    //        initialStage = this.currentStage,
    //        sudahTumbang = this.isRubuh,
    //    };
    //}

    //// FUNGSI UNTUK MEMUAT DATA (MEMBACA FORMULIR)
    //public void RestoreState(object state)
    //{
    //    // Terima data 'state', ubah kembali ke tipe data yang benar
    //    TreePlacementData savedData = (TreePlacementData)state;
    //    // Terapkan nilai dari data yang di-load ke komponen ini
    //    this.uniqueID = savedData.TreeID;
    //    transform.position = savedData.position;
    //    this.currentStage = savedData.initialStage;
    //    this.isRubuh = savedData.sudahTumbang;
    //    // PENTING: Setelah me-restore state, Anda mungkin perlu memperbarui visual pohon
    //    // agar cocok dengan `currentStage` yang baru.
    //    // UpdateVisuals(); 
    //}






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

            UpdateSprite();
            Debug.Log("Tahap pertumbuhan: " + currentStage);
        }
    }

    private void UpdateSprite()
    {
        // Cek apakah sudah tahap terakhir
        if (currentStage >= GrowthTree.MaturePlant) return;

        GameObject nextStagePrefab = DatabaseManager.Instance.GetNextStagePrefab(this.namaPohon, this.currentStage);

        if (nextStagePrefab != null)
        {
            Vector2 posisiPohon = transform.position;

            // --- PERBAIKAN KUNCI 1: Simpan ID lama sebelum hancur ---
            string oldUniqueID = this.UniqueID;

            GameObject pohonBaru = Instantiate(nextStagePrefab, posisiPohon, Quaternion.identity);
            TreeBehavior treeBehaviorBaru = pohonBaru.GetComponent<TreeBehavior>();

            treeBehaviorBaru.namaPohon = this.namaPohon;
            treeBehaviorBaru.nameEnvironment = this.nameEnvironment;
            treeBehaviorBaru.growthSpeed = this.growthSpeed;
            treeBehaviorBaru.daysSincePlanting = this.daysSincePlanting;
            treeBehaviorBaru.currentStage = (GrowthTree)((int)this.currentStage + 1);

            // --- PERBAIKAN KUNCI 2: Wariskan ID lama ke pohon baru ---
            treeBehaviorBaru.UniqueID = oldUniqueID;
            // Ganti juga nama GameObject agar konsisten di Hierarchy
            pohonBaru.name = oldUniqueID;

            pohonBaru.transform.SetParent(MainEnvironmentManager.Instance.pohonManager.transform);

            UpdateReferencesInManagers(this.gameObject, pohonBaru);
            MainEnvironmentManager.Instance.pohonManager.TumbuhkanPohonDalamAntrian(treeBehaviorBaru);

            Destroy(gameObject);
            Debug.Log($"Pohon '{this.namaPohon}' (ID: {oldUniqueID}) tumbuh ke tahap: {treeBehaviorBaru.currentStage}");
        }
        else
        {
            Debug.Log($"Pohon '{this.namaPohon}' sudah mencapai tahap pertumbuhan terakhir atau data tidak ditemukan.");
        }
        // Baris ini sepertinya tidak diperlukan lagi karena logika stage diurus oleh pohon baru
        // currentStage++; 
    }

    // Fungsi bantu untuk merapikan kode
    private void UpdateReferencesInManagers(GameObject pohonLama, GameObject pohonBaru)
    {
        // Perbarui referensi di GrowthManager
        //if (MainEnvironmentManager.Instance.pohonManager != null)
        //{
        //    int index = MainEnvironmentManager.Instance.pohonManager.allActiveTrees.IndexOf(this);
        //    if (index != -1)
        //    {
        //        MainEnvironmentManager.Instance.pohonManager.allActiveTrees[index] = pohonBaru.GetComponent<TreeBehavior>();
        //    }
        //}

        //// Perbarui referensi di PlantContainer
        //PlantContainer plantContainerScript = plantsContainer.GetComponent<PlantContainer>();
        //if (plantContainerScript != null)
        //{
        //    int index = plantContainerScript.plantObject.IndexOf(pohonLama);
        //    if (index != -1)
        //    {
        //        plantContainerScript.plantObject[index] = pohonBaru;
        //    }
        //}
    }


    public void TakeDamage(int damage)
    {
        if (plantEffectPrefab != null)
        {
            Debug.Log("Menampilkan efek pukulan pada posisi: " + gameObject.transform.position);
            // Buat instance dari prefab efek di lokasi pukulan dengan rotasi yang sesuai
            plantEffectPrefab.Play();
        }
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

    // Contoh di dalam skrip TreeBehavior.cs atau sejenisnya

    //public void OnTreeChoppedDown()
    //{
    //    // Panggil fungsi untuk mendapatkan "paket" data untuk tahap saat ini.
    //    GrowthStageTrees stageData = DatabaseManager.Instance.GetGrowthStageData(this.namaPohon, this.currentStage);

    //    //  Lakukan pengecekan untuk memastikan data ditemukan.
    //    if (stageData != null)
    //    {
    //        // Gunakan datanya. Anda sekarang bisa mengakses semua prefab dari "paket" tersebut.
    //        tumbangSprite = stageData.batangPrefab;
    //        akarPohonPrefab = stageData.AkarPrefab;

            
    //    }
    //    else
    //    {
    //        Debug.LogError("Gagal mendapatkan data tahap pertumbuhan untuk pohon ini!");
    //    }
    //}


    private IEnumerator FellTree()
    {
        isRubuh = true;
        Vector3 posisiPohon = transform.position;

        //Spawn akar di posisi pohon
        if (akarPohonPrefab != null)
        {
            GameObject akar = Instantiate(akarPohonPrefab, posisiPohon, Quaternion.identity, MainEnvironmentManager.Instance.pohonManager.transform);
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

        //EnvironmentManager environmentManager = plantsContainer.gameObject.GetComponent<EnvironmentManager>();

        //if (environmentManager != null)
        //{
        //    Vector2 posisiPohon2D = new Vector2(transform.position.x, transform.position.y);
        //    //foreach (var cekLokasiObjek in environmentManager.environmentList)
        //    //{
        //    //    Vector2 posisiCek2D = new Vector2(cekLokasiObjek.position.x, cekLokasiObjek.position.y);
        //    //    if (Vector2.Distance(posisiPohon2D, posisiCek2D) < 0.1f && nameEnvironment == cekLokasiObjek.TreeID)
        //    //    {
        //    //        cekLokasiObjek.isGrowing = false;
        //    //    }
        //    //}
        //}
        int daysToWait = UnityEngine.Random.Range(2, 6); // Menunggu 2 sampai 5 hari
        int respawn = TimeManager.Instance.date + daysToWait; // Asumsi Anda punya data hari saat ini
        Debug.Log(" total hari respawn : " + respawn);
        TreePlacementData treePlacementData = new TreePlacementData
        {
            TreeID = this.UniqueID,
            dayToRespawn = respawn,
            position = this.gameObject.transform.position,
            typePlant = this.typePlant,
            sudahTumbang = this.isRubuh,
            initialStage = this.currentStage
        };
        MainEnvironmentManager.Instance.pohonManager.AddSecondListTrees(treePlacementData);
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
