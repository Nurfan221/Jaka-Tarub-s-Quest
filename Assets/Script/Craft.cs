using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;


public class Craft : MonoBehaviour
{
    [Header("Database Crafting")]
    [SerializeField] private RecipeDatabase recipeDatabaseInstance;
    [SerializeField] private Checkingredients checkingredients;
    [SerializeField] private InventoryUI inventoryUI;

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
    public Button buttonCraft;

    [Header("item untuk Craft")]
    public Item hasilCraftItem;
    public List<Item> ingredientItemList;

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
        //buttonCraft.onClick.RemoveAllListeners();
        //buttonCraft.onClick.AddListener(Crafting);

    }


    private void CloseCraftUI()
    {
        GameController.Instance.ShowPersistentUI(true);
        gameObject.SetActive(false);
        Debug.Log("Crafting ditutup");
        ResetImageItem();


    }

    public void CheckItemtoCraft(int jumlahItemActive)
    {



        string item1, item2, item3, item4;
        item1 = itemCraft1.name;
        item2 = itemCraft2.name;
        item3 = itemCraft3.name;
        item4 = itemCraft4.name;

        Debug.Log($"Item 1: {item1}, Item 2: {item2}, Item 3: {item3}, Item 4: {item4}");

        for (int i = 0; i < jumlahItemActive; i++)
        {
            switch (i)
            {
                case 0:
                    Debug.Log("Memeriksa itemCraft1...");
                    CheckItemInInventory(itemCraft1, item1); break;
                case 1:
                    Debug.Log("Memeriksa itemCraft2...");
                    CheckItemInInventory(itemCraft2, item2); break;
                case 2:
                    Debug.Log("Memeriksa itemCraft3...");
                    CheckItemInInventory(itemCraft3, item3); break;
                case 3:
                    Debug.Log("Memeriksa itemCraft4...");
                    CheckItemInInventory(itemCraft4, item4); break;
                default:
                    Debug.Log("Item null");
                    break;
            }
        }
    }

    private void CheckItemInInventory(GameObject slotTemplate, string itemName)
    {
        Debug.Log($"Memeriksa inventory untuk item: {itemName}");

        GameObject itemSlot = slotTemplate;
        bool itemFound = false;

        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            if (item.itemName == itemName)
            {
                Debug.Log($"Item ditemukan: {itemName}, Jumlah: {item.stackCount}");

                // Set jumlah item in inventory
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

                itemFound = true;
                break;
            }
        }

        if (!itemFound)
        {
            Debug.Log($"Item {itemName} tidak ditemukan, set jumlah ke 0");
            Transform textTransform = itemSlot.transform.Find("ItemInInventory");
            if (textTransform != null)
            {
                textTransform.gameObject.SetActive(true);
                TMP_Text targetText = textTransform.GetComponent<TMP_Text>();
                targetText.text = "0";
            }
            else
            {
                Debug.LogWarning("Text untuk item tidak ditemukan di dalam slot!");
            }
        }
    }

   public void Crafting()
{
    Debug.Log("⏳ Memulai proses crafting...");

        // **1️⃣ Cek apakah semua bahan tersedia sebelum crafting**
        foreach (var item in ingredientItemList)
        {
            Item inventoryItem = Player_Inventory.Instance.itemList.Find(x => x.itemName == item.itemName);

            if (inventoryItem == null || inventoryItem.stackCount < item.stackCount)

            {
                Debug.LogWarning($"❌ Bahan {item.itemName} tidak cukup! Crafting dibatalkan.");
                return; // **Keluar dari fungsi jika bahan tidak cukup**
            }
        }

        // **2️⃣ Kurangi bahan yang digunakan**
        foreach (var item in ingredientItemList)
        {
            for (int i = Player_Inventory.Instance.itemList.Count - 1; i >= 0; i--)
            {
                if (Player_Inventory.Instance.itemList[i].itemName == item.itemName)
                {
                    Player_Inventory.Instance.itemList[i].stackCount -= item.stackCount;

                    // Jika stackCount menjadi 0, hapus item dari inventory
                    if (Player_Inventory.Instance.itemList[i].stackCount <= 0)
                    {
                        Player_Inventory.Instance.itemList.RemoveAt(i);
                    }
                    break; // **Keluar dari loop setelah menemukan item**
                }
            }
        }



        // **3️⃣ Tambahkan hasil crafting ke inventory**
        Item craftedItem = Instantiate(hasilCraftItem);  // **Pastikan hasil crafting tidak mempengaruhi ItemPool**
        craftedItem.stackCount = hasilCraftItem.stackCount;

        Player_Inventory.Instance.AddItem(craftedItem); // **Gunakan AddItem agar item bisa di-stack**

        Debug.Log($"✅ Berhasil crafting {craftedItem.itemName} x{craftedItem.stackCount}");

        inventoryUI.RefreshInventoryItems();
        inventoryUI.UpdateSixItemDisplay();

        ResetImageItem();



    }

    public void ResetImageItem()
    {
        GameObject[] imageItemCraft = { itemCraft1, itemCraft2, itemCraft3, itemCraft4 };

        foreach (GameObject item in imageItemCraft)
        {
            ResetChildVisibility(item, "itemImage");
            ResetChildVisibility(item, "ItemInInventory");
            ResetChildVisibility(item, "IngridientCount");
        }

        ResetChildVisibility(buttonCraft.gameObject, "itemImage");
        ResetChildVisibility(buttonCraft.gameObject, "itemCount");
        ingredientItemList.Clear();
    }

    // Fungsi untuk menyederhanakan pencarian dan menonaktifkan objek
    private void ResetChildVisibility(GameObject parent, string childName)
    {
        Transform childTransform = parent.transform.Find(childName);
        if (childTransform != null)
        {
            childTransform.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"'{childName}' tidak ditemukan dalam {parent.name}!");
        }
    }

}

