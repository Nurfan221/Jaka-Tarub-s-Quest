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

    public int spawnCount;
    public int maxSpawnCount = 5;
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
        if (maxSpawnCount < spawnCount && CanSpawn)
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
            ItemPool.Instance.DropItem(itemDrop.name, transform.position + offset, itemDrop.prefabItem);
            spawnCount = 0;
            gameObject.SetActive(false);
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
