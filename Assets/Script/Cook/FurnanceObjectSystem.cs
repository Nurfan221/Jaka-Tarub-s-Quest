using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FurnanceObjectSystem : MonoBehaviour
{
    public static FurnanceObjectSystem Instance { get; private set; }

    public List<FurnanceSaveData> environmentList = new List<FurnanceSaveData>();
    public Transform parentEnvironment; // Tempat menyimpan semua tungku aktif di scene
  

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


    }


    public void RegisterAllObject()
    {
        environmentList.Clear();

        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            Transform child = parentEnvironment.GetChild(i);
            CookInteractable cook = child.GetComponent<CookInteractable>();
            if (cook == null) continue; // lewati jika bukan tungku

            FurnanceSaveData data = new FurnanceSaveData
            {
                id = cook.interactableUniqueID.UniqueID,
                itemCook = cook.itemCook,
                fuelCook = cook.fuelCook,
                itemResult = cook.itemResult,
                quantityFuel = cook.quantityFuel,
                furnancePosition = child.position,
                typeKompor = cook.typeKompor,
            };
            Debug.Log("menginputkan furnance " + cook.interactableUniqueID.UniqueID);
            environmentList.Add(data);
        }

        Debug.Log($"[FurnanceSystem] Terdaftar {environmentList.Count} tungku di environmentList.");
    }


    public void AddStorageFromEnvironmentList()
    {
        Debug.Log("[FurnanceSystem] Sinkronisasi data tungku dari environmentList...");

        foreach (var furnanceData in environmentList)
        {
            Transform existing = parentEnvironment.Find(furnanceData.id);

            if (existing != null)
            {
                CookInteractable furnance = existing.GetComponent<CookInteractable>();
                if (furnance == null) continue;

                furnance.interactableUniqueID.UniqueID = furnanceData.id;
                furnance.itemCook = furnanceData.itemCook;
                furnance.fuelCook = furnanceData.fuelCook;
                furnance.itemResult = furnanceData.itemResult;
                furnance.quantityFuel = furnanceData.quantityFuel;
                existing.position = furnanceData.furnancePosition;

                Debug.Log($"[FurnanceSystem] Tungku {furnanceData.id} diperbarui di {furnanceData.furnancePosition}.");
            }
            else
            {
                GameObject prefab;
                switch (furnanceData.typeKompor)
                {
                    case typeKompor.furnance:
                        Debug.Log("type kompor furnance");
                        prefab = DatabaseManager.Instance.furnanceWorldPrefab;
                        break;
                    case typeKompor.kompor:
                        Debug.Log("type kompor kompor");
                        prefab = DatabaseManager.Instance.komporWorldPrefab;
                        break;
                    case typeKompor.apiUnggun:
                        Debug.Log("type kompr api unggun");
                        prefab = DatabaseManager.Instance.apiUnggun;
                        break;
                    default:
                        Debug.Log("kompor tidak terdefinisi");
                        prefab = DatabaseManager.Instance.furnanceWorldPrefab;
                        break;
                }
            
                if (prefab == null)
                {
                    Debug.LogError("[FurnanceSystem] Prefab tungku tidak ditemukan!");
                    continue;
                }

                //  spawn dulu tanpa parent agar posisinya world space
                GameObject newFurnance = Instantiate(prefab, furnanceData.furnancePosition, Quaternion.identity);
                newFurnance.transform.SetParent(parentEnvironment, true);
                Vector3 pos = newFurnance.transform.position;
                pos.z = -1f;
                newFurnance.transform.position = pos;


                CookInteractable furnance = newFurnance.GetComponent<CookInteractable>();
                InteractableUniqueID envId = newFurnance.GetComponent<InteractableUniqueID>();

                if (furnance == null || envId == null)
                {
                    Debug.LogError("[FurnanceSystem] Prefab tungku tidak memiliki komponen CookInteractable atau InteractableUniqueID!");
                    continue;
                }

                envId.UniqueID = furnanceData.id;
                furnance.itemCook = furnanceData.itemCook;
                furnance.fuelCook = furnanceData.fuelCook;
                furnance.itemResult = furnanceData.itemResult;
                furnance.quantityFuel = furnanceData.quantityFuel;

                newFurnance.name = furnanceData.id;
                Debug.Log($"[FurnanceSystem] Tungku baru {furnanceData.id} dibuat di posisi dunia {furnanceData.furnancePosition}.");
            }
        }

        RemoveDeletedStoragesFromScene();
    }



    private void RemoveDeletedStoragesFromScene()
    {
        HashSet<string> validIDs = new HashSet<string>(environmentList.Select(s => s.id));
        List<Transform> toRemove = new List<Transform>();

        foreach (Transform child in parentEnvironment)
        {
            InteractableUniqueID envId = child.GetComponent<InteractableUniqueID>();
            if (envId != null && !validIDs.Contains(envId.UniqueID))
                toRemove.Add(child);
        }

        foreach (var target in toRemove)
        {
            Debug.Log($"[FurnanceSystem] Menghapus tungku {target.name} (tidak ada di save data).");
            Destroy(target.gameObject);
        }
    }

    public void RemoveFurnanceByID(string id)
    {
        var target = environmentList.FirstOrDefault(s => s.id == id);
        if (target != null)
        {
            environmentList.Remove(target);
            Debug.Log($"[FurnanceSystem] Tungku {id} dihapus dari environmentList.");
        }

        Transform obj = parentEnvironment.Find(id);
        if (obj != null)
        {
            Destroy(obj.gameObject);
            Debug.Log($"[FurnanceSystem] Tungku {id} dihapus dari dunia.");
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

    // =====================================================
    //  EDITOR TOOLS
    // =====================================================
    //    [ContextMenu("Langkah 1: Daftarkan Semua Objek Anak ke List")]
    //    public void RegisterAllObjectsInEditor()
    //    {
    //        parentEnvironment = transform;
    //        RegisterAllObject();
    //        Debug.Log($"[Editor] {environmentList.Count} tungku berhasil diregistrasi ke environmentList.");
    //    }

    //    [ContextMenu("Langkah 2: Pindahkan Data ke Database SO")]
    //    public void MigrateDataToSO()
    //    {
    //        if (storageDatabaseSO == null)
    //        {
    //            Debug.LogError("storageDatabaseSO belum diatur!");
    //            return;
    //        }

    //        storageDatabaseSO.savedStorages.Clear();

    //        foreach (var data in environmentList)
    //            storageDatabaseSO.savedStorages.Add(data);

    //#if UNITY_EDITOR
    //        UnityEditor.EditorUtility.SetDirty(storageDatabaseSO);
    //        UnityEditor.AssetDatabase.SaveAssets();
    //#endif

    //        Debug.Log($"[Editor] Migrasi selesai. Total {storageDatabaseSO.savedStorages.Count} tungku disimpan ke ScriptableObject.");
    //    }
}
