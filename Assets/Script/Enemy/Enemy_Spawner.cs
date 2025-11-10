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
    public float spawnRadius = 2f;
    public float spawnCD = 2f;
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
        if (enemy_Bandit != null)
        {
            enemy_Bandit.spawner = this.gameObject;
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

    public void RemoveEnemyFromList(GameObject enemy)
    {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);

        Item itemDrop = ItemPool.Instance.GetItemWithQuality("PakaianBandit", ItemQuality.Normal);
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        if (enemies.Count == 0)
        {
            //ItemPool.Instance.DropItem(itemDrop.itemName,itemDrop.health, itemDrop.quality, transform.position + offset);
            //spawnCount = 0;
            //gameObject.SetActive(false);

            storageEnemies.UnlockStorage();
        }

    }
#if UNITY_EDITOR
    #region DEBUG
    private void OnDrawGizmos()
    {
        if (drawSpawnRadius)
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    #endregion
#endif
}
