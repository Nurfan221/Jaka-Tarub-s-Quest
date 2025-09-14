using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif









public class BatuManager : MonoBehaviour
{
    public static BatuManager Instance { get; private set; }
    [Header("Referensi Database")]
    public WorldStoneDatabaseSO stoneDatabase;
    public List<ListBatuManager> listBatuManager = new List<ListBatuManager>();
    public List<TemplateStoneActive> listStoneActivePerDay = new List<TemplateStoneActive>();



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
    }

    private void Start()
    {
        stoneDatabase = DatabaseManager.Instance.worldStoneDatabase;
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
    }

    // Fungsi untuk membersihkan batu dari hari sebelumnya
    public void ClearExistingStones()
    {
        Debug.Log($"Membersihkan {listStoneActivePerDay.Count} batu dari hari sebelumnya.");
        foreach (var stone in listStoneActivePerDay)
        {
            if (stone.stoneObject != null)
            {
                // Nonaktifkan GameObject-nya
                stone.stoneObject.SetActive(false);
                // Set statusnya menjadi tidak aktif
                stone.isActive = false;
            }
        }
        // Kosongkan list pelacak setelah selesai
        listStoneActivePerDay.Clear();
    }



    // Fungsi utama yang baru
    public void SpawnStonesForDay(float dailyLuck)
    {
        NonActiveGameObject();
        // PANGGIL FUNGSI PEMBERSIH DI AWAL
        ClearExistingStones();

        List<TemplateStoneActive> candidatePool = new List<TemplateStoneActive>();

        if (dailyLuck < 1) // HARI TIDAK BERUNTUNG
        {
            // Hanya bisa muncul Copper
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Copper);
            if (copperGroup != null) candidatePool.AddRange(copperGroup.listActive);
        }
        else if (dailyLuck < 3) // HARI NORMAL (luck antara 1 dan 2.99)
        {
            // Bisa muncul Copper dan Iron
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Iron);
            if (copperGroup != null) candidatePool.AddRange(copperGroup.listActive);
            if (ironGroup != null) candidatePool.AddRange(ironGroup.listActive);
        }
        else // HARI SANGAT BERUNTUNG (luck >= 3)
        {
            // Bisa muncul Copper, Iron, dan Gold
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Iron);
            var goldGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Gold);
            if (copperGroup != null) candidatePool.AddRange(copperGroup.listActive);
            if (ironGroup != null) candidatePool.AddRange(ironGroup.listActive);
            if (goldGroup != null) candidatePool.AddRange(goldGroup.listActive);
        }

        int maxSpawns = 0;
        if (dailyLuck < 1) maxSpawns = 5;
        else if (dailyLuck < 3) maxSpawns = 15;
        else maxSpawns = 20;
        int finalSpawnCount = Mathf.Min(maxSpawns, candidatePool.Count);

        var shuffledPool = candidatePool.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int i = 0; i < finalSpawnCount; i++)
        {
            TemplateStoneActive stoneDataToSpawn = shuffledPool[i];

            if (stoneDataToSpawn.stoneObject != null)
            {
                // Aktifkan objek batu di scene
                stoneDataToSpawn.stoneObject.SetActive(true);
                stoneDataToSpawn.isActive = true;

                // Tambahkan batu yang baru aktif ini ke dalam list pelacak
                listStoneActivePerDay.Add(stoneDataToSpawn);
            }
        }


        Debug.Log($"Hari ini (luck: {dailyLuck}), sebanyak {finalSpawnCount} batu telah dimunculkan.");
    }

    // Kode helper untuk mengisi candidate pool (agar lebih rapi)
    private void FillCandidatePool(float dailyLuck, List<TemplateStoneActive> pool)
    {
        pool.Clear();
        if (dailyLuck < 1)
        {
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Copper);
            if (copperGroup != null) pool.AddRange(copperGroup.listActive);
        }
        else if (dailyLuck < 3)
        {
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Iron);
            if (copperGroup != null) pool.AddRange(copperGroup.listActive);
            if (ironGroup != null) pool.AddRange(ironGroup.listActive);
        }
        else
        {
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Iron);
            var goldGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeStone.Gold);
            if (copperGroup != null) pool.AddRange(copperGroup.listActive);
            if (ironGroup != null) pool.AddRange(ironGroup.listActive);
            if (goldGroup != null) pool.AddRange(goldGroup.listActive);
        }
    }

    // FUNGSI ANDA UNTUK ME-RESET SEMUA BATU
    public void NonActiveGameObject()
    {
        Debug.Log("Menonaktifkan semua batu sumber daya (Copper, Iron, Gold)...");
        foreach (var group in listBatuManager)
        {
            // Kita hanya menonaktifkan yang bisa di-spawn (bukan batu biasa)
            if (group.typeStone != TypeStone.Stone)
            {
                foreach (var stoneData in group.listActive)
                {
                    if (stoneData.stoneObject != null)
                    {
                        stoneData.stoneObject.SetActive(false);
                        stoneData.isActive = false;
                    }
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
        var groups = new Dictionary<TypeStone, ListBatuManager>();

        foreach (var stone in childStones)
        {
            // Ambil tipe batu dari komponen StoneBehavior
            // Asumsi: Anda memiliki variabel public TypeStone typeStone di dalam StoneBehavior.cs
            TypeStone stoneType = stone.stoneType;
            UniqueID idStoneTemplate = stone.GetComponent<UniqueID>();
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
                stoneID = idStoneTemplate.ID,
                stoneObject = stone.gameObject,
                position = stone.transform.position,
                isActive = stone.gameObject.activeInHierarchy
            };

            // Tambahkan batu ini ke dalam grup yang sesuai di 'organizer'
            groups[stoneType].listActive.Add(newActiveTemplate);
        }

        // Langkah 4: Setelah semua batu dikelompokkan, pindahkan hasilnya ke list utama Anda
        listBatuManager = groups.Values.ToList();

        Debug.Log($"Proses selesai. Menemukan {childStones.Length} batu dan mengelompokkannya ke dalam {listBatuManager.Count} grup.");
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
                stoneToRespawn = stoneDataFromBlueprint,

                
                //dayToRespawn = respawnDay
            });
            stoneDataFromBlueprint.dayToRespawn = currentDay;
            
            
            stoneDataFromBlueprint.stoneObject.SetActive(false);
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


}
