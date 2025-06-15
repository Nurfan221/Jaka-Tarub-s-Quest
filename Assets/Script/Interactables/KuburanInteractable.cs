using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System;

public class KuburanInteractable : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] EnvironmentManager environmentKuburanManager;


    public Sprite[] spritesKuburan;
    public Sprite[] notifikasiSprites;
    public int jedaKuburanKotor;
    public int jedaMembersihkanKuburan;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer notifikasiSpritesRenderer;
    public GameObject notifikasi;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini
    private float useStamina = 10;

    //Logika kuburan kotor
    public bool isKotor;
    public int currentDayKotor;
    public int countDayKotor;
    void Start()
    {
        RandomCurrentDayKotor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {


        //// Mulai animasi
        //StartAnimationOpen();
        // Panggil OpenStorage setelah animasi selesai
        if (isKotor)
        {
            StartCoroutine(MembersihkanMakam());
        }
    }


    public void kondisiKuburanKotor()
    {
        if (!isKotor)
        {
            countDayKotor++;
            
        }

        if (currentDayKotor == countDayKotor)
        {
            isKotor = true;
            notifikasi.SetActive(true);
            spriteRenderer.sprite = spritesKuburan[1];
            StartCoroutine(PlayNotifikationAnimation()); // Mulai animasi
            environmentKuburanManager.UpdateStatusJob();
            countDayKotor = 0;
        }


    }

    private IEnumerator PlayNotifikationAnimation()
    {
        while (isKotor) // Hanya berjalan ketika kuburan kotor
        {
            if (notifikasiSprites.Length > 0)
            {
                notifikasiSpritesRenderer.sprite = notifikasiSprites[currentFrame];
                currentFrame = (currentFrame + 1) % notifikasiSprites.Length;
            }
            yield return new WaitForSeconds(frameRate);
        }
    }


    public IEnumerator MembersihkanMakam()
    {
        if (environmentKuburanManager.player_Health.stamina > 0)
        {
            // Tampilkan loading screen sementara proses pembersihan
            LoadingScreenUI.Instance.ShowLoading();
            yield return new WaitForSeconds(jedaMembersihkanKuburan);

            // Sembunyikan loading screen setelah pembersihan selesai
            LoadingScreenUI.Instance.HideLoading();

            // Ubah status kuburan menjadi bersih
            isKotor = false;
            spriteRenderer.sprite = spritesKuburan[0];  // Set sprite menjadi kuburan bersih
            notifikasi.SetActive(false);  // Matikan animasi notifikasi

            // Reset nilai currentDayKotor setelah kuburan dibersihkan
            RandomCurrentDayKotor();

            // Update jumlah kuburan yang dibersihkan
            environmentKuburanManager.jumlahdiBersihkan++;
        }
            

        environmentKuburanManager.player_Health.SpendStamina(useStamina);
        environmentKuburanManager.player_Health.SpendMaxCurrentStamina(useStamina);
    }


    public void RandomCurrentDayKotor()
    {
        // Deklarasikan variabel hanya sekali di luar loop
        float randomCurrentDayKotor = UnityEngine.Random.Range(4f, 10f);

        // Menghasilkan nilai yang berbeda jika nilai random sama dengan currentDayKotor
        do
        {
            randomCurrentDayKotor = UnityEngine.Random.Range(4f, 10f);
        } while (randomCurrentDayKotor == currentDayKotor);

        // Bulatkan nilai random
        int nilaiRandomdiBulatkan = (int)Math.Round(randomCurrentDayKotor);

        // Set nilai random sebagai currentDayKotor
        currentDayKotor = nilaiRandomdiBulatkan;
    }


}
