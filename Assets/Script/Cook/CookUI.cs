using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Tambahkan ini

public class CookUI : MonoBehaviour
{
    //[SerializeField] CookIngredient cookIngredient;

    [Header("Database Crafting")]
    [SerializeField] private RecipeDatabase recipeDatabaseInstance;
    [SerializeField] private CookInteractable interactableInstance;

    // public GameObject inventorySlots;
    public bool isCookUIPanelOpen = false;

    List<Item> Items;

    [Header("Inventory Slot")]
    [SerializeField] Transform itemSlotContainer;
    [SerializeField] Transform itemSlotTemplate;

    [Header("Item Cook")]
    public List<Item> itemsInCook = new List<Item>();  // Menyimpan item yang telah dipindahkan ke cooking
    public List<Item> fuelInCook = new List<Item>();  // Menyimpan Fuel yang telah dipindahkan ke cooking
    public List<Item> resultInCook = new List<Item>();  // Menyimpan Fuel yang telah dipindahkan ke cooking
    public GameObject ItemCook;  // Objek tempat item akan dipindahkan (misal, slot masak)
    public GameObject FuelCook;  // Objek tempat item akan dipindahkan (misal, slot masak)
    public GameObject cookingResult;  // Objek tempat item akan dipindahkan (misal, slot masak)
    public GameObject fire;
    public Transform imageFire;
    public Sprite[] newFireSprite; // Sprite baru yang akan digunakan saat kondisi terpenuhi
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)

    private Image fireImage; // Komponen UI Image
    private int currentFrame = 0; // Indeks frame saat ini

    public bool itemCookValue = false;
    public bool fuelCookValue = false;
    public bool resultCookValue = false;
    private bool isCooking = false; // Menandakan apakah sedang memasak
    public int cookTime;
    public float timeToCook = 0; // Waktu memasak yang tersisa 
    public int totalItemCooked = 0;
    public int totalBurning = 0;
    public int fuelStackCount;

    // Fungsi untuk memulai proses memasak
    public Queue<Item> cookingQueue = new Queue<Item>();

    //deklarasikan perbandikan bahan masakan 
    public ItemCategory[] validCategories = {
    ItemCategory.Fruit,
    ItemCategory.Meat,
    ItemCategory.Vegetable
    };

    [Header("button")]
    public Button closeButton;


    private void Start()
    {
        // SetRecipeDescription(recipes[0], CanCraft(recipes[0]));
        // LogRecipesToConsole();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCookUI);
            Debug.Log("tombol close listener added.");
        }
        else
        {
            Debug.Log("tombol close belum terhubung");
        }
        fireImage = imageFire.GetComponent<Image>();




    }

    private void Update()
    {
       





    }

    private IEnumerator PlayFireAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (newFireSprite.Length > 0) // Pastikan array sprite tidak kosong
            {
                fireImage.sprite = newFireSprite[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % newFireSprite.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }

    public void OpenCook()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        isCookUIPanelOpen = true;

        RefreshSlots();
    }

    private void CloseCookUI()
    {
        GameController.Instance.ShowPersistentUI(true);
        isCookUIPanelOpen = false;

        RefreshSlots();

        // Jangan stop dari sini. Langsung delegasikan ke pemilik coroutine
        interactableInstance.StartCookingExternally(ProcessCookingQueue(cookTime));

        gameObject.SetActive(false);
    }



    public void RefreshSlots()
    {
        foreach (Transform child in itemSlotContainer)
        {
            if (child == itemSlotTemplate)
                continue;
            Destroy(child.gameObject);
        }

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            Transform theItem = Instantiate(itemSlotTemplate, itemSlotContainer);
            theItem.name = item.itemName;
            theItem.gameObject.SetActive(true);
            theItem.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            theItem.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            theItem.GetComponent<DragCook>().itemName = item.itemName;

            // Menambahkan event listener ketika item diklik
            if (validCategories.Any(category => item.IsInCategory(category)))
            {
                theItem.GetComponent<Button>().onClick.AddListener(() => MoveItemToCook(item));
            }
            else if (item.IsInCategory(ItemCategory.Fuel))
            {
                theItem.GetComponent<Button>().onClick.AddListener(() => MoveFuelToCook(item));
            }

        }
    }

    // Fungsi untuk memindahkan item ke ItemCook
    public void MoveItemToCook(Item item)
    {
        //    if (cookIngredients.cekIngredient == true)
        //    {
        //        Debug.Log("please move item in itemcook inventory");
        //        cookIngredients.DestroyCekIngredient();
        //        cookIngredients.cekIngredient = false;
        //    }
        // Dapatkan item dari item pool
        item = ItemPool.Instance.GetItem(item.itemName);

        // Remove item dari inventory
        Player_Inventory.Instance.RemoveItem(item);

        // Cek apakah item sudah ada di dalam itemsInCook
        Item existingItem = itemsInCook.FirstOrDefault(i => i.itemName == item.itemName);

        // Jika item sudah ada di itemsInCook
        if (existingItem != null)
        {
            existingItem.stackCount += item.stackCount;
            Debug.Log("Item sudah ada, menambahkan ke stack.");
        }
        else
        {
            // Cek apakah resultInCook memiliki hasil masakan dari item yang sama
            bool hasMatchingResult = false;
            foreach (var recipe in recipeDatabaseInstance.cookRecipes)
            {
                if (recipe.ingredient.itemName == item.itemName)
                {
                    var matchedResult = resultInCook.FirstOrDefault(r => r.itemName == recipe.result.itemName);
                    if (matchedResult != null)
                    {
                        hasMatchingResult = true;
                        break;
                    }
                }
            }

            // Jika hasil sebelumnya cocok, tetap masukkan item ke dalam itemsInCook
            itemsInCook.Add(item);
            Debug.Log(hasMatchingResult
                ? "Item dimasukkan karena cocok dengan hasil sebelumnya."
                : "Item baru ditambahkan ke itemsInCook.");
        }

        // Update tampilan UI
        UpdateItemCookUI();
        RefreshSlots();
        TryStartCook();

    }




    public void MoveFuelToCook(Item item)
    {
        // Dapatkan item dari item pool
        item = ItemPool.Instance.GetItem(item.itemName);

        // Remove item dari inventory
        Player_Inventory.Instance.RemoveItem(item);

        // Cek apakah item sudah ada di dalam fuelInCook
        Item existingItem = fuelInCook.FirstOrDefault(i => i.itemName == item.itemName);

        if (existingItem != null)
        {
            // Tambahkan ke stack fuel yang sudah ada
            existingItem.stackCount += item.stackCount;
            Debug.Log("Fuel sudah ada, menambahkan ke stack. Total sekarang: " + existingItem.stackCount);
        }
        else
        {
            // Tambahkan fuel baru ke list
            fuelInCook.Add(item);
            Debug.Log("Fuel baru ditambahkan ke fuelInCook.");
        }

        // Update tampilan FuelCook UI
        UpdateFuelCookUI();

        // Refresh tampilan inventory
        RefreshSlots();

        // Coba mulai memasak jika kedua komponen sudah siap
        TryStartCook();
    }



    // Fungsi untuk memperbarui tampilan ItemCook setelah item ditambahkan
    public void UpdateItemCookUI()
    {
        itemCookValue = true;

        // Hapus item yang sudah ada di tampilan Cook
        foreach (Transform child in ItemCook.transform)
        {
            Destroy(child.gameObject);
        }

        // Pastikan hanya satu item yang ditambahkan ke ItemCook
        if (itemsInCook.Count > 0)
        {
            // Ambil item pertama dari itemsInCook
            Item item = itemsInCook[0];

            // Menambahkan item ke slot tampilan masak
            Transform itemSlot = Instantiate(itemSlotTemplate, ItemCook.transform);
            itemSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = item.itemName;
            itemSlot.GetComponent<DragCook>().itemName = item.itemName;
            // Tambahkan listener untuk mengembalikan item ke inventory saat diklik
            itemSlot.GetComponent<Button>().onClick.AddListener(() => ReturnItemToInventory(item));
        }

       

    }

    public void UpdateFuelCookUI()
    {
        fuelCookValue = true;

        // Hapus item yang sudah ada di tampilan Cook
        foreach (Transform child in FuelCook.transform)
        {
            Destroy(child.gameObject);
        }

        // Pastikan hanya satu item yang ditambahkan ke ItemCook
        if (fuelInCook.Count > 0)
        {
            // Ambil item pertama dari fuelInCook
            Item item = fuelInCook[0];

            // Menambahkan item ke slot tampilan masak
            Transform itemSlot = Instantiate(itemSlotTemplate, FuelCook.transform);
            itemSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = item.itemName;
            itemSlot.GetComponent<DragCook>().itemName = item.itemName;
            // Tambahkan listener untuk mengembalikan item ke inventory saat diklik
            itemSlot.GetComponent<Button>().onClick.AddListener(() => ReturnFuelToInventory(item));

            fuelStackCount = item.stackCount;
        }
        // fuelInCook.gameObject.SetActive(false);
        ChangeFireImage();

       

    }
    private void TryStartCook()
    {
        bool hasFuel = (fuelInCook.Count > 0) || (totalBurning > 0);

        if (itemsInCook.Count > 0 && hasFuel && !isCooking)
        {
            Debug.Log("TryStartCook berhasil. Memulai StartCook()");
            StartCook();
        }
        else
        {
            Debug.Log($"TryStartCook gagal. Kondisi: itemsInCook.Count={itemsInCook.Count}, hasFuel={hasFuel}, isCooking={isCooking}");
        }
    }



    void ChangeFireImage()
    {
        // Cari objek Fire di scene
        GameObject fireObject = GameObject.Find("Fire");

        if (fireObject != null)
        {
            // Dapatkan komponen Image dari objek Fire
            Image fireImage = fireObject.GetComponent<Image>();

            if (fireImage != null)
            {
                if (fuelInCook.Count == 0)
                {
                    StopCoroutine(PlayFireAnimation());
                    Debug.Log("Fuel empty, fire image set to none.");
                }
                else
                {
                    imageFire.gameObject.SetActive(true);
                    StartCoroutine(PlayFireAnimation());
                    Debug.Log("Fuel present, fire image set to newFireSprite.");
                }
            }
            else
            {
                Debug.LogWarning("Fire object ditemukan, tapi tidak memiliki komponen Image.");
            }
        }
        else
        {
            Debug.LogWarning("Fire object tidak ditemukan di scene.");
        }
    }





    // Metode untuk mengembalikan item ke inventory
    public void ReturnItemToInventory(Item item)
    {
        // Menambahkan item kembali ke inventory
        Player_Inventory.Instance.AddItem(item); // Asumsikan Anda memiliki metode AddItem di Player_Inventory

        // Hapus item dari itemsInCook
        itemsInCook.Remove(item);

        // Perbarui tampilan ItemCook
        UpdateItemCookUI();
        RefreshSlots();
        itemCookValue = false;


    }
    public void ReturnFuelToInventory(Item item)
    {
        // Menambahkan item kembali ke inventory
        Player_Inventory.Instance.AddItem(item); // Asumsikan Anda memiliki metode AddItem di Player_Inventory

        // Hapus item dari itemsInCook
        fuelInCook.Remove(item);

        // Perbarui tampilan ItemCook
        UpdateFuelCookUI();
        RefreshSlots();
        fuelCookValue = false;





    }




   

    //void LogRecipesToConsole()
    //{
    //    // Mengambil total recipes 
    //    int recipeCount = recipes.Count;
    //    Debug.Log($"Total Recipes: {recipeCount}");

    //    foreach (var recipe in recipes)
    //    {
    //        // Mengambil nama item yang dihasilkan 
    //        string recipeDetails = $"Recipe: {recipe.result.itemName}\nIngredients:";
    //        // Mengecek jumlah item dari sebuah resep 
    //        for (int i = 0; i < recipe.ingredients.Count; i++)
    //        {
    //            // Menampilkan ingredients dan ingredients Count
    //            recipeDetails += $"\n- {recipe.ingredients[i].itemName} x{recipe.ingredientsCount[i]}";
    //        }
    //        Debug.Log(recipeDetails);
    //    }
    //}

    //   



    // Fungsi untuk memulai proses memasak
    public void StartCook()
    {
        if (isCooking)
        {
            Debug.Log("Masih memasak. Tidak bisa mulai lagi.");
            return;
        }

        if (itemsInCook.Count > 0 )
        {
            itemCookValue = false;
            fuelCookValue = false;
            string itemName = itemsInCook[0].itemName;
            int stackCountItem = itemsInCook[0].stackCount;
            





            // Mencari resep yang cocok
            foreach (Recipe recipe in recipeDatabaseInstance.cookRecipes)
            {
                if (recipe.ingredientCount == 1 && itemName == recipe.ingredient.itemName)
                {
                    Debug.Log("Recipe found");
                    Debug.Log("time To Cook = " + timeToCook);


                    Item cookedItem = ItemPool.Instance.GetItem(recipe.result.itemName);
                    cookTime = recipe.result.CookTime;

                    // Tambahkan item ke antrian sebanyak stackCount
                    for (int i = 0; i < stackCountItem; i++)
                    {
                        cookingQueue.Enqueue(cookedItem); // Masukkan ke antrian
                        Debug.Log("Jumlah hasil yang akan di masak " +  i);
                    }

                    if (!resultCookValue) // Jika tidak ada hasil masakan, mulai proses masak
                    {
                        interactableInstance.StartCookingExternally(ProcessCookingQueue(cookTime));
                    }
                    else if (resultCookValue && resultInCook.Count > 0 && resultInCook[0].itemName == cookedItem.itemName)
                    {
                        // Jika ada hasil masakan dan item yang di-cook sesuai dengan item yang sedang dimasak
                        //cookingCoroutine = interactableInstance.StartCookingCoroutine(ProcessCookingQueue(cookTime));
                          interactableInstance.StartCookingExternally(ProcessCookingQueue(cookTime));
                    }
                    else
                    {
                        Debug.Log("Result item not match with cooked item");
                        return; // Hentikan proses jika item tidak cocok
                    }

                    break;
                }
                else
                {
                    Debug.Log("Recipe not found");
                }
            }
        }
        else
        {
            Debug.Log("No items or fuel in cook!");
        }
    }

    // Coroutine untuk memproses antrian memasak
    private IEnumerator ProcessCookingQueue(int cookTime)
    {
        isCooking = true;
        int cookCounter = 1; // Mulai dari 1

        while (cookingQueue.Count > 0)
        {
            Item cookedItem = cookingQueue.Dequeue(); // Ambil item dari antrian
            Debug.Log("fuel total : " + fuelStackCount);
            if (totalBurning <= 0 && fuelStackCount > 0)
            {
                fuelStackCount--;
                totalBurning = fuelInCook[0].BurningTime;
                Debug.Log("Fuel habis, ambil fuel baru: ");
                // Pengurangan stack count dari fuelInCook
                if (fuelInCook.Count > 0)
                {
                    Item firstFuel = fuelInCook[0];
                    if (firstFuel.stackCount > 0)
                    {
                        firstFuel.stackCount -= 1;

                        if (firstFuel.stackCount <= 0)
                        {
                            fuelInCook.RemoveAt(0);
                        }
                    }
                }
            }
            else if (totalBurning <= 0 && fuelStackCount == 0)
            {
                Debug.Log("Semua bahan bakar habis.");
                break;
            }



            totalBurning -= cookTime;

            yield return new WaitForSeconds(cookTime);

            CookItem(cookedItem);

            Debug.Log("Memasak item ke-" + cookCounter);
            cookCounter++; // Naikkan setelah log

            if ( totalBurning <= 0 && fuelStackCount <= 0)
            {
                Debug.Log("Fuel finished or time to cook is zero.");
                break;
            }
        }

        isCooking = false;

        if (itemsInCook.Count > 0 && fuelInCook.Count > 0)
        {
            Debug.Log("Memulai ulang memasak karena bahan dan fuel masih ada.");
            TryStartCook(); // Ini akan memanggil StartCook ulang
        }

    }


    // Fungsi untuk memproses satu item yang dimasak
    private void CookItem(Item cookedItem)
    {
        totalItemCooked++;
        Debug.Log("total masakan selesai : " +  totalItemCooked);
        // Cek apakah item sudah ada di dalam resultInCook
        Item existingItem = resultInCook.FirstOrDefault(item => item.itemName == cookedItem.itemName);

        if (existingItem != null)
        {
            // Jika item sudah ada, tambahkan ke stack
            existingItem.stackCount += 1;
            Debug.Log("Item sudah ada, menambahkan ke stack. Stack count: " + existingItem.stackCount);
        }
        else
        {
            // Jika item belum ada, tambahkan item baru
            resultInCook.Add(cookedItem);
            Debug.Log("Item baru ditambahkan ke resultInCook: " + cookedItem.itemName);
        }

        // Update UI dan slots setelah item dimasak
        UpdateCookingResultUI();
        RefreshSlots();
    }

    // Fungsi untuk memperbarui UI setelah memasak
    private void UpdateCookingResultUI()
    {

        isCooking = false;
        resultCookValue = true;

        // Hapus item yang sudah ada di tampilan Cook
        foreach (Transform child in cookingResult.transform)
        {
            Destroy(child.gameObject);
        }

        // Pengurangan stack count dari itemsInCook
        if (itemsInCook.Count > 0)
        {
            Item firstItem = itemsInCook[0];
            if (firstItem.stackCount > 0)
            {
                firstItem.stackCount -= 1;

                if (firstItem.stackCount <= 0)
                {
                    itemsInCook.RemoveAt(0);
                }
            }
        }

        

        // Pastikan hanya satu item yang ditambahkan ke cooking result
        if (resultInCook.Count > 0)
        {
            // Ambil item pertama dari itemsInCook
            Item item = resultInCook[0];

            // Menambahkan item ke slot tampilan masak
            Transform itemSlot = Instantiate(itemSlotTemplate, cookingResult.transform);
            itemSlot.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemSlot.GetChild(1).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemSlot.gameObject.SetActive(true);
            itemSlot.name = item.itemName;
            itemSlot.GetComponent<DragCook>().itemName = item.itemName;
            // Tambahkan listener untuk mengembalikan item ke inventory saat diklik
            itemSlot.GetComponent<Button>().onClick.AddListener(() => ReturnResultToInventory(item));
        }

        // Perbarui tampilan UI dan slot
        UpdateItemCookUI();
        UpdateFuelCookUI();
        RefreshSlots();
    }






    private void ClearCookingResult()
    {
        // Hapus item yang sudah ada di tampilan Cook
        foreach (Transform child in cookingResult.transform)
        {
            Destroy(child.gameObject);
        }
    }


    // Contoh fungsi ReturnFuelToInventory (jika diperlukan)
    private void ReturnResultToInventory(Item item)
    {

        resultInCook.Remove(item);


        // Logika untuk mengembalikan item ke inventory
        Debug.Log($"Returning {item.itemName} to inventory");
        Player_Inventory.Instance.AddItem(item); // Misalnya menambahkan kembali ke inventory
        ClearCookingResult();
        RefreshSlots();

        fuelCookValue = false;
        itemCookValue = false;
        resultCookValue = false;
    }




}
