using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSystem : MonoBehaviour
{

    
    public static StorageSystem Instance { get; private set; }
    public StorageUI StorageUI { get; private set; }

    public List<StorageInteractable> storages;

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

    public List<StorageInteractable> GetStorages()
    {
        return storages;
    }

    public void RegisterStorage(StorageUI storage)
    {
        this.StorageUI = storage;
        Debug.Log($"StorageController: Paket Storage '{storage.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterStorage(StorageUI storage)
    {
        if (this.StorageUI == storage)
        {
            this.StorageUI = null;
        }
    }

    public void OpenStorage(StorageInteractable storageToOpen)
    {
        // Cek apakah StorageUI sudah siap
        if (StorageUI != null)
        {
            // Perintahkan StorageUI untuk membuka dan menampilkan isi dari peti yang diklik.
            StorageUI.OpenStorage(storageToOpen);
        }
        else
        {
            Debug.LogError("StorageUI.Instance tidak ditemukan!");
        }
    }
}
