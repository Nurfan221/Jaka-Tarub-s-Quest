using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class TimeManager : MonoBehaviour
{
    // Menambahkan properti Singleton
    public static TimeManager Instance { get; private set; }

    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private FarmTile farmTile;
    //[SerializeField] private NPCManager npcManager;
    [SerializeField] private QuestManager questManager;
    //[SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] public Player_Health player_Health;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private SpawnerManager spawnerManager;

    [Header("Date & Time settings")]
    public int totalHari = 1;
    public int hari = 1;
    public int date = 1;
    public int minggu = 1;
    public int bulan = 1;
    public int tahun = 1;
    public Days currentDay = Days.Mon;
    public Season currentSeason = Season.Rain;
    public float dailyLuck;

    [Header("Logika Waktu")]
    public int secondsIncrease = 10;
    public float tickInterval = 1f;
    private float tickTimer = 0f;
    public int minutes = 0;
    public int hour = 0;

    public static event UnityAction OnTimeChanged;
    public static event Action<int> OnDayChanged;

    private List<TreeBehavior> registeredTrees = new List<TreeBehavior>(); // Menyimpan pohon-pohon yang terdaftar
    private List<PerangkapBehavior> registeredTrap = new List<PerangkapBehavior>();// Menyimpan perangkap-perangkap yang terdaftar
    public static event Action<int> OnHourChanged; // Event untuk perubahan jam

    private void Start()
    {
        shopUI.UpdateShopBySeason(currentSeason);
    }
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
            // npcManager.StartSchedule();
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
        }

        if (hour >= 24)
        {
            hour = 0;
            totalHari++;
            UpdateDay();
            HitungWaktu(totalHari);

            // Update musim setiap 28 hari
            if (totalHari % 29 == 0)
            {
                UpdateSeason();
            }
        }

        OnTimeChanged?.Invoke();
    }

   private void UpdateDay()
    {
        hour = 4;   
        currentDay = (Days)((totalHari % 7 == 0) ? 7 : totalHari % 7);

        // Tentukan probabilitas hujan berdasarkan musim
        weatherManager.SetRainChance();
        weatherManager.CheckForRain();

        farmTile.CheckTile();
        farmTile.ResetWateredTiles();

        questManager.CheckQuest();
        shopUI.RestockDaily(currentSeason);

        player_Health.ReverseHealthandStamina();
        GetLuck();
        spawnerManager.CheckChapter1IsDone(dailyLuck);


        // Panggil event OnDayChanged untuk memberi tahu semua pohon bahwa hari telah berubah
        Debug.Log($"Hari telah berganti: {totalHari}");
        OnDayChanged?.Invoke(totalHari); // Mengirim totalHari ke semua pohon

        //Update semua perangkap
        foreach (var trap in registeredTrap)
        {
            trap.GetAnimalToTrap(); // Memanggil logika di perangkap
        }

        // Debug jumlah listener yang terdaftar
        Debug.Log($"Jumlah pohon yang menerima event: {registeredTrees.Count}");
   }

    public void GetLuck()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        // Pembobotan lebih besar pada angka 1
        if (randomValue < 0.5f)  // 50% peluang untuk mendapatkan angka lebih dekat ke 1
        {
            dailyLuck = 0f;
        }
        else if (randomValue < 0.8f)  // 30% peluang untuk mendapatkan angka antara 1 dan 3
        {
            dailyLuck = UnityEngine.Random.Range(1f, 2f);  // Nilai acak antara 1 dan 2
        }
        else  // 20% peluang untuk mendapatkan 3
        {
            dailyLuck = 3f;
        }

    }

    private void UpdateSeason()
    {
        // Mengganti musim secara berurutan setiap kali fungsi ini dipanggil
        currentSeason = (Season)(((int)currentSeason + 1) % Enum.GetValues(typeof(Season)).Length);
        Debug.Log("Season updated to: " + currentSeason);

        shopUI.UpdateShopBySeason(currentSeason);

    }

    public void HitungWaktu(int totalHari)
    {
        int hariDalamBulan = 28;
        int bulanDalamTahun = 12;
        int hariDalamMinggu = 7;

        tahun = totalHari / (hariDalamBulan * bulanDalamTahun);
        int sisaHari = totalHari % (hariDalamBulan * bulanDalamTahun);

        bulan = sisaHari / hariDalamBulan;
        sisaHari %= hariDalamBulan;

        minggu = sisaHari / hariDalamMinggu;

        hari = sisaHari % hariDalamMinggu;
        hari = (hari == 0 && totalHari % hariDalamMinggu == 0) ? 7 : hari;

        date++;
        if (date == 29)
        {
            date = 1;
        }

        Debug.Log($"Total bermain: {tahun} tahun, {bulan} bulan, {minggu} minggu, {hari} hari, {date} tanggal");
    }

    public override string ToString()
    {
        return $"Time: {hour:D2}:{minutes:D2}, Hari: {currentDay}, Musim: {currentSeason}";
    }

    public string GetFormattedDate()
    {
        return $"{currentDay} - {date}";
    }

    public string GetFormattedTime()
    {
        return $"{hour:D2}:{minutes:D2}";
    }

    public string GetCurrentSeason()
    {
        return currentSeason.ToString();
    }

    public string GetCurrentWeek()
    {
        return $"Minggu ke-{minggu}";
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

    [Serializable]
    public enum Days
    {
        Null = 0,
        Mon = 1,
        Tue = 2,
        Wed = 3,
        Thu = 4,
        Fri = 5,
        Sat = 6,
        Sun = 7
    }

    [Serializable]
    public enum Season
    {
        
        Rain = 0,
        Dry = 1
    }

    //[Header("logika reset sesuatu setiap hari")]

    

}
