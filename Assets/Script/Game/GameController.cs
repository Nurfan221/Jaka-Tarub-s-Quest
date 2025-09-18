using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameObject persistent;
    public static GameController Instance;


    public bool isNewGame = true; // Gunakan nama yang berbeda untuk menghindari kebingungan
    public static int LatestMap = 1;
    public string LatestMapName;
    public Vector2 latestPlayerPos;
    public static int QuestItemCount = 0;
    public static bool CanFinishStory = false;

    public string playerName;
    public bool enablePlayerInput;

    public Transform player;



    [SerializeField] GameObject[] persistentUI;
    [SerializeField] GameObject playeyDiedUI;

    [HideInInspector] public bool canPause = true;
    public bool gamePaused;
    [SerializeField] GameObject pauseUI;

    [Header("Environment penting dalam game")]
    public Light sunlight;


    private void Awake()
    {
        if (persistent != null)
        {
            Destroy(transform.root.gameObject);
            return;
        }
        persistent = transform.root.gameObject;
        DontDestroyOnLoad(persistent);
        Instance = this;
    }

    private void Start()
    {

        // Coba muat data dari SaveDataManager
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

            if(saveData.savedStorages != null && saveData.savedStorages.Count > 0)
            {
                Debug.Log("GameController: Membangun Storage dari file save");
                RestoreAllStorages(saveData.savedStorages);
            }

            if (saveData.queueRespownStone !=  null  && saveData.queueRespownStone.Count >0)
            {
                Debug.Log("GameController: Membangun Stone dari file save");
                ReturnQueueStoneActive(saveData.queueRespownStone);
            }
        }
        else
        {
            Debug.Log("GameController: Membangun dunia baru dari Peta Awal...");
            GenerateDefaultWorld();
            FindObjectOfType<PlayerController>()?.InitializeForNewGame();
        }

        InitializePlayer();

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayCurrentSceneBGM();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        Debug.Log($"Scene '{scene.name}' telah dimuat. Memulai inisialisasi ulang.");

        // Panggil fungsi-fungsi setup Anda yang sudah ada
        PlayCurrentSceneBGM();
        InitializePlayer(); // Fungsi ini akan menemukan GameObject Player yang baru

        //INI BAGIAN BARUNYA: PANGGIL SEMUA FUNGSI REINITIALIZE 
        PlayerUI.Instance.Reinitialize();

    }

    private void InitializePlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }

        if (PlayerPrefs.GetInt("HaveSaved") == 99)
        {
            player.position = latestPlayerPos;
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
            StorageSystem.Instance.storages.Clear();
            Debug.Log("Daftar storage lama di StorageSystem telah dibersihkan.");
        }

        Debug.Log($"Memulai proses restore untuk {savedStorages.Count} storage...");

        // Loop melalui setiap data storage yang ada di file save
        foreach (StorageSaveData storageData in savedStorages)
        {
            //    (Diasumsikan Anda punya satu prefab generik untuk semua peti)
            GameObject storagePrefab = DatabaseManager.Instance.storageWorldPrefab;

            if (storagePrefab != null)
            {
                //    Pastikan di StorageSaveData Anda ada variabel Vector3 position.
                Vector3 spawnPosition = storageData.storagePosition;

                // Buat objek storage baru di posisi yang benar.
                GameObject newStorageGO = Instantiate(storagePrefab, spawnPosition, Quaternion.identity);
                StorageInteractable storageInteractable = newStorageGO.GetComponent<StorageInteractable>();

                if (storageInteractable != null)
                {
                    storageInteractable.RestoreState(storageData);

                    StorageSystem.Instance.storages.Add(storageInteractable);
                }
            }
        }
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
                pauseUI.SetActive(false);
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
        foreach (GameObject ui in persistentUI)
        {
            ui.SetActive(show);
        }
    }

    public void PauseWithUI()
    {
        PauseGame();
        ShowPersistentUI(false);
        pauseUI.SetActive(true);
    }

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0;
        //player_Movement.movement = Vector2.zero;
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1;
        ShowPersistentUI(true);
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        Debug.Log("Saving Game");
        PlayerPrefs.SetInt("HaveSaved", 99);
        LatestMap = SceneManager.GetActiveScene().buildIndex;
        SaveSystem.SaveData();
        isNewGame = false;
    }

    [ContextMenu("Load Game")]


    public void GoToMainMenu()
    {
        SaveGame();
        //LoadingScreenUI.Instance.LoadScene(0);
        Destroy(transform.root.gameObject);
    }

    public void PlayerDied()
    {
        PauseGame();
        playeyDiedUI.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PindahKeScene(string namaScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(namaScene);
        //LoadingScreenUI.Instance.Show(asyncLoad);
    }


    public void GenerateDefaultWorld()
    {

        foreach (TreePlacementData treeData in DatabaseManager.Instance.worldTreeDatabase.initialTreePlacements)
        {
            // Dapatkan prefab yang benar dari DatabaseManager
            GameObject treePrefab = DatabaseManager.Instance.GetPrefabForTreeStage(treeData.typePlant, treeData.initialStage);
            if (treePrefab != null)
            {
                // Munculkan pohon dan konfigurasikan

                GameObject newTree = Instantiate(treePrefab, treeData.position, Quaternion.identity);
                TreeBehavior treeBehavior = newTree.GetComponent<TreeBehavior>();
                if (treeBehavior != null)
                {
                    treeBehavior.UniqueID = treeData.TreeID;
                    treeBehavior.currentStage = treeData.initialStage;



                    treeBehavior.isRubuh = treeData.sudahTumbang;



                    // Daftarkan ke daftar pohon aktif
                    MainEnvironmentManager.Instance.pohonManager.environmentList.Add(treeData);
                }
            }
        }
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
                GameObject objectPohon = DatabaseManager.Instance.GetPrefabForTreeStage(savedItem.typePlant, savedItem.initialStage);

                Vector3 spawnPosition = new Vector3(savedItem.position.x, savedItem.position.y, 0);

                GameObject plant = Instantiate(objectPohon, spawnPosition, Quaternion.identity);

                // Panggil ForceGenerateUniqueID untuk memastikan pohon yang di-load punya ID yang benar
                plant.GetComponent<UniqueIdentifiableObject>()?.ForceGenerateUniqueID();

                treesManager.RestoreState(savedItem);
            }
        }
        Debug.Log("[LOAD] Restorasi state batu selesai.");
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
   
}