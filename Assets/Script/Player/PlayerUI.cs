using UnityEngine;
using UnityEngine.UI; // Tambahkan ini untuk Button
using TMPro;
using static UnityEditor.Progress;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }

    //public InventoryUI inventoryUI {  get; private set; }
    private PlayerData_SO stats;
   
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
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }
    [Header("Action Button")]
    public Button dashButton;
    public Image specialAttackUI;
    public Image healthUI;
    public Image staminaUI;
    public Image equippedUI;
    public Button actionInputButton;
    public Image itemUseUI;
    public Button weaponSlider;  // Slider untuk memilih senjata
    public Button itemSlider; // slider untuk mengganti item 

    public TMP_Text promptText;
    public Button promptButton; // Tambahkan ini, Button untuk membungkus promptText
    public Transform capacityUseItem;
    public Button switchWeaponImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    public Button switchUseItemImage; // Referensi ke Image yang digunakan untuk mengganti senjata

    [Header("Inventory UI")]
    public Button inventoryButton;  // Drag and drop the button in the inspector
    public Button closeInventoryButton;  // Drag and drop the close button in the inspector


    // Variabel untuk menyimpan coroutine yang sedang berjalan
    private Coroutine healthUICoroutine;
    private Coroutine staminaUICoroutine;

    // Kecepatan animasi bar
    public float barAnimationSpeed = 2f;

    public void Start()
    {
        UpdateEquippedWeaponUI();
        // Setup listener untuk tombol-tombol
        if (inventoryButton != null)
        {
            // Langsung panggil fungsi yang ada di SINI
            inventoryButton.onClick.AddListener(MechanicController.Instance.HandleOpenInventory);
        }

        if (closeInventoryButton != null)
        {
            closeInventoryButton.onClick.AddListener(MechanicController.Instance.HandleCloseInventory);
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

        if(equippedUI != null)
        {
            Button buttonAttack = equippedUI.GetComponent<Button>();
            buttonAttack.onClick.AddListener(PlayerController.Instance.HandleAttackButton);
        }

        if (specialAttackUI != null)
        {
            Button buttonSpesialAttack = specialAttackUI.GetComponent<Button>();
            buttonSpesialAttack.onClick.AddListener(PlayerController.Instance.HandleSpesialAttackButton);
        }
      
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

    public void UpdateCapacityBar(ItemData item)
    {
        capacityUseItem.gameObject.SetActive(true);
        Image capacityBarImage = capacityUseItem.Find("KapacityBar").GetComponent<Image>();
        //if (capacityBarImage != null && stats.equippedCombat != null)
        //{
        //    //Debug.Log("Target Image: " + capacityBarImage.name);
        //    if (item.itemName == playerInventory.equippedWeapon.itemName)
        //    {
        //        capacityBarImage.fillAmount = playerInventory.equippedWeapon.health / playerInventory.equippedWeapon.maxhealth;
        //        //Debug.Log("sisa health sekarang : " + playerInventory.equippedWeapon.health);
        //    }
        //}
    }

    public void TakeCapacityBar(Item item)
    {
        Debug.Log("health item di kurangi");
        //if (item.itemName == playerInventory.equippedWeapon.itemName)
        //{
        //    playerInventory.equippedWeapon.health -= 1;
        //    UpdateCapacityBar(playerInventory.equippedWeapon);
        //}
        //else
        //{

        //    Debug.Log("item tidak di temukan " + "item di kirim  : " + item.itemName + "item di equippedWeapon : " + playerInventory.equippedWeapon.itemName);
        //}
    }
    public void Reinitialize()
    {
        Debug.Log("PlayerUI: Melakukan inisialisasi ulang referensi...");

        // 1. Dapatkan koneksi ke manajer lain melalui GameController
        //this.playerInventory = GameController.Instance.playerInventory;
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
        stats.equippedWeaponTemplate = activeWeaponData;

        UpdateSingleIcon(equippedUI, activeWeaponData);
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
        stats.equippedItemTemplate = activeItemData;

        UpdateSingleIcon(itemUseUI, activeItemData);
    }


   
    private void UpdateSingleIcon(Image targetImage, ItemData itemData)
    {
        // Pastikan referensi UI tidak null
        if (targetImage == null) return;

        // Cek apakah slotnya kosong
        if (itemData == null || itemData.itemName == "Empty")
        {
            // Jika kosong, sembunyikan ikonnya
            targetImage.gameObject.SetActive(false);
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
}
