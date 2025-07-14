using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]


public class TrashManager : MonoBehaviour
{
    public List<Item> trashItems;        // Daftar item sampah yang ada
    public List<GameObject> trashLocations;  // Lokasi tempat sampah akan di-spawn
    public Transform trashTransform;      // Transform untuk parent objek sampah yang di-spawn
    public int randomCount;
    public int minimalRandomCount;


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

    public void HandleNewDay()
    {
        UpdateTrash();
    }
    void Start()
    {

    }

    // Fungsi untuk memeriksa dan memunculkan sampah sesuai lokasi
    public void UpdateTrash()
    {
        Debug.Log("update trash");
        randomCount = UnityEngine.Random.Range(minimalRandomCount, trashLocations.Count);
        //randomCount = 1;
        Debug.Log("randomcount : " + randomCount);

        for (int i = 0; i < randomCount; i++)
        {
            // Pilih sampah secara acak
            int randomTrash = UnityEngine.Random.Range(0, trashItems.Count);

            // Pilih lokasi sampah secara acak
            int randomLocationTrash = UnityEngine.Random.Range(0, trashLocations.Count);

            // Memastikan prefab sampah ada sebelum di-instantiate
            if (trashItems[randomTrash] != null)
            {
                // Menempatkan objek trash di lokasi yang acak
                Vector2 spawnLocation = trashLocations[randomLocationTrash].transform.position;

                //logika memasukan sampah ke tong sampah 
                TongSampahInteractable tongSampahInteractable = trashLocations[randomLocationTrash].GetComponent<TongSampahInteractable>();
                tongSampahInteractable.isFull = true;
                tongSampahInteractable.TongFull(trashItems[randomTrash]);



                // Log untuk memastikan objek muncul
                Debug.Log($"Menampilkan {trashItems[randomTrash].itemName}");
            }
            else
            {
                Debug.Log("item kosong");
            }
        }
    }
}
