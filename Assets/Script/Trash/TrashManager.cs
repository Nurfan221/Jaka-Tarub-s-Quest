using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]


public class TrashManager : MonoBehaviour
{
    public SampahDatabaseSO sampahDatabaseSO;
    public List<ItemData> sampahList;
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
        GetTrash(TimeManager.Instance.dailyLuck);
        UpdateTrash();
    }
    void Start()
    {
        sampahDatabaseSO = DatabaseManager.Instance.sampahDatabase;
        HandleNewDay();
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
            int randomTrash = UnityEngine.Random.Range(0, sampahList.Count);

            // Pilih lokasi sampah secara acak
            int randomLocationTrash = UnityEngine.Random.Range(0, trashLocations.Count);

            // Memastikan prefab sampah ada sebelum di-instantiate
            if (sampahList[randomTrash] != null)
            {
                // Menempatkan objek trash di lokasi yang acak
                Vector2 spawnLocation = trashLocations[randomLocationTrash].transform.position;

                //logika memasukan sampah ke tong sampah 
                TongSampahInteractable tongSampahInteractable = trashLocations[randomLocationTrash].GetComponent<TongSampahInteractable>();
                tongSampahInteractable.isFull = true;
                //Item  item = ItemPool.Instance.GetItemWithQuality(sampahList[randomTrash].itemName, sampahList[randomTrash].quality);
                tongSampahInteractable.TongFull(sampahList[randomTrash]);



                // Log untuk memastikan objek muncul
                Debug.Log($"Menampilkan {sampahList[randomTrash].itemName}");
            }
            else
            {
                Debug.Log("item kosong");
            }
        }
    }

    public void GetTrash(float dailyLuck)
    {
        Debug.Log("Mencari sampah dengan luck harian: " + dailyLuck);
        // Selalu bersihkan list hasil dari hari sebelumnya
        sampahList.Clear();

        // Logika ini sudah benar dari kode Anda.
        List<ItemData> candidatePool = new List<ItemData>();

        var lowLuckGroup = sampahDatabaseSO.listSampah.FirstOrDefault(g => g.luckLevel == LuckLevel.Low);
        var mediumLuckGroup = sampahDatabaseSO.listSampah.FirstOrDefault(g => g.luckLevel == LuckLevel.Medium);
        var highLuckGroup = sampahDatabaseSO.listSampah.FirstOrDefault(g => g.luckLevel == LuckLevel.High);

        if (dailyLuck < 1) // HARI TIDAK BERUNTUNG
        {
            // Menggunakan nama properti yang benar: 'itemDropSampah'
            if (lowLuckGroup != null) candidatePool.AddRange(lowLuckGroup.itemDropSampah);
        }
        else if (dailyLuck < 3) // HARI NORMAL
        {
            if (lowLuckGroup != null) candidatePool.AddRange(lowLuckGroup.itemDropSampah);
            if (mediumLuckGroup != null) candidatePool.AddRange(mediumLuckGroup.itemDropSampah);
        }
        else // HARI SANGAT BERUNTUNG
        {
            if (lowLuckGroup != null) candidatePool.AddRange(lowLuckGroup.itemDropSampah);
            if (mediumLuckGroup != null) candidatePool.AddRange(mediumLuckGroup.itemDropSampah);
            if (highLuckGroup != null) candidatePool.AddRange(highLuckGroup.itemDropSampah);
        }

        // Jika tidak ada kandidat sama sekali, hentikan proses
        if (candidatePool.Count == 0)
        {
            Debug.Log("Tidak ada sampah yang ditemukan hari ini.");
            return;
        }

        //int maxItems = 0;
        //if (dailyLuck < 1) maxItems = 1; // Dapat 1 item
        //else if (dailyLuck < 3) maxItems = 3; // Dapat 1-3 item
        //else maxItems = 5; // Dapat 2-5 item

        //// Dapatkan jumlah acak dalam rentang yang ditentukan
        //int finalItemCount = UnityEngine.Random.Range(1, maxItems + 1);

        // Pastikan kita tidak mencoba mengambil lebih banyak item dari yang tersedia
        //finalItemCount = Mathf.Min(finalItemCount, candidatePool.Count);

        var shuffledPool = candidatePool.OrderBy(x => UnityEngine.Random.value).ToList();
        for (int i = 0; i < shuffledPool.Count; i++)
        {
            sampahList.Add(shuffledPool[i]);
        }
        //for (int i = 0; i < finalItemCount; i++)
        //{
        //    sampahList.Add(shuffledPool[i]);
        //}

        Debug.Log($"Proses mencari sampah selesai. Hari ini (luck: {dailyLuck}), Anda mendapatkan {sampahList.Count} item sampah.");
    }
}
