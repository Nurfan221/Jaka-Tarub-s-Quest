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
            GameObject treePrefab = DatabaseManager.Instance.GetPrefabForTreeStage(treeData.treeName, treeData.initialStage);
            if (treePrefab != null)
            {
                // Munculkan pohon dan konfigurasikan

                GameObject newTree = Instantiate(treePrefab, treeData.position, Quaternion.identity);
                TreeBehavior treeBehavior = newTree.GetComponent<TreeBehavior>();
                if (treeBehavior != null)
                {
                    treeBehavior.nameEnvironment = treeData.treeName;
                    treeBehavior.currentStage = treeData.initialStage;



                    treeBehavior.isRubuh = treeData.sudahTumbang;



                    // Daftarkan ke daftar pohon aktif
                    MainEnvironmentManager.Instance.pohonManager.allActiveTrees.Add(treeBehavior);
                }
            }
        }
    }

    // Membangun kembali dunia dari data save yang sudah dimuat.
    public void BuildWorldFromSave(List<TreeSaveData> savedTrees)
    {
        // Bersihkan daftar pohon aktif yang lama sebelum memuat yang baru
        MainEnvironmentManager.Instance.pohonManager.allActiveTrees.Clear();

        foreach (var savedEntity in savedTrees)
        {
            // Pastikan ini adalah data pohon
            if (savedEntity is TreeSaveData treeData)
            {
                // Dapatkan prefab yang benar berdasarkan NAMA dan TAHAP YANG TERSIMPAN.
                GameObject treePrefab = DatabaseManager.Instance.GetPrefabForTreeStage(treeData.treeName, treeData.currentGrowthStage);

                if (treePrefab != null)
                {
                    // Munculkan prefab yang benar (misal: prefab MaturePlant) di posisi yang tersimpan
                    GameObject newTree = Instantiate(treePrefab, treeData.position, Quaternion.identity);
                    TreeBehavior treeBehavior = newTree.GetComponent<TreeBehavior>();

                    // PENTING: Panggil RestoreState untuk memuat data lain seperti growthTimer, dll.
                    // Ini akan memastikan semua data kembali seperti semula.
                    treeBehavior.RestoreState(treeData);

                    // Daftarkan pohon yang sudah di-load ke daftar pohon aktif
                    MainEnvironmentManager.Instance.pohonManager.allActiveTrees.Add(treeBehavior);
                }
            }
        }
    }
}
