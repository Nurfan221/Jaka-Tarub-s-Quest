// Ini BUKAN MonoBehaviour. Ini hanya sebuah struktur data sederhana.
[System.Serializable]
public class ItemData
{
    public string itemName; // Cukup simpan namanya
    public int count;      // Dan jumlahnya
    public ItemQuality quality;

    public ItemData(string name, int amount, ItemQuality quality)
    {
        itemName = name;
        count = amount;
        this.quality = quality;
    }
}