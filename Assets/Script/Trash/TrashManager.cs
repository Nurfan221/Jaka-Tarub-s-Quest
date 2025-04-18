using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Trash
{
    public GameObject prefabItem;   // Prefab sampah
    public string trashName;        // Nama sampah (bisa digunakan untuk logging atau label)
    public int trashCount;          // Jumlah sampah yang ingin spawn
}

public class TrashManager : MonoBehaviour
{
    public List<Trash> trashItems;        // Daftar item sampah yang ada
    public List<Vector2> trashLocations;  // Lokasi tempat sampah akan di-spawn
    public Transform trashTransform;      // Transform untuk parent objek sampah yang di-spawn
    public int randomCount;
    public int minimalRandomCount;

    void Start()
    {

    }

    // Fungsi untuk memeriksa dan memunculkan sampah sesuai lokasi
    public void UpdateTrash()
    {
        randomCount = UnityEngine.Random.Range(minimalRandomCount, trashLocations.Count);

        for (int i = 0; i < randomCount; i++)
        {
            // Pilih sampah secara acak
            int randomTrash = UnityEngine.Random.Range(0, trashItems.Count);

            // Pilih lokasi sampah secara acak
            int randomLocationTrash = UnityEngine.Random.Range(0, trashLocations.Count);

            // Memastikan prefab sampah ada sebelum di-instantiate
            if (trashItems[randomTrash].prefabItem != null)
            {
                // Instantiate objek trash pada lokasi yang sesuai
                GameObject newTrashObject = Instantiate(trashItems[randomTrash].prefabItem);

                // Menambahkan objek ke dalam hierarki tertentu (misalnya trashTransform)
                newTrashObject.transform.SetParent(trashTransform);

                // Menampilkan objek trash jika belum aktif
                newTrashObject.SetActive(true);

                // Menempatkan objek trash di lokasi yang acak
                Vector2 spawnLocation = trashLocations[randomLocationTrash];
                newTrashObject.transform.position = new Vector3(spawnLocation.x, spawnLocation.y, 0);

                // Log untuk memastikan objek muncul
                Debug.Log($"Menampilkan {trashItems[randomTrash].trashName} di lokasi {spawnLocation}");
            }
        }
    }
}
