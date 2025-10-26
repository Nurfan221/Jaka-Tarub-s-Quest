using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookInteractable : Interactable, ISaveable
{
    public InteractableUniqueID interactableUniqueID;
    public Sprite[] animationFire;
    public SpriteRenderer resultItemSprite;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)

    private SpriteRenderer spriteRenderer; // Komponen SpriteRenderer
    private int currentFrame = 0; // Indeks frame saat ini
    public Coroutine cookingCoroutine;
    public TypeCooking typeCooking;

    [Header("Cook settings")]
    public ItemData itemCook;
    public ItemData fuelCook;
    public ItemData itemResult;
    public bool isCooking = false; // Mencegah spam klik
    public float currentProgress = 0f;

    // QuantityFuel menandakan berapa item bisa di masak menggunakan fuel tersebut
    public int quantityFuel = 0; // Nilai bahan bakar saat ini
    public event Action<float> OnProgressUpdated; // progress bar
    public event Action<ItemData> OnResultUpdated; // hasil masak berubah
    public event Action<Item> OnStartCooking;  // saat mulai masak
    public event Action OnCookingFinished;         // saat selesai total

    private CookingDatabaseSO cookingDatabase;

    [Header("Unique ID")]
      //  Implementasi dari Kontrak IUniqueIdentifiable 
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;
    public ArahObject arahObject;
    public EnvironmentType environmentType;



    private void Start()
    {
        interactableUniqueID = gameObject.GetComponent<InteractableUniqueID>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Ambil komponen SpriteRenderer
        StartCoroutine(PlayFireAnimation()); // Mulai animasi
        UpdateSpriteHasil();
    }


    public object CaptureState()
    {
        Debug.Log("[SAVE-CAPTURE] Menangkap data tungku (Furnance)...");
        FurnanceSaveData data = new FurnanceSaveData();
        data.id = interactableUniqueID.UniqueID;
        data.itemCook = itemCook;
        data.fuelCook = fuelCook;
        data.itemResult = itemResult;
        data.quantityFuel = quantityFuel;
        data.furnancePosition = gameObject.transform.position;

        return data;
    }


    public void RestoreState(object state)
    {
        DeleteCokState();
        // Coba cast 'state' yang datang kembali ke tipe aslinya.
        var loadedData = state as CookSaveData;

        if (loadedData != null)
        {
            itemCook  = loadedData.itemCook;
            fuelCook  = loadedData.fuelCook;
            itemResult = loadedData.itemResult;
            quantityFuel = loadedData.quantityFuel;

            Debug.Log($"Data CookInteractable berhasil direstorasi. ");

            // Anda sudah punya fungsi ini, jadi kita panggil saja.
            StartCook();
        }
        else
        {
            Debug.LogWarning("Gagal merestorasi data quest: data tidak valid atau corrupt.");
        }
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


    protected override void Interact()
    {
        MechanicController.Instance.HandleOpenCookUI(this);
    }
    // fungsi memanggil corountine yang di inginkan


    private bool IsItemResultEmpty()
    {
        return itemResult == null || string.IsNullOrEmpty(itemResult.itemName);
    }
    public void StartCook()
    {
        if (isCooking)
        {
            Debug.LogWarning("Proses memasak sedang berjalan!");
            return;
        }

        bool hasFuel = (quantityFuel > 0) || (fuelCook != null && fuelCook.count > 0);

        if (itemCook == null || !hasFuel)
        {
            Debug.LogWarning("Pastikan item masak dan bahan bakar terisi sebelum memasak.");
            return; // Berhenti jika tidak ada item atau tidak ada bahan bakar sama sekali
        }



        // Cari Resep
        RecipeCooking foundRecipe = null;
        foreach (var recipeCooking in DatabaseManager.Instance.cookingDatabase.cookRecipes)
        {
            if (recipeCooking.ingredient.itemName == itemCook.itemName)
            {
                foundRecipe = recipeCooking;
                break; // Resep ditemukan, hentikan pencarian
            }
        }

        if (foundRecipe == null)
        {
            Debug.LogWarning("Tidak ada resep yang cocok untuk item ini.");
            return;
        }

        //Jika resep valid, mulai Coroutine
        if (IsItemResultEmpty() || foundRecipe.result.itemName == itemResult.itemName)
        {


            // Mulai Coroutine dan simpan referensinya
            StartCooking(foundRecipe);
        }
        else
        {
            Debug.LogWarning("Tidak ada resep yang cocok untuk item ini.");
            return;
        }
    }
    public void StartCooking(RecipeCooking recipe)
    {
        if (isCooking) return;

        if (cookingCoroutine != null)
        {

            StopCoroutine(cookingCoroutine);
            if (itemResult.count > 0)
            {
                resultItemSprite.sprite = ItemPool.Instance.GetItemWithQuality(itemResult.itemName, itemResult.quality).sprite;
            }
        }

        cookingCoroutine = StartCoroutine(CookingProcess(recipe));
    }



    private IEnumerator CookingProcess(RecipeCooking recipe)
    {
        Item resultItemCook = ItemPool.Instance.GetItemWithQuality(recipe.result.itemName, recipe.result.quality);
        isCooking = true;

        // Kirim event ke UI untuk menampilkan icon hasil
        OnStartCooking?.Invoke(recipe.result);

        float elapsedTime = 0;
        float cookTime = resultItemCook.CookTime;

        while (elapsedTime < cookTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / cookTime);

            // Kirim progress ke UI
            OnProgressUpdated?.Invoke(progress);
            yield return null;
        }

        // Update hasil masakan
        if (itemResult == null || itemResult.itemName != recipe.result.itemName)
            itemResult = new ItemData(recipe.result.itemName, 1, recipe.result.quality, 1);
        else
            itemResult.count++;

        OnResultUpdated?.Invoke(itemResult); // Beri tahu UI kalau hasil berubah

        // Kurangi bahan & fuel
        itemCook.count -= 1;
        quantityFuel -= 1;

        // Refill fuel jika perlu
        if (quantityFuel <= 0 && fuelCook != null && fuelCook.count > 0)
        {
            fuelCook.count--;
            Item fuelItem = ItemPool.Instance.GetItemWithQuality(fuelCook.itemName, fuelCook.quality);
            quantityFuel = fuelItem.QuantityFuel;
            if (fuelCook.count <= 0) fuelCook = null;
        }

        isCooking = false;
        cookingCoroutine = null;

        // Cek auto-cook
        if (itemCook != null && itemCook.count > 0 && quantityFuel > 0)
            StartCoroutine(CookingProcess(recipe));
        else
        {

            OnCookingFinished?.Invoke(); // Kirim event ke UI kalau semua selesai
            UpdateSpriteHasil();
        }
    }

    public void UpdateSpriteHasil()
    {
        if (itemResult != null && itemResult.count > 0)
        {
            resultItemSprite.gameObject.SetActive(true);
            resultItemSprite.sprite = ItemPool.Instance.GetItemWithQuality(itemResult.itemName, itemResult.quality).sprite;
        }
        else
        {
            resultItemSprite.gameObject.SetActive(false);
            resultItemSprite.sprite = null;
        }
    }

    public void DeleteCokState()
    {
        itemCook = null;
        fuelCook = null;
        itemResult = null;
        quantityFuel = 0;
        isCooking = false;
        currentProgress = 0f;
        UpdateSpriteHasil();
    }
}
