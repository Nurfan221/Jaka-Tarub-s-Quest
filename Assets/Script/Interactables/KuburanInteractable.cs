using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class KuburanInteractable : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Sprite[] spritesKuburan;
    public Sprite[] notifikasiSprites;
    public int jedaKuburanKotor;
    public int jedaMembersihkanKuburan;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer notifikasiSpritesRenderer;
    public GameObject notifikasi;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini


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


    // Di dalam skrip KuburanInteractable.cs
    public void kondisiKuburanKotor()
    {

        isKotor = true;
        notifikasi.SetActive(true);
        spriteRenderer.sprite = spritesKuburan[1]; // Ganti ke sprite kotor
        StartCoroutine(PlayNotifikationAnimation()); // Mulai animasi notifikasi
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
        if (PlayerController.Instance.playerData.stamina > 0)
        {
            // Tampilkan loading screen sementara proses pembersihan
            LoadingScreenUI.Instance.ShowLoading(false); // Ini akan memanggil PlayLoadingAnimation() dan PauseGame()
                                                               // Beri waktu tunggu minimal untuk pengalaman pengguna yang baik
            yield return new WaitForSecondsRealtime(1.5f); // Jeda minimal 1.5 detik agar tips terbaca
            LoadingScreenUI.Instance.HideLoading();

            // Ubah status kuburan menjadi bersih
            isKotor = false;
            spriteRenderer.sprite = spritesKuburan[0];  // Set sprite menjadi kuburan bersih
            notifikasi.SetActive(false);  // Matikan animasi notifikasi

            // Reset nilai currentDayKotor setelah kuburan dibersihkan
            RandomCurrentDayKotor();

            // Update jumlah kuburan yang dibersihkan
            //environmentKuburanManager.jumlahdiBersihkan++;
            MainEnvironmentManager.Instance.kuburanManager.UpdateStatusJob();
        }
            

        //environmentKuburanManager.player_Health.SpendStamina(useStamina);
        //environmentKuburanManager.player_Health.ApplyFatigue(useStamina);
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
