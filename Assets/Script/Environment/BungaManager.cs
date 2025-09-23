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




public class BungaManager : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    public EnvironmentDatabaseSO bungaDatabaseSO;


    public Transform parentEnvironment;
    public List<EnvironmentSaveData> environmentList = new List<EnvironmentSaveData>();
 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    public void RegisterAllObject()
    {
        environmentList.Clear();

        for (int i = 0; i < parentEnvironment.childCount; i++)
        {
            Transform child = parentEnvironment.GetChild(i);
            EnvironmentBehavior envBehavior = child.GetComponent<EnvironmentBehavior>();



            EnvironmentSaveData data = new EnvironmentSaveData
            {
               environmentId = envBehavior.UniqueID,
               typePlant = envBehavior.typePlant,
               typeObject = envBehavior.typeObject,
               environmentPosition = child.position
            };

            environmentList.Add(data);
        }

        Debug.Log($"Total environment terdaftar: {environmentList.Count}");
    }

    public void RandomSpawnFlower()
    {
        if (parentEnvironment == null )
        {
            Debug.LogError("Parent Environment atau Spawn Container belum diatur di Inspector!");
            return;
        }
        if (environmentList.Count == 0)
        {
            Debug.LogWarning("Environment List masih kosong, tidak ada bunga yang bisa di-spawn.");
            return;
        }

        int randomCount = UnityEngine.Random.Range(10, 21); // Spawn 10 sampai 20 bunga
        Debug.Log($"Akan men-spawn {randomCount} bunga secara acak...");

        for (int i = 0; i < randomCount; i++)
        {
            // Dapatkan data bunga acak
            int randomIndex = UnityEngine.Random.Range(0, environmentList.Count);
            EnvironmentSaveData flowerData = environmentList[randomIndex];
            GameObject flowerObject = DatabaseManager.Instance.GetFlower(flowerData.typePlant);

            // Cari prefab bunga di dalam parentEnvironment

            // Cek apakah prefab ditemukan
            if (flowerObject != null)
            {
                GameObject newFlower = Instantiate(flowerObject.gameObject, flowerData.environmentPosition, Quaternion.identity);

                // Jadikan anak dari spawnContainer agar hirarki tetap rapi
                newFlower.transform.SetParent(parentEnvironment);
                EnvironmentBehavior envBehavior = newFlower.GetComponent<EnvironmentBehavior>();
                envBehavior.ForceGenerateUniqueID();
                newFlower.name = envBehavior.UniqueID; // Ganti nama GameObject dengan uniqueID
                Debug.Log($"SUKSES: Flower '{envBehavior.typePlant}' di-spawn di posisi {flowerData.environmentPosition} dengan ID '{envBehavior.UniqueID}'.");

            }
            else
            {
                Debug.LogError($"GAGAL: Tidak dapat menemukan prefab anak bernama '{flowerData.environmentId}' di dalam '{parentEnvironment.name}'!");
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

    [ContextMenu("Langkah 2: Pindahkan Data Pohon ke Database SO")]
    public void MigrateTreeDataToSO()
    {
        // Pengecekan Keamanan
        if (bungaDatabaseSO == null)
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
        bungaDatabaseSO.FlowerSaveData.Clear();

        Debug.Log($"Memulai migrasi {environmentList.Count} data bunga ke {bungaDatabaseSO.name}...");

        // Loop melalui setiap entri di environmentList
        foreach (EnvironmentSaveData flowerData in environmentList)
        {
            // Hanya proses jika objek adalah pohon (berdasarkan komponen atau nama)
            // Anda mungkin perlu menyesuaikan kondisi ini
            // Buat entri TreePlacementData baru
            EnvironmentSaveData data = new EnvironmentSaveData
            {
                environmentId = flowerData.environmentId,
                typePlant = flowerData.typePlant,
                typeObject = flowerData.typeObject,
                environmentPosition = flowerData.environmentPosition
            };

            // Tambahkan data baru ke dalam list di ScriptableObject
            bungaDatabaseSO.FlowerSaveData.Add(data);
        }

        // Tandai aset ScriptableObject sebagai "kotor" agar Unity menyimpan perubahan
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(bungaDatabaseSO);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"Migrasi selesai! {bungaDatabaseSO.FlowerSaveData.Count} data bunga berhasil dipindahkan.");
    }
}
