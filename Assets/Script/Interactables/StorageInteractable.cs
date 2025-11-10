using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StorageInteractable : Interactable, ISaveable
{
    [Header("ID Unik")]
    public string uniqueID;


    [Header("Deklarasi Variabel Item")]
    public List<ItemData> storage = new List<ItemData>();
    public int maxItem = 12;
    public bool isLocked = false;
    public bool isSaveable = false;

    //[SerializeField] StorageUI storageUI;

    //Animation
    [Header("Animation")]
    public Sprite[] Chest;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)

    public SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini
    public object CaptureState()
    {
        return new StorageSaveData
        {
            id = uniqueID,
            itemsInStorage = storage,
            storagePosition = gameObject.transform.position,

        };
    }

    public void RestoreState(object state)
    {
        Debug.Log("Restoring Storage State...");
        StorageSaveData data = (StorageSaveData)state;
        storage = data.itemsInStorage;
        gameObject.transform.position = data.storagePosition;
    }

    private void Start()
    {



        //AddItemToList();

    }

    public void UnlockStorage()
    {
        isLocked = false;
        Debug.Log("Storage telah dibuka!");
        // Anda bisa tambahkan efek suara atau partikel di sini
    }
    protected override void Interact()
    {
        if (isLocked)
        {
            Debug.Log("Terkunci! Kalahkan semua musuh terlebih dahulu.");
            // Anda bisa memutar suara 'terkunci' di sini
            return; // Hentikan fungsi, jangan buka storage
        }

        // Mulai animasi
        StartAnimationOpen();
        // Panggil OpenStorage setelah animasi selesai
        StartCoroutine(WaitForAnimationAndOpenStorage());
    }

    private IEnumerator WaitForAnimationAndOpenStorage()
    {
        // Tunggu sampai animasi selesai
        yield return AnimationOpen(); // Tunggu sampai animasi selesai
                                      // Bertanya ke "Penjaga Gerbang" untuk mendapatkan akses ke StorageUI
        if (MechanicController.Instance != null)
        {
            // Biarkan MechanicController yang pusing mencari dan membuka StorageUI.
            MechanicController.Instance.HandleOpenStorage(this);
        }
        else
        {
            Debug.LogError("FATAL: MechanicController.Instance tidak ditemukan di scene!");
        }

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
