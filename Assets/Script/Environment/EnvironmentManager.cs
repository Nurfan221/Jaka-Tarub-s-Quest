using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using System.Linq;

[Serializable]
public class EnvironmentList
{
    public string prefabName;             // Nama prefab atau ID unik
    public Vector3 objectPosition;        // Posisi saat spawn
    public GrowthTree initialStage; // Tahap tumbuh saat pertama kali muncul
    public bool isGrowing;
}


public class EnvironmentManager : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    public WorldTreeDatabaseSO targetTreeDatabase;
    public List<TreeBehavior> allActiveTrees = new List<TreeBehavior>();

    public Transform parentEnvironment;
    public List<EnvironmentList> environmentList = new List<EnvironmentList>();
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
        }else
        {
            RegisterAllObject();
        }

        parentEnvironment = gameObject.transform;
        targetTreeDatabase = DatabaseManager.Instance.worldTreeDatabase;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleNewDay()
    {
        float dayLuck = TimeManager.Instance.GetDayLuck();
        if(isJanganAcak)
        {
            //return;
        }else
        {
            SpawnFromEnvironmentList(dayLuck);
        }

        if (isKuburanManager)
        {
            UpdateKondisiKuburan();
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



            EnvironmentList data = new EnvironmentList
            {
                prefabName = treeBehavior.UniqueID,
                objectPosition = child.position,
                initialStage = treeBehavior.currentStage,
                isGrowing = true
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
            if (!environment.isGrowing)
            {
                float randomCountSpawn = UnityEngine.Random.Range(0f, 1f);
                if (randomCountSpawn > 0.5f)
                {
                    // Cek apakah posisi sudah terisi
                    if (!IsPositionOccupied(environment.objectPosition))
                    {
                        GameObject prefab = GetPrefabByName(environment.prefabName);
                        if (prefab != null)
                        {
                            GameObject obj = Instantiate(prefab, environment.objectPosition, Quaternion.identity, parentEnvironment);
                            TreeBehavior treeBehavior = obj.GetComponent<TreeBehavior>();
                            EnvironmentBehavior environmentBehavior = obj.GetComponent<EnvironmentBehavior>();
                            if (environmentBehavior != null)
                            {
                                environmentBehavior.plantsContainer = parentEnvironment;
                            }
                            else if (treeBehavior != null)
                            {
                                treeBehavior.plantsContainer = parentEnvironment;
                            }


                            obj.name = prefab.name;
                            environment.isGrowing = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Posisi terisi, tidak jadi spawn: " + environment.objectPosition);
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

    public void UpdateKondisiKuburan()
    {
        // Reset jumlah kuburan yang kotor di awal hari
        countKuburanKotor = 0;

        // Tentukan secara acak berapa banyak kuburan yang akan dikotori hari ini
        // Range (6, 11) akan menghasilkan angka 6 sampai 10.
        int jumlahKuburanYangAkanKotor = UnityEngine.Random.Range(6, 11);

        // Ambil salinan daftar gameObjectsList untuk menghindari duplikasi
        List<GameObject> tempKuburanList = new List<GameObject>(gameObjectsList);

        for (int i = 0; i < jumlahKuburanYangAkanKotor; i++)
        {
            // Pastikan masih ada kuburan yang bisa dikotori
            if (tempKuburanList.Count == 0)
            {
                Debug.LogWarning("Tidak ada lagi kuburan yang bisa dikotori.");
                break;
            }

            // Dapatkan indeks acak dan objek dari list sementara
            int randomIndex = UnityEngine.Random.Range(0, tempKuburanList.Count);
            GameObject kuburan = tempKuburanList[randomIndex];

            // Dapatkan komponen KuburanInteractable
            KuburanInteractable kuburanInteractable = kuburan.GetComponent<KuburanInteractable>();

            if (kuburanInteractable != null)
            {
                // Set kuburan menjadi kotor
                kuburanInteractable.kondisiKuburanKotor();
            }

            // Hapus kuburan yang sudah dipilih dari list sementara
            tempKuburanList.RemoveAt(randomIndex);
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
        foreach (EnvironmentList treeData in environmentList)
        {
            // Hanya proses jika objek adalah pohon (berdasarkan komponen atau nama)
            // Anda mungkin perlu menyesuaikan kondisi ini
            if (treeData.prefabName.Contains("Pohon") || treeData.prefabName.Contains("Tree"))
            {
               

                // Buat entri TreePlacementData baru
                TreePlacementData newPlacementData = new TreePlacementData
                {
                    
                    TreeID = treeData.prefabName,
                    position = treeData.objectPosition,
                    // Asumsi semua pohon yang terdaftar dimulai dari tahap Seed
                    initialStage = treeData.initialStage,

                };

                // Tambahkan data baru ke dalam list di ScriptableObject
                targetTreeDatabase.initialTreePlacements.Add(newPlacementData);
            }
        }

        // Tandai aset ScriptableObject sebagai "kotor" agar Unity menyimpan perubahan
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetTreeDatabase);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"Migrasi selesai! {targetTreeDatabase.initialTreePlacements.Count} data pohon berhasil dipindahkan.");
    }
}
