using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI; // Tambahkan ini untuk Button

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }

    //public InventoryUI inventoryUI {  get; private set; }
    private PlayerController stats;
    private PlayerData_SO playerData;

    private void Awake()
    {
        // Logika Singleton lengkap
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject.transform.root.gameObject);
        }

        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
            playerData = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }

    [Header("References to other managers")]
    public GameObject[] persistentUI;
    public GameObject pauseUI;

    [Header("Environment penting dalam game")]
    public Transform player;


    [Header("Action Button")]
    public Button dashButton;
    public Image specialAttackUI;
    public Image healthUI;
    public Image staminaUI;
    public Image equippedUI;
    public Image imageEquippedUI;
    public Button actionInputButton;
    public Image itemUseUI;
    public Image imageItemUse;
    public Button weaponSlider;  // Slider untuk memilih senjata
    public Button itemSlider; // slider untuk mengganti item 
    public TMP_Text moneyText; // Reference to a UI Text element to display money
    public TMP_Text promptText;
    public Button promptButton; // Tambahkan ini, Button untuk membungkus promptText
    //public Transform capacityUseItem;
    public Button switchWeaponImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    public Button switchUseItemImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    public SpriteImageTemplate spriteImageTemplate;
    
    

    [Header("Setting Button")]
    public Button buttonSetting;
    public Button resumeSetting;
    public Button goMainMenu;

    [Header("Tilemap Layer")]
    public Tilemap tilemapLayerPlayer;

    [Header("Inventory UI")]
    public Button inventoryButton;  // Drag and drop the button in the inspector


    // Variabel untuk menyimpan coroutine yang sedang berjalan
    private Coroutine healthUICoroutine;
    private Coroutine staminaUICoroutine;

    // Kecepatan animasi bar
    public float barAnimationSpeed = 2f;

    [Header("References to quest UI")]
    // Komponen UI yang diperlukan
    public RectTransform questUI;
    public Button questButton;
    // Variabel untuk animasi
    public float animationDuration = 0.5f; // Durasi animasi dalam detik
    public float targetHeight = 300f; // Ketinggian akhir UI
    public float startHeight = 0f; // Ketinggian awal UI
    public ShowErrorUI errorUI;

    private bool isUIActive = false;

    public void Start()
    {
        spriteImageTemplate = DatabaseManager.Instance.GetSpriteTempalte("HealthItemUI");
        imageItemUse = itemUseUI.transform.Find("UseButton").GetComponent<Image>();
        imageEquippedUI = equippedUI.transform.Find("AttackButton").GetComponent<Image>();


        
        UpdateEquippedWeaponUI();
        UpdateItemUseUI();
        // Setup listener untuk tombol-tombol
        if (inventoryButton != null)
        {
            // Langsung panggil fungsi yang ada di SINI
            inventoryButton.onClick.AddListener(MechanicController.Instance.HandleOpenInventory);
        }



        if (dashButton != null)
        {
            // Tombol ini langsung memerintah PlayerController
            dashButton.onClick.AddListener(PlayerController.Instance.HandleDash);
        }

        if (switchWeaponImage != null)
        {
            // Tambahkan event listener untuk klik pada Image
            Button button = switchWeaponImage.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ToggleWeapon);
            }
            else
            {
                Debug.LogError("Image component does not have a Button component attached.");
            }
        }
        else
        {
            Debug.LogError("SwitchWeaponImage is not assigned.");
        }


        if (switchUseItemImage != null)
        {
            // Tambahkan event listener untuk klik pada Image
            Button button = switchUseItemImage.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ToggleUseItem);
            }
            else
            {
                Debug.LogError("Image component does not have a Button component attached.");
            }
        }
        else
        {
            Debug.LogError("switchUseItemImage is not assigned.");
        }

        if (imageEquippedUI != null)
        {
            Button buttonAttack = imageEquippedUI.GetComponent<Button>();
            buttonAttack.onClick.AddListener(() =>
            {
                Debug.Log("Button Attack Clicked");
                PlayerController.Instance.HandleAttackButton();

            });
            Debug.Log("Button Attack Ditemukan");
        }

        if (specialAttackUI != null)
        {
            Button buttonSpesialAttack = specialAttackUI.GetComponent<Button>();
            buttonSpesialAttack.onClick.AddListener(PlayerController.Instance.HandleSpesialAttackButton);
        }

        if (questButton != null)
        {
            questButton.onClick.AddListener(ToggleQuestUI);
        }

        if (imageItemUse != null)
        {

            Button buttonItemUse = imageItemUse.GetComponent<Button>();
            buttonItemUse.onClick.AddListener(() =>
            {
                Debug.Log("Item Use Button Clicked");
                PlayerController.Instance.HandleButtonUseItem();
            });
        }

        if (buttonSetting != null)
        {
            buttonSetting.onClick.AddListener(() =>
            {
                Debug.Log("Button Setting Clicked");
                GameController.Instance.PauseWithUI();
            });
        }

        if (resumeSetting != null)
        {
            resumeSetting.onClick.AddListener(() =>
            {
                Debug.Log("Button Resume Clicked");
                pauseUI.gameObject.SetActive(false);
                GameController.Instance.ResumeGame();
            });
        }

        if (goMainMenu != null)
        {
            goMainMenu.onClick.AddListener(() =>
            {
                Debug.Log("Button Go Main Menu Clicked");
                pauseUI.gameObject.SetActive(false);
                SceneManager.LoadScene("MainMenu");
            });
        }

        InitializePlayer(); // Fungsi ini akan menemukan GameObject Player yang baru

        Reinitialize();
    }


    public void InitializePlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }
        Debug.Log("Player found: " + player.name + "posisi playar : " + player.transform.position);
        //if (PlayerPrefs.GetInt("HaveSaved") == 99)
        //{
        //    player.position = latestPlayerPos;
        //}
    }
    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;

            // Aktifkan atau nonaktifkan tombol berdasarkan apakah teks kosong
            if (string.IsNullOrEmpty(text))
            {
                promptButton.gameObject.SetActive(false);
            }
            else
            {
                promptButton.gameObject.SetActive(true);
            }
        }
    }



    public void TakeCapacityBar(Item item)
    {
        Debug.Log("health item di kurangi");
        if (stats.equipped1)
        {
            if (item.itemName == stats.equippedItemData[0].itemName)
            {
                stats.equippedItemData[0].itemHealth -= 1;
                UpdateEquippedWeaponUI();
            }

        }
        else
        {
            if (item.itemName == stats.equippedItemData[1].itemName)
            {
                stats.equippedItemData[1].itemHealth -= 1;
                UpdateEquippedWeaponUI();
            }
            Debug.Log("item tidak di temukan " + "item di kirim  : " + item.itemName + "item di equippedWeapon : " + stats.equippedItemData[0].itemName);
        }
    }
    public void Reinitialize()
    {
        Debug.Log("PlayerUI: Melakukan inisialisasi ulang referensi...");

        // Dapatkan koneksi ke manajer lain melalui GameController
        // this.playerInventory = GameController.Instance.playerInventory;
    }




    public void UpdateEquippedWeaponUI()
    {
        ItemData activeWeaponData;
        if (stats.equipped1)
        {
            // Jika toggle melee/ranged aktif, gunakan slot 0.
            activeWeaponData = stats.equippedItemData[0];
        }
        else
        {
            // Jika tidak, gunakan slot 1.
            activeWeaponData = stats.equippedItemData[1];
        }

        // Simpan data ini ke "Papan Pengumuman" agar sistem lain tahu
        playerData.equippedWeaponTemplate = activeWeaponData;

        UpdateSingleIcon(equippedUI,imageEquippedUI, activeWeaponData);
        

      
    }


    public void UpdateItemUseUI()
    {
        ItemData activeItemData;
        if (stats.itemUse1)
        {
            activeItemData = stats.itemUseData[0];
        }
        else
        {
            activeItemData = stats.itemUseData[1];
        }

        // Simpan data ini ke "Papan Pengumuman"
        playerData.equippedItemTemplate = activeItemData;

        UpdateSingleIcon(equippedUI, imageItemUse, activeItemData);
        TMP_Text capacityBarImage = itemUseUI.transform.Find("ItemCount").GetComponent<TMP_Text>();
        capacityBarImage.text = activeItemData.count.ToString();

    }



    private void UpdateSingleIcon(Image imageUtama,Image targetImage, ItemData itemData)
    {
        Image capacityBarImage = equippedUI.transform.Find("capacityBarItem").GetComponent<Image>();

        if (imageUtama != null)
        {
            imageUtama.sprite = DatabaseManager.Instance.defaultSprite;
        }
        // Pastikan referensi UI tidak null
        if (targetImage == null) return;

        // Cek apakah slotnya kosong
        if (itemData == null || itemData.itemName == "Empty")
        {
            // Jika kosong, sembunyikan ikonnya
            targetImage.gameObject.SetActive(false);
            if (imageUtama == itemUseUI)
            {
                itemUseUI.sprite = DatabaseManager.Instance.defaultItemUseSprite;
                equippedUI.sprite = DatabaseManager.Instance.defaultEquipSprite;
                capacityBarImage.sprite = spriteImageTemplate.imagePersens[4].sprites; // Set sprite indikator kesehatan
                Debug.Log($"Item health is critical: {itemData.itemHealth}%");
            }
            else if (imageUtama == equippedUI)
            {
                equippedUI.sprite = DatabaseManager.Instance.defaultEquipSprite;
                itemUseUI.sprite = DatabaseManager.Instance.defaultItemUseSprite;
                capacityBarImage.sprite = spriteImageTemplate.imagePersens[4].sprites; // Set sprite indikator kesehatan
                Debug.Log($"Item health is critical: {itemData.itemHealth}%");
            }
        }
        else
        {
            // Jika terisi, dapatkan data visualnya dari ItemDatabase
            Item itemSO = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);

            if (itemSO != null)
            {
                // Tampilkan ikonnya
                targetImage.sprite = itemSO.sprite;
                targetImage.gameObject.SetActive(true);
                if (itemData.itemName != stats.playerData.emptyItemTemplate.itemName)
                {
                    float percentage = ((float)itemData.itemHealth / itemSO.maxhealth) * 100f;


                    // Gunakan if-else if untuk rentang nilai
                    if (percentage > 75) // Termasuk 100%
                    {
                        capacityBarImage.sprite = spriteImageTemplate.imagePersens[0].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is high: {percentage}%");
                    }
                    else if (percentage > 50) // Rentang 51% - 75%
                    {
                        capacityBarImage.sprite = spriteImageTemplate.imagePersens[1].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is medium: {percentage}%");
                    }
                    else if (percentage > 25) // Rentang 26% - 50%
                    {
                        capacityBarImage.sprite = spriteImageTemplate.imagePersens[2].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is low: {percentage}%");
                    }
                    else if (percentage > 10) // Rentang 11% - 25%
                    {
                        capacityBarImage.sprite = spriteImageTemplate.imagePersens[3].sprites; // Set sprite indikator kesehatan   
                    }
                    else // Rentang 0% - 25%
                    {
                        capacityBarImage.sprite = spriteImageTemplate.imagePersens[4].sprites; // Set sprite indikator kesehatan
                        Debug.Log($"Item health is critical: {percentage}%");
                    }
                }
            }
            else
            {
              
                // Jika data tidak ditemukan di database, sembunyikan untuk mencegah error
                targetImage.gameObject.SetActive(false);
            }
        }



    }

    public void ToggleWeapon()

    {

        stats.equipped1 = !stats.equipped1;

        UpdateEquippedWeaponUI();

        //spesialSkillWeapon.UseWeaponSkill(stats.equippedWeapon, false);

        Debug.Log("Weapon Toggle");


    }

    // Fungsi untuk mengganti item yang dapat digunakan

    public void ToggleUseItem()

    {

        stats.itemUse1 = !stats.itemUse1;

        UpdateItemUseUI();

        Debug.Log("Toggle uhuyyyyyyy");

    }




    // Ganti fungsi UpdateHealthDisplay dan UpdateStaminaDisplay yang lama
    // dengan versi baru yang memulai Coroutine.

    public void UpdateHealthDisplay(float currentHealth, float maxHealth)
    {
        if (healthUI == null) return;

        // Hitung nilai target baru
        float targetFillAmount = currentHealth / maxHealth;

        // Hentikan animasi health bar yang lama (jika ada) sebelum memulai yang baru
        if (healthUICoroutine != null)
        {
            StopCoroutine(healthUICoroutine);
        }

        // Mulai animasi untuk mengubah fill amount secara mulus
        healthUICoroutine = StartCoroutine(AnimateBar(healthUI, targetFillAmount));
    }

    public void UpdateStaminaDisplay(float currentStamina, float maxStamina)
    {
        if (staminaUI == null) return;

        float targetFillAmount = currentStamina / maxStamina;

        if (staminaUICoroutine != null)
        {
            StopCoroutine(staminaUICoroutine);
        }

        staminaUICoroutine = StartCoroutine(AnimateBar(staminaUI, targetFillAmount));
    }

    private IEnumerator AnimateBar(Image barImage, float targetValue)
    {
        float currentFill = barImage.fillAmount;
        float time = 0;

        // Loop akan berjalan sampai nilai saat ini sangat dekat dengan nilai target
        while (!Mathf.Approximately(currentFill, targetValue))
        {
            // Ubah nilai saat ini secara bertahap menggunakan Lerp
            currentFill = Mathf.Lerp(currentFill, targetValue, time * barAnimationSpeed);
            barImage.fillAmount = currentFill;

            time += Time.deltaTime;
            yield return null; // Tunggu frame berikutnya
        }

        // Pastikan nilai akhirnya presisi
        barImage.fillAmount = targetValue;
    }
    public void ToggleQuestUI()
    {
        if (isUIActive)
        {
            // Jika UI aktif, sembunyikan
            // Anda bisa membuat coroutine kebalikan dari AnimateShowUI
            StopAllCoroutines();
            StartCoroutine(AnimateHideUI());
        }
        else
        {
            // Jika UI tidak aktif, tampilkan
            StopAllCoroutines(); // Hentikan jika sedang beranimasi
            StartCoroutine(AnimateShowUI());
        }


        //  pengecekan Count > 0
        if (QuestManager.Instance.questActive != null && QuestManager.Instance.questActive.Count > 0)
        {
            // Cek juga apakah sideQuests di quest pertama tidak null
            var firstQuest = QuestManager.Instance.questActive[0];

            if (firstQuest.sideQuests != null)
            {
                foreach (var quest in firstQuest.sideQuests)
                {
                    // Cek Nama (Aman Typo Besar/Kecil)
                    if (string.Equals(quest.questName, "Tutorial Menanam Tanaman", StringComparison.OrdinalIgnoreCase))
                    {
                        TutorialManager.Instance.TriggerTutorial("Tutorial_Quest");
                        break;
                    }
                }
            }
        }

        isUIActive = !isUIActive;
    }

    // Coroutine untuk menyembunyikan UI (kebalikan dari AnimateShowUI)
    private IEnumerator AnimateHideUI()
    {
        float timer = 0f;
        Vector2 startSize = new Vector2(questUI.sizeDelta.x, targetHeight);
        Vector2 targetSize = new Vector2(questUI.sizeDelta.x, startHeight);
        float startPosY = -35;
        float targetPosY = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;

            float newHeight = Mathf.Lerp(startHeight, targetHeight, 1 - progress); // 1-progress untuk efek terbalik
            float newPosY = Mathf.Lerp(startPosY, targetPosY, progress);

            questUI.sizeDelta = new Vector2(questUI.sizeDelta.x, newHeight);
            questUI.anchoredPosition = new Vector2(questUI.anchoredPosition.x, newPosY);

            yield return null;
        }

        questUI.sizeDelta = targetSize;
        questUI.anchoredPosition = new Vector2(questUI.anchoredPosition.x, targetPosY);
        questUI.gameObject.SetActive(false);
    }
    private IEnumerator AnimateShowUI()
    {
        questUI.gameObject.SetActive(true);
        float timer = 0f;
        Vector2 startSize = new Vector2(questUI.sizeDelta.x, startHeight);
        Vector2 targetSize = new Vector2(questUI.sizeDelta.x, targetHeight);
        float startPosY = 0f;
        float targetPosY = -35; // Posisi Y agar terlihat 'menggulung' dari atas

        // Ubah posisi jangkar (anchor) ke bagian atas
        questUI.pivot = new Vector2(0.5f, 1f);
        questUI.anchorMin = new Vector2(0.5f, 1f);
        questUI.anchorMax = new Vector2(0.5f, 1f);
        questUI.sizeDelta = startSize;
        questUI.anchoredPosition = new Vector2(questUI.anchoredPosition.x, startPosY);

        // Loop untuk menggerakkan dan mengubah ukuran UI
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;

            // Perbarui ukuran dan posisi Y
            float newHeight = Mathf.Lerp(startHeight, targetHeight, progress);
            float newPosY = Mathf.Lerp(startPosY, targetPosY, progress);

            questUI.sizeDelta = new Vector2(questUI.sizeDelta.x, newHeight);
            questUI.anchoredPosition = new Vector2(questUI.anchoredPosition.x, newPosY);

            yield return null;
        }

        // Pastikan posisi dan ukuran akhir sudah tepat
        questUI.sizeDelta = targetSize;
        questUI.anchoredPosition = new Vector2(questUI.anchoredPosition.x, targetPosY);
    }

    public void ShowErrorUI(string message)
    {
        errorUI.gameObject.SetActive(true);
        errorUI.ShowError(message, 2f, 0.5f);
    }

}
