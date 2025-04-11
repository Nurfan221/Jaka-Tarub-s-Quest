using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public enum SpawnCategory
    {
        None,
        Siang,
        Malam
    }

    [System.Serializable]
    public class AnimalPool
    {
        public SpawnCategory category; // Kategori spawn
        public GameObject[] animals;   // Array prefab hewan untuk kategori ini
        public GameObject animalsSpesial;
    }

    public List<AnimalPool> animalPools = new List<AnimalPool>();

    public bool CanSpawn = true;
    [SerializeField] private Collider2D spawnArea; // Area spawn menggunakan Collider2D
    [SerializeField] private float spawnCD = 2f;   // Cooldown spawn
    private float spawnTimer;
    public int maxSpawnCount = 2;                     // Jumlah hewan yang akan spawn
    private SpawnCategory currentCategory = SpawnCategory.None; // Kategori spawn yang aktif
    public GameObject currentAnimalSpesial;

    private List<GameObject> enemies = new List<GameObject>(); // Daftar musuh/hewan yang sudah di-spawn

    private void Start()
    {
        // Validasi jika animalPools kosong
        if (animalPools.Count == 0)
        {
            Debug.LogError("AnimalPools kosong! Pastikan untuk menambahkan data di Inspector.");
            return;
        }

        // Daftar ke event OnHourChanged di TimeManager
        TimeManager.OnHourChanged += UpdateSpawnCategory;

        int spawnAnimal =  Random.Range(0, maxSpawnCount);
        // Spawn awal hewan
        for (int i = 0; i < spawnAnimal; i++)
        {
            if (CanSpawn)
                SpawnAnimal();
        }
    }

    private void OnDestroy()
    {
        // Hapus pendaftaran event untuk menghindari error
        TimeManager.OnHourChanged -= UpdateSpawnCategory;
    }

    private void Update()
    {
        if (enemies.Count < maxSpawnCount && CanSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnCD)
            {
                spawnTimer = 0;
                SpawnAnimal();
            }
        }
    }

    private void SpawnAnimal()
    {
        // Dapatkan prefab berdasarkan kategori aktif
        GameObject prefabToSpawn = GetRandomAnimalFromCategory(currentCategory);
        if (prefabToSpawn == null)
        {
            //Debug.LogWarning("Tidak ada hewan yang tersedia untuk kategori ini.");
            return;
        }

        // Spawn hewan di posisi yang valid
        GameObject newEnemy = Instantiate(prefabToSpawn);
        newEnemy.transform.position = GetSpawnPosition();
        newEnemy.transform.localScale = Vector3.one;  // Pastikan skala 1,1,1
        newEnemy.transform.parent = transform;        // Atur parent setelahnya


        // Atur skala prefab ke 1 untuk memastikan ukurannya sesuai dengan prefab aslinya
        newEnemy.transform.localScale = Vector3.one;

        enemies.Add(newEnemy);
        int spawnAnimalSpesial = Random.Range(0, 1);
        GameObject animalSpesial = GetRandomAnimalFromCategory(currentCategory);
        if (spawnAnimalSpesial < 0.8f)
        {
            // Spawn hewan di posisi yang valid
            GameObject newAnimalSpesial = Instantiate(currentAnimalSpesial);
            newAnimalSpesial.transform.position = GetSpawnPosition();
            newAnimalSpesial.transform.localScale = Vector3.one;  // Pastikan skala 1,1,1
            newAnimalSpesial.transform.parent = transform;        // Atur parent setelahnya
        }

    }

    private GameObject GetRandomAnimalFromCategory(SpawnCategory category)
    {
        foreach (var pool in animalPools)
        {
            if (pool.category == category)
            {
                if (pool.animals.Length > 0)
                {
                    int index = Random.Range(0, pool.animals.Length);
                    return pool.animals[index];
                }
                currentAnimalSpesial = pool.animalsSpesial;
            }
        }
        return null;
    }

    private void UpdateSpawnCategory(int hour)
    {
        if (hour >= 1 && hour < 18)
        {
            currentCategory = SpawnCategory.Siang;
        }
        else
        {
            currentCategory = SpawnCategory.Malam;
        }

        //Debug.Log($"Kategori spawn diperbarui ke: {currentCategory}");
    }

    private Vector2 GetSpawnPosition()
    {
        if (spawnArea == null)
        {
            //Debug.LogWarning("Spawn area tidak ditemukan, menggunakan posisi default.");
            return transform.position;
        }

        Bounds bounds = spawnArea.bounds;
        Vector2 randomPosition;
        int maxAttempts = 100; // Batas percobaan spawn
        int attempts = 0;

        do
        {
            randomPosition = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            attempts++;
        } while (!spawnArea.OverlapPoint(randomPosition) && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            //Debug.LogWarning("Gagal menemukan posisi spawn yang valid. Menggunakan posisi default.");
            return bounds.center;
        }

        return randomPosition;
    }
}
