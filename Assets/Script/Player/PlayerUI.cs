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
        if (capacityBarImage != null)
            {
            Debug.Log("Target Image: " + capacityBarImage.name);
            foreach (var itemToFilmount in playerInventory.itemList)
                {
                    if (item.itemName == itemToFilmount.itemName)
                    {
                        capacityBarImage.fillAmount = itemToFilmount.health / itemToFilmount.maxhealth;
                        Debug.Log("sisa health sekarang : " + itemToFilmount.health);
                    }
                }
            }else
            {
            Debug.Log("capacity bar image tidak di temukan ");
            }
        }

        public void TakeCapacityBar( Item item)
        {
            foreach (var itemInInventory in playerInventory.itemList)
            {
                if(item.itemName == itemInInventory.itemName)
                {
                    itemInInventory.health -= 1;
                    UpdateCapacityBar(itemInInventory);
                }
            }
        }
}
