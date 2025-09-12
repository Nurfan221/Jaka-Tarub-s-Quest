using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class StoneBehavior : MonoBehaviour
{

    public ItemData[] itemDropMines; // Array hasil tambang yang mungkin dijatuhkan
    //Animation idle 
    public string nameStone;
    public TypeStone stoneType;
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

   

    public void Start()
    {
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

                if (dayLuck == 3) // Jika lucky tinggi, pilih item dari belakang
                {
                    // Buat probabilitas untuk memilih item dari belakang
                    if (isLucky)
                    {
                        index = UnityEngine.Random.Range(countHasilTambang - 3, countHasilTambang);  // Pilih indeks dari belakang
                        isLucky = false;
                    }
                    else
                    {
                        index = UnityEngine.Random.Range(0, countHasilTambang - 2);  // Pilih indeks dari depan atau tengah
                    }
                }
                else if (dayLuck == 2) // Lucky sedang, pilih item secara acak
                {
                    // Pilih item secara acak dari seluruh array dengan sedikit lebih banyak kesempatan di awal
                    if (isLucky)
                    {
                        index = UnityEngine.Random.Range(0, countHasilTambang);  // Pilih acak dari seluruh array
                        isLucky = false;
                    }
                    else
                    {
                        index = UnityEngine.Random.Range(0, countHasilTambang);  // Pilih item acak dari seluruh array
                    }
                }
                else // Jika lucky rendah, pilih item dari depan
                {
                    index = UnityEngine.Random.Range(0, countHasilTambang / 2);  // Pilih item dari awal array
                }

                // Tambahkan hasil tambang yang dipilih ke list hasil
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

        // Hancurkan Batu setelah menjatuhkan item
        gameObject.SetActive(false);  // Atau Destroy(gameObject);
    }




}
