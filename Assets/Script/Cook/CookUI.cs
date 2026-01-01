using System.Linq; // Tambahkan ini
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CookUI : MonoBehaviour
{
    //[SerializeField] CookIngredient cookIngredient;
    public static CookUI Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);

        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    [Header("Database Crafting")]
    public CookIngredient cookIngredient;
    public CookInteractable interactableInstance;
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
    public Slider fuelSlider;
    public Image fireCookImage;
    public Sprite[] fireCookSprite;
    public Sprite fireNotActive;
    public Image quantityFuelImage;
    public Sprite[] quantityFuelSprite;
    public Button closeButton;

    private ItemCategory[] validCookCategories = {
        ItemCategory.Food,
        ItemCategory.Meat,
        ItemCategory.Vegetable,
    };



    [Header("Ingriedient State")]
    public TypeCooking typeCooking;
    public bool isIngredientAdded = false;
    public bool isCookReady = false;
    public Item currentIngredient;
    public Item resultIngredient;
    public int currentIngredientCount = 0;
    public int resultIngredientCount = 0;


    [Header("Animation Settings")]
    //Animation idle 
    public float frameRate = 0.2f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini
    public bool isFireActive = false;
    public int maxQuantityFuel = 5; // max bahan bakar berdasarkan item fuel
    private Coroutine fireAnimationCoroutine; // Menyimpan referensi ke coroutine animasi api


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


        if (IsItemResultEmpty())
        {
            Image fillImage = resultCookTemplate.GetChild(0).GetComponent<Image>(); // Ikon
            TMP_Text resultCountText = resultCookTemplate.GetChild(1).GetComponent<TMP_Text>();

            resultCookTemplate.GetComponent<Button>().onClick.RemoveAllListeners();
            resultCookTemplate.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (interactableInstance.itemResult != null && interactableInstance.itemResult.count > 0)
                {
                    // Player mengambil hasil masak
                    bool isSuccess = ItemPool.Instance.AddItem(interactableInstance.itemResult);
                    if (isSuccess)
                    {
                        if (interactableInstance.isCooking)
                        {
                            // Kalau masih masak, jangan hilangkan item — set count ke 0 agar sprite tetap tampil
                            interactableInstance.itemResult.count = 0;
                            Debug.Log("Player mengambil hasil, tapi tungku masih memasak. ItemResult diset count=0.");
                        }
                        else
                        {
                            // Kalau sudah tidak masak, hapus item
                            interactableInstance.itemResult = null;
                            Debug.Log("Player mengambil hasil dan tungku sudah berhenti. ItemResult dihapus.");
                        }

                        // Hentikan api hanya jika benar-benar tidak masak
                        if (!interactableInstance.isCooking && isFireActive)
                        {
                            isFireActive = false;
                            Debug.Log("Menghentikan animasi api karena memasak selesai.");

                            if (fireAnimationCoroutine != null)
                            {
                                StopCoroutine(fireAnimationCoroutine);
                                fireAnimationCoroutine = null;
                                fireCookImage.sprite = fireNotActive;
                            }
                        }
                    }
                    else
                    {
                        // Jangan hapus, biarkan di tungku
                        Debug.Log("Tas penuh, item tetap di tungku.");
                    }
                    // cek apakah tungku masih aktif memasak


                    RefreshSlots();
                    interactableInstance.UpdateSpriteHasil();
                }
            });
        }



    }
    public void OpenCook(CookInteractable cookInteractable)
    {
        this.typeCooking = cookInteractable.typeCooking;
        if (PlayerController.Instance != null)
        {
            Debug.Log("PlayerController Instance found and assigned in CookUI Awake.");
            stats = PlayerController.Instance;
        }
        else
        {
            Debug.LogError("PlayerController Instance is null in CookUI Awake.");
        }

        BindToCook(cookInteractable);
        interactableInstance = cookInteractable;


        if (SoundManager.Instance != null)
            //SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

        cookIngredient.RefreshRecipe(typeCooking);

        RefreshSlots();
    }

    private void CloseCookUI()
    {
        GameController.Instance.ShowPersistentUI(true);
        UnbindFromCook();
        RefreshSlots();
        interactableInstance = null;

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

            if (typeCooking == TypeCooking.FoodCook)
            {
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
            else if (typeCooking == TypeCooking.SmeltCook)
            {
                Debug.Log("Menampilkan item smelt: ");
                if (item.IsInCategory(ItemCategory.Smelt) || item.IsInCategory(ItemCategory.Fuel)) // jika membuka smelt objek tampilkan item dengan category smelt Tampilkan juga fuel
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
        }

        if (interactableInstance.itemCook != null && interactableInstance.itemCook.count > 0)
        {
            itemCookTemplate.name = interactableInstance.itemCook.itemName;
            itemCookTemplate.GetChild(0).GetComponent<Image>().sprite = ItemPool.Instance.GetItemWithQuality(interactableInstance.itemCook.itemName, interactableInstance.itemCook.quality).sprite;
            itemCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = interactableInstance.itemCook.count.ToString();
            itemCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1); // Tampilkan gambar
        }
        else
        {
            // Kosongkan slot jika itemCook null atau count 0
            interactableInstance.itemCook = null; // Pastikan null jika count 0
            itemCookTemplate.name = "Slot_Item";
            itemCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            itemCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            itemCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0); // Sembunyikan gambar
        }

        if (interactableInstance.fuelCook != null && interactableInstance.fuelCook.count > 0)
        {
            fuelCookTemplate.name = interactableInstance.fuelCook.itemName;
            fuelCookTemplate.GetChild(0).GetComponent<Image>().sprite = ItemPool.Instance.GetItemWithQuality(interactableInstance.fuelCook.itemName, interactableInstance.fuelCook.quality).sprite;
            fuelCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = interactableInstance.fuelCook.count.ToString();
            fuelCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            // Kosongkan slot jika fuelCook null atau count 0
            interactableInstance.fuelCook = null; // Pastikan null jika count 0
            fuelCookTemplate.name = "Slot_Fuel";
            fuelCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            fuelCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            fuelCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }

        if (IsItemResultEmpty())
        {
            // Kosongkan slot jika fuelCook null atau count 0
            interactableInstance.itemResult = null; // Pastikan null jika count 0
            resultCookTemplate.name = "Slot_Result";
            resultCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            resultCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            resultCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);


        }
        else
        {

            resultCookTemplate.name = interactableInstance.itemResult.itemName;
            Item item = ItemPool.Instance.GetItemWithQuality(interactableInstance.itemResult.itemName, interactableInstance.itemResult.quality);
            HandleStartCooking(item);
        }



        UpdateFuelSlider();
        if (!interactableInstance.isCooking)
        {
            interactableInstance.StartCook();
        }
        else
        {
            return;
        }
    }

    public void OnClickItemInInventory(ItemData itemDataFromInventory)
    {
        isCookReady = true;
        Debug.Log($"Item diklik: {itemDataFromInventory.itemName}, Jumlah: {itemDataFromInventory.count}");
        Item item = ItemPool.Instance.GetItemWithQuality(itemDataFromInventory.itemName, itemDataFromInventory.quality);

        bool itemMoved = false; // Flag untuk menandai jika ada item yang berpindah

        if (validCookCategories.Any(category => item.IsInCategory(category)) || item.IsInCategory(ItemCategory.Smelt))
        {
            // Jika slot masak kosong, buat tumpukan baru
            if (interactableInstance.itemCook == null)
            {
                // Buat ItemData BARU untuk slot masak
                interactableInstance.itemCook = new ItemData(itemDataFromInventory.itemName, 1, itemDataFromInventory.quality, itemDataFromInventory.itemHealth);
                itemMoved = true;
            }
            // Jika slot masak berisi item yang SAMA, tambahkan tumpukan
            else if (interactableInstance.itemCook.itemName == itemDataFromInventory.itemName)
            {
                interactableInstance.itemCook.count += 1;
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
            // Cek apakah "tank" bahan bakar kosong
            if (interactableInstance.quantityFuel <= 0)
            {
                // Jangan tampilkan di slot fuelCook

                // Cek apakah slot cadangan KOSONG atau berisi item SAMA
                if (interactableInstance.fuelCook == null || interactableInstance.fuelCook.itemName == itemDataFromInventory.itemName)
                {
                    interactableInstance.quantityFuel = item.QuantityFuel;
                    itemMoved = true;
                }
                else
                {
                    Debug.LogWarning("Tidak bisa mengisi bahan bakar, slot cadangan terisi item fuel berbeda!");
                }
            }
            // JIKA TANK MASIH TERISI: Tambahkan item ini ke slot cadangan (fuelCook)
            else
            {
                // Jika slot fuel cadangan kosong
                if (interactableInstance.fuelCook == null)
                {
                    interactableInstance.fuelCook = new ItemData(itemDataFromInventory.itemName, 1, itemDataFromInventory.quality, itemDataFromInventory.itemHealth);
                    itemMoved = true;
                }
                // Jika slot fuel cadangan berisi item yang SAMA
                else if (interactableInstance.fuelCook.itemName == itemDataFromInventory.itemName)
                {
                    interactableInstance.fuelCook.count += 1;
                    itemMoved = true;
                }
                // Jika slot fuel cadangan berisi item BERBEDA
                else
                {
                    Debug.LogWarning("Slot bahan bakar sudah terisi item lain. Habiskan dulu!");
                }
            }
        }

        if (itemMoved)
        {
            itemDataFromInventory.count -= 1;
            if (itemDataFromInventory.count <= 0)
                stats.inventory.Remove(itemDataFromInventory);


            // Sekarang baru bersihkan & update tampilan
            if (isIngredientAdded)
            {
                DestroyCraftItems();
            }
            RefreshSlots();
        }
    }


    public void OnClickItemCook()
    {
        if (interactableInstance.itemCook != null && !interactableInstance.isCooking)
        {
            // Jika jumlah lebih dari 1, kembalikan 1 ke inventory
            if (interactableInstance.itemCook.count > 1)
            {
                ItemData newItemData = new ItemData(
                    interactableInstance.itemCook.itemName,
                    1,
                    interactableInstance.itemCook.quality,
                    interactableInstance.itemCook.itemHealth
                );
                bool isSuccess = ItemPool.Instance.AddItem(newItemData);

                if (isSuccess)
                {
                    // Hapus item dari tungku
                    interactableInstance.itemCook.count -= 1;
                }
                else
                {
                    // Jangan hapus, biarkan di tungku
                    Debug.Log("Tas penuh, item tetap di tungku.");
                }

            }
            // Jika hanya 1 item, kembalikan semuanya
            else if (interactableInstance.itemCook.count == 1)
            {
                bool isSuccess = ItemPool.Instance.AddItem(interactableInstance.itemCook);

                if (isSuccess)
                {
                    // Hapus item dari tungku
                    interactableInstance.itemCook = null; // Kosongkan slot masak
                }
                else
                {
                    // Jangan hapus, biarkan di tungku
                    Debug.Log("Tas penuh, item tetap di tungku.");
                }

            }

            isCookReady = false;
            RefreshSlots();
        }
    }

    public void OnClickFuelCook()
    {
        if (interactableInstance.fuelCook != null)
        {
            isCookReady = false;
            ItemData newItemData = new ItemData(interactableInstance.fuelCook.itemName, 1, interactableInstance.fuelCook.quality, interactableInstance.fuelCook.itemHealth);
            ItemPool.Instance.AddItem(newItemData);
            interactableInstance.fuelCook.count -= 1;
        }
        RefreshSlots();
    }

    private bool IsItemResultEmpty()
    {
        return interactableInstance.itemResult == null || string.IsNullOrEmpty(interactableInstance.itemResult.itemName);
    }




    public void BindToCook(CookInteractable interactable)
    {
        if (interactableInstance != null)
        {
            UnbindFromCook();
        }
        interactableInstance = interactable;

        // Daftarkan event listener
        interactableInstance.OnStartCooking += HandleStartCooking;
        interactableInstance.OnProgressUpdated += HandleProgressUpdate;
        interactableInstance.OnResultUpdated += HandleResultUpdate;
        interactableInstance.OnCookingFinished += HandleCookingFinished;

        RefreshVisualState();
    }
    private void UnbindFromCook()
    {
        if (interactableInstance == null) return;

        interactableInstance.OnStartCooking -= HandleStartCooking;
        interactableInstance.OnResultUpdated -= HandleResultUpdate;
        interactableInstance.OnCookingFinished -= HandleCookingFinished;
        interactableInstance.OnProgressUpdated -= HandleProgressUpdate;
    }

    private void RefreshVisualState()
    {
        if (interactableInstance == null) return;

        if (interactableInstance.isCooking)
        {
            Debug.Log("Menampilkan progress dari tungku yang sedang memasak...");

            if (interactableInstance.itemResult != null)
            {
                Item item = ItemPool.Instance.GetItemWithQuality(interactableInstance.itemResult.itemName, interactableInstance.itemResult.quality);
                HandleStartCooking(item);
                HandleProgressUpdate(interactableInstance.currentProgress);
            }
            else
            {
                Debug.LogWarning("itemResult masih null, tunggu hasil masakan pertama.");
                fireCookImage.sprite = fireNotActive;
            }
        }
        else
        {
            Debug.Log("Tungku ini belum memasak, tampilkan state idle.");
            fireCookImage.sprite = fireNotActive;
            Image hasilCookImage = resultCookTemplate.GetComponent<Image>();
            hasilCookImage.fillAmount = maxQuantityFuel;

            RefreshSlots();
            if (IsItemResultEmpty())
            {
                Debug.Log("itemResult kosong");
            }
            else
            {
                HandleResultUpdate(interactableInstance.itemResult);
            }
        }

    }


    private void HandleStartCooking(Item resultData)
    {
        Image fillImage = resultCookTemplate.GetChild(0).GetComponent<Image>();
        fillImage.sprite = resultData.sprite;
        if (interactableInstance.itemResult.count > 0)
        {
            fillImage.color = new Color(1, 1, 1, 1f);
        }
        else
        {
            fillImage.color = new Color(1, 1, 1, 0.5f);
        }
        fillImage.gameObject.SetActive(true);
    }

    private void HandleProgressUpdate(float progress)
    {
        Image hasilCookImage = resultCookTemplate.GetComponent<Image>();
        hasilCookImage.fillAmount = progress;
    }

    private void HandleResultUpdate(ItemData itemResult)
    {
        if (itemResult == null)
        {
            Debug.LogWarning("HandleResultUpdate dipanggil tapi itemResult null!");
            return;
        }

        Image hasilCookImage = resultCookTemplate.GetComponent<Image>();
        TMP_Text resultCountText = resultCookTemplate.GetChild(1).GetComponent<TMP_Text>();
        hasilCookImage.gameObject.SetActive(true);
        resultCountText.gameObject.SetActive(true);
        resultCountText.text = itemResult.count.ToString();
    }


    private void HandleCookingFinished()
    {
        RefreshSlots();
        Debug.Log("Memasak selesai — matikan animasi api, reset UI.");
        fireCookImage.sprite = fireNotActive;
    }



    //private IEnumerator PlayFireAnimation()
    //{
    //    while (true) // Loop tanpa batas (animasi berulang)
    //    {
    //        if (fireCookSprite.Length > 0) // Pastikan array sprite tidak kosong
    //        {
    //            fireCookImage.sprite = fireCookSprite[currentFrame]; // Setel sprite saat ini
    //            currentFrame = (currentFrame + 1) % fireCookSprite.Length; // Pindah ke frame berikutnya (loop)
    //        }
    //        yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
    //    }
    //}

    private void UpdateFuelSlider()
    {
        if (fuelSlider == null) return; // Pengaman jika slider tidak di-set

        // Jika ada bahan bakar, tampilkan dan atur slider
        if (maxQuantityFuel > 0)
        {
            fuelSlider.gameObject.SetActive(true); // Tampilkan slider
            fuelSlider.maxValue = maxQuantityFuel;
            fuelSlider.value = interactableInstance.quantityFuel;
        }
        // Jika tidak ada bahan bakar, sembunyikan slider
        else
        {
            fuelSlider.gameObject.SetActive(false); // Sembunyikan slider
            fuelSlider.value = 0;
        }
    }

    public void CekIngredient()
    {
        Button resultCookButton = resultCookTemplate.GetComponent<Button>();
        Button itemCookButton = itemCookTemplate.GetComponent<Button>();

        if (isIngredientAdded)
        {
            // Nonaktifkan tombol agar tidak bisa diklik saat bahan sudah dipilih
            resultCookButton.interactable = false;
            itemCookButton.interactable = false;

            // Setup slot item bahan
            itemCookTemplate.name = currentIngredient.itemName;

            // Pastikan child aktif sebelum diubah tampilannya
            var itemIconObj = itemCookTemplate.GetChild(0).gameObject;
            var itemTextObj = itemCookTemplate.GetChild(1).gameObject;
            itemIconObj.SetActive(true);
            itemTextObj.SetActive(true);

            var itemIcon = itemIconObj.GetComponent<Image>();
            var itemText = itemTextObj.GetComponent<TMP_Text>();

            itemIcon.sprite = ItemPool.Instance.GetItemWithQuality(currentIngredient.itemName, currentIngredient.quality).sprite;
            itemText.text = currentIngredientCount.ToString();
            itemIcon.color = new Color(1, 1, 1, 0.5f); // tampilkan semi-transparan


            // Setup slot hasil masakan
            resultCookTemplate.name = resultIngredient.itemName;

            // Aktifkan child terlebih dahulu
            var resultIconObj = resultCookTemplate.GetChild(0).gameObject;
            var resultTextObj = resultCookTemplate.GetChild(1).gameObject;
            resultIconObj.SetActive(true);
            resultTextObj.SetActive(true);

            var resultIcon = resultIconObj.GetComponent<Image>();
            var resultText = resultTextObj.GetComponent<TMP_Text>();

            resultIcon.sprite = ItemPool.Instance.GetItemWithQuality(resultIngredient.itemName, resultIngredient.quality).sprite;
            resultText.text = resultIngredientCount.ToString();
            resultIcon.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            // Jika bahan kosong, aktifkan interaksi lagi
            resultCookButton.interactable = true;
            itemCookButton.interactable = true;

            // Reset bahan
            currentIngredient = null;
            itemCookTemplate.name = "Slot_Item";

            itemCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            itemCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            itemCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);

            // Reset hasil
            resultIngredient = null;
            resultCookTemplate.name = "Slot_Fuel";
            resultCookTemplate.GetChild(1).GetComponent<TMP_Text>().text = "";
            resultCookTemplate.GetChild(0).GetComponent<Image>().sprite = null;
            resultCookTemplate.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
        }
    }

    public void DestroyCraftItems()
    {
        Debug.Log("Menghancurkan item craft di CookUI...");
        // Menghapus item yang ada di setiap ItemCraft slot
        isIngredientAdded = false;
        currentIngredient = null;
        resultIngredient = null;
        currentIngredientCount = 0;
        resultIngredientCount = 0; // Asumsikan hasil crafting selalu 1
        CekIngredient();

    }

}
