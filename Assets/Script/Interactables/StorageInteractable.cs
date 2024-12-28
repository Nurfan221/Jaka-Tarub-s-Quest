using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageInteractable : Interactable
{
    public List<Item> Items = new();
    //[SerializeField] StorageUI storageUI;

    //Animation
    [Header("Animation")]
    public Sprite[] Chest;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)

    public SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini


    private void Start()
    {

        if (StorageUI.Instance == null)
        {
            Debug.LogError("StorageUI.Instance tidak ditemukan!");
        }
        else
        {
            Debug.Log("StorageUI.Instance berhasil ditemukan.");
        }

    }
    protected override void Interact()
    {


        // Mulai animasi
        StartAnimationOpen();
        // Panggil OpenStorage setelah animasi selesai
        StartCoroutine(WaitForAnimationAndOpenStorage());
    }

    private IEnumerator WaitForAnimationAndOpenStorage()
    {
        // Tunggu sampai animasi selesai
        yield return AnimationOpen(); // Tunggu sampai animasi selesai
        StorageUI.Instance.OpenStorage(this, Items); // Setelah animasi selesai, buka storage

    }


    public IEnumerator AnimationOpen()
    {
        // Loop through each frame in the Chest array
        for (int i = 0; i < Chest.Length; i++)
        {
            spriteRenderer.sprite = Chest[i]; // Set the current sprite
            yield return new WaitForSeconds(frameRate); // Wait for the specified frame rate
        }

        // Optionally, you can reset the current frame after the animation
        currentFrame = 0; // Reset to the first frame if needed
    }

    public void StartAnimationOpen()
    {
        StartCoroutine(AnimationOpen());
    }

    public void StartAnimationClose()
    {
        StartCoroutine(AnimationClose());
    }
    public IEnumerator AnimationClose()
    {
        // Loop melalui setiap frame di array Chest dari belakang
        for (int i = Chest.Length - 1; i >= 0; i--)
        {
            spriteRenderer.sprite = Chest[i]; // Setel sprite saat ini
            yield return new WaitForSeconds(frameRate); // Tunggu selama frame rate yang ditentukan
        }


        // Opsional, Anda dapat mereset currentFrame setelah animasi
        currentFrame = 0; // Reset ke frame pertama jika diperlukan
    }
}
