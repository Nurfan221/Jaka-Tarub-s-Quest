using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StorageInteractable : Interactable, ISaveable, IUniqueIdentifiable
{
    [Header("ID Unik")]
    [SerializeField] private string uniqueID;
    public string UniqueID { get => uniqueID; set => uniqueID = value; }

    [Header("Data Pohon")]
    public TypeObject typeObject;
    public EnvironmentHardnessLevel hardnessLevel;

    // --- Implementasi dari Kontrak IUniqueIdentifiable ---
    public string GetBaseName() => typeObject.ToString();
    public string GetObjectType() => typeObject.ToString(); // Menggunakan nama dari enum TypeTree
    public EnvironmentHardnessLevel GetHardness() => hardnessLevel;
    public string GetVariantName() => typeObject.ToString();

    [Header("Deklarasi Variabel Item")]
    public List<ItemData> storage = new List<ItemData>();
    public int maxItem = 12;

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

    //public void AddItemToList()
    //{
    //    // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama
    //    List<Item> newItemList = new List<Item>();

    //    // Menambahkan item baru ke dalam itemQuests berdasarkan countItem
    //    for (int i = 0; i < storage.Count; i++)
    //    {

    //        Item newItem = ItemPool.Instance.GetItem(storage[i].itemName); 
            

    //        // Menambahkan item baru ke dalam newItemList
    //        newItemList.Add(newItem);
    //    }

    //    // Menghapus semua item lama setelah menambahkan item baru
    //    Items.Clear();

    //    // Menambahkan item baru dari newItemList ke quest.itemQuests
    //    foreach (var item in newItemList)
    //    {
    //        Items.Add(item);
    //    }
    //}
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
