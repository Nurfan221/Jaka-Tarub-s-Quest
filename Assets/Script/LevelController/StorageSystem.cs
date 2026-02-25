using System.Collections.Generic;
using System.Linq;

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

    private void Start()
    {
        //RegisterAllObjectsInEditor();
        //AddStorageFromEnvironmentList();
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
                useArrawVisual = storageInteractable.useArrowVisual
            };

            environmentList.Add(data);
        }

        Debug.Log($"Total environment terdaftar: {environmentList.Count}");
    }

    public void AddStorageFromEnvironmentList()
    {
        Debug.Log("Sinkronisasi storage berdasarkan environmentList...");

        foreach (var storageData in environmentList)
        {
            Transform existing = parentEnvironment.Find(storageData.id);

            if (existing != null)
            {
                //  Update data storage yang sudah ada
                StorageInteractable storage = existing.GetComponent<StorageInteractable>();
                storage.storage = storageData.itemsInStorage;
                existing.position = storageData.storagePosition;
                storage.useArrowVisual = storageData.useArrawVisual;
                storage.UseArrawVisualfunction();
                Debug.Log($"Storage {storageData.id} diperbarui.");
            }
            else
            {
                //  Buat storage baru
                GameObject prefab = DatabaseManager.Instance.storageWorldPrefab;
                if (prefab == null)
                {
                    Debug.LogError("Prefab storageWorldPrefab tidak ditemukan!");
                    continue;
                }

                GameObject newStorage = Instantiate(prefab, storageData.storagePosition, Quaternion.identity, parentEnvironment);
                StorageInteractable storage = newStorage.GetComponent<StorageInteractable>();
                EnvironmentIdentity envId = newStorage.GetComponent<EnvironmentIdentity>();

                storage.uniqueID = storageData.id;
                storage.storage = storageData.itemsInStorage;
                envId.UniqueID = storageData.id;
                storage.useArrowVisual = storageData.useArrawVisual;
                newStorage.name = storageData.id;
                storage.UseArrawVisualfunction();
                Debug.Log($"Storage baru {storageData.id} dibuat di {storageData.storagePosition}.");
            }
        }

        // Hapus storage yang tidak ada lagi di data save
        RemoveDeletedStoragesFromScene();
    }
    private void RemoveDeletedStoragesFromScene()
    {
        // Buat daftar ID dari data save
        HashSet<string> validIDs = new HashSet<string>(environmentList.Select(s => s.id));

        // Cek setiap anak di parentEnvironment
        List<Transform> toRemove = new List<Transform>();

        foreach (Transform child in parentEnvironment)
        {
            EnvironmentIdentity envId = child.GetComponent<EnvironmentIdentity>();
            if (envId != null && !validIDs.Contains(envId.UniqueID))
            {
                // Tidak ada di data save — tandai untuk dihapus
                toRemove.Add(child);
            }
        }

        // Hapus objek yang tidak valid
        foreach (var target in toRemove)
        {
            Debug.Log($"Menghapus storage {target.name} karena tidak ada di data save.");
            Destroy(target.gameObject);
        }
    }
    //public void RemoveStorage()
    //{
    //    // 1️⃣ Drop semua item
    //    if (storage != null && storage.Count > 0)
    //    {
    //        foreach (var item in storage)
    //        {
    //            if (item != null)
    //            {
    //                GameObject dropped = Instantiate(
    //                    ItemPool.Instance.GetItemWithQuality(item.itemName, item.quality).prefabItem,
    //                    transform.position + Vector3.up * 0.5f,
    //                    Quaternion.identity
    //                );
    //                Debug.Log($"Menjatuhkan item: {item.itemName}");
    //            }
    //        }
    //    }

    //    // 2️⃣ Hapus storage dari environmentList
    //    StorageSystem.Instance.RemoveStorageByID(UniqueId);

    //    // 3️⃣ Hapus GameObject storage dari dunia
    //    Destroy(gameObject);

    //    Debug.Log($"Storage {uniqueID} dihapus dari dunia dan data save.");
    //}

    //public void RemoveStorageByID(string id)
    //{
    //    var target = environmentList.FirstOrDefault(s => s.id == id);
    //    if (target != null)
    //    {
    //        environmentList.Remove(target);
    //        Debug.Log($"Data storage {id} dihapus dari environmentList.");
    //    }
    //}

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
