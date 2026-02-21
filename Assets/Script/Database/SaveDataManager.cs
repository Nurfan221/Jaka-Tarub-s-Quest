using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
//using static UnityEditor.Progress;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance;
    private string saveFilePath;
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene utama yang akan dimuat.")]
    [SerializeField] private string mainGameSceneName = "MainGameScene";

    // Properti publik untuk mengakses nama scene secara aman (read-only dari luar)
    public string MainGameSceneName => mainGameSceneName;



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
        // Ambil semua objek ISaveable di scene
        var allSaveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        Debug.Log($"[SAVE SYSTEM] Memulai Absen. Ditemukan {allSaveables.Count()} objek ISaveable di scene.");

        foreach (ISaveable saveable in allSaveables)
        {
            // Debugging: Lihat siapa yang sedang diabsen
            // Debug.Log($"Checking Object: {saveable.GetType().Name}"); 

            // REES
            if (saveable is TreesManager tree)
            {
                Debug.Log("[SAVE] Ditemukan TreesManager.");
                if (tree.CaptureState() is List<TreePlacementData> treeData)
                {
                    saveData.savedTrees = treeData;
                    Debug.Log($"Data pohon tersimpan: {treeData.Count} pohon.");
                }
            }
            // PLAYER
            else if (saveable is PlayerController player)
            {
                Debug.Log("[SAVE] Ditemukan PlayerController.");
                if (player.CaptureState() is PlayerSaveData playerData)
                {
                    saveData.savedPlayerData.Add(playerData);
                    Debug.Log("Data pemain tersimpan.");
                }
            }
            //  STORAGE
            else if (saveable is StorageInteractable storage)
            {
                if (storage.CaptureState() is StorageSaveData storageData && storage.isSaveable)
                {
                    if (saveData.savedStorages == null) saveData.savedStorages = new List<StorageSaveData>();

                    // Logika Update/Add Storage
                    bool found = false;
                    foreach (var s in saveData.savedStorages)
                    {
                        if (s.id == storageData.id)
                        {
                            s.itemsInStorage = new List<ItemData>(storageData.itemsInStorage); // Copy list baru
                            s.storagePosition = storageData.storagePosition;
                            found = true; break;
                        }
                    }
                    if (!found) saveData.savedStorages.Add(storageData);
                }
            }
            //  FURNACE (Perbaiki Kurung Kurawal Disini)
            else if (saveable is CookInteractable furnance)
            {
                Debug.Log("[SAVE] Ditemukan CookInteractable.");
                if (furnance.CaptureState() is FurnanceSaveData furnanceData)
                {
                    if (saveData.furnanceSaveData == null) saveData.furnanceSaveData = new List<FurnanceSaveData>();

                    bool found = false;
                    foreach (var f in saveData.furnanceSaveData)
                    {
                        if (f.id == furnanceData.id)
                        {
                            f.itemCook = furnanceData.itemCook;
                            f.fuelCook = furnanceData.fuelCook;
                            f.itemResult = furnanceData.itemResult;
                            f.quantityFuel = furnanceData.quantityFuel;
                            f.furnancePosition = furnanceData.furnancePosition;
                            found = true;
                            break;
                        }
                    }

                    if (!found) saveData.furnanceSaveData.Add(furnanceData);
                }
            }
            // 5. BATU MANAGER
            else if (saveable is BatuManager stone)
            {
                Debug.Log("[SAVE] Ditemukan BatuManager.");
                if (stone.CaptureState() is List<StoneRespawnSaveData> queueData)
                {
                    saveData.queueRespownStone = queueData;
                }
            }
            // 6. TIME MANAGER
            else if (saveable is TimeManager time)
            {
                Debug.Log("[SAVE] Ditemukan TimeManager.");
                if (time.CaptureState() is TimeSaveData timeData)
                {
                    saveData.timeSaveData = timeData;
                }
            }
            // 7. FARM TILE
            else if (saveable is FarmTile hoedTile)
            {
                if (hoedTile.CaptureState() is List<HoedTileData> hoedTiles)
                {
                    saveData.savedHoedTilesList = hoedTiles;
                }
            }
            // 8. SHOP
            else if (saveable is ShopInteractable shop)
            {
                if (shop.CaptureState() is ItemShopSaveData shopData)
                {
                    saveData.itemShopSaveData.Add(shopData);
                }
            }
            // 9. QUEST
            else if (saveable is QuestManager quest)
            {
                if (quest.CaptureState() is List<ChapterQuestActiveDatabase> questData)
                {
                    saveData.savedQuestList = questData;
                }
            }
            // 10. UPGRADE TOOLS
            else if (saveable is UpgradeToolsInteractable upgrade)
            {
                if (upgrade.CaptureState() is UpgradeToolsSaveData upgradeData)
                {
                    saveData.upgradeToolsSaveData = upgradeData;
                }
            }
            // 11. PERANGKAP
            else if (saveable is PerangkapManager perangkap)
            {
                Debug.Log("[SAVE] perangkap Ditemukan ! Memproses...");

                if (perangkap.CaptureState() is List<PerangkapSaveData> pData)
                {
                    Debug.Log($"[SAVE] perangkap Sukses! {pData.Count} tutorial tersimpan.");


                    saveData.perangkapSaveData.AddRange(pData);
                }
            }
            // 12. TUTORIAL MANAGER (TARGET UTAMA KITA)
            else if (saveable is TutorialManager tutorialManager)
            {
                Debug.Log("[SAVE] Ditemukan TutorialManager! Memproses...");

                // Panggil Capture
                object capturedData = tutorialManager.CaptureState();

                // Cek Tipe Data
                if (capturedData is List<string> tutorialData)
                {
                    saveData.completedTutorials = tutorialData;
                    Debug.Log($"[SAVE] Sukses! {tutorialData.Count} tutorial tersimpan.");
                }
                else
                {
                    Debug.LogError($"[SAVE] Gagal! CaptureState mengembalikan tipe yang salah: {capturedData?.GetType()}");
                }
            }
        } // Tutup Loop Foreach
    } // Tutup Fungsi CaptureAllSaveableStates

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