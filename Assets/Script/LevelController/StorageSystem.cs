using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSystem : MonoBehaviour
{

    
    public static StorageSystem Instance { get; private set; }
    //public StorageUI StorageUI { get; private set; }

    public List<StorageInteractable> storages = new List<StorageInteractable>();

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
    }

    [ContextMenu("Kosongkan Daftar Storage (Clear List)")]
    public void ClearStoragesList()
    {
        if (storages != null)
        {
            storages.Clear(); // Perintah ini akan menghapus semua isi dari list.
            Debug.Log("List 'storages' telah dikosongkan secara manual.");
        }
    }
}
