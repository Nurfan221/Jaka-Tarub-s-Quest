using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvironmentBehavior : MonoBehaviour
{
    //Animation idle 
    public Sprite[] rumputAnimation;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)

    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini
    public string nameEnvironment;
    public Item itemDrop;
    public Transform plantsContainer;

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Ambil komponen SpriteRenderer
        StartCoroutine(PlayrumputAnimation()); // Mulai animasi
    }

    private IEnumerator PlayrumputAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (rumputAnimation.Length > 0) // Pastikan array sprite tidak kosong
            {
                spriteRenderer.sprite = rumputAnimation[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % rumputAnimation.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }
}
