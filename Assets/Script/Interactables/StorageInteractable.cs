using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageInteractable : Interactable
{

    [SerializeField]
    public List<Item> Items = new();
    public int maxItem = 12;
    public int[] countItems;


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

        AddItemToList();

    }

    public void AddItemToList()
    {
        // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama
        List<Item> newItemList = new List<Item>();

        // Menambahkan item baru ke dalam itemQuests berdasarkan countItem
        for (int i = 0; i < Items.Count; i++)
        {
            // Membuat salinan baru dari item yang ada untuk menghindari modifikasi referensi langsung
            Item itemCopy = new Item
            {
                itemName = Items[i].itemName, // Salin nama item
                stackCount = countItems[i]          // Set stackCount dari countItem
            };

            // Menambahkan item baru ke dalam newItemList
            newItemList.Add(itemCopy);
        }

        // Menghapus semua item lama setelah menambahkan item baru
        Items.Clear();

        // Menambahkan item baru dari newItemList ke quest.itemQuests
        foreach (var item in newItemList)
        {
            // Mendapatkan salinan item dari ItemPool (menggunakan Instantiate)
            Item itemFromPool = ItemPool.Instance.GetItem(item.itemName, item.stackCount);

            // Menambahkan item yang diinstansiasi ke dalam quest.itemQuests
            if (itemFromPool != null)
            {
                Items.Add(itemFromPool);
                Debug.Log($"Item: {itemFromPool.itemName}, Jumlah: {itemFromPool.stackCount}");
            }
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
