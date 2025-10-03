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
    [Header("Daftar Hubungan")]
    public WorldTreeDatabaseSO targetTreeDatabase;
    public List<TreePlacementData> secondListTrees = new List<TreePlacementData>();

    [Header("Daftar list Antrian kemunculan Pohon/Environment")]
    public List<TreePlacementData> treeRespawnQueue = new List<TreePlacementData>();

    public Transform parentEnvironment;
    public List<TreePlacementData> environmentList = new List<TreePlacementData>();
    public List<GameObject> gameObjectsList = new List<GameObject>();
    public bool isGameObjectManager;
    public bool isJanganAcak;
    public bool isKuburanManager;
    public int countKuburanKotor;
    public int jumlahdiBersihkan;
    public int useStamina = 10;

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

        if (isGameObjectManager)
        {
            RegisterAllGameObject();
        }
        else
        {
            //RegisterAllObject();
        }

        parentEnvironment = gameObject.transform;
        targetTreeDatabase = DatabaseManager.Instance.worldTreeDatabase;
        ProsesPenanamanUlangPohon();
        HandleNewDay();


    }

    public object CaptureState()
    {
        Debug.Log("[SAVE] Menangkap data antrian respawn pohon...");

        // Buat list baru dengan format yang siap disimpan
        var saveTreeQueue = new List<TreePlacementData>();

        // Konversi setiap item di antrian runtime ke format save
        foreach (var respawnItem in secondListTrees)
        {
            saveTreeQueue.Add(new TreePlacementData
            {
                TreeID = respawnItem.TreeID,
                dayToRespawn = respawnItem.dayToRespawn,
                position = respawnItem.position,
                typePlant = respawnItem.typePlant,
                sudahTumbang = respawnItem.sudahTumbang,
                initialStage = respawnItem.initialStage,
                isGrow = respawnItem.isGrow,
                
            });
        }

        // Kembalikan SELURUH LIST yang sudah siap disimpan
        return saveTreeQueue;

       
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
        if (isJanganAcak)
        {
            //return;
        }
        else
        {
            SpawnFromEnvironmentList(dayLuck);
        }

       

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

    

    public void UpdateStatusJob()
    {
        // Tambahkan 1 ke jumlahdiBersihkan setiap kali event dipicu
        jumlahdiBersihkan++;

        // Tampilkan log untuk memverifikasi
        Debug.Log($"Kuburan berhasil dibersihkan! Total: {jumlahdiBersihkan}");


        // Logika tambahan untuk stamina, dll.
        PlayerController.Instance.HandleDrainStamina(useStamina);

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

    public void ProsesPenanamanUlangPohon()
    {
        Debug.Log("memanggil fungsi menanam ulang pohon");
        foreach (var item in secondListTrees)
        {
            if (item.isGrow == true && TimeManager.Instance.date >= item.dayToRespawn)
            {
                Debug.Log($"[RESPAWN POHON] Menanam ulang pohon ID: {item.TreeID} di posisi {item.position} pada hari ke-{TimeManager.Instance.date} (target hari: {item.dayToRespawn} munculkan {item.initialStage}) ");
                GameObject objectPohon = DatabaseManager.Instance.GetPrefabForTreeStage(item.typePlant, item.initialStage);

                Vector3 spawnPosition = new Vector3(item.position.x, item.position.y, 0);

                GameObject plant = Instantiate(objectPohon, spawnPosition, Quaternion.identity);

                // Panggil ForceGenerateUniqueID untuk memastikan pohon yang di-load punya ID yang benar
                plant.GetComponent<UniqueIdentifiableObject>().UniqueID = item.TreeID;
                plant.name = item.TreeID.ToString();
                plant.transform.SetParent(parentEnvironment);
            }


        }

        
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

    public void HandleAddTreesObject()
    {
        
        // yang ada di dalam secondListTrees (data save/perubahan).
        var idsInSecondList = new HashSet<string>(secondListTrees.Select(data => data.TreeID));

        foreach (var item in environmentList)
        {
            // Cek apakah ID dari pohon template ini TIDAK ADA di dalam daftar pelacak.
            if (!idsInSecondList.Contains(item.TreeID))
            {
                // Aman untuk memunculkan versi default-nya.

                Debug.Log($"[SPAWN] Pohon ID: {item.TreeID} tidak ada di secondListTrees. Memunculkan versi default...");
                GameObject objectPohon = DatabaseManager.Instance.GetPrefabForTreeStage(item.typePlant, item.initialStage);

                // Tambahkan pengecekan null untuk keamanan
                if (objectPohon != null)
                {
                    Vector3 spawnPosition = new Vector3(item.position.x, item.position.y, 0);
                    GameObject plant = Instantiate(objectPohon, spawnPosition, Quaternion.identity);

                    // Atur ID dan nama pohon yang baru dibuat
                    plant.GetComponent<UniqueIdentifiableObject>().UniqueID = item.TreeID;
                    plant.name = item.TreeID; // .ToString() tidak perlu karena sudah string
                    plant.transform.SetParent(parentEnvironment);
                }
            }
            else
            {
               
                // Kita tidak melakukan apa-apa dan melewatkannya.
                Debug.Log($"[SKIP] Melewatkan pohon default '{item.TreeID}' karena datanya sudah ada di secondListTrees.");
            }
        }


       
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
