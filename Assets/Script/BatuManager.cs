using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using static UnityEditorInternal.VersionControl.ListControl;
using System.Runtime.ConstrainedExecution;


#if UNITY_EDITOR
using UnityEditor;
#endif









public class BatuManager : MonoBehaviour, ISaveable
{
    public static BatuManager Instance { get; private set; }
    [Header("Referensi Database")]
    public WorldStoneDatabaseSO stoneDatabase;
    public List<ListBatuManager> listBatuManager = new List<ListBatuManager>();
    public List<ListBatuManager> listStoneActivePerDay = new List<ListBatuManager>();
    public Transform parentEnvironment;



    [Header("Pengaturan Spawning")]
    [Tooltip("Daftar semua kemungkinan lokasi spawn batu.")]
    public List<StoneRespawnSaveData> respawnQueue = new List<StoneRespawnSaveData>();

    [Tooltip("Jumlah maksimal batu yang akan muncul dalam satu siklus.")]
    public int maxStonesToSpawn = 30;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        if (listBatuManager == null)
        {
            listBatuManager = new List<ListBatuManager>();
        }
    }

    private void Start()
    {
        stoneDatabase = DatabaseManager.Instance.worldStoneDatabase;
        //NewDay();
        parentEnvironment = this.transform;

    }

    private void OnEnable()
    {
        TimeManager.OnDayChanged += NewDay;
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged += NewDay;

    }
    // Fungsi utama yang dipanggil untuk memulai proses spawning
    // Anda bisa memanggil ini dari GameManager setiap pagi, misalnya.


    public void NewDay()
    {
        float luck = TimeManager.Instance.GetDayLuck();
        SpawnStonesForDay(luck);
        ProcessRespawnQueue();
    }

   

    public object CaptureState()
    {
        Debug.Log("[SAVE] Menangkap data antrian respawn batu...");

        // Buat list baru dengan format yang siap disimpan
        var saveQueue = new List<StoneRespawnSaveData>();

        // Konversi setiap item di antrian runtime ke format save
        foreach (var respawnItem in respawnQueue)
        {
            saveQueue.Add(new StoneRespawnSaveData
            {
                id = respawnItem.id,
                dayToRespawn = respawnItem.dayToRespawn,
                stonePosition = respawnItem.stonePosition
            });
        }

        // Kembalikan SELURUH LIST yang sudah siap disimpan
        return saveQueue;
    }

    public void RestoreState(object state)
    {
        Debug.Log("[LOAD] Merestorasi data antrian respawn batu...");
        StoneRespawnSaveData data = (StoneRespawnSaveData)state;
        respawnQueue.Add(new StoneRespawnSaveData
        {
            id = data.id,
            dayToRespawn = data.dayToRespawn,
            stonePosition = data.stonePosition
        });

    }



    // Fungsi utama yang baru
    public void SpawnStonesForDay(float dailyLuck)
    {
        Debug.Log($"Memulai proses spawn batu untuk hari ini dengan keberuntungan: {dailyLuck}");
        listStoneActivePerDay.Clear(); // Selalu kosongkan list di awal!

        //  Selalu tambahkan grup batu BIASA (Stone) ---
        var stoneGroupTemplate = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Stone);
        if (stoneGroupTemplate != null)
        {
            // Buat grup baru untuk output hari ini dan tambahkan ke daftar
            listStoneActivePerDay.Add(new ListBatuManager
            {
                listName = stoneGroupTemplate.typeStone.ToString(),
                typeStone = stoneGroupTemplate.typeStone,
                // Buat salinan list agar tidak mengubah data asli
                listActive = new List<TemplateStoneActive>(stoneGroupTemplate.listActive)
            });
        }

        //  Tentukan pool batu BERHARGA berdasarkan keberuntungan ---
        List<TemplateStoneActive> valuableCandidatePool = new List<TemplateStoneActive>();

        var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
        var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Iron);
        var goldGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Gold);

        if (dailyLuck < 1) // HARI TIDAK BERUNTUNG
        {
            if (copperGroup != null) valuableCandidatePool.AddRange(copperGroup.listActive);
        }
        else if (dailyLuck < 3) // HARI NORMAL
        {
            if (copperGroup != null) valuableCandidatePool.AddRange(copperGroup.listActive);
            if (ironGroup != null) valuableCandidatePool.AddRange(ironGroup.listActive);
        }
        else // HARI SANGAT BERUNTUNG
        {
            if (copperGroup != null) valuableCandidatePool.AddRange(copperGroup.listActive);
            if (ironGroup != null) valuableCandidatePool.AddRange(ironGroup.listActive);
            if (goldGroup != null) valuableCandidatePool.AddRange(goldGroup.listActive);
        }

        // Pilih batu BERHARGA dalam jumlah terbatas ---
        int maxSpawns = 0;
        if (dailyLuck < 1) maxSpawns = 10;
        else if (dailyLuck < 3) maxSpawns = 20;
        else maxSpawns = 30;

        int valuableSpawnCount = Mathf.Min(maxSpawns, valuableCandidatePool.Count);

        // Acak pool dan ambil sejumlah yang dibutuhkan
        var shuffledValuablePool = valuableCandidatePool.OrderBy(x => UnityEngine.Random.value).ToList();
        List<TemplateStoneActive> selectedValuables = shuffledValuablePool.Take(valuableSpawnCount).ToList();

        // Kelompokkan batu berharga yang terpilih dan tambahkan ke list akhir ---
        var groupedValuables = selectedValuables.GroupBy(stone => stone.typeStone);

        foreach (var group in groupedValuables)
        {
            listStoneActivePerDay.Add(new ListBatuManager
            {
                listName = group.Key.ToString(),
                typeStone = group.Key,
                listActive = group.ToList() // Konversi hasil pengelompokan menjadi List
            });
        }

        // Hitung total batu yang di-spawn untuk logging
        int totalSpawnedStones = listStoneActivePerDay.Sum(group => group.listActive.Count);

        Debug.Log($"Proses selesai. Hari ini (luck: {dailyLuck}), sebanyak {totalSpawnedStones} total batu (termasuk {valuableSpawnCount} batu berharga) telah ditambahkan ke antrian spawn dalam {listStoneActivePerDay.Count} grup.");


        //Spawner Stone di sini 
        SpawnStone();
    }

    public void SpawnStone()
    {
        Debug.Log($"Memulai proses Instantiate untuk {listStoneActivePerDay.Sum(g => g.listActive.Count)} batu...");
        foreach (var group in listStoneActivePerDay)
        {
            foreach (var stone in group.listActive)
            {
                // Coba dapatkan prefab dari database
                GameObject stoneObject = DatabaseManager.Instance.GetStone(stone.typeStone, stone.hardnessLevel);

                // Cek apakah prefab berhasil ditemukan
                if (stoneObject != null)
                {
                    // Jika berhasil, munculkan batunya
                    GameObject newStone = Instantiate(stoneObject, stone.position, Quaternion.identity, parentEnvironment);
                    StoneBehavior stoneBehavior = newStone.GetComponent<StoneBehavior>();
                    if (stoneBehavior != null)
                    {
                        stoneBehavior.UniqueID = stone.stoneID;
                        newStone.name = stone.stoneID;
                        Debug.Log("Berhasil memunculkan Stone dengan ID: " + stone.stoneID);
                    }
                }
                else
                {
                    // Jika GAGAL, berikan pesan error yang spesifik agar mudah dilacak!
                    Debug.LogError($"Gagal memunculkan Stone Tidak dapat menemukan prefab di DatabaseManager untuk batu dengan tipe '{stone.typeStone}' dan kekerasan '{stone.hardnessLevel}'. ID: {stone.stoneID}");
                }
            }
        }
    }

    // Kode helper untuk mengisi candidate pool (agar lebih rapi)
    private void FillCandidatePool(float dailyLuck, List<TemplateStoneActive> pool)
    {
        pool.Clear();
        if (dailyLuck < 1)
        {
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
            if (copperGroup != null) pool.AddRange(copperGroup.listActive);
        }
        else if (dailyLuck < 3)
        {
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Iron);
            if (copperGroup != null) pool.AddRange(copperGroup.listActive);
            if (ironGroup != null) pool.AddRange(ironGroup.listActive);
        }
        else
        {
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Iron);
            var goldGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Gold);
            if (copperGroup != null) pool.AddRange(copperGroup.listActive);
            if (ironGroup != null) pool.AddRange(ironGroup.listActive);
            if (goldGroup != null) pool.AddRange(goldGroup.listActive);
        }
    }




    public void ScheduleRespawn(string stoneID, int currentDay)
    {
        Debug.Log($"Mencoba menjadwalkan respawn untuk ID: '{stoneID}'");
        TemplateStoneActive stoneDataFromBlueprint = FindStoneDataByID(stoneID);

        // --- TAMBAHKAN PENGECEKAN INI ---
        if (stoneDataFromBlueprint != null)
        {
            // Kode ini HANYA akan berjalan jika batu berhasil ditemukan
            Debug.Log("Data ditemukan: " + stoneDataFromBlueprint.stoneID);



            respawnQueue.Add(new StoneRespawnSaveData
            {
                id = stoneDataFromBlueprint.stoneID,
                stonePosition = stoneDataFromBlueprint.position,
                dayToRespawn = currentDay


                //dayToRespawn = respawnDay
            });


        }
        else
        {
            // Kode ini akan berjalan jika batu TIDAK ditemukan
            Debug.LogError($"GAGAL: Tidak dapat menemukan batu dengan ID: '{stoneID}' di dalam listBatuManager!");
        }
    }
    // Fungsi pencarian Anda sudah benar, tidak perlu diubah
    public TemplateStoneActive FindStoneDataByID(string idToFind)
    {
        var foundStone = listBatuManager
                         .SelectMany(group => group.listActive)
                         .FirstOrDefault(stoneData => stoneData.stoneID == idToFind);

        return foundStone;
    }



    public void ProcessRespawnQueue()
    {
        // Gunakan FOR loop yang berjalan MUNDUR untuk keamanan saat menghapus item
        for (int i = respawnQueue.Count - 1; i >= 0; i--)
        {
            // Ambil item saat ini dari antrian
            var item = respawnQueue[i];

            // Cek dulu apakah waktunya sudah tiba untuk respawn
            if (TimeManager.Instance.date >= item.dayToRespawn)
            {
                // Jika sudah waktunya, baru kita cari GameObject-nya
                Transform stoneTransform = transform.Find(item.id);

                if (stoneTransform != null)
                {
                    Debug.Log($"Batu '{item.id}' telah respawn pada hari ke-{TimeManager.Instance.date}.");

                    // Aktifkan kembali GameObject-nya
                    stoneTransform.gameObject.SetActive(true);

                    // Hapus item dari antrian karena tugasnya sudah selesai
                    respawnQueue.RemoveAt(i);
                }
                else
                {
                    // Kasus langka: objeknya hilang dari scene, mungkin lebih baik dihapus dari antrian
                    Debug.LogWarning($"Gagal respawn: Child object dengan nama/ID '{item.id}' tidak ditemukan. Menghapus dari antrian.");
                    respawnQueue.RemoveAt(i);
                }
            }
        }
    }

    [ContextMenu("Langkah 1: Cari & Kelompokkan Semua Batu Child")]
    public void FindAndRegisterChildStones()
    {
        StoneBehavior[] childStones = GetComponentsInChildren<StoneBehavior>();

        if (childStones.Length == 0)
        {
            Debug.LogWarning("Tidak ada child object dengan script StoneBehavior yang ditemukan.");
            return;
        }

        // Kuncinya adalah TypeStone, nilainya adalah grup list-nya.
        var groups = new Dictionary<TypeObject, ListBatuManager>();

        foreach (var stone in childStones)
        {
            // Ambil tipe batu dari komponen StoneBehavior
            // Asumsi: Anda memiliki variabel public TypeStone typeStone di dalam StoneBehavior.cs
            TypeObject stoneType = stone.stoneType;
            // Cek apakah grup untuk tipe ini sudah ada di 'organizer' kita
            if (!groups.ContainsKey(stoneType))
            {
                // Jika belum ada, buat grup baru
                groups[stoneType] = new ListBatuManager
                {
                    // Beri nama grup berdasarkan nama enum-nya
                    listName = stoneType.ToString() + " Stones",
                    typeStone = stoneType,
                    listActive = new List<TemplateStoneActive>()
                };
            }

            // Buat entri TemplateStoneActive baru untuk batu ini
            TemplateStoneActive newActiveTemplate = new TemplateStoneActive
            {
                stoneID = stone.UniqueID,
                typeStone = stone.stoneType,
                hardnessLevel = stone.environmentHardnessLevel,
                position = stone.transform.position,
            };

            // Tambahkan batu ini ke dalam grup yang sesuai di 'organizer'
            groups[stoneType].listActive.Add(newActiveTemplate);
        }

        // Langkah Setelah semua batu dikelompokkan, pindahkan hasilnya ke list utama Anda
        listBatuManager = groups.Values.ToList();

        Debug.Log($"Proses selesai. Menemukan {childStones.Length} batu dan mengelompokkannya ke dalam {listBatuManager.Count} grup.");
    }

    [ContextMenu("Langkah 2: Masukan list batu kedalam database")]
    public void RegisterStonesToDatabase()
    {
        if (stoneDatabase == null)
        {
            Debug.LogError("stoneDatabase belum diatur! Silakan atur referensi database terlebih dahulu.");
            return;
        }

        stoneDatabase.stoneBehaviors = listBatuManager;
        // Tandai database sebagai 'dirty' agar Unity tahu ada perubahan yang perlu disimpan
    }
}

