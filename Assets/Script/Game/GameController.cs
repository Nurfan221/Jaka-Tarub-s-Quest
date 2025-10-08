using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;

public enum GameState { Playing, Paused, InDialogue, Loading }
public class GameController : MonoBehaviour
{
    public static GameObject persistent;
    public static GameController Instance;


    public static bool IsNewGame = false;
    public static int LatestMap = 1;
    //public string LatestMapName;
    public Vector2 latestPlayerPos;
    //public static int QuestItemCount = 0;
    //public static bool CanFinishStory = false;

    public string playerName;
    public bool enablePlayerInput;

    //public FarmData_SO databaseManager;
    public FarmTile tilemap;





    public bool canPause = true;
    public bool gamePaused;
    public GameState currentState;



    // Berlangganan ke event saat GameController aktif
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Berhenti berlangganan saat GameController nonaktif untuk menghindari error
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //if (DatabaseManager.Instance != null)
        //{
        //    databaseManager = DatabaseManager.Instance.farmData_SO;
        //}else
        //{
        //    Debug.LogError("DatabaseManager Instance is null in GameController Awake");
        //}

        if (FarmTile.Instance != null)
        {
            tilemap = FarmTile.Instance;
        }
    }

  

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ganti "MainGameScene" dengan nama scene utama Anda jika berbeda
        string mainGameSceneName = "MainGameScene";

        // KITA HANYA BERTINDAK JIKA SCENE YANG DIMUAT ADALAH SCENE GAME UTAMA
        if (scene.name == mainGameSceneName)
        {
            // Pada titik ini, dijamin semua objek di MainGameScene (termasuk MainEnvironmentManager)
            // sudah menjalankan Awake(). Jadi, sekarang aman untuk memanggil logika inisialisasi.

            if (IsNewGame)
            {
                Debug.Log($"Scene '{mainGameSceneName}' dimuat. Memulai sebagai Game Baru...");
                GenerateDefaultWorld();
                IsNewGame = false; // Reset "catatan"
            }
            else
            {
                Debug.Log($"Scene '{mainGameSceneName}' dimuat. Melanjutkan dari Save File...");
                LoadGame();
            }
        }
    }





    private void RestorePlayerData(PlayerSaveData playerData)
    {
        // Cari objek pemain di scene
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // Panggil fungsi RestoreState-nya dan berikan data yang sudah di-load
            player.RestoreState(playerData);
        }
        else
        {
            Debug.LogError("Gagal me-restore data: PlayerController tidak ditemukan di scene!");
        }
    }

    public void RestoreAllStorages(List<StorageSaveData> savedStorages)
    {
        // Hapus semua storage lama dari daftar aktif SEBELUM memuat yang baru.
        // Ini penting agar tidak ada data ganda jika fungsi ini dipanggil lebih dari sekali.
        if (StorageSystem.Instance != null)
        {
            StorageSystem.Instance.environmentList.Clear();
            Debug.Log("Daftar storage lama di StorageSystem telah dibersihkan.");
        }

        Debug.Log($"Memulai proses restore untuk {savedStorages.Count} storage...");

        // Loop melalui setiap data storage yang ada di file save
        foreach (StorageSaveData storageData in savedStorages)
        {

            StorageSystem.Instance.environmentList.Add(storageData); // Tambahkan data ke daftar di StorageSystem

           
        }

        StorageSystem.Instance.AddStorageFromEnvironmentList();
        Debug.Log("Restore semua storage selesai.");
    }

    private void PlayCurrentSceneBGM()
    {
        if (SoundManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            switch (currentSceneName)
            {
                case "Village":
                    SoundManager.Instance.StopBGM();
                    SoundManager.Instance.PlayBGM("VillageBGM");
                    //  SoundManager.Instance.Stop("BGMDanau");

                    break;
                case "Forest":
                    SoundManager.Instance.StopBGM();
                    SoundManager.Instance.PlayBGM("BGMDanau");
                    SoundManager.Instance.PlayBGM("ForestBGM");
                    break;
                default:
                    SoundManager.Instance.StopBGM();
                    SoundManager.Instance.PlayBGM("DefaultBGM");
                    break;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            {
                ResumeGame();
                ShowPersistentUI(true);
                PlayerUI.Instance.pauseUI.SetActive(false);
            }
        }

        // Update enablePlayerInput logic
        //if (Player_Action.Instance != null)
        //{
        //    enablePlayerInput = Player_Action.Instance.canAttack && !gamePaused;
        //}
    }

    public void ShowPersistentUI(bool show)
    {
        Debug.Log("pause");
        canPause = show;
        foreach (GameObject ui in PlayerUI.Instance.persistentUI)
        {
            ui.SetActive(show);
        }
    }

    public void PauseWithUI()
    {
        PauseGame();
        ShowPersistentUI(false);
        PlayerUI.Instance.pauseUI.SetActive(true);
    }

    public void PauseGame()
    {
        gamePaused = true;
        currentState = GameState.Paused;
        Time.timeScale = 0;
        //player_Movement.movement = Vector2.zero;
    }

    public void ResumeGame()
    {
        if (currentState != GameState.InDialogue)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            gamePaused = false;
            ShowPersistentUI(true);
        }

    }

    public void StartDialogue()
    {
        currentState = GameState.InDialogue;
        Time.timeScale = 0f;
    }



    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        GenerateDefaultWorld();
        GameSaveData saveData = SaveDataManager.Instance.LoadGame();

        // Cek apakah ada data yang berhasil di-load
        if (saveData != null)
        {
            // Cek apakah ada data pemain yang tersimpan
            if (saveData.savedPlayerData != null && saveData.savedPlayerData.Count > 0)
            {
                Debug.Log("GameController: Menemukan data pemain. Memulai proses restore...");

                // Panggil fungsi khusus untuk me-restore pemain
                RestorePlayerData(saveData.savedPlayerData[0]); // Ambil data pemain pertama
            }

            // Cek apakah ada data pohon yang tersimpan
            if (saveData.savedTrees != null && saveData.savedTrees.Count > 0)
            {
                Debug.Log("GameController: Membangun dunia dari file save...");
                BuildWorldFromSave(saveData.savedTrees);
            }

            if (saveData.savedStorages != null && saveData.savedStorages.Count > 0)
            {
                Debug.Log("GameController: Membangun Storage dari file save");
                RestoreAllStorages(saveData.savedStorages);
            }

            if (saveData.queueRespownStone != null && saveData.queueRespownStone.Count > 0)
            {
                Debug.Log("GameController: Membangun Stone dari file save");
                ReturnQueueStoneActive(saveData.queueRespownStone);
            }

            if (saveData.timeSaveData != null && saveData.timeSaveData.totalHari > 0)
            {
                Debug.Log("GameController: Membangun Time dari file save");
                TimeManager.Instance.RestoreState(saveData.timeSaveData);
            }
            if (saveData.savedHoedTilesList != null && saveData.savedHoedTilesList.Count > 0)
            {
                RestoreFarmStateFromData(saveData.savedHoedTilesList);
            }

            if(saveData.itemShopSaveData != null && saveData.itemShopSaveData.Count > 0)
            {
                Debug.Log("GameController: Membangun item shop dari file save" + saveData.itemShopSaveData.Count);
                BuildItemToSeelFromSaveData(saveData.itemShopSaveData);
            }
        }
        else
        {
            Debug.Log("GameController: Membangun dunia baru dari Peta Awal...");
            GenerateDefaultWorld();
            FindObjectOfType<PlayerController>()?.InitializeForNewGame();
        }
    }


    public void GoToMainMenu()
    {
        //LoadingScreenUI.Instance.LoadScene(0);
        Destroy(transform.root.gameObject);
    }

    public void PlayerDied()
    {
        PauseGame();
        PlayerUI.Instance.playeyDiedUI.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PindahKeScene(string namaScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(namaScene);
        ResumeGame();
        ShowPersistentUI(true);
        //LoadingScreenUI.Instance.Show(asyncLoad);
    }


    public void GenerateDefaultWorld()
    {
        Debug.Log("Membangun dunia default dari DatabaseManager...");

        // Pengecekan keamanan untuk memastikan manajer sudah ada
        if (MainEnvironmentManager.Instance == null)
        {
            Debug.LogError("GenerateDefaultWorld GAGAL: MainEnvironmentManager tidak ditemukan!");
            return;
        }

        // Generate Pohon
        TreesManager treesManager = MainEnvironmentManager.Instance.pohonManager;
        if (treesManager != null)
        {
            treesManager.environmentList = new List<TreePlacementData>(DatabaseManager.Instance.worldTreeDatabase.initialTreePlacements);
            treesManager.HandleAddTreesObject();
        }

        // Generate Lingkungan Lain
        GenerateDefaultEnvirontment();
    }

    public void GenerateDefaultEnvirontment()
    {
        Debug.Log("Membangun lingkungan default dari DatabaseManager...");
        BungaManager bungaManager = MainEnvironmentManager.Instance.bungaManager;
        JamurManager jamurManager = MainEnvironmentManager.Instance.jamurManager;
        BatuManager batuManager = MainEnvironmentManager.Instance.batuManager;

        bungaManager.environmentList.Clear();
        jamurManager.environmentList.Clear();
        batuManager.listBatuManager.Clear();
        bungaManager.environmentList = DatabaseManager.Instance.environmentDatabase.FlowerSaveData;
        jamurManager.environmentList = DatabaseManager.Instance.environmentDatabase.jamurSaveData;
        batuManager.listBatuManager = DatabaseManager.Instance.worldStoneDatabase.stoneBehaviors;

        bungaManager.RandomSpawnFlower();
        jamurManager.RandomSpawnJamur();

    }

    // Membangun kembali dunia dari data save yang sudah dimuat.
    public void BuildWorldFromSave(List<TreePlacementData> savedTrees)
    {

        TreesManager treesManager = MainEnvironmentManager.Instance.pohonManager;
        if (treesManager == null) return;

        Debug.Log($"[LOAD] Merestorasi state untuk {savedTrees.Count} batu...");



        treesManager.secondListTrees.Clear();

        foreach (var savedItem in savedTrees)
        {
            if (savedItem.isGrow)
            {
               

                treesManager.RestoreState(savedItem);
            }
        }


    }

    public void BuildItemToSeelFromSaveData(List<ItemShopSaveData> itemToSale)
    {

        MainEnvironmentManager.Instance.HandleAddItemSellToShops(itemToSale);
    }

    public void ReturnQueueStoneActive(List<StoneRespawnSaveData> savedStone)
    {
        BatuManager batuManager = BatuManager.Instance;
        if (batuManager == null) return;

        Debug.Log($"[LOAD] Merestorasi state untuk {savedStone.Count} batu...");

        // Kita tetap butuh loop ini untuk memastikan semua batu dalam kondisi default (aktif).
        // Cara ini masih lebih aman daripada mengasumsikan semuanya nonaktif.
        foreach (var group in batuManager.listBatuManager)
        {
            foreach (var stoneData in group.listActive)
            {
                Transform stoneTransform = batuManager.transform.Find(stoneData.stoneID);
                if (stoneTransform != null)
                {
                    stoneTransform.gameObject.SetActive(true);
                }
            }
        }

        batuManager.respawnQueue.Clear();

        foreach (var savedItem in savedStone)
        {
            // CARI CHILD DENGAN NAMA YANG SESUAI DENGAN ID
            Transform stoneTransform = batuManager.transform.Find(savedItem.id);
            Debug.Log($"[LOAD] nama stoneTransform" + stoneTransform.gameObject.name);
            if (stoneTransform != null )
            {
                Debug.Log($"[LOAD] Menemukan batu dengan nama/ID: '{savedItem.id}'. Nonaktifkan karena sudah waktunya respawn.");
                // Jika ditemukan, nonaktifkan GameObject-nya
                stoneTransform.gameObject.SetActive(false);

                
            }
            else
            {
                Debug.LogWarning($"[LOAD] Gagal menemukan child object dengan nama/ID: '{savedItem.id}'.");
            }
            batuManager.RestoreState(savedItem);



        }
        Debug.Log("[LOAD] Restorasi state batu selesai.");
    }

    public void RestoreFarmStateFromData(List<HoedTileData> hoedTilesList)
    {
        if (tilemap == null)
        {
            Debug.LogError("FarmTile Instance is null in RestoreFarmStateFromData");
            return;
        }

        tilemap.hoedTilesList.Clear();
        foreach (var item in hoedTilesList)
        {
            tilemap.RestoreState(item);
        }

        tilemap.RestoreFarmStateList();
    }

    public void StartPassOutSequence()
    {
        Debug.Log("Pemain pingsan karena kelelahan!");
        GameEconomy.Instance.LostMoney(200); // kehilangan uang sebesar 500
        ItemPool.Instance.DropRandomItemsOnPassOut();
        PlayerController.Instance.HandlePlayerPingsan();

        //SaveDataManager.Instance.SaveGame();

        //TimeManager.Instance.AdvanceToNextDay();


    }

  
}