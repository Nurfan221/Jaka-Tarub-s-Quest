using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public class Recipe
{
    public string recipeName; // Nama resep
    public Item ingredient;    // Bahan-bahan yang dibutuhkan
    public float ingredientCount; // jumlah bahan yang di perlukan
    public Item result;    // Hasil dari resep ini
}

[System.Serializable]
public class  EmoticonTemplate
{
    public string emoticonName;
    public Sprite emoticonSprite;
}

[System.Serializable]
public class CraftRecipe
{
    public List<ItemData> ingredients; // Satu list untuk bahan dan jumlahnya
    public ItemData result;            // Satu object untuk hasil dan jumlahnya
}

[System.Serializable]
public class  SpriteImageTemplate
{
    public string nameImage;
    public List<imagePersen> imagePersens;
}

[System.Serializable]
public class  imagePersen
{
    public int persen;
    public Sprite sprites;
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    // Seret aset-aset database Anda ke sini di Inspector
    public CraftingDatabaseSO craftingDatabase;
    public CookingDatabaseSO cookingDatabase;
    public EmoticonDatabaseSO emoticonDatabase;
    public SpriteDatabaseSO spriteDatabase;
    public GameObject itemWorldPrefab; // Prefab untuk item di dunia

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

    public SpriteImageTemplate GetSpriteTempalte(string nameImage)
    {
        SpriteImageTemplate foundTemplate = spriteDatabase.spriteImageTemplates.FirstOrDefault(template =>
            template.nameImage.Equals(nameImage, System.StringComparison.OrdinalIgnoreCase)
        );

        // Jika template tidak ditemukan (hasilnya null), beri peringatan.
        if (foundTemplate == null)
        {
            Debug.LogWarning($"DatabaseManager: Tidak dapat menemukan SpriteImageTemplate dengan nama '{nameImage}'");
        }

        // Kembalikan hasilnya, baik itu template yang ditemukan atau null.
        return foundTemplate;
    }

    public GameObject GetItemWorldPrefab(string prefabName, ItemQuality itemQuality, int itemHealth)
    {
        if (itemWorldPrefab == null)
        {
            Debug.LogWarning("DatabaseManager: itemWorldPrefab belum diatur di Inspector!");
            ItemDropInteractable itemDropInteractable = itemWorldPrefab.GetComponent<ItemDropInteractable>();
            Item item = ItemPool.Instance.GetItemWithQuality(prefabName, itemQuality);
            if (itemDropInteractable != null && item != null)
            {
                itemDropInteractable.itemdata.itemName = item.itemName;
                itemDropInteractable.itemdata.count = 1; // Set jumlah awal ke 1
                itemDropInteractable.itemdata.quality = itemQuality;
                itemDropInteractable.itemdata.itemHealth = itemHealth;
            }
            else
            {
                Debug.LogWarning("DatabaseManager: Gagal mengatur itemdata pada itemWorldPrefab.");
            }
        }
        return itemWorldPrefab;
    }
}