using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif





public class BatuManager : MonoBehaviour
{
    [Header("Referensi Database")]
    public WorldStoneDatabaseSO stoneDatabase;


    [Header("Pengaturan Spawning")]
    [Tooltip("Daftar semua kemungkinan lokasi spawn batu.")]
    public List<Transform> spawnPoints;

    [Tooltip("Jumlah maksimal batu yang akan muncul dalam satu siklus.")]
    public int maxStonesToSpawn = 30;

    private void Start()
    {
        stoneDatabase = DatabaseManager.Instance.worldStoneDatabase;
    }

    private void OnEnable()
    {
        TimeManager.OnDayChanged += NewDay;
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged += NewDay;

    }
    // Fungsi utama yang dipanggil untuk memulai proses spawning
    // Anda bisa memanggil ini dari GameManager setiap pagi, misalnya.

    public void NewDay()
    {
        float luck = TimeManager.Instance.GetDayLuck();
        SpawnStonesBasedOnLuck(luck);
    }
    public void SpawnStonesBasedOnLuck(float playerLuck)
    {
        Debug.Log("hari yang baru ayoo kita munculkan batu");
        if (stoneDatabase == null || spawnPoints.Count == 0)
        {
            Debug.LogError("Database atau Spawn Points belum di-set di StoneManager!");
            return;
        }

        List<TemplateStoneObject> eligibleStones = GetEligibleStones(playerLuck);

        if (eligibleStones.Count == 0)
        {
            Debug.LogWarning("Tidak ada batu yang memenuhi syarat untuk spawn dengan luck: " + playerLuck);
            return;
        }

        // Gunakan lokasi spawn yang tersedia untuk siklus ini
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        // Jumlah batu yang akan di-spawn adalah nilai terkecil dari:
        // maxStones, jumlah batu yang valid, atau jumlah lokasi yang ada.
        int amountToSpawn = Mathf.Min(maxStonesToSpawn, eligibleStones.Count, availableSpawnPoints.Count);

        for (int i = 0; i < amountToSpawn; i++)
        {
            // Pilih batu acak dari kolam yang valid
            TemplateStoneObject chosenStoneTemplate = eligibleStones[UnityEngine.Random.Range(0, eligibleStones.Count)];

            // Pilih lokasi acak dari yang masih tersedia
            int spawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            Transform spawnLocation = availableSpawnPoints[spawnPointIndex];

            // Hapus lokasi agar tidak dipakai lagi
            availableSpawnPoints.RemoveAt(spawnPointIndex);

            // Ciptakan objek batu di dunia game!
            Instantiate(chosenStoneTemplate.stoneObject, spawnLocation.position, spawnLocation.rotation);
        }

        Debug.Log($"Spawning selesai. Total {amountToSpawn} batu telah dimunculkan.");
    }

    // Fungsi helper untuk memfilter batu berdasarkan luck
    private List<TemplateStoneObject> GetEligibleStones(float luck)
    {
        // Menggunakan LINQ agar kode lebih bersih dan singkat
        switch (luck)
        {
            case 1:
                return stoneDatabase.templateStoneObject
                    .Where(s => s.hardnessLevel == EnvironmentHardnessLevel.Soft).ToList();
            case 2:
                return stoneDatabase.templateStoneObject
                    .Where(s => s.hardnessLevel == EnvironmentHardnessLevel.Soft ||
                                s.hardnessLevel == EnvironmentHardnessLevel.Medium).ToList();
            case 3:
                return stoneDatabase.templateStoneObject
                    .Where(s => s.hardnessLevel == EnvironmentHardnessLevel.Soft ||
                                s.hardnessLevel == EnvironmentHardnessLevel.Medium ||
                                s.hardnessLevel == EnvironmentHardnessLevel.Hard).ToList();
            default:
                return new List<TemplateStoneObject>(stoneDatabase.templateStoneObject);
        }
    }



}
