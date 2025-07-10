using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{
    // Menambahkan properti Singleton
    public static TimeManager Instance { get; private set; }
    public WeatherManager WeatherManager { get; private set; }
    private void Awake()
    {
        // Pastikan hanya ada satu instance dari TimeManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Agar tidak dihancurkan saat scene berganti
        }
        else
        {
            Destroy(gameObject);  // Hancurkan objek jika sudah ada instance lain
        }
    }


    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private QuestManager questManager;
    //[SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] public Player_Health player_Health;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private SpawnerManager spawnerManager;
    [SerializeField] private TrashManager trashManager;
    [SerializeField] private BatuManager batuManager;
    [SerializeField] PlantContainer plantContainer;
    [SerializeField] EnvironmentManager environmentManagerTrees;
    [SerializeField] EnvironmentManager environmentManagerJamur;
    [SerializeField] EnvironmentManager environmentManagerKuburan;
    [SerializeField] EnvironmentManager environmentManagerBunga;
    [SerializeField] BuffScrollController buffScrollController;
   

    [Header("Logika Waktu")]
    public int secondsIncrease = 10;
    public float tickInterval = 1f;
    private float tickTimer = 0f;
    public int minutes = 0;
    public int hour = 0;

    public static event UnityAction OnTimeChanged;

    //logika mengirim waktu 
    public static event Action<int> OnDayChanged;

    private List<TreeBehavior> registeredTrees = new List<TreeBehavior>(); // Menyimpan pohon-pohon yang terdaftar
    private List<PerangkapBehavior> registeredTrap = new List<PerangkapBehavior>();// Menyimpan perangkap-perangkap yang terdaftar
    public static event Action<int> OnHourChanged; // Event untuk perubahan jam

    [Header("Data Waktu")]
    public TimeData_SO timeData_SO;


    private void Start()
    {
        shopUI.UpdateShopBySeason(timeData_SO.currentSeason);
        batuManager.CheckLocationResource();
        AdvanceTime();
    }


    private void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            AdvanceTime();
        }

        if (hour >= 6 && hour <= 24)
        {
            //npcManager.StartSchedule();
            
        }
    }

    public void RegisterWeather(WeatherManager weatherManager)
    {
        this.weatherManager = weatherManager;
        Debug.Log($"PlayerController: Paket Player '{weatherManager.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterWeather(WeatherManager weatherManager)
    {
        if (this.weatherManager == weatherManager)
        {
            this.weatherManager = null;
        }
    }


    private void AdvanceTime()
    {
        minutes += secondsIncrease;

        if (minutes >= 60)
        {
            minutes -= 60;
            hour++;
            OnHourChanged?.Invoke(hour); // Memanggil event saat jam berubah
            if (buffScrollController.isBuffDamage || buffScrollController.isBuffSprint || buffScrollController.isBuffProtection)
            {
                buffScrollController.UpdateBuffTime();
            }

           
        }

        if (hour >= 24)
        {
            hour = 0;
            timeData_SO.totalHari++;
            UpdateDay();
            HitungWaktu(timeData_SO.totalHari);

            // Update musim setiap 28 hari
            if (timeData_SO.totalHari % 29 == 0)
            {
                UpdateSeason();
            }
        }

        OnTimeChanged?.Invoke();
    }

   private void UpdateDay()
    {
        hour = 4;
        timeData_SO.currentDay = (Days)((timeData_SO.totalHari % 7 == 0) ? 7 : timeData_SO.totalHari % 7);

        // Tentukan probabilitas hujan berdasarkan musim
        //weatherManager.SetRainChance();
        weatherManager.CheckForRain();
        FarmTile.Instance.AdvanceDay(weatherManager.isRain);

        //FarmTile.Instance.HandleNewDay();

        questManager.CheckQuest();
        //shopUI.RestockDaily(currentSeason);

        PlayerController.Instance.HandleReverseHealthandStamina();
        GetLuck();
        spawnerManager.SetSpawnerActive(timeData_SO.dailyLuck);


        trashManager.UpdateTrash();

        batuManager.UpdatePositionMiner(timeData_SO.dailyLuck);
        plantContainer.HitungPertumbuhanPohon();
        environmentManagerTrees.SpawnFromEnvironmentList(timeData_SO.dailyLuck);
        environmentManagerJamur.SpawnFromEnvironmentList(timeData_SO.dailyLuck);
        environmentManagerBunga.SpawnFromEnvironmentList(timeData_SO.dailyLuck);
        environmentManagerKuburan.UpdateKondisiKuburan();


        // Panggil event OnDayChanged untuk memberi tahu semua pohon bahwa hari telah berubah
        Debug.Log($"Hari telah berganti: {timeData_SO.totalHari}");
        OnDayChanged?.Invoke(timeData_SO.totalHari); // Mengirim timeData_SO.totalHari ke semua pohon
        OnDayChanged?.Invoke(timeData_SO.date);

        //Update semua perangkap
        foreach (var trap in registeredTrap)
        {
            trap.GetAnimalToTrap(); // Memanggil logika di perangkap
        }

        // Debug jumlah listener yang terdaftar
        Debug.Log($"Jumlah pohon yang menerima event: {registeredTrees.Count}");
   }
    public void GetCurrentDate()
    {
        var date = DateTime.Now;
    }
    public void GetLuck()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        // Pembobotan lebih besar pada angka 1
        if (randomValue < 0.4f)  // 60% peluang untuk mendapatkan angka lebih dekat ke 1
        {
            timeData_SO.dailyLuck = 0f;
        }
        else if (randomValue < 0.7f)  // 40% peluang untuk mendapatkan angka antara 1 dan 3
        {
            timeData_SO.dailyLuck = UnityEngine.Random.Range(1f, 2f);  // Nilai acak antara 1 dan 2
        }
        else  // 30% peluang untuk mendapatkan 3
        {
            timeData_SO.dailyLuck = 3f;
        }

    }

    private void UpdateSeason()
    {
        // Mengganti musim secara berurutan setiap kali fungsi ini dipanggil
        timeData_SO.currentSeason = (Season)(((int)timeData_SO.currentSeason + 1) % Enum.GetValues(typeof(Season)).Length);
        Debug.Log("Season updated to: " + timeData_SO.currentSeason);

        shopUI.UpdateShopBySeason(timeData_SO.currentSeason);

    }

    public void HitungWaktu(int totalHari)
    {
        int hariDalamMinggu = 7;
        int hariDalamBulan = 28;
        int hariDalamTahun = 336;

        // Tahun ke-berapa (mulai dari 1)
        timeData_SO.tahun = (totalHari / hariDalamTahun) + 1;

        // Sisa hari setelah dihitung tahun
        int sisaHari = totalHari % hariDalamTahun;

        // Bulan ke-berapa (mulai dari 1)
        timeData_SO.tahun = (sisaHari / hariDalamBulan) + 1;

        sisaHari %= hariDalamBulan;

        // Minggu ke-berapa (mulai dari 1)
        timeData_SO.minggu = (sisaHari / hariDalamMinggu) + 1;

        sisaHari %= hariDalamMinggu;

        // Hari ke-berapa dalam minggu (1–7)
        timeData_SO.hari = (sisaHari == 0 && totalHari > 0) ? 7 : (sisaHari == 0 ? 1 : sisaHari);

        // Tanggal (1–28)
        timeData_SO.date = ((totalHari - 1) % 28) + 1;

        Debug.Log($"Tanggal: {timeData_SO.date}, Hari: {timeData_SO.hari}, Minggu: {timeData_SO.minggu}, Bulan: {timeData_SO.bulan}, Tahun: {timeData_SO.tahun}, TotalHari: {totalHari}");
    }

    public override string ToString()
    {
        return $"Time: {hour:D2}:{minutes:D2}, Hari: {timeData_SO.currentDay}, Musim: {timeData_SO.currentSeason}";
    }

    public string GetFormattedDate()
    {
        return $"{timeData_SO.currentDay} - {timeData_SO.date}";
    }

    public string GetFormattedTime()
    {
        return $"{hour:D2}:{minutes:D2}";
    }

    public string GetCurrentSeason()
    {
        return timeData_SO.currentSeason.ToString();
    }

    public string GetCurrentWeek()
    {
        return $"Minggu ke-{timeData_SO.minggu}";
    }

    public void RegisterTree(TreeBehavior tree)
    {
        if (!registeredTrees.Contains(tree))
        {
            registeredTrees.Add(tree);
            Debug.Log("Pohon terdaftar: " + tree.name);
        }
    }


    // Fungsi untuk menghapus pohon dari daftar terdaftar
    public void UnregisterTree(TreeBehavior tree)
    {
        if (registeredTrees.Contains(tree))
        {
            registeredTrees.Remove(tree);
        }
    }

    public void RegisterTrap(PerangkapBehavior trap)
    {
        if (!registeredTrap.Contains(trap))
        {
            registeredTrap.Add(trap);
            Debug.Log("Perangkap terdaftar: " + trap.name);
        }
    }

    public void UnregisterTrap(PerangkapBehavior trap)
    {
        if (registeredTrap.Contains(trap))
        {
            registeredTrap.Remove(trap);
            Debug.Log("Perangkap dihapus: " + trap.name);
        }
    }

  

    //[Header("logika reset sesuatu setiap hari")]

    

}
