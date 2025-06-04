using UnityEngine;
using UnityEngine.UI; // Tambahkan ini untuk Button
using TMPro;
using static UnityEditor.Progress;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;
    [SerializeField] public Player_Inventory playerInventory;

    public Image dashUI;
    public Image specialAttackUI;
    public TMP_Text promptText;
    public Button promptButton; // Tambahkan ini, Button untuk membungkus promptText
    public Image healthUI;
    public Image staminaUI;
    public Image equippedUI;
    public Transform capacityUseItem;
    public Image itemUseUI;
    public Button actionInputButton;
    public GameObject inventoryUI;
    public Button weaponSlider;  // Slider untuk memilih senjata
    public Button itemSlider; // slider untuk mengganti item 
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
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
            if (capacityBarImage != null && playerInventory.equippedCombat != null)
            {
                //Debug.Log("Target Image: " + capacityBarImage.name);
                if (item.itemName == playerInventory.equippedWeapon.itemName)
                {
                    capacityBarImage.fillAmount = playerInventory.equippedWeapon.health / playerInventory.equippedWeapon.maxhealth;
                    //Debug.Log("sisa health sekarang : " + playerInventory.equippedWeapon.health);
                }
            }
        }

        public void TakeCapacityBar( Item item)
        {
        Debug.Log("health item di kurangi");
        if (item.itemName == playerInventory.equippedWeapon.itemName)
        {
            playerInventory.equippedWeapon.health -= 1;
            UpdateCapacityBar(playerInventory.equippedWeapon);
        }
        else
        {

            Debug.Log("item tidak di temukan " + "item di kirim  : " + item.itemName + "item di equippedWeapon : " + playerInventory.equippedWeapon.itemName);
        }
    }
}
