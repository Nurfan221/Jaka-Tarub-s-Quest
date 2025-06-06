using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookInteractable : Interactable
{

    public Sprite[] animationFire;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)

    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini
    private Coroutine cookingCoroutine;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Ambil komponen SpriteRenderer
        StartCoroutine(PlayFireAnimation()); // Mulai animasi
    }

    private IEnumerator PlayFireAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (animationFire.Length > 0) // Pastikan array sprite tidak kosong
            {
                spriteRenderer.sprite = animationFire[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % animationFire.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }


    [SerializeField] CookUI cookUI;
    protected override void Interact()
    {
        cookUI.OpenCook();
    }
    // fungsi memanggil corountine yang di inginkan
    public void StartCookingExternally(IEnumerator coroutine)
    {
        if (cookingCoroutine != null)
        {
            StopCoroutine(cookingCoroutine);
        }

        cookingCoroutine = StartCoroutine(coroutine);
    }


}
