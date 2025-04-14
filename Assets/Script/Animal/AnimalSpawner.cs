using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] SpawnerManager spawnerManager;
    [SerializeField] TimeManager timeManager;
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

    public List<GameObject> enemies = new List<GameObject>(); // Daftar musuh/hewan yang sudah di-spawn

    private void Start()
    {
        DeleteEnemiesFromArray();

        // Validasi jika animalPools kosong
        if (animalPools.Count == 0)
        {
            Debug.LogError("AnimalPools kosong! Pastikan untuk menambahkan data di Inspector.");
            return;
        }

        // Daftar ke event OnHourChanged di TimeManager
       


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

    public void SpawnAnimal()
    {
        if (enemies.Count >= maxSpawnCount) return;  // Pastikan jumlah hewan tidak melebihi maxSpawnCount

        TimeManager.OnHourChanged += UpdateSpawnCategory;

        int spawnAnimal = Random.Range(0, maxSpawnCount - enemies.Count); // Membatasi spawn hanya untuk sisa slot yang kurang
        for (int i = 0; i < spawnAnimal; i++)
        {
            if (CanSpawn)
            {
                // Dapatkan prefab berdasarkan kategori aktif
                GameObject prefabToSpawn = GetRandomAnimalFromCategory(currentCategory);
                if (prefabToSpawn == null)
                {
                    // Debug.LogWarning("Tidak ada hewan yang tersedia untuk kategori ini.");
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
            }
        }
    }


    public void SpawnAnimalSpesial()
    {
        float spawnAnimalSpesial = Random.Range(0f, 1f);
        float spawnChance = Mathf.Clamp(1f - (timeManager.dailyLuck * 0.2f), 0f, 1f);


        // Cek apakah spawnAnimalSpesial lebih kecil dari peluang yang disesuaikan dengan luck
        if (spawnAnimalSpesial < spawnChance)
        {
            Debug.Log("Munculkan hewan spesial sebanyak ");
            // Spawn hewan di posisi yang valid
            GameObject newAnimalSpesial = Instantiate(currentAnimalSpesial);
            newAnimalSpesial.transform.position = GetSpawnPosition();
            newAnimalSpesial.transform.localScale = Vector3.one;  // Pastikan skala 1,1,1
            newAnimalSpesial.transform.parent = transform;        // Atur parent setelahnya

            // Atur skala prefab ke 1 untuk memastikan ukurannya sesuai dengan prefab aslinya
            newAnimalSpesial.transform.localScale = Vector3.one;
            enemies.Add(newAnimalSpesial);
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

    public void DeleteEnemiesFromArray()
    {
        foreach (var animals in enemies)
        {
            Destroy(animals);
        }
        enemies.Clear();
    }
}
