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

[System.Serializable]
public class TemplateTreesObject
{
    public string treeName;
    public List<GrowthStageTrees> growthStages; // Daftar tahap pertumbuhan

}

[System.Serializable]
public class GrowthStageTrees
{
    public string stageName; // Nama tahap pertumbuhan
    public GrowthTree growthTree; // Data pertumbuhan untuk tahap ini
    public GameObject stagePrefab; // Prefab untuk tahap ini
    public GameObject batangPrefab; // Prefab untuk batang pohon
    public GameObject AkarPrefab; // Prefab untuk akar pohon
}

[System.Serializable]
public class TreePlacementData
{
    public string treeName; // Jenis pohon (misal: "Pohon Apel")
    public Vector2 position; // Posisi di dunia
    public GrowthTree initialStage; // Tahap tumbuh saat pertama kali muncul
    public bool sudahTumbang; // Apakah pohon sudah ditumbangkan
    // Anda bisa tambahkan variabel lain jika perlu, misal: float initialGrowthTimer;
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    // Seret aset-aset database Anda ke sini di Inspector
    public CraftingDatabaseSO craftingDatabase;
    public CookingDatabaseSO cookingDatabase;
    public EmoticonDatabaseSO emoticonDatabase;
    public SpriteDatabaseSO spriteDatabase;
    public GrowthTreesDatabase templateTreesObject;
    public WorldTreeDatabaseSO worldTreeDatabase;
    public GameObject itemWorldPrefab; // Prefab untuk item di dunia
    public GameObject plantWorldPrefab; // Prefab untuk tanaman di dunia

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

    public GameObject GetTreePrefab(string treeName)
    {
        //    Abaikan perbedaan huruf besar/kecil untuk keamanan.
        TemplateTreesObject foundTree = templateTreesObject.growthTrees.FirstOrDefault(tree =>
            tree.treeName.Equals(treeName, System.StringComparison.OrdinalIgnoreCase)
        );

        // Jika template pohon tidak ditemukan, beri peringatan dan kembalikan null.
        if (foundTree == null)
        {
            Debug.LogWarning($"DatabaseManager: Tidak dapat menemukan pohon dengan nama '{treeName}'");
            return null;
        }

        //  Di dalam template pohon yang sudah ditemukan, cari tahap pertumbuhan "Seed".
        GrowthStageTrees seedStage = foundTree.growthStages.FirstOrDefault(stage =>
            stage.growthTree == GrowthTree.Seed
        );

        //  Jika tahap "Seed" tidak ditemukan di dalam pohon tersebut, beri peringatan dan kembalikan null.
        if (seedStage == null)
        {
            Debug.LogWarning($"DatabaseManager: Pohon '{treeName}' tidak memiliki tahap pertumbuhan 'Seed'.");
            return null;
        }

        //  Jika semuanya berhasil, kembalikan prefab dari tahap "Seed".
        return seedStage.stagePrefab;
    }

    public int GetTotalGrowthStages(string treeName)
    {
        TemplateTreesObject foundTree = templateTreesObject.growthTrees.FirstOrDefault(tree =>
            tree.treeName.Equals(treeName, System.StringComparison.OrdinalIgnoreCase)
        );
        if (foundTree == null)
        {
            Debug.LogWarning($"DatabaseManager: Tidak dapat menemukan pohon dengan nama '{treeName}'");
            return 0;
        }
        return foundTree.growthStages.Count;
    }
    public GrowthStageTrees GetGrowthStageData(string treeName, GrowthTree stageToFind)
    {
        // Cari template pohon yang namanya cocok.
        TemplateTreesObject foundTree = templateTreesObject.growthTrees.FirstOrDefault(tree =>
            tree.treeName.Equals(treeName, System.StringComparison.OrdinalIgnoreCase));

        if (foundTree == null)
        {
            Debug.LogWarning($"DatabaseManager: Tidak dapat menemukan data untuk pohon bernama '{treeName}'");
            return null; // Mengembalikan null karena pohon tidak ada
        }

        // Di dalam pohon yang sudah ditemukan, cari tahap pertumbuhan yang cocok.
        GrowthStageTrees foundStage = foundTree.growthStages.FirstOrDefault(stage =>
            stage.growthTree == stageToFind);

        if (foundStage == null)
        {
            Debug.LogWarning($"DatabaseManager: Pohon '{treeName}' tidak memiliki data untuk tahap '{stageToFind}'.");
            return null; // Mengembalikan null karena tahap tidak ada
        }

        // Kembalikan seluruh "paket" data tahap pertumbuhan yang ditemukan.
        return foundStage;
    }

    public GameObject GetNextStagePrefab(string treeName, GrowthTree currentStageEnum)
    {
        Debug.Log($"Mencari tahap berikutnya untuk pohon '{treeName}' dari tahap '{currentStageEnum}'");
        TemplateTreesObject foundTree = templateTreesObject.growthTrees.FirstOrDefault(tree =>
            tree.treeName.Equals(treeName, System.StringComparison.OrdinalIgnoreCase));

        if (foundTree == null)
        {
            Debug.LogWarning($"DatabaseManager: Tidak dapat menemukan pohon dengan nama '{treeName}'");
            return null;
        }

        int currentIndex = foundTree.growthStages.FindIndex(stage => stage.growthTree == currentStageEnum);
        Debug.Log($"Current Index for {treeName} at stage {currentStageEnum} is {currentIndex}");

        // Cek jika tahap saat ini tidak ditemukan, atau jika ini sudah tahap terakhir.
        if (currentIndex == -1 || currentIndex + 1 >= foundTree.growthStages.Count)
        {
            // Tidak ada tahap berikutnya (sudah dewasa atau data tidak ditemukan).
            return null;
        }

        GrowthStageTrees nextStage = foundTree.growthStages[currentIndex + 1];
        Debug.Log($"Next stage for {treeName} after {currentStageEnum} is {nextStage.stageName}");
        return nextStage.stagePrefab;
    }

    public GameObject GetPrefabForTreeStage(string treeName, GrowthTree stageToFind)
    {
        //    Kita gunakan FirstOrDefault agar aman jika tidak ditemukan.
        TemplateTreesObject foundTree = templateTreesObject.growthTrees.FirstOrDefault(tree =>
            tree.treeName.Equals(treeName, System.StringComparison.OrdinalIgnoreCase));

        // Jika template pohonnya tidak ditemukan, hentikan proses dan kembalikan null.
        if (foundTree == null)
        {
            Debug.LogWarning($"DatabaseManager: Tidak dapat menemukan data untuk pohon bernama '{treeName}'");
            return null;
        }

        //  Di dalam data pohon yang sudah ditemukan, cari tahap pertumbuhan yang cocok.
        GrowthStageTrees foundStage = foundTree.growthStages.FirstOrDefault(stage =>
            stage.growthTree == stageToFind);

        //  Jika tahapnya tidak ditemukan di dalam data pohon tersebut, hentikan dan kembalikan null.
        if (foundStage == null)
        {
            Debug.LogWarning($"DatabaseManager: Pohon '{treeName}' tidak memiliki data untuk tahap '{stageToFind}'.");
            return null;
        }

        //. Jika semua berhasil, kembalikan prefab dari tahap yang ditemukan.
        return foundStage.stagePrefab;
    }


}