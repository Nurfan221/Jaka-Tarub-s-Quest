using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif









public class BatuManager : MonoBehaviour, ISaveable
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

        if (listBatuManager == null)
        {
            listBatuManager = new List<ListBatuManager>();
        }
    }

    private void Start()
    {
        stoneDatabase = DatabaseManager.Instance.worldStoneDatabase;
        NewDay();
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
            }
        }
        // Kosongkan list pelacak setelah selesai
        listStoneActivePerDay.Clear();
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
        NonActiveGameObject();
        // PANGGIL FUNGSI PEMBERSIH DI AWAL
        ClearExistingStones();

        List<TemplateStoneActive> candidatePool = new List<TemplateStoneActive>();

        if (dailyLuck < 1) // HARI TIDAK BERUNTUNG
        {
            // Hanya bisa muncul Copper
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
            if (copperGroup != null) candidatePool.AddRange(copperGroup.listActive);
        }
        else if (dailyLuck < 3) // HARI NORMAL (luck antara 1 dan 2.99)
        {
            // Bisa muncul Copper dan Iron
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Iron);
            if (copperGroup != null) candidatePool.AddRange(copperGroup.listActive);
            if (ironGroup != null) candidatePool.AddRange(ironGroup.listActive);
        }
        else // HARI SANGAT BERUNTUNG (luck >= 3)
        {
            // Bisa muncul Copper, Iron, dan Gold
            var copperGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Copper);
            var ironGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Iron);
            var goldGroup = listBatuManager.FirstOrDefault(g => g.typeStone == TypeObject.Gold);
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

    // FUNGSI ANDA UNTUK ME-RESET SEMUA BATU
    public void NonActiveGameObject()
    {
        Debug.Log("Menonaktifkan semua batu sumber daya (Copper, Iron, Gold)...");
        foreach (var group in listBatuManager)
        {
            // Kita hanya menonaktifkan yang bisa di-spawn (bukan batu biasa)
            if (group.typeStone != TypeObject.Stone)
            {
                foreach (var stoneData in group.listActive)
                {
                    if (stoneData.stoneObject != null)
                    {
                        stoneData.stoneObject.SetActive(false);
                    }
                }
            }
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
                stoneObject = stone.gameObject,
                position = stone.transform.position,
            };

            // Tambahkan batu ini ke dalam grup yang sesuai di 'organizer'
            groups[stoneType].listActive.Add(newActiveTemplate);
        }

        // Langkah 4: Setelah semua batu dikelompokkan, pindahkan hasilnya ke list utama Anda
        listBatuManager = groups.Values.ToList();

        Debug.Log($"Proses selesai. Menemukan {childStones.Length} batu dan mengelompokkannya ke dalam {listBatuManager.Count} grup.");
    }
}

