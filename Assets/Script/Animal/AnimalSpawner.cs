using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] SpawnerManager spawnerManager;
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
    public Vector2[] spawnPoint; // Titik spawn yang akan digunakan

    private float spawnTimer;
    public int maxSpawnCount;  // Jumlah hewan yang akan spawn
    public int spawnCount;
    public int minSpawnCount;
    private SpawnCategory currentCategory = SpawnCategory.None; // Kategori spawn yang aktif
    public GameObject currentAnimalSpesial;

    public List<GameObject> enemies = new List<GameObject>(); // Daftar musuh/hewan yang sudah di-spawn

    private void Start()
    {
        spawnCount = Random.Range(minSpawnCount, maxSpawnCount);
        // Hapus pendaftaran event di OnDestroy()
        TimeManager.OnHourChanged -= UpdateSpawnCategory;
        // Daftar event di sini agar hanya sekali
        TimeManager.OnHourChanged += UpdateSpawnCategory;

        // Pastikan enemies bersih di awal
        DeleteEnemiesFromArray();

        if (animalPools.Count == 0)
        {
            Debug.LogError("AnimalPools kosong! Pastikan untuk menambahkan data di Inspector.");
            return;
        }

        // Inisialisasi kategori awal
        UpdateSpawnCategory();
    }

    private void OnDestroy()
    {
        // Hapus pendaftaran event untuk menghindari error
        TimeManager.OnHourChanged -= UpdateSpawnCategory;
    }

    private void Update()
    {
        if (enemies.Count < spawnCount && CanSpawn)
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
        // Pastikan tidak ada pendaftaran event berulang di sini
        if (enemies.Count >= spawnCount) return;

        // Hitung berapa banyak hewan yang perlu di-spawn untuk mencapai maxSpawnCount
        //spawnCount = maxSpawnCount - enemies.Count;

        for (int i = 0; i < spawnCount; i++)
        {
            if (CanSpawn)
            {
                GameObject prefabToSpawn = GetRandomAnimalFromCategory(currentCategory);
                if (prefabToSpawn == null)
                {
                    Debug.LogWarning("Tidak ada hewan yang tersedia untuk kategori ini.");
                    return;
                }

                GameObject newEnemy = Instantiate(prefabToSpawn);
                newEnemy.transform.position = GetSpawnPosition();
                newEnemy.transform.parent = transform;
                newEnemy.transform.localScale = Vector3.one;

                enemies.Add(newEnemy);
            }
        }
    }


    public void SpawnAnimalSpesial()
    {
        float spawnAnimalSpesial = Random.Range(0f, 1f);
        float dayluck = TimeManager.Instance.GetDayLuck();
        float spawnChance = Mathf.Clamp(1f - (dayluck * 0.2f), 0f, 1f);


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

    private void UpdateSpawnCategory()
    {
        int hour = TimeManager.Instance.hour;
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
        int randomPositionPoint = Random.Range(0, spawnPoint.Length);
        return spawnPoint[randomPositionPoint];
    }

    // Fungsi tambahan untuk memeriksa apakah sebuah titik berada di dalam poligon


    public void DeleteEnemiesFromArray()
    {
        foreach (var animals in enemies)
        {
            Destroy(animals);
        }
        enemies.Clear();
    }
}
