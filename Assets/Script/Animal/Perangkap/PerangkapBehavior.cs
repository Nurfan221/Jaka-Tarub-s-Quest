using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PerangkapBehavior : MonoBehaviour
{
    public bool isfull; // Apakah perangkap penuh
    public SpriteRenderer spriteRenderer; // Renderer untuk menampilkan sprite perangkap
    public Item[] itemPerangkap; // Item yang digunakan untuk perangkap
    public Item itemTertangkap; // Item hewan yang tertangkap
    public int perangkapHealth; // Kesehatan perangkap

    private void OnEnable()
    {
        TimeManager.OnDayChanged += NewDay;
    }
    private void OnDisable()
    {
        TimeManager.OnDayChanged -= NewDay;
    }
    private void Start()
    {
        GetRandomAnimal(); 
    }
    public void TakeAnimal()
    {
        if (isfull)
        {
            // Logika untuk mengambil hewan dari perangkap
            Debug.Log("Mengambil hewan dari perangkap.");
            isfull = false;
            ItemData itemData = new ItemData
            {
                itemName = itemTertangkap.itemName,
                count = 1,
                quality = itemTertangkap.quality,
                itemHealth = itemTertangkap.health
            };

            ItemPool.Instance.AddItem(itemData);
            spriteRenderer.gameObject.SetActive(false);
            itemTertangkap = null;
        }
        else
        {
            Debug.Log("Perangkap kosong, tidak ada hewan untuk diambil.");
        }
    }
    public void NewDay()
    {
        GetRandomAnimal();
    }
    public void GetRandomAnimal()
    {
        if (isfull)
        {
            Debug.Log("Perangkap sudah penuh, tidak bisa menangkap hewan lagi.");
            return;
        }

        // Ambil nilai keberuntungan hari ini (0 - 3, bertipe float)
        float dayLuck = TimeManager.Instance.GetDayLuck();
        Debug.Log($"[Perangkap] Day Luck hari ini: {dayLuck:F2}");

        // Gunakan dayLuck untuk meningkatkan peluang tangkapan
        float luckMultiplier = 1f + (dayLuck * 0.25f);

        // Tentukan apakah perangkap berhasil menangkap sesuatu hari ini
        float catchChance = Random.value * luckMultiplier;
        if (catchChance < 0.4f)
        {
            Debug.Log($"[Perangkap] Gagal menangkap hewan hari ini. (Chance={catchChance:F2})");
            return;
        }

        // Jika berhasil, tentukan hewan berdasarkan keberuntungan
        // Konversi hasil menjadi integer agar bisa digunakan untuk index
        int randomIndex = Mathf.Clamp(
            Mathf.FloorToInt(Random.Range(0f, itemPerangkap.Length) + dayLuck),
            0,
            itemPerangkap.Length - 1
        );

        itemTertangkap = itemPerangkap[randomIndex];
        Debug.Log($"[Perangkap] Menangkap hewan: {itemTertangkap.itemName} (Luck={dayLuck:F2}, Index={randomIndex})");

        spriteRenderer.gameObject.SetActive(true);
        spriteRenderer.sprite = itemTertangkap.sprite;
        isfull = true;
    }

    public void TakePerangkap()
    {
        if (!isfull)
        {
            Debug.Log("Mengambil perangkap kosong.");
            // Logika untuk mengambil perangkap kosong
            ItemData itemData = new ItemData
            {
                itemName = "Perangkap",
                count = 1,
                quality = ItemQuality.Normal,
                itemHealth = perangkapHealth
            };

            ItemPool.Instance.AddItem(itemData);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Perangkap penuh, tidak bisa diambil.");
        }
    }



}
