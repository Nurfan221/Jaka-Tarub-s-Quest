using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSystem : MonoBehaviour
{

    
    public static StorageSystem Instance { get; private set; }
    //public StorageUI StorageUI { get; private set; }

    public List<StorageSaveData> environmentList = new List<StorageSaveData>();
    public StorageDatabaseSO storageDatabaseSO;
    public Transform parentEnvironment; // Parent untuk menyimpan storage yang ada di scene

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

    
    public void RegisterAllObject()
    {

        environmentList.Clear();

        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            Transform child = parentEnvironment.GetChild(i);
            StorageInteractable storageInteractable = child.GetComponent<StorageInteractable>();



            StorageSaveData data = new StorageSaveData
            {
                id = storageInteractable.uniqueID,
                storagePosition = storageInteractable.gameObject.transform.position,
                itemsInStorage = storageInteractable.storage,
            };

            environmentList.Add(data);
        }

        Debug.Log($"Total environment terdaftar: {environmentList.Count}");
    }

    public void AddStorageFromEnvironmentList()
    {
        Debug.Log("Memulai proses penambahan storage dari environmentList...");
        foreach (var storageData in environmentList)
        {
            //    (Diasumsikan Anda punya satu prefab generik untuk semua peti)
            GameObject storagePrefab = DatabaseManager.Instance.storageWorldPrefab;

            if (storagePrefab != null)
            {
                //    Pastikan di StorageSaveData Anda ada variabel Vector3 position.
                Vector3 spawnPosition = storageData.storagePosition;

                // Buat objek storage baru di posisi yang benar.
                GameObject newStorageGO = Instantiate(storagePrefab, spawnPosition, Quaternion.identity, StorageSystem.Instance.parentEnvironment);
                StorageInteractable storageInteractable = newStorageGO.GetComponent<StorageInteractable>();
                EnvironmentIdentity environmentIdentity = newStorageGO.GetComponent<EnvironmentIdentity>();
                storageInteractable.uniqueID = storageData.id;
                storageInteractable.storage = storageData.itemsInStorage;
                environmentIdentity.UniqueID = storageData.id;
                newStorageGO.name = storageData.id; // Ganti nama GameObject dengan ID unik dari data
            }
        }
    }

    [ContextMenu("Langkah 1: Daftarkan Semua Objek Anak ke List")]
    public void RegisterAllObjectsInEditor()
    {
        // Pastikan parentEnvironment diatur ke transform dari GameObject ini
        parentEnvironment = this.transform;

        // Panggil fungsi pendaftaran yang sudah ada
        RegisterAllObject();

        Debug.Log($"Proses pendaftaran di Editor selesai. {environmentList.Count} objek terdaftar di environmentList.");
    }

    [ContextMenu("Langkah 2: Pindahkan Data Jamur ke Database SO")]
    public void MigrateTreeDataToSO()
    {
        // Pengecekan Keamanan
        if (storageDatabaseSO == null)
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
        storageDatabaseSO.savedStorages.Clear();

        Debug.Log($"Memulai migrasi {environmentList.Count} data bunga ke {storageDatabaseSO.name}...");

        // Loop melalui setiap entri di environmentList
        foreach (StorageSaveData storageData in environmentList)
        {
            // Hanya proses jika objek adalah pohon (berdasarkan komponen atau nama)
            // Anda mungkin perlu menyesuaikan kondisi ini
            // Buat entri TreePlacementData baru
            StorageSaveData data = new StorageSaveData
            {
                id = storageData.id,
                storagePosition = storageData.storagePosition,
                itemsInStorage = storageData.itemsInStorage,
            };

            // Tambahkan data baru ke dalam list di ScriptableObject
            storageDatabaseSO.savedStorages.Add(data);
        }

        // Tandai aset ScriptableObject sebagai "kotor" agar Unity menyimpan perubahan
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(storageDatabaseSO);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"Migrasi selesai! {storageDatabaseSO.savedStorages.Count} data storage berhasil dipindahkan.");
    }


}
