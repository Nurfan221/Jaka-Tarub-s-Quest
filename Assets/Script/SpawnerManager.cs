using System.Collections.Generic;
using UnityEngine;


public class SpawnerManager : MonoBehaviour
{

    public List<Enemy_Spawner> enemySpawnerList = new List<Enemy_Spawner>();
    public List<Item> itemStorageEnemy = new List<Item>();
    public int minItemStorage = 4;
    public int maxItemStorage =  6;



    private void Start()
    {

        AddSpawnerToList();
        HandleNewDay();
    }

    private void OnEnable()
    {
        TimeManager.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        TimeManager.OnDayChanged -= HandleNewDay;
    }

    public void HandleNewDay()
    {
        float luck = TimeManager.Instance.GetDayLuck();
        ActivateSpawnersBasedOnLuck(luck);
    }

    public void AddSpawnerToList()
    {
        // Ini sangat penting agar tidak ada duplikat setiap reload scene
        enemySpawnerList.Clear();

        foreach (Transform childTransform in transform)
        {
            Enemy_Spawner spawner = childTransform.GetComponent<Enemy_Spawner>();

            if (spawner != null)
            {
                enemySpawnerList.Add(spawner);
            }
        }
    }
    public void ActivateSpawnersBasedOnLuck(float luckValue)
    {
        // Ini adalah logika yang Anda berikan
        int activeSpawnerCount = 0;
        if (luckValue <= 1f)
            activeSpawnerCount = 6;
        else if (luckValue <= 2f)
            activeSpawnerCount = 5;
        else
            activeSpawnerCount = 3;

        // Matikan semua spawner terlebih dahulu agar kondisi selalu bersih
        foreach (Enemy_Spawner spawner in enemySpawnerList)
        {
            if (spawner != null)
            {
                spawner.gameObject.SetActive(false);
            }
        }


        // Buat list untuk menampung semua indeks (0, 1, 2, 3, ...)
        List<int> indexes = new List<int>();
        for (int i = 0; i < enemySpawnerList.Count; i++)
        {
            indexes.Add(i);
        }

        // Acak list indeks tersebut (Fisher-Yates Shuffle)
        for (int i = 0; i < indexes.Count; i++)
        {
            // Pilih satu indeks acak dari 'i' sampai akhir list
            int randomIndex = Random.Range(i, indexes.Count);

            // Tukar posisi
            int temp = indexes[i];
            indexes[i] = indexes[randomIndex];
            indexes[randomIndex] = temp;
        }

   
        // lebih sedikit dari activeSpawnerCount (misal: hanya ada 5 spawner)
        int countToActivate = Mathf.Min(activeSpawnerCount, enemySpawnerList.Count);

        Debug.Log($"[SpawnerManager] Berdasarkan Luck {luckValue}, akan mengaktifkan {countToActivate} spawner.");

        // Loop sebanyak jumlah yang ingin diaktifkan
        for (int i = 0; i < countToActivate; i++)
        {
            // Ambil "indeks pemenang" dari list yang sudah diacak
            int indexToActivate = indexes[i];

            // Aktifkan GameObject spawner di indeks tersebut
            if (enemySpawnerList[indexToActivate] != null)
            {
                enemySpawnerList[indexToActivate].gameObject.SetActive(true);
                Enemy_Spawner spawner = enemySpawnerList[indexToActivate];
                StorageInteractable storage = spawner.storageEnemies;

                if (storage != null)
                {
                    List<ItemData> newItems = AddItemForStorage();

                    // (Saya asumsikan list di StorageInteractable bernama 'storage')
                    storage.storage.AddRange(newItems);

                    Debug.Log($" [SpawnerManager] Menambahkan {newItems.Count} item ke {spawner.name}. Total item: {storage.storage.Count}");
                }
                else
                {
                    Debug.LogWarning($"[SpawnerManager] Spawner {spawner.name} tidak memiliki komponen StorageInteractable!");
                }
                Debug.Log($"[SpawnerManager] Spawner {enemySpawnerList[indexToActivate].name} diaktifkan.");
            }
        }
    }

    public List<ItemData> AddItemForStorage()
    {
        // DEBUG: Cek apakah master list item Anda valid
        if (itemStorageEnemy == null || itemStorageEnemy.Count == 0)
        {
            // Ini adalah Error, karena item PASTI GAGAL dibuat
            Debug.LogError("[AddItemForStorage] GAGAL! List 'itemStorageEnemy' (di Inspector) kosong. Tidak ada item yang bisa digenerate.");
            return new List<ItemData>(); // Kembalikan list kosong
        }

        // Tentukan jumlah item yang akan ditambahkan
        int randomCount = UnityEngine.Random.Range(minItemStorage, maxItemStorage + 1);

        // DEBUG: Cek jumlah item yang akan di-generate
        Debug.Log($"[AddItemForStorage] Akan meng-generate {randomCount} item (Min: {minItemStorage}, Max: {maxItemStorage}).");

        List<ItemData> generatedItems = new List<ItemData>();

        // DEBUG: Jika randomCount 0, beri tahu
        if (randomCount == 0)
        {
            Debug.LogWarning("[AddItemForStorage] randomCount adalah 0. Tidak ada item yang ditambahkan.");
            return generatedItems;
        }

        // Loop untuk menambahkan sejumlah item acak
        for (int i = 0; i < randomCount; i++)
        {
            // Pilih satu item acak dari daftar itemStorageEnemy
            int randomIndex = UnityEngine.Random.Range(0, itemStorageEnemy.Count);
            var baseItem = itemStorageEnemy[randomIndex];

            // DEBUG: Cek jika slot di master list ternyata kosong (null)
            if (baseItem == null)
            {
                Debug.LogWarning($"[AddItemForStorage] Peringatan! Item di 'itemStorageEnemy' index {randomIndex} ternyata null. Slot ini dilewati.");
                continue; // Lanjut ke loop berikutnya, jangan tambahkan item null
            }

            Debug.Log($"[AddItemForStorage] ...menghasilkan item ke-{i + 1}: {baseItem.itemName}");

            // Buat salinan ItemData baru berdasarkan template
            ItemData newItem = new ItemData
            {
                itemName = baseItem.itemName,
                itemHealth = baseItem.health,
                count = UnityEngine.Random.Range(1, 3),
                quality = ItemQuality.Normal,
            };

            generatedItems.Add(newItem);
        }

        Debug.Log($"[AddItemForStorage] Selesai. Total item di-generate: {generatedItems.Count}.");
        return generatedItems;
    }
}
