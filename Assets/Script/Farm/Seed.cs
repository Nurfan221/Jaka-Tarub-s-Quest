using System.Collections;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public bool isReadyToHarvest = false;
    public bool siram = false;
    public Sprite[] growthSprites; // Array sprite untuk setiap tahap pertumbuhan
    private bool isGrowing = true;
    private int growthCount = 0;
    private int maxGrowthCount = 2; // Jumlah pertumbuhan maksimum sebelum panen
    private float growthTime = 10f; // Waktu pertumbuhan dalam detik

    public string namaSeed;
    public ItemData dropItem;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(Grow());
    }

      // Fungsi untuk inisialisasi properti Seed berdasarkan Item
    //public void InitializeSeed(string itemName, GameObject itemDropPrefab)
    //{
    //    namaSeed = itemName;
    //    dropItem = itemDropPrefab;
    //}

    private IEnumerator Grow()
    {
        transform.GetChild(1).GetComponent<ParticleSystem>().Play();
        while (isGrowing)
        {
            yield return new WaitForSeconds(growthTime); // Tunggu growthTime detik

            if (growthCount < maxGrowthCount)
            {
                growthCount++;

                // Ubah sprite setiap kali biji tumbuh
                if (growthCount < growthSprites.Length)
                {
                    spriteRenderer.sprite = growthSprites[growthCount];
                }

                // Pindahkan objek ke posisi tertentu pada growthCount



                if (growthCount == maxGrowthCount)
                {
                    transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
                    isReadyToHarvest = true;
                    isGrowing = false; // Berhenti pertumbuhan setelah biji siap panen
                }
            }
        }
    }

    public void Harvest()
    {
        if (isReadyToHarvest)
        {
            // if (GameEventSystem.Instance.DoneDialogue_FirstKakRen && !GameEventSystem.Instance.DoneDialogue_FirstBandit)
            //     Player_Quest.Instance.SetQuest("Cari sebilah pedang di samping rumah");

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("Pick");
            Debug.Log("Biji dipanen!");
            // Drop item menggunakan ItemPool
            ItemPool.Instance.DropItem(dropItem.itemName,dropItem.itemHealth, dropItem.quality, transform.position + new Vector3(0, 0.5f, 0));
            // Atur ulang fase pertumbuhan ke awal dan lain-lain...

            growthCount = 0;
            spriteRenderer.sprite = growthSprites[growthCount];
            isReadyToHarvest = false;
            siram = true;

            transform.GetChild(0).GetComponent<ParticleSystem>().Play();

        }
        else if (siram)
        {
            Debug.Log("Siram dulu bos");
        }
        else
        {
            Debug.Log("Biji belum siap dipanen bos");
        }
    }

    public void Siram()
    {
        if (siram == true)
        {
            transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            isGrowing = true; // Mulai pertumbuhan kembali
            StartCoroutine(Grow());
            siram = false;
        }
        else
        {
            Debug.Log("bukan waktunya siram bro ");
        }
    }
}
