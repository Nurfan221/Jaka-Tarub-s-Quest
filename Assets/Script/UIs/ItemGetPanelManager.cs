using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemGetPanelManager : MonoBehaviour
{
    public static ItemGetPanelManager Instance { get; private set; }

    // Prefab template untuk setiap slot item
    public GameObject itemSlotTemplate;

    // Kontainer di UI tempat slot item akan dibuat
    public Transform contentParent;

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

    // Metode ini akan menampilkan item yang diberikan
    public void ShowItems(ItemData itemToShow)
    {
        ItemData itemData = itemToShow;
        // Pastikan tidak ada item sebelumnya yang tersisa
        GameObject newSlot = Instantiate(itemSlotTemplate, contentParent);

        // Pastikan template memiliki komponen-komponen ini
        Image itemImage = newSlot.transform.Find("ItemImage").GetComponent<Image>();
        TMP_Text itemName = newSlot.transform.Find("ItemName").GetComponent<TMP_Text>();

        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToShow.itemName, itemToShow.quality);

        // Atur data item
        Image templateImage = newSlot.transform.Find("ItemImage").GetComponent<Image>();
        TMP_Text templateName = newSlot.transform.Find("NameItemGet").GetComponent<TMP_Text>();
        // Aktifkan panel utama
        gameObject.SetActive(true);
    }

   
}