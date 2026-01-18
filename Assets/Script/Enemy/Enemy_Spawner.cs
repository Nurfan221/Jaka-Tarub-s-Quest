using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawner : UniqueIdentifiableObject
{
    [Header("Spawner Settings")]
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;
    public ArahObject arahObject;
    public EnvironmentType environmentType;

    public bool CanSpawn = true;
    public bool drawSpawnRadius;
    public float spawnRadius = 10f;
    public float spawnCD = 2f;
    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)
    float spawnTimer;

    public int spawnCount;
    public int maxSpawnCount = 5;
    public GameObject enemyPrefab;
    public List<GameObject> enemies;
    public bool canOpenStorage = false;
    public bool isQuestSpawner = false;
    public StorageInteractable storageEnemies;
    Queue<GameObject> objectPool = new Queue<GameObject>();
    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return typeObject.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return hardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        if (typePlant == TypePlant.None && arahObject != ArahObject.None)
        {
            return arahObject.ToString();
        }
        else
        {
            return typePlant.ToString();
        }

    }

    public override string GetVariantName()
    {
        return environmentType.ToString();

    }

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        enemyPrefab = DatabaseManager.Instance.EnemyWorldPrefab;
        foreach (Transform child in transform)
        {
            Enemy_Bandit enemy_Bandit = child.GetComponent<Enemy_Bandit>();
            if (enemy_Bandit != null)
            {
                Destroy(enemy_Bandit.gameObject);

            }
        }

        // Pastikan list referensi juga kosong
        enemies.Clear();
        for (int i = 0; i < spawnCount; i++)
        {
            if (CanSpawn)
                SpawnEnemy();
        }




    }

    // Update is called once per frame
    void Update()
    {
        if (spawnCount < maxSpawnCount && CanSpawn)

        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnCD)
            {
                spawnTimer = 0;
                SpawnEnemy();
                spawnCount++;
            }
        }
    }

    void SpawnEnemy()
    {

        GameObject newEnemy = Instantiate(enemyPrefab, transform);
        newEnemy.transform.localPosition = GetSpawnPosition();
        //newEnemy.GetComponent<Enemy_Health>().theSpawner = this;
        newEnemy.gameObject.SetActive(true);
        Enemy_Bandit enemy_Bandit = newEnemy.GetComponent<Enemy_Bandit>();
        FootstepController footstep = enemy_Bandit.GetComponent<FootstepController>();

        if (footstep != null)
        {
            footstep.tilemaps = PlayerUI.Instance.tilemapLayerPlayer;
        }else
        {
            Debug.LogError("FootstepController component not found on Sprite child of Enemy_Bandit.");
        }
        if (enemy_Bandit != null)
        {
            enemy_Bandit.spawnerReference = this.gameObject;
        }

        enemies.Add(newEnemy);
    }

    // Helper function returns randomized position inside spawnRadius
    Vector2 GetSpawnPosition()
    {
        Vector2 spawnPosition;
        spawnPosition.x = Random.Range(-spawnRadius / 2, spawnRadius / 2);
        spawnPosition.y = Random.Range(-spawnRadius / 2, spawnRadius / 2);

        return spawnPosition;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cek apakah yang keluar itu adalah Enemy?
        if (other.CompareTag("Bandit"))
        {
            // Cek apakah enemy ini milik spawner ini (ada di list?)
            if (enemies.Contains(other.gameObject))
            {
                Debug.Log($"{other.name} kabur keluar area spawner!");

                // Hapus dari list atau hancurkan
               Enemy_Bandit banditScript = other.GetComponent<Enemy_Bandit>();
                if (banditScript != null && !banditScript.isReturning)
                {
                    Debug.Log($"{other.name} kabur! Memaksa pulang.");

                    // Panggil StartCoroutine DARI script si Bandit
                    banditScript.StartCoroutine(banditScript.BackToSpawner(transform.position));
                }
            }
        }
    }
    public void RemoveEnemyFromList(GameObject enemy)
    {
        // Optimasi: Remove mengembalikan true/false, jadi tidak perlu cek Contains dulu
        if (enemies.Remove(enemy))
        {
            // Jika berhasil dihapus, cek apakah habis
            if (enemies.Count == 0)
            {
                // Panggil Coroutine untuk urutan kemenangan yang rapi
                StartCoroutine(VictorySequence());
            }
        }
    }

    private IEnumerator VictorySequence()
    {
        SoundEffect getSound = SoundManager.Instance.GetSfx(SoundName.Victory);

       
        SoundManager.Instance.PlayMusic(getSound.clip, 0);

        if (storageEnemies != null)
        {
            storageEnemies.UnlockStorage();
        }

        yield return new WaitForSeconds(getSound.clip.length + 0.5f);

        SoundManager.Instance.CheckGameplayMusic(ClockManager.Instance.isIndoors, 1.0f); // Fade in 1 detik biar halus

        spawnCount = 0;

        

        //gameObject.SetActive(false);
    }
#if UNITY_EDITOR
    #region DEBUG
    private void OnDrawGizmos()
    {
        // Jika verticalOffset negatif, titik ini akan turun.
        Vector3 centerPoint = transform.position + new Vector3(0, verticalOffset, 0);
        if (drawSpawnRadius)
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Gambar Lingkaran Deteksi (Sensor Mata) - WARNA HIJAU
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint, spawnRadius);
    }
    #endregion
#endif
}
