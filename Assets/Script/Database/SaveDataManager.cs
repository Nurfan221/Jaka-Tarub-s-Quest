using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;
    private string saveFilePath;

    private void OnEnable()
    {
        TimeManager.OnDayChanged += SaveGame;
    }
    private void OnDisable()
    {
        TimeManager.OnDayChanged -= SaveGame;
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "game_save.json");
    }

    public void SaveGame()
    {
        Debug.Log("Menyimpan data ke: " + saveFilePath);
        GameSaveData saveData = new GameSaveData();
        CaptureAllSaveableStates(saveData);
        SaveToFile(saveData);
    }

    public GameSaveData LoadGame()
    {
        // Cek apakah file save ada
        if (File.Exists(saveFilePath))
        {
            Debug.Log("Memuat data dari: " + saveFilePath);
            string json = File.ReadAllText(saveFilePath);
            GameSaveData gameData = JsonUtility.FromJson<GameSaveData>(json);
            return gameData; // Kembalikan data yang sudah di-load
        }
        else
        {
            Debug.Log("File save tidak ditemukan.");
            return null; // Kembalikan null jika tidak ada file
        }
    }

   

    private GameSaveData LoadFromFile()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("File save tidak ditemukan.");
            return null;
        }

        string json = File.ReadAllText(saveFilePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    // "Manajer Sensus" mengumpulkan data (SUDAH DIPERBAIKI)
    private void CaptureAllSaveableStates(GameSaveData saveData)
    {
        foreach (ISaveable saveable in FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>())
        {
            // Pengecekan pertama: Apakah ini sebuah pohon?
            if (saveable is TreeBehavior tree)
            {
                if (tree.CaptureState() is TreeSaveData treeData)
                {
                    treeData.id = tree.GetComponent<UniqueID>().ID;
                    saveData.savedTrees.Add(treeData);
                }
            }
            // Pengecekan kedua: Apakah ini pemain?
            else if (saveable is PlayerController player)
            {
                    Debug.Log("[SAVE] Ditemukan PlayerController. Memanggil CaptureState...");
                if (player.CaptureState() is PlayerSaveData playerData)
                {
                    // Anda mungkin tidak perlu ID untuk pemain jika hanya ada satu,
                    // tapi ini adalah praktik yang baik.
                    // playerData.id = player.GetComponent<UniqueID>().ID; 
                    saveData.savedPlayerData.Add(playerData);
                    Debug.Log("Data pemain telah ditangkap untuk penyimpanan." + playerData.inventory.Count);
                }
            }else if (saveable is StorageInteractable storage)
            {
                if (storage.CaptureState() is StorageSaveData storageData)
                {
                    storageData.id = storage.GetComponent<UniqueID>().ID;
                    saveData.savedStorages.Add(storageData);
                }
            }
            // Tambahkan 'else if' lain untuk Chest, Bunga, dll. di masa depan.
        }
    }

    private void SaveToFile(GameSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);

        // --- DEBUG LOG KUNCI #3 ---
        // Lihat seperti apa isi file JSON yang akan disimpan
        Debug.Log("[Save Process] Konten JSON yang akan disimpan:\n" + json);
        // -------------------------

        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game berhasil disimpan ke: " + saveFilePath);
    }

    //public void RestoreAllSaveableStates(GameSaveData saveData)
    //{
    //    // Buat dictionary dari objek yang bisa di-save untuk pencarian cepat
    //    var saveableEntities = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>()
    //        .ToDictionary(e => (e as MonoBehaviour).GetComponent<UniqueID>()?.ID);

    //    foreach (var savedEntity in saveData.savedEntities)
    //    {
    //        // Cari objek di scene yang memiliki ID yang cocok
    //        if (saveableEntities.TryGetValue(savedEntity.id, out ISaveable saveable))
    //        {
    //            // Minta objek tersebut untuk me-restore datanya
    //            saveable.RestoreState(savedEntity.state);
    //        }
    //    }
    //}
}