using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : Interactable
{

    public List<ItemData> currentSeasonItems; // List yang sedang aktif
    public int minItemShop;
    public int maxItemShop;
    public TypeShop typeShop;
    private Season currenSeason;

    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
        TimeManager.OnSeasonChanged += HandleNewSeason;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
        TimeManager.OnSeasonChanged -= HandleNewSeason;
    }

    private void Start()
    {
        HandleNewDay();
    }
    protected override void Interact()
    {
        MechanicController.Instance.HandleOpenShop(typeShop, currentSeasonItems);
    }
    public void HandleNewDay()
    {
        //RestockDaily(currentSeason)
        currenSeason = TimeManager.Instance.GetCurrentSeason();
        UpdateItemInShop(currenSeason);

    }

    public void HandleNewSeason()
    {
        currenSeason = TimeManager.Instance.GetCurrentSeason();
    }

    public void UpdateItemInShop(Season season)
    {
        ShopTypeDatabase shopTypeDatabase = DatabaseManager.Instance.GetTypeShopDatabase(typeShop);
        Debug.Log("Update Item di Shop untuk season: " + season + " dan tipe shop: " + shopTypeDatabase.typeShopName);
        ItemShopDatabase itemShopDatabase = DatabaseManager.Instance.GetCurrentItemShopDatabase(season, shopTypeDatabase);
        AddItemToList(itemShopDatabase.itemsForSale);
    }
    public void AddItemToList(List<Item> Items)
    {
        // Membuat salinan dari item yang ada di quest.itemQuests sebelum menghapus item lama

        foreach (var item in Items)
        {
            //Debug.Log($"Sebelum Clear Item: {item.itemName}, Jumlah: {item.stackCount}");
            ItemData itemData = new ItemData(item.itemName, 1, item.quality, item.health);
            int randomCount = UnityEngine.Random.Range(minItemShop, maxItemShop + 1);
            itemData.count = randomCount;
            currentSeasonItems.Add(itemData);

        }

        foreach (var item in DatabaseManager.Instance.itemShopDatabase.itemWajib)
        {
            ItemData itemData = new ItemData(item.itemName, 1, item.quality, item.health);
            int randomCount = UnityEngine.Random.Range(minItemShop, maxItemShop + 1);
            itemData.count = randomCount;
            currentSeasonItems.Add(itemData);
        }


    }
}
