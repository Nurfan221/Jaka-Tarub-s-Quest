using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif





public class BatuManager : MonoBehaviour
{


    public List<ResourceData> minerResource;
    [Header("Target Database (ScriptableObject)")]
    public WorldStoneDatabaseSO worldStoneDatabase;
    // Start is called before the first frame update

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
        RegisterAllStones();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleNewDay()
    {
        float dailyLuck = TimeManager.Instance.GetDayLuck();
        CheckLocationResource();
        UpdatePositionMiner(dailyLuck);


    }
    public void CheckLocationResource()
    {
        foreach (var item in minerResource)
        {
            foreach (var itemObject in item.resources)
            {
                if (itemObject.resourceObject != null)
                {
                    itemObject.location = (Vector2)itemObject.resourceObject.transform.position;

                }
            }
        }
    }


    public void UpdatePositionMiner(float luckValue)
    {
        if (minerResource == null) return;

        foreach (var itemObject in minerResource)
        {
            int totalItems = itemObject.resources.Length;

            // Menentukan berapa persen item yang akan dimunculkan
            float percentage = 0.3f; // default 30%
            if (luckValue == 2) percentage = 0.5f;
            else if (luckValue == 3) percentage = 0.7f;

            // Hitung jumlah item yang ingin dimunculkan berdasarkan luck
            int itemsToShow = Mathf.CeilToInt(totalItems * percentage);

            // Buat list acak dari resource
            List<Resource> shuffledList = new List<Resource>(itemObject.resources);
            ShuffleList(shuffledList); // Kita acak urutannya


            // Tampilkan hanya sejumlah itemToShow
            for (int i = 0; i < shuffledList.Count; i++)
            {
                var item = shuffledList[i];
                bool show = i < itemsToShow;
                item.resourceObject.SetActive(show);

                StoneBehavior stoneBehavior = item.resourceObject.GetComponent<StoneBehavior>();
                stoneBehavior.dayLuck = luckValue;
            }
        }
    }


    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    public void RegisterAllStones()
    {
        // Bersihkan dulu supaya tidak dobel
        minerResource.Clear();

        // Cari semua StoneBehavior di child objek
        StoneBehavior[] allStones = GetComponentsInChildren<StoneBehavior>(true);

        // Buat dictionary sementara untuk grouping berdasarkan TypeStone
        Dictionary<TypeStone, List<Resource>> groupedResources = new Dictionary<TypeStone, List<Resource>>();

        foreach (var stone in allStones)
        {
            if (stone == null) continue;

            // Buat Resource untuk setiap batu
            Resource res = new Resource
            {
                resourceObject = stone.gameObject,
                location = stone.transform.position,
                isHarvested = false
            };

            // Tambahkan ke group sesuai typeStone
            if (!groupedResources.ContainsKey(stone.stoneType))
            {
                groupedResources[stone.stoneType] = new List<Resource>();
            }
            groupedResources[stone.stoneType].Add(res);
        }

        // Konversi dictionary ke dalam minerResource
        foreach (var kvp in groupedResources)
        {
            ResourceData data = new ResourceData
            {
                nameResource = kvp.Key.ToString(),
                typeStone = kvp.Key,
                resources = kvp.Value.ToArray()
            };
            minerResource.Add(data);
        }

        Debug.Log($"RegisterAllStones: {allStones.Length} StoneBehavior berhasil dimasukkan ke minerResource.");
    }


    [ContextMenu("Langkah 2: Pindahkan Data Stone ke Database SO")]
    public void MigrateTreeDataToSO()
    {
        if (worldStoneDatabase == null)
        {
            Debug.LogError("[BatuManager] WorldStoneDatabaseSO belum di-assign di Inspector.");
            return;
        }

        if (minerResource == null)
        {
            Debug.LogWarning("[BatuManager] minerResource null. Tidak ada yang dipindahkan.");
            return;
        }

        // Pastikan list di SO siap dipakai
        if (worldStoneDatabase.stoneBehaviors == null)
            worldStoneDatabase.stoneBehaviors = new List<ResourceData>();

        // Hapus isi lama agar hasil migrasi bersih/terkini
        worldStoneDatabase.stoneBehaviors.Clear();

        // Copy data (shallow copy sudah cukup: referensi GameObject & array tetap sama)
        foreach (var src in minerResource)
        {
            if (src == null) continue;

            var copied = new ResourceData
            {
                nameResource = src.nameResource,
                typeStone = src.typeStone,
                resources = src.resources    // shallow copy referensi array sudah memadai
            };

            worldStoneDatabase.stoneBehaviors.Add(copied);
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(worldStoneDatabase);
        AssetDatabase.SaveAssets();
#endif

        Debug.Log($"[BatuManager] Migrasi selesai. Total group: {worldStoneDatabase.stoneBehaviors.Count}");
    }

}
