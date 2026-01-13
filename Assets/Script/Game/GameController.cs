using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Paused, InDialogue, Loading }
public class GameController : MonoBehaviour
{
    public static GameObject persistent;
    public static GameController Instance;


    public static bool IsNewGame = false;
    public static int LatestMap = 1;
    public Vector2 latestPlayerPos;


    public string playerName;
    public bool enablePlayerInput;

    //public FarmData_SO databaseManager;
    public FarmTile tilemap;





    public bool canPause = true;
    public bool gamePaused;
    public GameState currentState;

    [Header("Logika Perawatan")]
    //simpan lokasi perawatan player ketika pengsan 
    public Vector2 lokasiPerawatan = new Vector2(-185f, -13f);
    public Dialogues dialogPingsan;
    public Dialogues dialogSekarat;



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

    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (FarmTile.Instance != null)
        {
            tilemap = FarmTile.Instance;
        }
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
                Debug.Log("GameController: Membangun dunia baru dari Peta Awal...");
                GenerateDefaultWorld();
                FindObjectOfType<PlayerController>()?.InitializeForNewGame();
                FindObjectOfType<PlayerController>()?.StartPlayerPosition(latestPlayerPos);
                IsNewGame = false; // Reset "catatan"

            }
            else
            {
                Debug.Log($"Scene '{mainGameSceneName}' dimuat. Melanjutkan dari Save File...");
                LoadGame();
            }
            SmoothCameraFollow.Instance.SnapToTarget();
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
                    //SoundManager.Instance.StopBGM();
                    //SoundManager.Instance.PlayBGM("VillageBGM");
                    //  SoundManager.Instance.Stop("BGMDanau");

                    break;
                case "Forest":
                    //SoundManager.Instance.StopBGM();
                    //SoundManager.Instance.PlayBGM("BGMDanau");
                    //SoundManager.Instance.PlayBGM("ForestBGM");
                    break;
                default:
                    //SoundManager.Instance.StopBGM();
                    //SoundManager.Instance.PlayBGM("DefaultBGM");
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

            if (saveData.furnanceSaveData != null && saveData.furnanceSaveData.Count > 0)
            {
                Debug.Log("GameController: Membangun Furnance dari file save");
                // Ini penting agar tidak ada data ganda jika fungsi ini dipanggil lebih dari sekali.
                if (FurnanceObjectSystem.Instance != null)
                {
                    FurnanceObjectSystem.Instance.environmentList.Clear();
                    Debug.Log("Daftar storage lama di FurnanceObjectSystem telah dibersihkan.");
                }

                Debug.Log($"Memulai proses restore untuk {saveData.furnanceSaveData.Count} storage...");

                // Loop melalui setiap data storage yang ada di file save
                foreach (FurnanceSaveData storageData in saveData.furnanceSaveData)
                {

                    FurnanceObjectSystem.Instance.environmentList.Add(storageData); // Tambahkan data ke daftar di StorageSystem


                }

                FurnanceObjectSystem.Instance.AddStorageFromEnvironmentList();
                Debug.Log("Restore semua storage selesai.");
            }

            if (saveData.queueRespownStone != null && saveData.queueRespownStone.Count > 0)
            {
                Debug.Log("GameController: Membangun Stone dari file save");
                ReturnQueueStoneActive(saveData.queueRespownStone);
            }


            if (saveData.savedHoedTilesList != null && saveData.savedHoedTilesList.Count > 0)
            {
                RestoreFarmStateFromData(saveData.savedHoedTilesList);
            }

            if (saveData.itemShopSaveData != null && saveData.itemShopSaveData.Count > 0)
            {
                Debug.Log("GameController: Membangun item shop dari file save" + saveData.itemShopSaveData.Count);
                BuildItemToSeelFromSaveData(saveData.itemShopSaveData);
            }

            if (saveData.savedQuestList != null && saveData.savedQuestList.Count > 0)
            {
                //QuestManager.Instance.RestoreQuestStateFromData(saveData.savedQuestList);
                QuestManager.Instance.RestoreState(saveData.savedQuestList);
            }

            if (saveData.upgradeToolsSaveData != null && saveData.upgradeToolsSaveData.resultItemUpgrade != null)
            {
                MainEnvironmentManager.Instance.upgradeToolsInteractable.RestoreState(saveData.upgradeToolsSaveData);
            }

            if (saveData.perangkapSaveData != null && saveData.perangkapSaveData.Count > 0)
            {
                MainEnvironmentManager.Instance.perangkapManager.RestoreState(saveData.perangkapSaveData);
                Debug.Log("GameController: Membangun Perangkap dari file save");
            }
            else
            {
                Debug.Log("GameController: Tidak ada data Perangkap yang ditemukan di file save");
            }

            if (saveData.timeSaveData != null && saveData.timeSaveData.totalHari > 0)
            {
                Debug.Log("GameController: Membangun Time dari file save");
                TimeManager.Instance.RestoreState(saveData.timeSaveData);
            }

            GenerateDefaultWorld();
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

        TreesManager treesManager = MainEnvironmentManager.Instance.pohonManager;
        if (treesManager != null)
        {
            treesManager.environmentList = new List<TreePlacementData>(
                DatabaseManager.Instance.worldTreeDatabase.initialTreePlacements
            );
            treesManager.SpawnAllTrees();
        }

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
        batuManager.RestoreState(savedStone);
        batuManager.SpawnStonesForDay(TimeManager.Instance.dailyLuck);

        // Kita tetap butuh loop ini untuk memastikan semua batu dalam kondisi default (aktif).
        // Cara ini masih lebih aman daripada mengasumsikan semuanya nonaktif.
        //foreach (var group in batuManager.listBatuManager)
        //{
        //    foreach (var stoneData in group.listActive)
        //    {
        //        Transform stoneTransform = batuManager.transform.Find(stoneData.stoneID);
        //        if (stoneTransform != null)
        //        {
        //            stoneTransform.gameObject.SetActive(true);
        //        }
        //    }
        //}

        //batuManager.respawnQueue.Clear();

        //foreach (var savedItem in savedStone)
        //{
        //    // CARI CHILD DENGAN NAMA YANG SESUAI DENGAN ID
        //    Transform stoneTransform = batuManager.transform.Find(savedItem.id);
        //    Debug.Log($"[LOAD] nama stoneTransform" + stoneTransform.gameObject.name);
        //    if (stoneTransform != null )
        //    {
        //        Debug.Log($"[LOAD] Menemukan batu dengan nama/ID: '{savedItem.id}'. Nonaktifkan karena sudah waktunya respawn.");
        //        // Jika ditemukan, nonaktifkan GameObject-nya
        //        stoneTransform.gameObject.SetActive(false);


        //    }
        //    else
        //    {
        //        Debug.LogWarning($"[LOAD] Gagal menemukan child object dengan nama/ID: '{savedItem.id}'.");
        //    }
        //    batuManager.RestoreState(savedItem);



        //}
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

    public void StartPassOutSequence(bool isPingsan)
    {

        // Terapkan Penalti (Logika Game)
        Debug.Log(isPingsan ? "Pemain pingsan karena kelelahan!" : "Pemain pingsan karena sekarat!");


        // Jika ada logika save atau next day, lakukan di sini saat layar gelap

        // Matikan kontrol pemain agar tidak bisa gerak saat proses pingsan

        // Tentukan Teks dan Dialog berdasarkan kondisi
        string judulKeadaan;
        string deskripsiKeadaan;
        Dialogues dialogToPlay;

        if (isPingsan)
        {
            judulKeadaan = "Kamu kehilangan kesadaran";
            deskripsiKeadaan = "Kamu kehilangan kesadaran karena kelelahan.";
            dialogToPlay = dialogPingsan;

        }
        else
        {
            judulKeadaan = "Kamu hampir meninggal";
            deskripsiKeadaan = "Kamu terluka parah dan tidak sadarkan diri.";
            dialogToPlay = dialogSekarat;

        }

        // Mulai Coroutine untuk mengatur urutan visual (Loading -> Pindah -> Dialog)
        StartCoroutine(ProcessPassOutSequence(judulKeadaan, deskripsiKeadaan, dialogToPlay));
    }

    // Coroutine utama untuk menangani urutan kejadian
    private IEnumerator ProcessPassOutSequence(string judul, string deskripsi, Dialogues dialog)
    {
        //  Mulai Loading Screen
        yield return StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(true, judul));


        // Pindah Hari
        TimeManager.Instance.AdvanceToNextDay();

        // Reset Darah/Stamina & Input (Pastikan input masih terkunci di fungsi ini)
        PlayerController.Instance.HandlePlayerPingsan();

        // Hukuman
        GameEconomy.Instance.LostMoney(200);
        ItemPool.Instance.DropRandomItemsOnPassOut();

        // Save Game (Penting dilakukan setelah semua perubahan data)
        SaveDataManager.Instance.SaveGame();


        // LOGIKA VISUAL (Pindah Posisi & Siapkan Sprite)
        Transform playerTransform = PlayerController.Instance.ActivePlayer.transform;
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        playerTransform.position = lokasiPerawatan;
        Debug.Log("Pemain di RS: " + playerTransform.position);

        // Ambil semua sprite
        List<SpriteRenderer> allSprites = new List<SpriteRenderer>();
        allSprites.Add(playerTransform.GetComponent<SpriteRenderer>());
        allSprites.AddRange(playerTransform.GetComponentsInChildren<SpriteRenderer>());

        // Pastikan sprite benar-benar transparan (0) sebelum mulai fade-in
        foreach (SpriteRenderer sr in allSprites)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f; // Paksa 0
                sr.color = c;
            }
        }

        // ANIMASI FADE IN (Muncul Perlahan di Kasur RS)
        float fadeDuration = 1.5f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);

            foreach (SpriteRenderer sr in allSprites)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = newAlpha;
                    sr.color = c;
                }
            }
            yield return null;
        }

        // Tunggu sebentar biar estetik
        yield return new WaitForSecondsRealtime(1.0f);

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;
        // Munculkan Dialog (Sekarang HP sudah penuh, jadi aman dilihat)
        DialogueSystem.Instance.HandlePlayDialogue(dialog);
        
    }

    // atau disesuaikan jika masih dipakai skrip lain)
    public IEnumerator UseLoadingScreenUI(bool show, string keadaan)
    {
        yield return StartCoroutine(LoadingScreenUI.Instance.SetLoadingandTimer(show, keadaan));
        yield return new WaitForSecondsRealtime(1.5f);
        LoadingScreenUI.Instance.HideLoading();
    }

}