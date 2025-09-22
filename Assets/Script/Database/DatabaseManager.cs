using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    public TypePlant plantType;
    public List<GrowthStageTrees> growthStages; // Daftar tahap pertumbuhan

}

[System.Serializable]
public class GrowthStageTrees
{
    public string TreeID; // Nama tahap pertumbuhan
    public GrowthTree growthTree; // Data pertumbuhan untuk tahap ini

    public GameObject stagePrefab; // Prefab untuk tahap ini
    public GameObject batangPrefab; // Prefab untuk batang pohon
    public GameObject AkarPrefab; // Prefab untuk akar pohon
}

[System.Serializable]
public class TreePlacementData
{
    public string TreeID; // Jenis pohon (misal: "Pohon Apel")
    public Vector2 position; // Posisi di dunia
    public TypePlant typePlant;
    public GrowthTree initialStage; // Tahap tumbuh saat pertama kali muncul
    public bool sudahTumbang; // Apakah pohon sudah ditumbangkan
    public bool isGrow; // apakah pohon sedang tumbuh
    public int dayToRespawn; // Hari ke berapa pohon akan respawn
    // Anda bisa tambahkan variabel lain jika perlu, misal: float initialGrowthTimer;
}

[System.Serializable]
public class TemplateStoneObject
{
    public string stoneID;
    public TypeObject typeStone;
    public EnvironmentHardnessLevel hardnessLevel;
    public int healthStone;
    public GameObject stoneObject;
    public ItemData[] hasilTambang;
}
[System.Serializable]
public class TemplateStoneActive
{
    public string stoneID;
    public GameObject stoneObject;
    public Vector2 position;
}

[System.Serializable]
public class ListBatuManager
{
    public string listName;
    public TypeObject typeStone;
    public List<TemplateStoneActive> listActive;
}

[System.Serializable]
public class HoedTileData
{
    public string plantedItemName;
    public Vector3Int tilePosition;
    public int hoedTime;
    public bool watered;
    public bool isPlanted;
    public int growthProgress;
    public GrowthStage currentStage;
    public bool isReadyToHarvest;

    //public HoedTileData(Vector3Int pos, int time)
    //{
    //    tilePosition = pos;
    //    hoedTime = time;
    //    watered = false;
    //    isPlanted = false;
    //    growthProgress = 0;
    //    plantedItemName = null;
    //    currentStage = GrowthStage.Seed; // Inisialisasi dengan tahap awal
    //    isReadyToHarvest = false;
    //}
}

[System.Serializable]
public class TimeSaveData
{
    public int totalHari;
    public int hari;
    public int date;
    public int minggu;
    public int bulan;
    public int tahun;
}

public enum TypeObject
{
    None,
    Tree,
    Stone,
    Storage,
    Copper,
    Iron,
    Gold,
    AkarPohon
}
public enum TypePlant
{
    None,
    Apple,
    Oak,
    Cemara,
    Birch
}

public enum EnvironmentHardnessLevel
{
    Soft = 1,     // misalnya tanah liat
    Medium = 2,   // misalnya batu biasa
    Hard = 3,     // misalnya besi
    VeryHard = 4, // misalnya emas
    Extreme = 5   // misalnya berlian
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
    public WorldStoneDatabaseSO worldStoneDatabase;
    public TimeSaveData timeManagerDatabase;
    public FarmData_SO farmData_SO;
    public GameObject itemWorldPrefab; // Prefab untuk item di dunia
    public GameObject plantWorldPrefab; // Prefab untuk tanaman di dunia
    public GameObject storageWorldPrefab;




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
        Debug.Log($"Next stage for {treeName} after {currentStageEnum} is {nextStage.TreeID}");
        return nextStage.stagePrefab;
    }

    public GameObject GetPrefabForTreeStage(TypePlant treeName, GrowthTree stageToFind)
    {
        // Ambil nama pohon yang dicari sebagai string.
        string treeNameString = treeName.ToString();

        TemplateTreesObject foundTree = templateTreesObject.growthTrees.FirstOrDefault(tree =>

            tree.plantType.ToString().Equals(treeNameString, System.StringComparison.OrdinalIgnoreCase)
        );

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

    public ItemData[] GetHasilTambang(TypeObject typeStone, EnvironmentHardnessLevel environmentHardnessLevel)
    {
        foreach (var item in worldStoneDatabase.templateStoneObject)
        {
            if (item.typeStone == typeStone && item.hardnessLevel == environmentHardnessLevel)
            {
                return item.hasilTambang; // langsung keluar, hanya entri pertama
            }
        }

        return null; // jika tidak ada match
    }

    public GameObject GetObjectTambang(string id)
    {
        foreach (var item in worldStoneDatabase.templateStoneObject)
        {
            if (item.stoneID == id)
            {
                return item.stoneObject; // langsung keluar, hanya entri pertama
            }
        }
        return null; // jika tidak ada match
    }

    

}