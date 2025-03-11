using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Daftar Hubungan")]
    public Player_Inventory player_Inventory;
    List<Item> itemInShop = new();

    [Header("Logika Shop")]
    public Transform contentSellUI;
    public Transform TemplateSellUI;
    public Transform contentBuyUI;
    public Transform templateBuyUI;


    public void OpenShop()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");
        GameController.Instance.ShowPersistentUI(false);
        gameObject.SetActive(true);
        RefreshItemInventory();


    }

    private void RefreshItemInventory()
    {
        foreach (Transform child in contentBuyUI)
        {
            if (child == templateBuyUI) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in contentSellUI)
        {
            if (child == TemplateSellUI) continue;
            Destroy(child.gameObject);
        }
        foreach (Item item in Player_Inventory.Instance.itemList)
        {
            Transform itemInInventory = Instantiate(TemplateSellUI, contentSellUI);
            itemInInventory.name = item.itemName;
            itemInInventory.gameObject.SetActive(true);
            itemInInventory.GetChild(0).GetComponent<Image>().sprite = item.sprite;
            itemInInventory.GetChild(1).GetComponent<TMP_Text>().text = item.itemName;
            itemInInventory.GetChild(2).GetComponent<TMP_Text>().text = item.stackCount.ToString();
            itemInInventory.GetChild(3).GetComponent<TMP_Text>().text = "Rp." + item.SellValue.ToString();

            itemInInventory.GetChild(4).GetComponent<Button>().onClick.AddListener(() =>
            {
                SellItem(item);
            });

        }
    }

    private void SellItem(Item itemToSell)
    {
        itemInShop.Add(itemToSell);
    }

}
