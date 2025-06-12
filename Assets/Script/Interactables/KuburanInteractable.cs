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
        StartCoroutine(MembersihkanMakam());
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
            
            countDayKotor = 0;
        }


    }

    private IEnumerator PlayNotifikationAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (notifikasiSprites.Length > 0) // Pastikan array sprite tidak kosong
            {
                notifikasiSpritesRenderer.sprite = notifikasiSprites[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % notifikasiSprites.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }

    public IEnumerator MembersihkanMakam()
    {
        LoadingScreenUI.Instance.ShowLoading();
        yield return new WaitForSeconds(jedaMembersihkanKuburan); // Tunggu sebelum beralih ke frame berikutnya

        // Tunggu animasi selesai sebelum menutup loading
        LoadingScreenUI.Instance.HideLoading();
        isKotor = false;
        spriteRenderer.sprite = spritesKuburan[0];
        notifikasi.SetActive(false);
        RandomCurrentDayKotor();

        environmentKuburanManager.jumlahdiBersihkan++;




    }

    public void RandomCurrentDayKotor()
    {
        float randomCurrentDayKotor = UnityEngine.Random.Range(4f, 8f);
        int nilaiRandomdiBulatkan = (int)Math.Round(randomCurrentDayKotor);
        currentDayKotor = nilaiRandomdiBulatkan;
    }

}
