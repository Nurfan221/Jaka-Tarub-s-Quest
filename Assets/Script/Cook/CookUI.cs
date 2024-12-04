using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Tambahkan ini

public class CookUI : MonoBehaviour
{
    [SerializeField] CookIngredients cookIngredients;

    [System.Serializable]
    public class CookRecipe
    {
        public Item result;
        public List<Item> ingredients;
        public List<int> ingredientsCount;
    }

    public List<CookRecipe> recipes;

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
     public Sprite newFireSprite; // Sprite baru yang akan digunakan saat kondisi terpenuhi


    public bool itemCookValue = false; 
    public bool fuelCookValue = false; 
    public bool resultCookValue = false; 

    private bool isCooking = false; // Menandakan apakah sedang memasak

    private float timeToCook = 0; // Waktu memasak yang tersisa 
    // Fungsi untuk memulai proses memasak
    private Queue<Item> cookingQueue = new Queue<Item>();

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
        }else
        {
            Debug.Log("tombol close belum terhubung");
        }


       

    }

    private void Update()
    {
        // Close CookUI
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCook();
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
        gameObject.SetActive(false);
        isCookUIPanelOpen = false;

        RefreshSlots();
             
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
            }else if (item.IsInCategory(ItemCategory.Fuel))
             {
              theItem.GetComponent<Button>().onClick.AddListener(() => MoveFuelToCook(item));
             }

        }
    }

    

  

    // Fungsi untuk memindahkan item ke ItemCook
    public void MoveItemToCook(Item item)
    {
        if (cookIngredients.cekIngredient == true)
            {
                Debug.Log("please move item in itemcook inventory");
                cookIngredients.DestroyCekIngredient();
                cookIngredients.cekIngredient = false ;
            }
        // Dapatkan item dari item pool
        item = ItemPool.Instance.GetItem(item.itemName);

        // Remove item dari inventory
        Player_Inventory.Instance.RemoveItem(item);

        // Cek apakah item sudah ada di dalam itemsInCook
        Item existingItem = itemsInCook.FirstOrDefault(i => i.itemName == item.itemName);

        if (existingItem != null && itemCookValue == true)
        {
            // Jika item sudah ada dan itemCookValue adalah true, tambahkan ke stack
            existingItem.stackCount += 1; // Tambahkan jumlah item
            Debug.Log("item ada lebih dari 1");
        }
        else if (existingItem == null && itemCookValue == false)
        {
            // Jika item belum ada atau itemCookValue adalah false, tambahkan item baru
            item.stackCount = 1; // Atur stackCount untuk item baru
            itemsInCook.Add(item); // Tambahkan item baru ke list
        }

        // Update tampilan ItemCook UI
        UpdateItemCookUI();

        // Refresh tampilan inventory
        RefreshSlots();
    }


    // public void MoveFuelToCook(Item item)
    // {
    //     // Dapatkan item dari item pool
    //     item = ItemPool.Instance.GetItem(item.itemName);

    //     // Remove item dari inventory
    //     Player_Inventory.Instance.RemoveItem(item);

    //     item.stackCount = 1;
    //     fuelInCook.Add(item);

    //     // Update tampilan ItemCook UI
    //     UpdateFuelCookUI();

    //     // Refresh tampilan inventory
    //     RefreshSlots();
    // }

       public void MoveFuelToCook(Item item)
        {
            if (cookIngredients.cekIngredient == true)
            {
                cookIngredients.DestroyCekIngredient();
                Debug.Log("please move item in itemcook inventory");
                cookIngredients.cekIngredient = false;
            }
            // Dapatkan item dari item pool
            item = ItemPool.Instance.GetItem(item.itemName);

            // Remove item dari inventory
            Player_Inventory.Instance.RemoveItem(item);

            // Cek apakah item sudah ada di dalam fuelInCook
            Item existingItem = fuelInCook.FirstOrDefault(i => i.itemName == item.itemName);

            if (existingItem != null && fuelCookValue == true)
            {
                // Jika item sudah ada dan fuelCookValue adalah false, tambahkan ke stack
                existingItem.stackCount += 1; // Tambahkan jumlah item
            }
            else if (existingItem == null && fuelCookValue == false)
            {
                // Jika item belum ada atau fuelCookValue adalah true, tambahkan item baru
                item.stackCount = 1; // Atur stackCount untuk item baru
                fuelInCook.Add(item); // Tambahkan item baru ke list
            }

            // Update tampilan ItemCook UI
            UpdateFuelCookUI();

            // Refresh tampilan inventory
            RefreshSlots();
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

       if (itemCookValue && fuelCookValue == true)
        {
            StartCook();
            Debug.Log("memulai memasak");
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
        }
        // fuelInCook.gameObject.SetActive(false);
        ChangeFireImage();

         if (itemCookValue && fuelCookValue == true)
        {
            StartCook();
            Debug.Log("memulai memasak");
        }
        
    }

    void ChangeFireImage()
    {
        // Cari objek Fire di scene
        GameObject fireObject = GameObject.Find("Fire"); 
        
        // Dapatkan komponen Image dari objek Fire
        Image fireImage = fireObject.GetComponent<Image>();

        // Jika fireImage tidak null, lakukan pengecekan fuelInCook
        if (fireImage != null)
        {
            // Jika fuelInCook kosong, ubah fireImage menjadi none (tidak ada gambar)
            if (fuelInCook.Count == 0)
            {
                fireImage.sprite = null; // Mengatur sprite menjadi none (tidak ada gambar)
                Debug.Log("Fuel empty, fire image set to none.");
            }
            // Jika fuelInCook berisi item, ubah fireImage menjadi newFireSprite
            else if (fuelInCook.Count > 0)
            {
                fireImage.sprite = newFireSprite; // Mengatur sprite menjadi newFireSprite
                Debug.Log("Fuel present, fire image set to newFireSprite.");
            }
        }
        else
        {
            Debug.LogError("Image component not found on Fire object!");
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

   


    public void CloseCook()
    {
        SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
        isCookUIPanelOpen = false;
    }

    void LogRecipesToConsole()
    {
        // Mengambil total recipes 
        int recipeCount = recipes.Count;
        Debug.Log($"Total Recipes: {recipeCount}");

        foreach (var recipe in recipes)
        {
            // Mengambil nama item yang dihasilkan 
            string recipeDetails = $"Recipe: {recipe.result.itemName}\nIngredients:";
            // Mengecek jumlah item dari sebuah resep 
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                // Menampilkan ingredients dan ingredients Count
                recipeDetails += $"\n- {recipe.ingredients[i].itemName} x{recipe.ingredientsCount[i]}";
            }
            Debug.Log(recipeDetails);
        }
    }

//   

   

// Fungsi untuk memulai proses memasak
public void StartCook()
{

    itemCookValue = false;
    fuelCookValue = false;
    if (isCooking)
    {
        Debug.Log("Cooking is already running. Cannot start a new cooking session");
        return;
    }
    
    if (itemsInCook.Count > 0 && fuelInCook.Count > 0)
    {
        string itemName = itemsInCook[0].itemName;
        int stackCountItem = itemsInCook[0].stackCount;
        string fuelName = fuelInCook[0].itemName;
        int burningTime = fuelInCook[0].BurningTime;
        int stackCountFuel = fuelInCook[0].stackCount;
        timeToCook = burningTime * stackCountFuel; // Set waktu total memasak berdasarkan fuel
        
        // Mencari resep yang cocok
        foreach (CookRecipe recipe in recipes)
        {
            if (recipe.ingredients.Count == 1 && itemName == recipe.ingredients[0].itemName)
            {
                Debug.Log("Recipe found");
                Debug.Log("time To Cook = " + timeToCook);
                Debug.Log("stack fuel is = " + stackCountFuel);


                Item cookedItem = ItemPool.Instance.GetItem(recipe.result.itemName);
                int cookTime = recipe.result.CookTime;

                // Tambahkan item ke antrian sebanyak stackCount
                for (int i = 0; i < stackCountItem; i++)
                {
                    cookingQueue.Enqueue(cookedItem); // Masukkan ke antrian
                }

               if (!resultCookValue) // Jika tidak ada hasil masakan, mulai proses masak
                {
                    StartCoroutine(ProcessCookingQueue(cookTime));
                }
                else if (resultCookValue && resultInCook.Count > 0 && resultInCook[0].itemName == cookedItem.itemName)
                {
                    // Jika ada hasil masakan dan item yang di-cook sesuai dengan item yang sedang dimasak
                    StartCoroutine(ProcessCookingQueue(cookTime));
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

    while (cookingQueue.Count > 0 && timeToCook > 0)
    {
        Item cookedItem = cookingQueue.Dequeue(); // Ambil item dari antrian

        // Kurangi waktu memasak dari total waktu yang tersedia
        timeToCook -= cookTime;

        

        // Jeda untuk waktu memasak item ini
        yield return new WaitForSeconds(cookTime);
        // Proses memasak item
        CookItem(cookedItem);

        // Cek apakah fuel masih tersedia
        if (fuelInCook.Count == 0 || timeToCook <= 0)
        {
            Debug.Log("Fuel finished or time to cook is zero.");
            break; // Hentikan proses jika waktu atau fuel habis
        }
    }

    isCooking = false;
}

// Fungsi untuk memproses satu item yang dimasak
private void CookItem(Item cookedItem)
{
    
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
        cookedItem.stackCount = 1;
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
