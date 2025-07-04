// Ini BUKAN MonoBehaviour. Ini hanya sebuah struktur data sederhana.
[System.Serializable]
public class ItemData
{
    public string itemName; // Cukup simpan namanya
    public int count;      // Dan jumlahnya

    public ItemData(string name, int amount)
    {
        itemName = name;
        count = amount;
    }
}