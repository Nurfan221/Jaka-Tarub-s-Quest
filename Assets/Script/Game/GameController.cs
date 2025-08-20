using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameObject persistent;
    public static GameController Instance;
    [SerializeField] Player_Movement player_Movement;
    public Player_Inventory playerInventory;
    public PlayerUI playerUI;
    public QuestManager questManager;

    public bool isNewGame = true; // Gunakan nama yang berbeda untuk menghindari kebingungan
    public static int LatestMap = 1;
    Vector2 latestPlayerPos;
    public static int QuestItemCount = 0;
    public static bool CanFinishStory = false;

    public string playerName;
    public bool enablePlayerInput;

    public Transform player;

    public bool fromPortal = true;
    public bool supposedRaid = false;

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
        if (isNewGame)
        {
            //GameData newGameData = new(true);
            //newGameData.ResetGameData();
            //LoadGame(newGameData);
        }
        else
        {
            //LoadGame();
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
        if (playerUI != null)
        {
            playerUI.Reinitialize();
        }

        if (questManager != null)
        {
            // questManager.Reinitialize(); // Jika QuestManager juga perlu di-reset
        }

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
    //public void LoadGame(GameData theData = null)
    //{
    //    Debug.Log("Loading Game");
    //    GameData data = theData ?? SaveSystem.LoadData();
    //    Player_Inventory inventory = Player_Inventory.Instance;

    //    playerName = data.playerName;

    //    LatestMap = data.LatestMap;
    //    latestPlayerPos = new(data.playerPos[0], data.playerPos[1]);

    //    inventory.itemList = new();
    //    foreach (GameData.SimpleItem item in data.PlayerInventory_ItemNameAndCount)
    //    {
    //        inventory.AddItem(ItemPool.Instance.GetItem(item.itemName, item.stackCount, item.level));
    //    }

    //    inventory.EquipItem(inventory.FindItemInInventory(data.PlayerInventory_ActiveItemAndCount[0].itemName));
    //    inventory.EquipItem(inventory.FindItemInInventory(data.PlayerInventory_ActiveItemAndCount[1].itemName));
    //    inventory.AddQuickSlot(inventory.FindItemInInventory(data.PlayerInventory_ActiveItemAndCount[2].itemName), 0);
    //    inventory.AddQuickSlot(inventory.FindItemInInventory(data.PlayerInventory_ActiveItemAndCount[3].itemName), 1);

    //    // Load storage items to each storage container
    //    if (StorageSystem.Instance != null)
    //    {
    //        foreach (KeyValuePair<int, List<GameData.SimpleItem>> ele in data.Storages_ItemNameAndCount)
    //        {
    //            StorageSystem.Instance.storages[ele.Key].Items = new();
    //            List<Item> storage = StorageSystem.Instance.storages[ele.Key].Items;
    //            foreach (GameData.SimpleItem item in ele.Value)
    //            {
    //                storage.Add(ItemPool.Instance.GetItem(item.itemName, item.stackCount, item.level));
    //            }
    //        }
    //    }

    //    if (data.currentQuest != string.Empty)


    //    // GameEventSystem.Instance.DoneFirstNarration = data.gameEvent_DoneFirstNarration;
    //    // GameEventSystem.Instance.DoneDialogue_TamashiiGiveName = data.gameEvent_DoneDialogue_1;
    //    // GameEventSystem.Instance.DoneDialogue_DanauPertamaKeDesa = data.gameEvent_DoneDialogue_2;
    //    // GameEventSystem.Instance.DoneDialogue_FirstDesaWarga = data.gameEvent_DoneDialogue_3;
    //    // GameEventSystem.Instance.DoneDialogue_FirstKakRen = data.gameEvent_DoneDialogue_4;
    //    // GameEventSystem.Instance.DoneDialogue_FirstBandit = data.gameEvent_DoneDialogue_5;
    //    // GameEventSystem.Instance.DoneDialogue_FirstBanditDone = data.gameEvent_DoneDialogue_6;
    //    // GameEventSystem.Instance.DoneDialogue_FinshDialogue = data.gameEvent_DoneDialogue_7;

    //    if (VillageController.Instance != null)
    //    {
    //        if (data.pedangKakRen)
    //            Destroy(VillageController.Instance.PedangKakRen);
    //    }

    //    if (ForestController.Instance != null)
    //    {
    //        var theForest = ForestController.Instance;

    //        if (data.quanta_pedang)
    //            Destroy(theForest.QUEST_GagangPedang);
    //        if (data.quanta_tongkat)
    //            Destroy(theForest.QUEST_Tongkat);
    //        if (data.quanta_perisai)
    //            Destroy(theForest.QUEST_Perisai);
    //        if (data.quanta_armor)
    //            Destroy(theForest.QUEST_Armor);
    //        if (data.quanta_buku)
    //            Destroy(theForest.QUEST_Buku);
    //    }
    //}

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
        StartCoroutine(ProsesLoadingDanPindahScene(namaScene));
    }


    //COROUTINE YANG MENANGANI SELURUH PROSES 
    private IEnumerator ProsesLoadingDanPindahScene(string namaScene)
    {
        // Tampilkan UI Loading dan jeda game 
        LoadingScreenUI.Instance.ShowLoading(); // Ini akan memanggil PlayLoadingAnimation() dan PauseGame()

        // Beri waktu agar animasi fade-in atau tampilan loading screen terlihat mulus
        yield return new WaitForSecondsRealtime(0.5f);

        // Muat scene baru secara asynchronous 
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(namaScene);

        // Hentikan agar scene tidak langsung aktif saat selesai dimuat
        asyncLoad.allowSceneActivation = false;

        // Tampilkan progres loading jika Anda ingin
        while (asyncLoad.progress < 0.9f) // progress 0.9f adalah ketika scene hampir selesai dimuat
        {
            // Di sini Anda bisa mengupdate loading bar jika ada
            // float progress = asyncLoad.progress / 0.9f;
            // loadingSlider.value = progress;
            yield return null;
        }

        // Beri waktu tunggu minimal untuk pengalaman pengguna yang baik 
        yield return new WaitForSecondsRealtime(1.5f); // Jeda minimal 1.5 detik agar tips terbaca

        // Aktifkan scene baru dan sembunyikan UI loading 
        asyncLoad.allowSceneActivation = true;
        // Tunggu sampai scene benar-benar aktif
        yield return new WaitUntil(() => asyncLoad.isDone);
        TimeManager.Instance.UpdateDay(); // Memastikan waktu diperbarui saat pindah scene

        LoadingScreenUI.Instance.HideLoading(); // Ini akan memanggil ResumeGame()
    }
}
