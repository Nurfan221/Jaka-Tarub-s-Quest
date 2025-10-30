using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TreeEditor;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using static UnityEditor.Progress;




public class TreesManager : MonoBehaviour, ISaveable
{
    [Header("Tree Control")]
    public bool hasSpawnedInitialTrees = false; // penanda agar pohon awal hanya di-spawn sekali


    [Header("Daftar Hubungan")]
    public WorldTreeDatabaseSO targetTreeDatabase;
    public List<TreePlacementData> secondListTrees = new List<TreePlacementData>();

    [Header("Daftar list Antrian kemunculan Pohon/Environment")]
    public List<TreePlacementData> treeRespawnQueue = new List<TreePlacementData>();

    public Transform parentEnvironment;
    public List<TreePlacementData> environmentList = new List<TreePlacementData>();
    public List<GameObject> gameObjectsList = new List<GameObject>();


    public GameObject kuburanMbokRini;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        TimeManager.OnDayChanged += HandleNewDay;
    }
    private void OnDisable()
    {
        TimeManager.OnDayChanged -= HandleNewDay;
    }


    void Start()
    {

  
        //RegisterAllGameObject();

        parentEnvironment = gameObject.transform;
        targetTreeDatabase = DatabaseManager.Instance.worldTreeDatabase;
        HandleNewDay();


    }

    public object CaptureState()
    {
        Debug.Log("[SAVE] Menangkap data antrian respawn pohon (dengan peningkatan stage)...");

        var saveTreeQueue = new List<TreePlacementData>();

        foreach (var respawnItem in secondListTrees)
        {
            // Buat salinan baru dengan stage yang dinaikkan
            GrowthTree nextStage = GetNextStage(respawnItem.initialStage);

            saveTreeQueue.Add(new TreePlacementData
            {
                TreeID = respawnItem.TreeID,
                dayToRespawn = respawnItem.dayToRespawn,
                position = respawnItem.position,
                typePlant = respawnItem.typePlant,
                sudahTumbang = respawnItem.sudahTumbang,
                initialStage = nextStage, // gunakan stage yang sudah ditingkatkan
                isGrow = respawnItem.isGrow,
            });
        Debug.Log($"Menangkap data antrian respawn pohon : {nextStage}");
        }

        return saveTreeQueue;
    }

    private GrowthTree GetNextStage(GrowthTree currentStage)
    {
        switch (currentStage)
        {
            case GrowthTree.Seed: return GrowthTree.Sprout;
            case GrowthTree.Sprout: return GrowthTree.YoungPlant;
            case GrowthTree.MaturePlant: return GrowthTree.MaturePlant;
            default: return currentStage;
        }
    }


    public void RestoreState(object state)
    {
        Debug.Log("[LOAD] Merestorasi data antrian respawn Pohon...");
        TreePlacementData data = (TreePlacementData)state;
        secondListTrees.Add(new TreePlacementData
        {
            TreeID = data.TreeID,
            dayToRespawn = data.dayToRespawn,
            position = data.position,
            typePlant = data.typePlant,
            sudahTumbang = data.sudahTumbang,
            initialStage = data.initialStage,
            isGrow = data.isGrow
            
        });

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleNewDay()
    {

        float dayLuck = TimeManager.Instance.GetDayLuck();
        SpawnAllTrees();

        //if (isJanganAcak)
        //{
        //    //return;
        //}
        //else
        //{
        //    SpawnFromEnvironmentList(dayLuck);
        //}



    }
    public void RegisterAllObject()
    {
        environmentList.Clear();

        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            Transform child = parentEnvironment.GetChild(i);
            EnvironmentBehavior envBehavior = child.GetComponent<EnvironmentBehavior>();
            TreeBehavior treeBehavior = child.GetComponent<TreeBehavior>();



            TreePlacementData data = new TreePlacementData
            {
                TreeID = treeBehavior.UniqueID,
                position = child.position,
                typePlant = treeBehavior.typePlant,
                initialStage = treeBehavior.currentStage,
            };

            environmentList.Add(data);
        }

        Debug.Log($"Total environment terdaftar: {environmentList.Count}");
    }

    public void RegisterAllGameObject()
    {
        gameObjectsList.Clear();

        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            GameObject gameObject = parentEnvironment.GetChild(i).gameObject;
            gameObjectsList.Add(gameObject);
        }

        Debug.Log($"Total environment terdaftar: {environmentList.Count}");
    }

    private GameObject GetPrefabByName(string name)
    {
        foreach (GameObject prefab in gameObjectsList)
        {
            if (prefab.name == name)
                return prefab;
        }

        //Debug.LogWarning("Prefab tidak ditemukan untuk nama: " + name);
        return null;
    }

    private bool IsPositionOccupied(Vector3 position, float tolerance = 0.1f)
    {
        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            Transform child = parentEnvironment.GetChild(i);
            if (Vector3.Distance(child.position, position) < tolerance)
            {

                return true; // Sudah ada objek di dekat posisi
            }
        }

        return false; // Kosong, boleh spawn
    }

    public void SpawnFromEnvironmentList(float luck)
    {
        ActivateEnvironmentByLuck(luck);

        foreach (var environment in environmentList)
        {
            if (environment.initialStage != GrowthTree.MaturePlant)
            {
                float randomCountSpawn = UnityEngine.Random.Range(0f, 1f);
                if (randomCountSpawn > 0.5f)
                {
                    // Cek apakah posisi sudah terisi
                    if (!IsPositionOccupied(environment.position))
                    {
                        GameObject prefab = GetPrefabByName(environment.TreeID);
                        if (prefab != null)
                        {
                            GameObject obj = Instantiate(prefab, environment.position, Quaternion.identity, parentEnvironment);
                            TreeBehavior treeBehavior = obj.GetComponent<TreeBehavior>();
                            //EnvironmentBehavior environmentBehavior = obj.GetComponent<EnvironmentBehavior>();
                            //if (environmentBehavior != null)
                            //{
                            //    environmentBehavior.plantsContainer = parentEnvironment;
                            //}
                            //else if (treeBehavior != null)
                            //{
                            //    treeBehavior.plantsContainer = parentEnvironment;
                            //}


                            obj.name = prefab.name;
                        }
                    }
                    else
                    {
                        Debug.Log("Posisi terisi, tidak jadi spawn: " + environment.position);
                    }
                }
            }
        }
    }

    public void ActivateEnvironmentByLuck(float luck)
    {
        float spawnChance = 0.5f; // default nilai fallback

        // Tentukan peluang muncul berdasarkan nilai luck
        if (parentEnvironment.name == "Jamur" || parentEnvironment.name == "Bunga")
        {
            if (luck == 3f)
                spawnChance = 0.7f; // 70% peluang muncul
            else if (luck == 2f)
                spawnChance = 0.5f; // 50%
            else
                spawnChance = 0.3f; // 30%
        }

        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            Transform child = parentEnvironment.GetChild(i);

            // Tentukan apakah objek ini akan aktif hari ini
            float roll = UnityEngine.Random.Range(0f, 1f);
            bool willShow = roll <= spawnChance;

            child.gameObject.SetActive(willShow);

            //Debug.Log($"Objek: {child.name} | Luck: {luck} | Roll: {roll} | Aktif: {willShow}");
        }
    }




    public void TumbuhkanPohonDalamAntrian(TreeBehavior treeBehavior)
    {
        TreePlacementData foundTree = secondListTrees.FirstOrDefault(treeData =>
            treeData.TreeID.Equals(treeBehavior.UniqueID, System.StringComparison.OrdinalIgnoreCase)
        );

        // Pastikan kita menemukan data yang benar sebelum mengubahnya
        if (foundTree != null)
        {
            // Perbarui tahap pertumbuhan di dalam data
            foundTree.initialStage = treeBehavior.currentStage;
            Debug.Log($"Data untuk pohon ID '{treeBehavior.UniqueID}' telah diperbarui ke tahap {foundTree.initialStage}.");
        }
        else
        {
            Debug.LogWarning($"Tidak dapat menemukan data untuk pohon dengan ID '{treeBehavior.UniqueID}' di dalam secondListTrees.");
        }
    }

    public void CheckTreefromSecondList(string id)
    {
        foreach (var item in secondListTrees)
        {
            if (item.TreeID == id)
            {
                item.isGrow = true;
                item.initialStage = GrowthTree.Seed;
            }
        }
    }
    public void AddSecondListTrees(TreePlacementData data)
    {
        secondListTrees.Add(data);
    }

  

    public void CheckDataInSecondList(string id)
    {
        foreach (var item in secondListTrees)
        {
            if (item.TreeID == id)
            {
                Debug.Log("pohon saat ini : " + item.initialStage);
                item.initialStage++;
                Debug.Log("pohon saat ini : " + item.initialStage);
            }
        }
    }

    public void SpawnAllTrees()
    {
        int currentDate = TimeManager.Instance.date;
        int spawnedCount = 0;
        List<string> respawnedTreeIDs = new List<string>();

        Debug.Log($"[TreesManager] Memulai proses spawn pohon...");

        // PRIORITAS: Pohon hasil tebang (yang menunggu respawn)
        if (secondListTrees != null && secondListTrees.Count > 0)
        {
            Debug.Log($"[TreesManager] Mengecek {secondListTrees.Count} pohon di daftar respawn...");
            Debug.Log($"[TreesManager] Hari sekarang: {currentDate}");

            foreach (var respawnData in secondListTrees)
            {
                Debug.Log($"[TreesManager] --- Mengecek pohon ID: {respawnData.TreeID}, " +
                          $"tipe: {respawnData.typePlant}, posisi: {respawnData.position}, " +
                          $"dayToRespawn: {respawnData.dayToRespawn}, stage: {respawnData.initialStage}");

                // 1️⃣ Kalau belum waktunya tumbuh, lewati
                if (currentDate < respawnData.dayToRespawn)
                {
                    Debug.Log($"[TreesManager] Pohon {respawnData.TreeID} BELUM waktunya tumbuh. (currentDate={currentDate}, dayToRespawn={respawnData.dayToRespawn})");
                    continue;
                }

                // 2️⃣ Cegah duplikasi jika sudah ada di scene
                var existingTree = parentEnvironment.Find(respawnData.TreeID);
                if (existingTree != null)
                {
                    Debug.LogWarning($"[TreesManager] Pohon {respawnData.TreeID} sudah ada di scene! Melewati spawn ulang.");
                    continue;
                }

                // 3️⃣ Ambil prefab pohon
                GameObject treePrefab = DatabaseManager.Instance.GetPrefabForTreeStage(respawnData.typePlant, respawnData.initialStage);
                if (treePrefab == null)
                {
                    Debug.LogError($"[TreesManager] Prefab pohon TIDAK ditemukan untuk tipe '{respawnData.typePlant}' dengan stage '{respawnData.initialStage}'!");
                    continue;
                }
                else
                {
                    Debug.Log($"[TreesManager] Prefab ditemukan: {treePrefab.name}");
                }

                // 4️⃣ Spawn pohon
                GameObject newTree = Instantiate(treePrefab, respawnData.position, Quaternion.identity, parentEnvironment);
                newTree.name = respawnData.TreeID;

                Debug.Log($"[TreesManager] Berhasil instantiate pohon {respawnData.TreeID} di posisi {respawnData.position}");

                // 5️⃣ Set Unique ID
                var unique = newTree.GetComponent<UniqueIdentifiableObject>();
                if (unique != null)
                {
                    unique.UniqueID = respawnData.TreeID;
                    Debug.Log($"[TreesManager] UniqueID diset: {unique.UniqueID}");
                }
                else
                {
                    Debug.LogWarning($"[TreesManager] Komponen UniqueIdentifiableObject tidak ditemukan di prefab {treePrefab.name}");
                }

                spawnedCount++;
                respawnedTreeIDs.Add(respawnData.TreeID);
            }

            // Debug akhir hasil respawn
            Debug.Log($"[TreesManager] Total pohon respawn hari ini: {spawnedCount}");
            if (respawnedTreeIDs.Count > 0)
            {
                Debug.Log($"[TreesManager] Pohon yang berhasil respawn: {string.Join(", ", respawnedTreeIDs)}");
            }
            else
            {
                Debug.Log($"[TreesManager] Tidak ada pohon yang memenuhi syarat untuk respawn hari ini.");
            }
        }
        else
        {
            Debug.Log("[TreesManager] secondListTrees kosong atau null — tidak ada pohon untuk direspawn.");
        }


        // Spawn awal dunia (hanya jika belum pernah di-spawn)
        if (environmentList != null && environmentList.Count > 0)
        {
            Debug.Log($"[TreesManager] Mengecek {environmentList.Count} pohon dari environmentList (default world)...");

            foreach (var data in environmentList)
            {
                // Lewati jika pohon ini sudah di tebang (ada di secondListTrees)
                if (secondListTrees.Any(t => t.TreeID == data.TreeID))
                    continue;

                // Lewati jika pohon sudah ada di scene
                if (parentEnvironment.Find(data.TreeID) != null)
                    continue;

                GameObject treePrefab = DatabaseManager.Instance.GetPrefabForTreeStage(data.typePlant, data.initialStage);
                if (treePrefab == null)
                {
                    Debug.LogError($"[TreesManager] Gagal menemukan prefab untuk pohon {data.TreeID}!");
                    continue;
                }

                GameObject newTree = Instantiate(treePrefab, data.position, Quaternion.identity, parentEnvironment);
                newTree.name = data.TreeID;

                var unique = newTree.GetComponent<UniqueIdentifiableObject>();
                if (unique != null)
                    unique.UniqueID = data.TreeID;

                spawnedCount++;
            }
        }

        Debug.Log($"[TreesManager] Proses spawn selesai. Total pohon yang dimunculkan: {spawnedCount}");
    }




    // TOMBOL BARU: Untuk mengisi 'environmentList' di dalam Editor
    [ContextMenu("Langkah 1: Daftarkan Semua Objek Anak ke List")]
    public void RegisterAllObjectsInEditor()
    {
        // Pastikan parentEnvironment diatur ke transform dari GameObject ini
        parentEnvironment = this.transform;

        // Panggil fungsi pendaftaran yang sudah ada
        RegisterAllObject();

        Debug.Log($"Proses pendaftaran di Editor selesai. {environmentList.Count} objek terdaftar di environmentList.");
    }

    [ContextMenu("Langkah 2: Pindahkan Data Pohon ke Database SO")]
    public void MigrateTreeDataToSO()
    {
        // Pengecekan Keamanan
        if (targetTreeDatabase == null)
        {
            Debug.LogError("Target WorldTreeDatabaseSO belum diatur! Harap seret asetnya ke Inspector.");
            return;
        }

        // Pastikan environmentList sudah diisi dengan data
        if (environmentList.Count == 0)
        {
            Debug.LogWarning("environmentList masih kosong. Jalankan RegisterAllObject terlebih dahulu jika perlu.");
            return;
        }

        // Kosongkan list di SO untuk menghindari data duplikat
        targetTreeDatabase.initialTreePlacements.Clear();

        Debug.Log($"Memulai migrasi {environmentList.Count} data pohon ke {targetTreeDatabase.name}...");

        // Loop melalui setiap entri di environmentList
        foreach (TreePlacementData treeData in environmentList)
        {
            // Hanya proses jika objek adalah pohon (berdasarkan komponen atau nama)
            // Anda mungkin perlu menyesuaikan kondisi ini
            // Buat entri TreePlacementData baru
            TreePlacementData newPlacementData = new TreePlacementData
            {

                TreeID = treeData.TreeID,
                position = treeData.position,
                typePlant = treeData.typePlant,
                // Asumsi semua pohon yang terdaftar dimulai dari tahap Seed
                initialStage = treeData.initialStage,

            };

            // Tambahkan data baru ke dalam list di ScriptableObject
            targetTreeDatabase.initialTreePlacements.Add(newPlacementData);
        }

        // Tandai aset ScriptableObject sebagai "kotor" agar Unity menyimpan perubahan
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetTreeDatabase);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"Migrasi selesai! {targetTreeDatabase.initialTreePlacements.Count} data pohon berhasil dipindahkan.");
    }

    [ContextMenu("Langkah 3: Bangun Dunia di Editor (Untuk Desain)")]
    public void SpawnTreesInEditor()
    {
        // Pengecekan keamanan
        if (targetTreeDatabase == null)
        {
            Debug.LogError("Database Pohon (targetTreeDatabase) belum diatur di WorldManager!");
            return;
        }
        if (parentEnvironment == null)
        {
            Debug.LogError("Kontainer Pohon Editor (editorTreeContainer) belum diatur! Buat GameObject kosong dan seret ke sini.");
            return;
        }

        // Hapus dulu pohon lama agar tidak duplikat jika tombol ini ditekan lagi
        ClearSpawnedTreesInEditor();

        Debug.Log($"Memulai pembangunan dunia di Editor dari '{targetTreeDatabase.name}'...");
        foreach (var treeData in targetTreeDatabase.initialTreePlacements)
        {
            GameObject objectPohon = DatabaseManager.Instance.GetPrefabForTreeStage(treeData.typePlant, treeData.initialStage);
            if (objectPohon != null)
            {
                // Munculkan pohon dan jadikan anak dari kontainer editor
                GameObject treeInstance = Instantiate(objectPohon, treeData.position, Quaternion.identity);

                

                // Transfer data penting agar pohonnya "tahu" siapa dirinya
                TreeBehavior behavior = treeInstance.GetComponent<TreeBehavior>();
                if (behavior != null)
                {
                    behavior.UniqueID = treeData.TreeID;
                    treeInstance.name = treeData.TreeID;
                    behavior.currentStage = treeData.initialStage;
                }
                treeInstance.gameObject.name = treeData.TreeID;
                treeInstance.transform.SetParent(parentEnvironment);
            }
        }
        Debug.Log("Pembangunan dunia di Editor selesai.");
    }

    [ContextMenu("Langkah 4: Hapus Semua Pohon Editor (Bersihkan Scene)")]
    public void ClearSpawnedTreesInEditor()
    {
        if (parentEnvironment == null)
        {
            Debug.LogError("Kontainer Pohon Editor (editorTreeContainer) belum diatur!");
            return;
        }

        Debug.Log($"Membersihkan semua pohon di dalam '{parentEnvironment.name}'...");
        // Kita loop dari belakang agar tidak merusak indeks saat menghapus objek
        for (int i = parentEnvironment.childCount - 1; i >= 0; i--)
        {
            // Gunakan DestroyImmediate karena ini adalah script editor, bukan runtime
            DestroyImmediate(parentEnvironment.GetChild(i).gameObject);
        }
        Debug.Log("Pembersihan selesai. Scene sekarang bersih dari pohon yang dibuat oleh tool ini.");
    }
}
