using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;
    private string saveFilePath;
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene utama yang akan dimuat.")]
    [SerializeField] private string mainGameSceneName = "MainGameScene";

    // Properti publik untuk mengakses nama scene secara aman (read-only dari luar)
    public string MainGameSceneName => mainGameSceneName;

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
    [ContextMenu("Hapus Save File (Mulai Baru)")] // Tombol untuk testing di Editor
    public void StartNewGame()
    {
        // Hapus file save yang ada jika ditemukan
        if (File.Exists(saveFilePath))
        {
            try
            {
                File.Delete(saveFilePath);
                Debug.Log("File save berhasil dihapus. Memulai game baru.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Gagal menghapus file save: {e.Message}");
                return;
            }
        }
        else
        {
            Debug.Log("Tidak ada file save yang ditemukan. Langsung memulai game baru.");
        }

        // Beri "catatan" bahwa ini adalah game baru.
        GameController.IsNewGame = true;

        //  Cukup muat scene. Jangan panggil fungsi apa pun setelah ini.
        SceneManager.LoadScene(mainGameSceneName);
    }
    // MEMUAT SCENE DARI MAIN MENU 
    public void LoadGameScene()
    {
        Debug.Log($"Memuat scene game utama: {mainGameSceneName}");
        SceneManager.LoadScene(mainGameSceneName);
    }

    // MENGECEK APAKAH SAVE FILE ADA 
    public bool SaveFileExists()
    {
        return File.Exists(saveFilePath);
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
            if (saveable is TreesManager tree)
            {
                Debug.Log("[SAVE] Ditemukan EnvironmentManager. Memanggil CaptureState...");
                if (tree.CaptureState() is List<TreePlacementData> treeData)
                {
                    saveData.savedTrees = tree.secondListTrees;
                    Debug.Log("Data pohon telah ditangkap untuk penyimpanan." + saveData.savedTrees.Count);
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
                Debug.Log("[SAVE] Ditemukan StorageInteractable. Memanggil CaptureState...");
                if (storage.CaptureState() is StorageSaveData storageData)
                {
                    storageData.id = storage.uniqueID;
                    saveData.savedStorages.Add(storageData);
                }
            }else if (saveable is BatuManager stone)
            {
                Debug.Log("[SAVE] Ditemukan BatuManager. Memanggil CaptureState...");
                if (stone.CaptureState() is List<StoneRespawnSaveData> queueData)
                {
                    
                    saveData.queueRespownStone = stone.respawnQueue;
                    Debug.Log("Data pemain telah ditangkap untuk StoneManager." + stone.respawnQueue.Count);
                }
            } else if (saveable is TimeManager time)
            {
                Debug.Log("[SAVE] Ditemukan TimeManager. Memanggil CaptureState...");
                if (time.CaptureState() is TimeSaveData timeData)
                {
                    saveData.timeSaveData = timeData;
                    Debug.Log($"Data waktu telah ditangkap untuk penyimpanan. total hari = {saveData.timeSaveData.totalHari} hari ke-{saveData.timeSaveData.hari} tanggal ke-{saveData.timeSaveData.date} minggu ke-{saveData.timeSaveData.minggu} ");
                }
            } else if (saveable is FarmTile hoedTile)
            {
                Debug.Log("[SAVE] Ditemukan TiledHoedManager. Memanggil CaptureState...");
                if (hoedTile.CaptureState() is List<HoedTileData> hoedTiles)
                {
                    saveData.savedHoedTilesList = hoedTile.hoedTilesList;
                    Debug.Log("Data Hoed Tile telah ditangkap untuk penyimpanan." + saveData.savedHoedTilesList.Count);
                }
            }else if (saveable is ShopInteractable shopInteractable)
            {
                Debug.Log("[Save] Ditemukan ShopInteractable, memanggil CaptureState...");

                // Panggil CaptureState() dan pastikan hasilnya adalah ItemShopSaveData
                // (Berdasarkan perbaikan kita sebelumnya, fungsi ini mengembalikan satu objek, bukan list)
                if (shopInteractable.CaptureState() is ItemShopSaveData shopData)
                {
                    // Tambahkan data dari toko ini ke dalam list utama di GameSaveData
                    saveData.itemShopSaveData.Add(shopData);
                    Debug.Log($"Data untuk toko '{shopData.typeShop}' berhasil ditambahkan. Jumlah item: {shopData.items.Count}");
                }
            }

            // Tambahkan 'else if' lain untuk Chest, Bunga, dll. di masa depan.
        }
    }

    private void SaveToFile(GameSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);

        // Lihat seperti apa isi file JSON yang akan disimpan
        Debug.Log("[Save Process] Konten JSON yang akan disimpan:\n" + json);

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