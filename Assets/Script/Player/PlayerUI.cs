using UnityEngine;
using UnityEngine.UI; // Tambahkan ini untuk Button
using TMPro;
using static UnityEditor.Progress;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }

    public InventoryUI inventoryUI {  get; private set; }
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

    [Header("Inventory UI")]
    public Button inventoryButton;  // Drag and drop the button in the inspector
    public Button closeInventoryButton;  // Drag and drop the close button in the inspector


    public void Start()
    {
        // Setup listener untuk tombol-tombol
        if (inventoryButton != null)
        {
            // Langsung panggil fungsi yang ada di SINI
            inventoryButton.onClick.AddListener(OpenInventory);
        }

        if (closeInventoryButton != null)
        {
            closeInventoryButton.onClick.AddListener(CloseInventory);
        }

        if (dashButton != null)
        {
            // Tombol ini langsung memerintah PlayerController
            dashButton.onClick.AddListener(PlayerController.Instance.HandleDash);
        }
    }

    public void RegisterInventoryUI(InventoryUI inventory)
    {
        this.inventoryUI = inventory;
        Debug.Log($"PlayerController: Paket Player '{inventory.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterInventoryUI(InventoryUI inventory)
    {
        if (this.inventoryUI == inventory)
        {
            this.inventoryUI = null;
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

    public void UpdateCapacityBar(Item item)
    {
        capacityUseItem.gameObject.SetActive(true);
        Image capacityBarImage = capacityUseItem.Find("KapacityBar").GetComponent<Image>();
        //if (capacityBarImage != null && playerInventory.equippedCombat != null)
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

    private void OpenInventory()
    {
        //GameController.Instance.PindahKeScene("Village");
        inventoryUI.OpenInventory();
    }

    private void CloseInventory()
    {
        inventoryUI.CloseInventory();
    }
}
