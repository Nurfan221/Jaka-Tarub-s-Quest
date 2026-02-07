using System.Collections;
using UnityEngine;


public class TreeBehavior : UniqueIdentifiableObject
{


    //  Implementasi dari Kontrak IUniqueIdentifiable 
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;


    [Header("Statistik Pohon")]
    public float health = 100f;
    public Item kayu;
    public Item daun;
    public Item getah;
    public Item benih;
    public Item seratTanaman;

    [Header("Logika Tanam Pohon")]
    public ParticleSystem plantEffectPrefab; // Efek partikel saat menanam
    public GrowthTree currentStage = GrowthTree.Seed;
    public TypePlant typePlant;
    public GameObject growthObject; // Gambar untuk tiap tahap pertumbuhan
    public float growthTime; // Waktu total untuk mencapai tahap akhir
    public string nameEnvironment;
    public string namaPohon;
    public bool isRubuh;

    [Header("Animation Settings")]
    public AnimationCurve fallCurve;       // Kurva jatuh (bikin melengkung naik di inspector)
    public float fallDuration = 1.0f;


    [Header("Visual References")]
    public GameObject akarPohonPrefab;
    public GameObject tumbangSpritePrefab; // Pastikan Pivot sprite ini di "Bottom"
    public SpriteRenderer visualPohonAsli; // Assign visual child pohon di sini
    public Collider2D treeCollider;        // Assign collider pohon di sini
    public float growthSpeed; // Waktu jeda antar tahap pertumbuhan
    public int daysSincePlanting = 1; // Hari sejak pohon ditanam
    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer untuk mengganti gambar

    //logika menentukan jumlah minimal dan maksimal dari item yang akan di jatuhkan 
    public int minWood;  // Jumlah minimum kayu
    public int maxWood;  // Jumlah maksimum kayu
    public int minSap;   // Jumlah minimum getah
    public int maxSap;   // Jumlah maksimum getah
    public int minLeaf;  // Jumlah minimum daun
    public int maxLeaf;  // Jumlah maksimum daun
    public int minSeratTanaman;
    public int maxSeratTanaman;





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
        Transform visualChild = transform.Find("Visual");

        if (visualChild != null)
        {
            // Ambil komponen dari anak tersebut
            visualPohonAsli = visualChild.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Gawat! Tidak ada anak bernama 'Visual' di objek ini!");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        //OnTreeChoppedDown();
        plantEffectPrefab = gameObject.GetComponentInChildren<ParticleSystem>();

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

            MainEnvironmentManager.Instance.pohonManager.CheckDataInSecondList(UniqueID);
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

            string oldUniqueID = this.UniqueID;

            GameObject pohonBaru = Instantiate(nextStagePrefab, posisiPohon, Quaternion.identity);
            TreeBehavior treeBehaviorBaru = pohonBaru.GetComponent<TreeBehavior>();

            treeBehaviorBaru.namaPohon = this.namaPohon;
            treeBehaviorBaru.nameEnvironment = this.nameEnvironment;
            treeBehaviorBaru.growthSpeed = this.growthSpeed;
            treeBehaviorBaru.daysSincePlanting = this.daysSincePlanting;
            treeBehaviorBaru.currentStage = (GrowthTree)((int)this.currentStage + 1);

            treeBehaviorBaru.UniqueID = oldUniqueID;
            // Ganti juga nama GameObject agar konsisten di Hierarchy
            pohonBaru.name = oldUniqueID;

            pohonBaru.transform.SetParent(MainEnvironmentManager.Instance.pohonManager.transform);
            GetDatabaseItemDropPohon();
            //UpdateReferencesInManagers(this.gameObject, pohonBaru);

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
    public void GetDatabaseItemDropPohon()
    {
        MainEnvironmentManager.Instance.pohonManager.TumbuhkanPohonDalamAntrian(this);

        DatabaseItemTrees databaseTree = DatabaseManager.Instance.GetDatabaseItemTrees(this.typePlant);
        if (databaseTree != null)
        {
            this.kayu = databaseTree.kayu;
            this.getah = databaseTree.getah;
            this.daun = databaseTree.itemDaun;
            this.benih = databaseTree.itemSeed;
            this.seratTanaman = databaseTree.seratTanaman;
        }
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
        if (isRubuh) return;

        if (plantEffectPrefab != null)
        {
            plantEffectPrefab.Play();
        }

        // Kurangi HP
        health -= Mathf.Min(damage, health);
        Debug.Log($"Pohon terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            // Ini akan memblokir panggilan TakeDamage kedua yang datang milidetik kemudian.
            isRubuh = true;

            StartCoroutine(FellTree());
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

        if (treeCollider != null) treeCollider.enabled = false;
        if (visualPohonAsli != null) visualPohonAsli.enabled = false;

        if (akarPohonPrefab != null)
        {
            GameObject akar = Instantiate(akarPohonPrefab, posisiPohon, Quaternion.identity, MainEnvironmentManager.Instance.pohonManager.transform);

            // [FIX ANIMASI] Matikan efek PopUp untuk akar
            DisablePopUp(akar);

            AkarPohon komponenAkar = akar.GetComponent<AkarPohon>();
            if (komponenAkar != null) komponenAkar.IdObjectUtama = this.UniqueID;
        }

        if (tumbangSpritePrefab != null)
        {
            GameObject batang = Instantiate(tumbangSpritePrefab, posisiPohon, Quaternion.identity);
            batang.transform.SetParent(MainEnvironmentManager.Instance.pohonManager.transform);

            DisablePopUp(batang);

            float targetRotation = -90f;
            yield return StartCoroutine(AnimateFallingLog(batang.transform, targetRotation));

            Destroy(batang, 0.5f);
        }

        DropResources(posisiPohon);
        HandleRespawnData();
        Destroy(gameObject);
    }


    // serta mengembalikan ukuran objek ke normal (Scale 1)
    private void DisablePopUp(GameObject targetObj)
    {
        // Cari script PopUpAnimation di objek ini atau anaknya (karena ada di child "Visual")
        PopUpAnimation popUp = targetObj.GetComponentInChildren<PopUpAnimation>();

        if (popUp != null)
        {
            // Kita ambil referensi transform visualnya dulu sebelum hapus script
            Transform visualTransform = popUp.transform;

            // Hapus script animasinya agar tidak jalan
            Destroy(popUp);

            // Karena biasanya script PopUp mengubah scale jadi 0 di awal (Awake/Start).
            // Jika tidak di-reset, objeknya akan invisible.
            visualTransform.localScale = Vector3.one;
        }
    }

    private IEnumerator AnimateFallingLog(Transform targetInfo, float targetAngle)
    {
        float timer = 0f;
        Quaternion startRotation = targetInfo.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;

            // Hitung progress waktu (0.0 sampai 1.0)
            float normalizedTime = timer / fallDuration;

            // Ambil nilai dari kurva
            float curveValue = fallCurve.Evaluate(normalizedTime);

            // Terapkan rotasi
            targetInfo.rotation = Quaternion.Lerp(startRotation, endRotation, curveValue);

            // Jika curveValue sudah mencapai 1 (atau sangat dekat), artinya pohon sudah di tanah.
            // Paksa keluar dari loop jatuh agar bounce langsung jalan.
            if (curveValue >= 0.99f)
            {
                // Pastikan posisi sudah pol di endRotation sebelum break
                targetInfo.rotation = endRotation;
                break;
            }

            yield return null;
        }

        // Pastikan rotasi final pas (safety net)
        targetInfo.rotation = endRotation;

        // Sekarang Bounce akan langsung jalan begitu pohon menyentuh tanah
        // Tanpa menunggu sisa timer yang tidak perlu
        yield return BounceEffect(targetInfo, targetAngle);
    }

    private IEnumerator BounceEffect(Transform target, float groundAngle)
    {
        // Sedikit membal (contoh: dari -90 ke -85 lalu balik -90)
        float bounceAngle = groundAngle + 5f;
        float bounceTime = 0.2f;
        float t = 0;

        // Naik Dikit
        Quaternion groundRot = Quaternion.Euler(0, 0, groundAngle);
        Quaternion bounceRot = Quaternion.Euler(0, 0, bounceAngle);

        while (t < 1f)
        {
            t += Time.deltaTime / bounceTime;
            // PingPong effect sederhana
            target.rotation = Quaternion.Lerp(groundRot, bounceRot, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        target.rotation = groundRot;
    }

    private void HandleRespawnData()
    {
        int daysToWait = UnityEngine.Random.Range(2, 6);
        int respawnDate = TimeManager.Instance.date + daysToWait;

        TreePlacementData treeData = new TreePlacementData
        {
            TreeID = this.UniqueID,
            dayToRespawn = respawnDate,
            position = transform.position,
            typePlant = this.typePlant,
            sudahTumbang = this.isRubuh,
            initialStage = this.currentStage
        };
        MainEnvironmentManager.Instance.pohonManager.AddSecondListTrees(treeData);
    }
    private void DropResources(Vector3 posisi)
    {
        // Hitung jumlah random untuk masing-masing item
        int woodCount = Random.Range(minWood, maxWood + 1);
        int sapCount = Random.Range(minSap, maxSap + 1);
        int leafCount = Random.Range(minLeaf, maxLeaf + 1);
        int seedCount = Random.Range(minLeaf, maxLeaf + 1);
        int seratCount = Random.Range(minSeratTanaman, maxSeratTanaman + 1);

        Debug.Log($"Menjatuhkan item: Kayu={woodCount}, Getah={sapCount}, Daun={leafCount}, Seed={seedCount}");

        // Drop kayu
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
        if (kayu != null)
        {
            ItemPool.Instance.DropItem(kayu.itemName, kayu.health, kayu.quality, posisi + offset, woodCount);
        }


        // Drop getah
        if (getah != null)
        {
            ItemPool.Instance.DropItem(getah.itemName, getah.health, getah.quality, posisi + offset, sapCount);
        }

        // Drop daun
        if (daun != null)
        {
            ItemPool.Instance.DropItem(daun.itemName, daun.health, daun.quality, posisi + offset, woodCount);
        }

        // Drop Serat Tanaman

        if (seratTanaman != null)
        {
            ItemPool.Instance.DropItem(seratTanaman.itemName, seratTanaman.health, seratTanaman.quality, posisi + offset, sapCount);
        }
        // Drop benih
        if (benih != null)
        {
            ItemPool.Instance.DropItem(benih.itemName, benih.health, benih.quality, posisi + offset, sapCount);
        }

        isRubuh = false;
    }

    public void InstantlyDestroy()
    {
        // Kirim damage sebesar HP saat ini (pasti mati)
        TakeDamage(9999);
    }




}
