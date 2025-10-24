using System.Collections;
using System.Collections.Generic;
using System.Linq; // Tambahkan ini
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CookUI : MonoBehaviour
{
    //[SerializeField] CookIngredient cookIngredient;

    [Header("Database Crafting")]
    [SerializeField] private CookInteractable interactableInstance;
    public PlayerController stats;

    [Header("UI Elements")]
    // ui untuk menampilkan inventory yang bisa di masak
    public Transform inventoryContent;
    public Transform inventorySlotTemplate;

    // ui untuk menampilkan proses memasak
    public Transform itemCookTemplate;
    public Transform fuelCookTemplate;
    public Transform resultCookTemplate;
    public Transform fireCookTemplate;
    public Button closeButton;

    [Header("Cook settings")]
    private ItemCategory[] validCookCategories = {
        ItemCategory.Food,
        ItemCategory.Meat,
        ItemCategory.Vegetable,
    };
    public ItemData itemCook;
    public ItemData fuelCook;
    public ItemData itemResult;
    private bool isCooking = false; // Mencegah spam klik
    private Coroutine cookingCoroutine; // Menyimpan referensi ke coroutine

    // QuantityFuel menandakan berapa item bisa di masak menggunakan fuel tersebut
    public int quantityFuel = 0; // Nilai bahan bakar saat ini


    private void Start()
    {
        if (itemCookTemplate != null)
        {
            itemCookTemplate.GetComponent<Button>().onClick.AddListener(OnClickItemCook);
        }
        if (fuelCookTemplate != null)
        {
            fuelCookTemplate.GetComponent<Button>().onClick.AddListener(OnClickFuelCook);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCookUI);
        }
    
    }
    public void OpenCook()
    {
        stats = PlayerController.Instance;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

        RefreshSlots();
    }

    private void CloseCookUI()
    {
        GameController.Instance.ShowPersistentUI(true);

        //RefreshSlots();

        // Jangan stop dari sini. Langsung delegasikan ke pemilik coroutine
        //interactableInstance.StartCookingExternally(ProcessCookingQueue(cookTime));

        gameObject.SetActive(false);
    }

    public void RefreshSlots()
    {
        foreach (Transform child in inventoryContent)
        {
            if (child == inventorySlotTemplate)
                continue;
            Destroy(child.gameObject);
        }

        //  Mengisi slot inventory baru (Kode Anda sudah benar)
        foreach (ItemData itemData in stats.inventory)
        {
            Item item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            if (validCookCategories.Any(category => item.IsInCategory(category)) || item.IsInCategory(ItemCategory.Fuel)) // Tampilkan juga fuel
            {
                Transform theItem = Instantiate(inventorySlotTemplate, inventoryContent);
                theItem.name = item.itemName;
                theItem.gameObject.SetActive(true);

                theItem.GetChild(0).GetComponent<Image>().sprite = item.sprite;
                theItem.GetChild(1).GetComponent<TMP_Text>().text = itemData.count.ToString();
                theItem.GetComponent<DragCook>().itemName = item.itemName;

                ItemData currentItemData = itemData;
                theItem.GetComponent<Button>().onClick.AddListener(() => OnClickItemInInventory(currentItemData));
            }
        }

        if (itemCook != null && itemCook.count > 0)
        {
            itemCookTemplate.name = itemCook.itemName;
            itemCookTemplate.GetChild(0).GetComponent<Image>().sprite = ItemPool.Instance.GetItemWithQuality(itemCook.itemName, itemCook.quality).sprite;
            itemCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = itemCook.count.ToString();
            itemCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1); // Tampilkan gambar
        }
        else
        {
            // Kosongkan slot jika itemCook null atau count 0
            itemCook = null; // Pastikan null jika count 0
            itemCookTemplate.name = "Slot_Item";
            itemCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            itemCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            itemCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0); // Sembunyikan gambar
        }

        if (fuelCook != null && fuelCook.count > 0)
        {
            fuelCookTemplate.name = fuelCook.itemName;
            fuelCookTemplate.GetChild(0).GetComponent<Image>().sprite = ItemPool.Instance.GetItemWithQuality(fuelCook.itemName, fuelCook.quality).sprite;
            fuelCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = fuelCook.count.ToString();
            fuelCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            // Kosongkan slot jika fuelCook null atau count 0
            fuelCook = null; // Pastikan null jika count 0
            fuelCookTemplate.name = "Slot_Fuel";
            fuelCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            fuelCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            fuelCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }

        StartCook();
    }

    public void OnClickItemInInventory(ItemData itemDataFromInventory)
    {
        Debug.Log($"Item diklik: {itemDataFromInventory.itemName}, Jumlah: {itemDataFromInventory.count}");
        Item item = ItemPool.Instance.GetItemWithQuality(itemDataFromInventory.itemName, itemDataFromInventory.quality);

        bool itemMoved = false; // Flag untuk menandai jika ada item yang berpindah

        if (validCookCategories.Any(category => item.IsInCategory(category)))
        {
            // Jika slot masak kosong, buat tumpukan baru
            if (itemCook == null)
            {
                // Buat ItemData BARU untuk slot masak
                itemCook = new ItemData(itemDataFromInventory.itemName, 1, itemDataFromInventory.quality, itemDataFromInventory.itemHealth);
                itemMoved = true;
            }
            // Jika slot masak berisi item yang SAMA, tambahkan tumpukan
            else if (itemCook.itemName == itemDataFromInventory.itemName)
            {
                itemCook.count += 1;
                itemMoved = true;
            }
            // Jika slot masak berisi item BERBEDA
            else
            {
                Debug.LogWarning("Slot masak sudah terisi item lain. Kembalikan dulu!");
            }
        }
        else if (item.IsInCategory(ItemCategory.Fuel))
        {
            // Jika slot fuel kosong
            if (fuelCook == null)
            {
                fuelCook = new ItemData(itemDataFromInventory.itemName, 1, itemDataFromInventory.quality, itemDataFromInventory.itemHealth);
                itemMoved = true;
            }
            //  Jika slot fuel berisi item yang SAMA
            else if (fuelCook.itemName == itemDataFromInventory.itemName)
            {
                fuelCook.count += 1;
                quantityFuel = item.QuantityFuel; // Perbarui fuel value berdasarkan item fuel yang ditambahkan
                itemMoved = true;
            }
            //  Jika slot fuel berisi item BERBEDA
            else
            {
                Debug.LogWarning("Slot bahan bakar sudah terisi item lain. Kembalikan dulu!");
            }
        }
        else
        {
            Debug.LogWarning("Item tidak valid untuk memasak atau bahan bakar.");
        }

        if (itemMoved)
        {
            // Kurangi jumlah item di inventaris
            itemDataFromInventory.count -= 1;

            // Jika item di inventaris habis (count <= 0), hapus dari list
            if (itemDataFromInventory.count <= 0)
            {
                stats.inventory.Remove(itemDataFromInventory);
            }

            RefreshSlots();
        }
    }


    public void OnClickItemCook()
    {
        if (itemCook != null)
        {
            ItemData newItemData = new ItemData(itemCook.itemName, 1, itemCook.quality, itemCook.itemHealth);
            ItemPool.Instance.AddItem(newItemData);
            itemCook.count -= 1;
        }
        RefreshSlots();
    }
    public void OnClickFuelCook()
    {
        if (fuelCook != null)
        {
            ItemData newItemData = new ItemData(fuelCook.itemName, 1, fuelCook.quality, fuelCook.itemHealth);
            ItemPool.Instance.AddItem(newItemData);
            fuelCook.count -= 1;
        }
        RefreshSlots();
    }

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

        if (itemCook == null || fuelCook == null)
        {
            Debug.LogWarning("Pastikan item masak dan bahan bakar terisi sebelum memasak.");
            return;
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
            cookingCoroutine = StartCoroutine(CookItemCoroutine(foundRecipe));
        }
        else
        {
            Debug.LogWarning("Tidak ada resep yang cocok untuk item ini.");
            return;
        }
    }

    private IEnumerator CookItemCoroutine(RecipeCooking recipe) // Menggunakan nama parameter dari sebelumnya
    {
        // Ambil data item, bukan ItemData
        Item resultItemCook = ItemPool.Instance.GetItemWithQuality(recipe.result.itemName, recipe.result.quality);
        isCooking = true;

        // Ambil referensi UI
        Image fillImage = resultCookTemplate.GetChild(0).GetComponent<Image>(); // Ini adalah Ikon
        Image hasilCookImage = resultCookTemplate.GetComponent<Image>(); // Ini adalah Filler (Slot)
        TMP_Text resultCountText = resultCookTemplate.GetChild(1).GetComponent<TMP_Text>();

        // Setup Ikon (gambar transparan di atas)
        fillImage.gameObject.SetActive(true);
        fillImage.sprite = resultItemCook.sprite;
        fillImage.color = new Color(1, 1, 1, 0.5f);

        // Setup Filler (gambar radial di slot)
        hasilCookImage.fillAmount = 0;

    

        float elapsedTime = 0;

        // Loop timer
        while (elapsedTime < resultItemCook.CookTime) // Asumsi Item punya properti CookTime
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / resultItemCook.CookTime;
            hasilCookImage.fillAmount = progress;

      

            yield return null;
        }

        //  PROSES MEMASAK SELESAI 

        hasilCookImage.fillAmount = 1;      // Set filler 100%
        fillImage.color = new Color(1, 1, 1, 1); // Set ikon 100% buram

        // Cek apakah slot hasil kosong ATAU berisi item yang berbeda
        if (itemResult == null || itemResult.itemName != recipe.result.itemName)
        {
            // Buat tumpukan baru
            // Asumsi konstruktor: (nama, kualitas, jumlah)
            itemResult = new ItemData(recipe.result.itemName,1, recipe.result.quality, 1);

            // Hanya reset teks jika ini adalah tumpukan baru/berbeda
            resultCountText.text = "";
        }
        // Jika itemnya SAMA, tambahkan saja jumlahnya
        else if (itemResult.itemName == recipe.result.itemName)
        {
            itemResult.count += 1;
        }



        // Update UI dengan data yang sudah di-stack
        resultCookTemplate.name = itemResult.itemName;
        resultCountText.gameObject.SetActive(true);
        resultCountText.text = itemResult.count.ToString();

        // Kurangi bahan SATU KALI setelah selesai
        itemCook.count -= 1;
        fuelCook.count -= 1;

        RefreshSlots();

        isCooking = false;
        cookingCoroutine = null;

        //  LOGIKA AUTO-COOK 
        // Cek apakah bahan & fuel masih ada untuk memasak item berikutnya
        if (itemCook != null && itemCook.count > 0 && fuelCook != null && fuelCook.count > 0)
        {
            Debug.Log("Bahan masih ada, melanjutkan memasak...");
            StartCook(); // Panggil fungsi StartCook() utama lagi
        }
        else
        {
            Debug.Log("Bahan masak atau bahan bakar habis. Berhenti memasak.");
            resultCookTemplate.GetComponent<Button>().onClick.RemoveAllListeners();
            resultCookTemplate.GetComponent<Button>().onClick.AddListener(()=>
            {
                if (itemResult != null)
                {
                    ItemPool.Instance.AddItem(itemResult);
                    itemResult = null;

                    fillImage.gameObject.SetActive(false);
                    resultCountText.gameObject.SetActive(false);
                    RefreshSlots();

                }
            });

        }
    }
}
