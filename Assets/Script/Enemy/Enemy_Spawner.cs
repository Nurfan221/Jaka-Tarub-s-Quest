using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawner : MonoBehaviour
{
    public bool CanSpawn = true;
    [SerializeField] bool drawSpawnRadius;
    [SerializeField] float spawnRadius = 2f;
    [SerializeField] float spawnCD = 2f;
    float spawnTimer;

    public int spawnCount = 2;
    [SerializeField] GameObject enemyPrefab;
    public List<GameObject> enemies;
    Queue<GameObject> objectPool = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            if (CanSpawn)
                SpawnEnemy();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enemies.Count < spawnCount && CanSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnCD)
            {
                spawnTimer = 0;
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(enemyPrefab, transform);
        newEnemy.transform.localPosition = GetSpawnPosition();
        //newEnemy.GetComponent<Enemy_Health>().theSpawner = this;

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
