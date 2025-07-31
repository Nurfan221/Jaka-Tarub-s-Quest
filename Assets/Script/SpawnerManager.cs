using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance { get; private set; }
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
    [Serializable]
    public class Spawner
    {
        public string nameSpawner;
        public int idSpawner;
        public GameObject[] spawner;
    }
    public Spawner[] spawner;
    public List<GameObject> spawnerListQuestActive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleNewDay()
    {
        float luck = TimeManager.Instance.GetDayLuck();
        SetSpawnerActive(luck);
    }
    public void SetSpawnerActive(float random)
    {
        // Loop untuk menonaktifkan semua spawner
        for (int i = 0; i < spawner[0].spawner.Length; i++)
        {
            // Ambil referensi AnimalSpawner dari setiap spawner yang ada
            AnimalSpawner animalSpawner = spawner[0].spawner[i].GetComponent<AnimalSpawner>();

            // Hapus musuh lama dari daftar enemies sebelum spawn baru
            animalSpawner.DeleteEnemiesFromArray();

            // Nonaktifkan spawner
            spawner[0].spawner[i].gameObject.SetActive(false);
        }

        if (spawner[0] != null && spawner[0].spawner.Length > 0)
        {
            // Loop sebanyak "random" untuk memilih spawner yang aktif
            for (int i = 0; i < Mathf.FloorToInt(random); i++)
            {
                // Menghasilkan angka acak dalam rentang jumlah elemen spawner[0].spawner
                int randomIntSpawner = UnityEngine.Random.Range(0, spawner[0].spawner.Length);

                // Aktifkan objek yang sesuai dengan index acak
                spawner[0].spawner[randomIntSpawner].gameObject.SetActive(true);

                // Ambil referensi AnimalSpawner dari spawner yang baru saja diaktifkan
                AnimalSpawner animalSpawner = spawner[0].spawner[randomIntSpawner].GetComponent<AnimalSpawner>();

                // Spawn hewan baru dan hewan spesial
                animalSpawner.SpawnAnimal();
                //if (questManager.chapter1IsDone)
                //{
                //    animalSpawner.SpawnAnimalSpesial();
                //}
            }
        }

        SetSpawnerBanditActive(random);
    }

    public void SetSpawnerBanditActive(float random)
    {
        // Loop pertama untuk menonaktifkan semua spawner
        for (int i = 0; i < spawner[1].spawner.Length; i++)
        {
            spawner[1].spawner[i].gameObject.SetActive(false);
        }

        if (spawner[1] != null && spawner[1].spawner.Length > 0)
        {

            List<GameObject> availableSpawners = new List<GameObject>();
            for (int i = 0; i < spawner[1].spawner.Length; i++)
            {
                LocationConfiguration locationConfiguration = spawner[1].spawner[i].GetComponent<LocationConfiguration>();
                // Tambahkan spawner ke list jika nama lokasinya BUKAN "SpawnerQuest1_6"
                if (locationConfiguration != null && locationConfiguration.locationName != "SpawnerQuest1_6")
                {
                    availableSpawners.Add(spawner[1].spawner[i]);
                }
            }

            // Jika tidak ada spawner yang tersedia, hentikan
            if (availableSpawners.Count == 0)
            {
                Debug.LogWarning("Tidak ada spawner bandit yang tersedia.");
                return;
            }

            // Tentukan jumlah spawner yang akan diaktifkan
            int spawnerToActivateCount = 0;
            float adjustedRandom = Mathf.Lerp(0.5f, 3f, 1 - random);

            if (adjustedRandom >= 0 && adjustedRandom < 1f)
            {
                spawnerToActivateCount = 7;
            }
            else if (adjustedRandom >= 1 && adjustedRandom < 2)
            {
                spawnerToActivateCount = 4;
            }
            else if (adjustedRandom >= 2)
            {
                spawnerToActivateCount = 2;
            }

            // Pastikan jumlah spawner yang diaktifkan tidak melebihi yang tersedia
            spawnerToActivateCount = Mathf.Min(spawnerToActivateCount, availableSpawners.Count);

            //Lakukan loop sebanyak spawner yang harus diaktifkan
            for (int i = 0; i < spawnerToActivateCount; i++)
            {
                // Pilih indeks acak dari list spawner yang tersedia
                int randomIndex = UnityEngine.Random.Range(0, availableSpawners.Count);

                // Aktifkan spawner yang terpilih
                availableSpawners[randomIndex].gameObject.SetActive(true);

                // Hapus spawner yang sudah diaktifkan dari list agar tidak terpilih lagi
                availableSpawners.RemoveAt(randomIndex);
            }
        }

        foreach (var item in spawnerListQuestActive)
        {
            item.gameObject.SetActive(true);
        }
    }
    public void HandleSpawnerActive(string nameSpawner)
    {

        for (int i = 0; i < spawner[1].spawner.Length; i++)

        {

            if (spawner[1].spawner[i].name == nameSpawner)

            {
                spawnerListQuestActive.Add(spawner[1].spawner[i].gameObject);
                Debug.Log("lokasi SpawnerQuest1_6 ditemukan");
                spawner[1].spawner[i].gameObject.SetActive(true);

            }
            else

            {

                Debug.Log("Lokasi SpawnerQuest1_6 tidak ditemukan");

            }

        }
    }


}
