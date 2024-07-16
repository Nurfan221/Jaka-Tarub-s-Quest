using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageSystem : MonoBehaviour
{
    public static StorageSystem Instance;

    public List<StorageInteractable> storages;

    private void Awake()
    {
        Instance = this;
    }

    public List<StorageInteractable> GetStorages()
    {
        return storages;
    }
}
