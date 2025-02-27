using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class Craft : MonoBehaviour
{
    [Header("Database Crafting")]
    [SerializeField] private RecipeDatabase recipeDatabaseInstance;
    [SerializeField] private Checkingredients checkingredients;

    [Header("Kategori Item yang Valid")]
    public ItemCategory[] validCategories = {
        ItemCategory.Fruit,
        ItemCategory.Meat,
        ItemCategory.Vegetable,
        ItemCategory.Crafting_Material
    };


    [Header("Button Container")]
    [SerializeField] private Button buttonClose;

    [Header("Craft container")]


    // Menambahkan UI untuk slot crafting
    public GameObject itemCraft1;
    public GameObject itemCraft2;
    public GameObject itemCraft3;
    public GameObject itemCraft4;

    private bool hasilCraftValue = false; // Variabel untuk status crafting

    void Start()
    {
        if (buttonClose != null)
        {
            buttonClose.onClick.AddListener(CloseCraftUI);
            Debug.Log("Tombol close listener added.");
        }
        else
        {
            Debug.LogError("Tombol close belum terhubung");
        }
    }



    public void OpenCraft()
    {
        Debug.Log("craft active");
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);

    }


    private void CloseCraftUI()
    {
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
        Debug.Log("Crafting ditutup");

    }

    public void CheckItemtoCraft(int jumlahItemActive)
    {
        string item1,item2,item3,item4;
        item1 = itemCraft1.name;
        item2 = itemCraft2.name;
        item3 = itemCraft3.name;
        item4 = itemCraft4.name;

        for (int i = 0; i < jumlahItemActive; i++)
        {
            switch(i)
            {
                case 0:
                    CheckItemInInventory(itemCraft1, item1); break;
                case 1:
                    CheckItemInInventory(itemCraft2, item2); break;
                case 2:
                    CheckItemInInventory(itemCraft3, item3); break;
                case 3:
                    CheckItemInInventory(itemCraft4, item4); break;
                default:
                    Debug.Log("item null");
                    break;

            }
        }

    }
    private void CheckItemInInventory(GameObject slotTemplate, string itemName)
    {
        GameObject itemSlot = slotTemplate;
        bool itemFound = false; // Flag untuk mengecek apakah item ditemukan

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            if (item.itemName == itemName)
            {
                // Jika item ditemukan, tampilkan jumlahnya
                Transform textTransform = itemSlot.transform.Find("ItemInInventory");
                if (textTransform != null)
                {
                    textTransform.gameObject.SetActive(true);
                    TMP_Text targetText = textTransform.GetComponent<TMP_Text>();
                    targetText.text = item.stackCount.ToString();
                }
                else
                {
                    Debug.LogWarning("Text untuk item tidak ditemukan di dalam slot!");
                }

                itemFound = true; // Tandai bahwa item telah ditemukan
                break; // Keluar dari loop karena sudah menemukan item
            }
        }

        // Jika item tidak ditemukan, set jumlah menjadi 0
        if (!itemFound)
        {
            Transform textTransform = itemSlot.transform.Find("ItemInInventory");
            if (textTransform != null)
            {
                textTransform.gameObject.SetActive(true);
                TMP_Text targetText = textTransform.GetComponent<TMP_Text>();
                targetText.text = "0"; // Jika tidak ada item, tampilkan 0
            }
            else
            {
                Debug.LogWarning("Text untuk item tidak ditemukan di dalam slot!");
            }
        }
    }

}

