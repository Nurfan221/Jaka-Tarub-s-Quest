using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;



public class StoneBehavior : MonoBehaviour, IUniqueIdentifiable
{
    [Header("ID Unik")]
    [SerializeField] private string uniqueID; // Gunakan SerializeField agar bisa dilihat tapi tidak mudah diubah
    public string UniqueID { get => uniqueID; set => uniqueID = value; }
    public string GetBaseName() => stoneType.ToString();
    public string GetObjectType() => stoneType.ToString(); // Menggunakan nama dari enum TypeStone
    public EnvironmentHardnessLevel GetHardness() => environmentHardnessLevel;

    public ItemData[] itemDropMines; // Array hasil tambang yang mungkin dijatuhkan
    public ParticleSystem hitEffectPrefab; // Variabel untuk Prefab partikel
    //Animation idle 
    public string nameStone;
    public TypeObject stoneType;
    public EnvironmentHardnessLevel environmentHardnessLevel;
    public Sprite[] stoneAnimation;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)

    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini
    public int minHasil;
    public int maxHasil;

    public float health; // Kesehatan batu
    public float dayLuck;
    public bool isLucky;
    public int dayToRespawn;



    public void Start()
    {
        hitEffectPrefab = gameObject.GetComponentInChildren<ParticleSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Ambil komponen SpriteRenderer
        StartCoroutine(PlayStoneAnimation()); // Mulai animasi
        
    }



    public void PushHasilTambang()
    {
        itemDropMines = null; // kosongkan dulu
        itemDropMines = DatabaseManager.Instance.GetHasilTambang(stoneType, environmentHardnessLevel);
    }

    private IEnumerator PlayStoneAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (stoneAnimation.Length > 0) // Pastikan array sprite tidak kosong
            {
                spriteRenderer.sprite = stoneAnimation[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % stoneAnimation.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }

    public void TakeDamage(int damage)
    {
        if (hitEffectPrefab != null)
        {
            Debug.Log("Menampilkan efek pukulan pada posisi: " + gameObject.transform.position);
            // Buat instance dari prefab efek di lokasi pukulan dengan rotasi yang sesuai
            hitEffectPrefab.Play();
        }
        health -= Mathf.Min(damage, health);
        Debug.Log($"Batu terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            DestroyStone();
        }
    }

    private void DestroyStone()
    {
        Debug.Log("Batu dihancurkan!");

        if (itemDropMines.Length > 0)  // Pastikan array hasil tambang tidak kosong
        {
            int countHasilTambang = itemDropMines.Length;

            // Menentukan persentase berdasarkan dayLuck
            float percentage = 0.3f; // default 30%
            if (dayLuck == 2) percentage = 0.5f;
            else if (dayLuck == 3) percentage = 0.7f;

            // Tentukan jumlah hasil tambang yang akan dimunculkan (min dan max)
            int randomCount = UnityEngine.Random.Range(minHasil, maxHasil + 1);

            // Pilih hasil tambang berdasarkan dayLuck
            List<ItemData> results = new List<ItemData>();  // Menyimpan hasil tambang yang dipilih
            if (dayLuck == 3)
            {
                 isLucky = true; // Variabel untuk memastikan variasi antara pilih awal dan akhir
            }else
            {
                isLucky = false;
            }

            for (int i = 0; i < randomCount; i++)
            {
                int index = 0;
                int totalItems = itemDropMines.Length;

                if (dayLuck >= 3) // Sangat Beruntung
                {
                    // 75% kemungkinan dapat item langka (setengah akhir), 25% item biasa (setengah awal)
                    if (UnityEngine.Random.value < 0.75f)
                    {
                        index = UnityEngine.Random.Range(totalItems / 2, totalItems); // Ambil dari setengah akhir (item bagus)
                    }
                    else
                    {
                        index = UnityEngine.Random.Range(0, totalItems / 2); // Ambil dari setengah awal (item biasa)
                    }
                }
                else if (dayLuck >= 2) // Normal
                {
                    // Bisa dapat item dari mana saja
                    index = UnityEngine.Random.Range(0, totalItems);
                }
                else // Tidak Beruntung
                {
                    // Hanya bisa dapat item dari setengah awal (item paling umum)
                    index = UnityEngine.Random.Range(0, totalItems / 2);
                }

                results.Add(itemDropMines[index]);
            }

            // Menampilkan hasil yang dipilih (untuk debug)
            foreach (var result in results)
            {
                Debug.Log("Item yang didapat: " + result.itemName);
            }

            // Jatuhkan batu sebagai stack item tunggal
            if (results != null)
            {
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                for (int j = 0; j < results.Count; j++)
                {
                    ItemPool.Instance.DropItem(results[j].itemName, results[j].itemHealth, results[j].quality, transform.position + offset, 1);
                }
            }
        }
        Debug.Log("ubah Data untuk batu nonActive dengan id : " + uniqueID);
        int daysToWait = UnityEngine.Random.Range(2, 6); // Menunggu 2 sampai 5 hari
        dayToRespawn = TimeManager.Instance.date + daysToWait; // Asumsi Anda punya data hari saat ini
        BatuManager.Instance.ScheduleRespawn(uniqueID, dayToRespawn);

        // Hancurkan Batu setelah menjatuhkan item
        gameObject.SetActive(false);  // Atau Destroy(gameObject);
    }


  

}
