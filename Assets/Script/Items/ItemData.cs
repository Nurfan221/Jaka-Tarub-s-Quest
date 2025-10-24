// Ini BUKAN MonoBehaviour. Ini hanya sebuah struktur data sederhana.
using UnityEngine;
[System.Serializable]
public class ItemData
{
    public string itemName; // Cukup simpan namanya
    public int count;      // Dan jumlahnya
    public ItemQuality quality;
    public int itemHealth; // Untuk menyimpan nilai kesehatan item (jika diperlukan)

    public ItemData(string name, int amount, ItemQuality quality, int itemHealth)
    {
        itemName = name;
        count = amount;
        this.quality = quality;
        this.itemHealth = itemHealth;
    }
    public ItemData(ItemData other)
    {
        this.itemName = other.itemName;
        this.count = other.count;
        this.quality = other.quality;
        this.itemHealth = other.itemHealth;
    }

    public ItemData()
    {
    }

}