using System.Collections.Generic;
using System.Linq;
using UnityEngine;












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
    public int maxSpesialStonesToSpawn = 6;


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
        NewDay();
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
        //ProcessRespawnQueue();
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
        var loadedData = state as List<StoneRespawnSaveData>;

        // Pengecekan keamanan tambahan: Pastikan data yang dimuat tidak null
        if (loadedData == null)
        {
            Debug.LogWarning("[LOAD] Gagal merestorasi data batu, data yang dimuat null.");
            return;
        }

        // Pengecekan keamanan tambahan: Pastikan parentEnvironment sudah di-assign
        if (parentEnvironment == null)
        {
            Debug.LogError("[LOAD] Gagal merestorasi batu: parentEnvironment belum di-assign!");
            return;
        }

        respawnQueue.Clear();
        respawnQueue = loadedData.ToList();


    }


    // Fungsi utama yang baru
    public void SpawnStonesForDay(float dailyLuck)
    {
        Debug.Log($"Memulai proses spawn batu untuk hari ini dengan keberuntungan: {dailyLuck}");
        listStoneActivePerDay.Clear(); // Selalu kosongkan list di awal!
        ClearContainer(parentEnvironment);

        //  Selalu tambahkan grup batu BIASA (Stone)
        var stoneGroupTemplate = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Stone);
        if (stoneGroupTemplate != null)
        {
            listStoneActivePerDay.Add(new ListBatuManager
            {
                listName = stoneGroupTemplate.typeStone.ToString(),
                typeStone = stoneGroupTemplate.typeStone,
                listActive = new List<TemplateStoneActive>(stoneGroupTemplate.listActive)
            });
        }

        //  Tentukan pool batu BERHARGA berdasarkan keberuntungan
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
            if (ironGroup != null) valuableCandidatePool.AddRange(ironGroup.listActive); // Iron sudah ada di pool
        }
        else // HARI SANGAT BERUNTUNG
        {
            if (copperGroup != null) valuableCandidatePool.AddRange(copperGroup.listActive);
            if (ironGroup != null) valuableCandidatePool.AddRange(ironGroup.listActive);
            if (goldGroup != null) valuableCandidatePool.AddRange(goldGroup.listActive);
        }

        // Tentukan jumlah spawn MAKSIMAL (untuk pool acak)
        int maxSpawns = 0;
        if (dailyLuck < 1) maxSpawns = 10;
        else if (dailyLuck < 3) maxSpawns = 20;
        else maxSpawns = 30;

        int valuableSpawnCount = Mathf.Min(maxSpawns, valuableCandidatePool.Count);

        // Acak pool dan ambil sejumlah yang dibutuhkan
        var shuffledValuablePool = valuableCandidatePool.OrderBy(x => UnityEngine.Random.value).ToList();
        List<TemplateStoneActive> selectedValuables = shuffledValuablePool.Take(valuableSpawnCount).ToList();


        if (dailyLuck < 3)
        {
            if (ironGroup != null && ironGroup.listActive.Count > 0)
            {
                // Hitung iron yang sudah terpilih
                int ironAlreadySelected = selectedValuables.Count(stone => stone.typeStone == TypeObject.Iron);

                // Hitung berapa banyak lagi iron yang kita butuhkan
                int ironToSpawn = Mathf.Max(0, maxSpesialStonesToSpawn - ironAlreadySelected);

                if (ironToSpawn > 0)
                {
                    // Ambil iron yang BELUM terpilih
                    var availableIron = ironGroup.listActive.Except(selectedValuables).ToList();

                    // Acak iron yang tersedia & ambil sebanyak yang kita butuhkan
                    var extraIron = availableIron.OrderBy(x => UnityEngine.Random.value)
                                                 .Take(ironToSpawn)
                                                 .ToList();

                    if (extraIron.Count > 0)
                    {
                        // 5. Tambahkan ke list spawn utama
                        selectedValuables.AddRange(extraIron);
                        Debug.Log($"BALANCE: Menambahkan {extraIron.Count} Iron Stone ekstra untuk jaminan.");
                    }
                }
            }
        }


        // Kelompokkan batu berharga yang terpilih dan tambahkan ke list akhir
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
        // Kita hitung dari selectedValuables.Count karena itu jumlah batu berharga yg *sebenarnya*
        int totalValuableStones = selectedValuables.Count;
        int totalSpawnedStones = listStoneActivePerDay.Sum(group => group.listActive.Count); // Ini termasuk batu biasa

        Debug.Log($"Proses selesai. Hari ini (luck: {dailyLuck}), sebanyak {totalSpawnedStones} total batu (termasuk {totalValuableStones} batu berharga) telah ditambahkan ke antrian spawn dalam {listStoneActivePerDay.Count} grup.");

        //Spawner Stone di sini 
        SpawnStone();
    }

    public void ClearContainer(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("Parent transform belum diatur!");
            return;
        }

        Debug.Log($"Membersihkan {parent.childCount} anak di dalam '{parent.name}'...");

        // Loop dari anak TERAKHIR ke anak PERTAMA untuk menghindari masalah indeks.
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            // Hancurkan setiap GameObject anak.
            Destroy(parent.GetChild(i).gameObject);
        }

        Debug.Log("Pembersihan selesai.");
    }
    public void SpawnStone()
    {
        // Kunci: stone.id, Nilai: item.dayToRespawn
        var respawnLookup = new Dictionary<string, int>();
        foreach (var item in respawnQueue)
        {
            // Jika ada ID duplikat, ini akan mengambil yang terakhir (seharusnya tidak masalah)
            respawnLookup[item.id] = item.dayToRespawn;
        }

        // Ambil tanggal saat ini CUKUP SATU KALI di luar loop untuk efisiensi
        int currentDate = TimeManager.Instance.date;

        // Buat daftar sementara untuk melacak ID batu yang berhasil di-respawn hari ini
        List<string> respawnedStoneIDs = new List<string>();

        Debug.Log($"Memulai proses Instantiate... Mengecek {listStoneActivePerDay.Sum(g => g.listActive.Count)} batu.");
        int spawnedCount = 0;

        foreach (var group in listStoneActivePerDay)
        {
            foreach (var stone in group.listActive)
            {
                bool shouldSpawn = false; // Flag untuk memutuskan apakah batu ini akan di-spawn

                // 2. Cek apakah batu ini ada di kamus respawn
                if (respawnLookup.TryGetValue(stone.stoneID, out int dayToRespawn))
                {
                    //  Batu ada di antrian. Cek tanggalnya.
                    if (currentDate >= dayToRespawn)
                    {
                        // SUDAH WAKTUNYA RESPAWN!
                        shouldSpawn = true;
                        // Tandai batu ini untuk dihapus dari antrian respawn nanti
                        respawnedStoneIDs.Add(stone.stoneID);
                    }
                    else
                    {
                        //  Batu ada di antrian, TAPI BELUM WAKTUNYA.
                        // 'shouldSpawn' tetap false, jadi batu ini akan dilewati.
                        continue; // Lanjut ke batu berikutnya
                    }
                }
                else
                {
                    // Batu TIDAK ADA di antrian. Ini adalah batu normal.
                    shouldSpawn = true;
                }

                // Lakukan spawning jika 'shouldSpawn' adalah true (dari Kasus A atau C)
                if (shouldSpawn)
                {
                    GameObject stoneObject = DatabaseManager.Instance.GetStone(stone.typeStone, stone.hardnessLevel);

                    if (stoneObject != null)
                    {
                        GameObject newStone = Instantiate(stoneObject, stone.position, Quaternion.identity, parentEnvironment);
                        StoneBehavior stoneBehavior = newStone.GetComponent<StoneBehavior>();

                        if (stoneBehavior != null)
                        {
                            stoneBehavior.UniqueID = stone.stoneID;
                            newStone.name = stone.stoneID;
                            spawnedCount++;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Gagal memunculkan Stone! Tidak dapat menemukan prefab di DatabaseManager untuk tipe '{stone.typeStone}' dan kekerasan '{stone.hardnessLevel}'. ID: {stone.stoneID}");
                    }
                }
            }
        }

        // Setelah semua loop selesai, bersihkan 'respawnQueue' dari batu yang sudah di-respawn
        if (respawnedStoneIDs.Count > 0)
        {
            Debug.Log($"Membersihkan {respawnedStoneIDs.Count} batu dari antrian respawn...");
            // Gunakan RemoveAll untuk menghapus semua item yang ID-nya ada di 'respawnedStoneIDs'
            respawnQueue.RemoveAll(item => respawnedStoneIDs.Contains(item.id));
        }

        Debug.Log($"Proses Spawn Selesai. Berhasil memunculkan {spawnedCount} batu.");
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

        // TAMBAHKAN PENGECEKAN INI
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

