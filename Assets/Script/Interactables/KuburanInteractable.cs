using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuburanInteractable : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isKotor;
    public Sprite[] spritesKuburan;
    public Sprite[] notifikasiSprites;
    public int jedaKuburanKotor;
    public int jedaMembersihkanKuburan;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer notifikasiSpritesRenderer;
    public GameObject notifikasi;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini
    void Start()
    {
        StartCoroutine(PlayNotifikationAnimation()); // Mulai animasi
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

    public void kuburanKotor()
    {
        isKotor = true;
        spriteRenderer.sprite = spritesKuburan[1];
        notifikasi.SetActive(true);
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
        
    }
}
