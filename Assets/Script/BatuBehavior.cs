using System.Collections.Generic;
using UnityEngine;



public class StoneBehavior : UniqueIdentifiableObject
{


    public List<ItemData> itemDropMines; // Array hasil tambang yang mungkin dijatuhkan
    public ParticleSystem hitEffectPrefab; // Variabel untuk Prefab partikel
    //Animation idle 
    public string nameStone;
    public TypeObject stoneType;
    public EnvironmentHardnessLevel environmentHardnessLevel;
    public bool useAnimation;

    public int minHasil;
    public int maxHasil;

    public float health; // Kesehatan batu
    public bool isLucky;
    public int dayToRespawn;
    public bool diHancurkan = false;

    [Header("Animation Settings")]
    // Masukkan file Override Animator Controller yang spesifik untuk batu ini
    public RuntimeAnimatorController stoneAnimatorController;
    public SpriteRenderer srVisualChild;
    private Animator stoneAnimator;

    [Header("Drop Tiers & Balancing")]
    [Tooltip("Batas maksimal item langka yang bisa didapat dari satu batu, untuk menjaga keseimbangan.")]
    [SerializeField] private int maxRareDropsLimit = 2;

    // List ini akan diisi secara otomatis saat game dimulai
    public List<ItemData> commonTierItems = new List<ItemData>();
    public List<ItemData> uncommonTierItems = new List<ItemData>();
    public List<ItemData> rareTierItems = new List<ItemData>();

    // Enum untuk membuat kode lebih mudah dibaca
    private enum ItemTier { Common, Uncommon, Rare }

    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return stoneType.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return environmentHardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        return stoneType.ToString();
    }

    public override string GetVariantName()
    {
        return "";
    }

    #endregion

    private void Awake()
    {
        PushHasilTambang();
        CategorizeItemsIntoTiers();
    }

    public void Start()
    {
        hitEffectPrefab = gameObject.GetComponentInChildren<ParticleSystem>();

        if (useAnimation)
        {
         SetupVisualComponent();

        }


        //StartCoroutine(PlayStoneAnimation()); // Mulai animasi

    }

    private void SetupVisualComponent()
    {
        Transform visualChild = transform.Find("Visual");
        Debug.Log("memanggil fungsi setup component visual");
      
        if (visualChild != null)
        {
            Debug.Log("ya component visual ditemukan");
            srVisualChild = visualChild.GetComponent<SpriteRenderer>();
            
            // Kita cek dulu biar ga double, kalau belum ada baru tambah
            stoneAnimator = visualChild.GetComponent<Animator>();
            if (stoneAnimator == null)
            {
                stoneAnimator = visualChild.gameObject.AddComponent<Animator>();
            }
            if (useAnimation)
            {
                // Jika butuh animasi, baru kita cek apakah controllernya sudah dipasang
                if (stoneAnimatorController != null)
                {
                    stoneAnimator.runtimeAnimatorController = stoneAnimatorController;
                }
                else
                {
                    // Niatnya mau pakai animasi (useAnimation = true), tapi lupa pasang controller.
                    Debug.LogError($"Gawat! {stoneType} disetting pakai animasi, tapi stoneAnimatorController-nya kosong!");
                }
            }

            // Karena ini lewat code, kita harus set manual agar tidak lag di kota
            stoneAnimator.cullingMode = AnimatorCullingMode.CullCompletely;
        }
        else
        {
            Debug.LogError("ohhh tidak component visual tidak ditemukan anda dongok");
        }
    }

    private void CategorizeItemsIntoTiers()
    {
        if (itemDropMines == null || itemDropMines.Count == 0) return;

        int totalItems = itemDropMines.Count;

        // Tentukan titik pembagian (40% awal, 20% tengah, 40% akhir)
        int commonEndIndex = Mathf.FloorToInt(totalItems * 0.4f);
        int uncommonEndIndex = Mathf.FloorToInt(totalItems * 0.6f);

        // Bersihkan list untuk mencegah duplikasi jika fungsi ini dipanggil lagi
        commonTierItems.Clear();
        uncommonTierItems.Clear();
        rareTierItems.Clear();

        for (int i = 0; i < totalItems; i++)
        {
            if (i < commonEndIndex)
            {
                commonTierItems.Add(itemDropMines[i]);
            }
            else if (i < uncommonEndIndex)
            {
                uncommonTierItems.Add(itemDropMines[i]);
            }
            else
            {
                rareTierItems.Add(itemDropMines[i]);
            }
        }

        // Debug untuk memastikan pembagiannya benar
        Debug.Log($"Item Tiers Categorized for {gameObject.name}: Common({commonTierItems.Count}), Uncommon({uncommonTierItems.Count}), Rare({rareTierItems.Count})");
    }


    public void PushHasilTambang()
    {
        itemDropMines.Clear();
        itemDropMines = DatabaseManager.Instance.GetHasilTambang(stoneType, environmentHardnessLevel);
        Debug.Log($"[LOAD] Batu {nameStone} memiliki {itemDropMines.Count} hasil tambang yang mungkin.");
    }

   

    public void TakeDamage(int damage)
    {
        if (diHancurkan) return;

        if (hitEffectPrefab != null)
        {
            Debug.Log("Menampilkan efek pukulan pada posisi: " + gameObject.transform.position);
            // Buat instance dari prefab efek di lokasi pukulan dengan rotasi yang sesuai
            hitEffectPrefab.Play();
        }
        health -= Mathf.Min(damage, health);
        Debug.Log($"Batu terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            diHancurkan = true;
            DestroyStone();
        }
    }

    private void DestroyStone()
    {
        Debug.Log($"Memulai DestroyStone. DayLuck: {TimeManager.Instance.dailyLuck}");

        if (itemDropMines.Count == 0)
        {
            Debug.LogWarning("itemDropMines kosong. Tidak ada item yang dijatuhkan.");
            ScheduleRespawnAndDeactivate();
            return;
        }

        int amountToDrop = UnityEngine.Random.Range(minHasil, maxHasil + 1);
        List<ItemData> chosenDrops = new List<ItemData>();


        if (itemDropMines.Count < 3)
        {
            Debug.Log($"Jumlah item drop kurang dari 3 ({itemDropMines.Count}). Menggunakan logika drop sederhana, mengabaikan DailyLuck.");

            // Logika sederhana (tidak diubah)
            for (int i = 0; i < amountToDrop; i++)
            {
                int index = UnityEngine.Random.Range(0, itemDropMines.Count);
                ItemData droppedItem = itemDropMines[index];
                chosenDrops.Add(droppedItem);
                Debug.Log($"Iterasi {i + 1}: Item terpilih -> {droppedItem.itemName}");
            }
        }
        else
        {
            int rareItemsDroppedCount = 0;
            // Kita gunakan variabel baru untuk loop, agar 'amountToDrop' tetap jadi total akhir
            int amountOfDropsForLoop = amountToDrop;

            // Cek kondisi balance (di bawah 3)
            if (TimeManager.Instance.dailyLuck < 3)
            {
                Debug.Log($"BALANCE: DailyLuck < 3 ({TimeManager.Instance.dailyLuck}). Menerapkan jaminan drop.");

                // Pastikan list-nya ada isinya DAN kita masih punya jatah drop
                if (uncommonTierItems.Count > 0 && amountOfDropsForLoop > 0)
                {
                    ItemData guaranteedUncommon = uncommonTierItems[UnityEngine.Random.Range(0, uncommonTierItems.Count)];
                    chosenDrops.Add(guaranteedUncommon);
                    amountOfDropsForLoop--; // Kurangi jatah drop untuk loop
                    Debug.Log($"BALANCE: Menjamin 1 item Uncommon -> {guaranteedUncommon.itemName}");
                }

                // Pastikan list-nya ada isinya, kita masih punya jatah drop, DAN kita belum melebihi batas rare
                if (rareTierItems.Count > 0 && amountOfDropsForLoop > 0 && rareItemsDroppedCount < maxRareDropsLimit)
                {
                    ItemData guaranteedRare = rareTierItems[UnityEngine.Random.Range(0, rareTierItems.Count)];
                    chosenDrops.Add(guaranteedRare);
                    amountOfDropsForLoop--; // Kurangi jatah drop untuk loop
                    rareItemsDroppedCount++; // PENTING: Hitung ini sebagai drop rare pertama!
                    Debug.Log($"BALANCE: Menjamin 1 item Rare -> {guaranteedRare.itemName}");
                }
            }

            // Kita sekarang menjalankan loop untuk sisa drop (jika ada)
            Debug.Log($"Jumlah item cukup. Menggunakan sistem tier. Total drop: {amountToDrop}. ({chosenDrops.Count} sudah dijamin, {amountOfDropsForLoop} akan diacak).");

            // Loop sekarang menggunakan amountOfDropsForLoop
            for (int i = 0; i < amountOfDropsForLoop; i++)
            {
                ItemTier chosenTier = GetTierBasedOnLuck();

                if (chosenTier == ItemTier.Rare)
                {
                    if (rareItemsDroppedCount >= maxRareDropsLimit)
                    {
                        // Gunakan (i + chosenDrops.Count) agar log iterasi tetap urut
                        Debug.Log($"Iterasi {i + chosenDrops.Count + 1}: Batas item langka ({maxRareDropsLimit}) tercapai. Menurunkan tier dari Rare menjadi Uncommon.");
                        chosenTier = ItemTier.Uncommon;
                    }
                    else
                    {
                        rareItemsDroppedCount++;
                    }
                }

                ItemData droppedItem = null;
                switch (chosenTier)
                {
                    case ItemTier.Common:
                        if (commonTierItems.Count > 0)
                            droppedItem = commonTierItems[UnityEngine.Random.Range(0, commonTierItems.Count)];
                        break;
                    case ItemTier.Uncommon:
                        if (uncommonTierItems.Count > 0)
                            droppedItem = uncommonTierItems[UnityEngine.Random.Range(0, uncommonTierItems.Count)];
                        break;
                    case ItemTier.Rare:
                        if (rareTierItems.Count > 0)
                            droppedItem = rareTierItems[UnityEngine.Random.Range(0, rareTierItems.Count)];
                        break;
                }

                if (droppedItem != null)
                {
                    chosenDrops.Add(droppedItem);
                    Debug.Log($"Iterasi {i + chosenDrops.Count}: Tier terpilih -> {chosenTier}. Item -> {droppedItem.itemName}");
                }
            }
        }

        // Drop semua item yang sudah terkumpul (tidak diubah)
        if (chosenDrops.Count > 0)
        {
            Debug.Log($"Proses pemilihan selesai. Total item yang akan didrop: {chosenDrops.Count}");
            foreach (var item in chosenDrops)
            {
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.1f, 0.5f));
                ItemPool.Instance.DropItem(item.itemName, item.itemHealth, item.quality, transform.position + offset, 1);
            }
        }

        ScheduleRespawnAndDeactivate();
    }

    private void ScheduleRespawnAndDeactivate()
    {
        int daysToWait = UnityEngine.Random.Range(2, 6);
        dayToRespawn = TimeManager.Instance.date + daysToWait;

        Debug.Log($"Batu akan respawn dalam {daysToWait} hari, yaitu pada hari ke-{dayToRespawn}.");

        BatuManager.Instance.ScheduleRespawn(UniqueID, dayToRespawn);
        Destroy(gameObject);
    }


    private ItemTier GetTierBasedOnLuck()
    {
        float roll = UnityEngine.Random.value; // Hasilnya adalah angka antara 0.0 dan 1.0

        switch (TimeManager.Instance.dailyLuck)
        {
            // Sangat Beruntung
            case 3:
                if (roll < 0.1f) return ItemTier.Common;      // 10%
                if (roll < 0.4f) return ItemTier.Uncommon;    // 30% (0.4 - 0.1)
                else return ItemTier.Rare;                    // 60% sisanya

            // Normal
            case 2:
                if (roll < 0.3f) return ItemTier.Common;      // 30%
                if (roll < 0.8f) return ItemTier.Uncommon;    // 50% (0.8 - 0.3)
                else return ItemTier.Rare;                    // 20% sisanya

            // Tidak Beruntung (dayLuck <= 1)
            default:
                if (roll < 0.05f) return ItemTier.Rare;       // 5% (Jackpot!)
                else return ItemTier.Common;                  // 95% sisanya
        }
    }
    public void InstantlyDestroy()
    {
        // Kirim damage sebesar HP saat ini (pasti mati)
        TakeDamage(9999);
    }


}
