using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

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
public class TimeManager : MonoBehaviour, ISaveable
{
    // Menambahkan properti Singleton
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // Gunakan jika Anda ingin waktu terus berjalan antar scene
        }
    }




   

    [Header("Logika Waktu")]
    public int secondsIncrease = 10;
    public float tickInterval = 1f;
    private float tickTimer = 0f;
    public int minutes;// Ubah menjadi properti agar lebih aman
    public int hour;    // Ubah menjadi properti agar lebih aman

    [Header("Date & Time Settings")]
    public int totalHari;
    public int hari;
    public int date;
    public int minggu;
    public int bulan;
    public int tahun;
    public Days currentDay = Days.Mon;
    public Season currentSeason = Season.Rain;
    public float dailyLuck;
    public bool isRain;
    public float rainChance = 0f;




    public static event UnityAction OnTimeChanged;

    //logika mengirim waktu 

    private List<TreeBehavior> registeredTrees = new List<TreeBehavior>(); // Menyimpan pohon-pohon yang terdaftar
    private List<PerangkapBehavior> registeredTrap = new List<PerangkapBehavior>();// Menyimpan perangkap-perangkap yang terdaftar
    public static event System.Action OnHourChanged;
    public static event System.Action OnDayChanged;
    public static event System.Action OnSeasonChanged;

    


    private void Start()
    {
        //shopUI.UpdateShopBySeason(currentSeason);
        //AdvanceTime();
        //OnDayChanged?.Invoke(); // Mengirim totalHari ke semua pohon
        //UpdateDay();

    }

    public object CaptureState()
    {
        Debug.Log("[SAVE] Menangkap data waktu (TimeManager)...");

        // Buat SATU objek data baru
        var saveData = new TimeSaveData
        {
            // Isi objek tersebut dengan nilai saat ini dari TimeManager
            totalHari = this.totalHari,
            hari = this.hari,
            date = this.date,
            minggu = this.minggu,
            bulan = this.bulan,
            tahun = this.tahun
        };

        // Kembalikan SATU objek data tersebut.
        return saveData;
    }

    public void RestoreState(object state)
    {
        Debug.Log("[LOAD] Merestorasi data waktu (TimeManager)...");

        // Ubah (cast) object 'state' menjadi tipe data yang benar
        var loadedData = (TimeSaveData)state;

        // Kembalikan nilai dari data yang di-load ke TimeManager
        totalHari = loadedData.totalHari;
        hari = loadedData.hari;
        date = loadedData.date;
        minggu = loadedData.minggu;
        bulan = loadedData.bulan;
        tahun = loadedData.tahun;

        Debug.Log($"Waktu berhasil direstorasi ke hari ke-{totalHari}");
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            AdvanceTime();
        }

    }




    public void AdvanceToNextDay()
    {

        hour = 4; // Atau jam berapa pun hari baru dimulai
        totalHari += 1;
        currentDay = (Days)((totalHari % 7 == 0) ? 7 : totalHari % 7);

        HitungWaktu(totalHari);
        if (totalHari % 29 == 0)
        {
            UpdateSeason();
        }

        GetLuck();
        Debug.Log($"Hari telah berganti: {totalHari}");

        // Update semua sistem yang bergantung pada hari baru
        foreach (var trap in registeredTrap)
        {
            trap.GetAnimalToTrap();
        }
        AdvanceAllTreeGrowth();
        SetRainChance(currentSeason);
        OnDayChanged?.Invoke();

    }

    private void AdvanceTime()
    {
        minutes += secondsIncrease;

        if (minutes >= 60)
        {
            minutes -= 60;
            hour++;
            OnHourChanged?.Invoke();
        }

        if (hour >= 25)
        {
           
            hour = 1;

            // Panggil fungsi pingsan Anda di sini
            GameController.Instance.StartPassOutSequence();
        }


        OnTimeChanged?.Invoke();
    }


    public void SetRainChance(Season currentSeason)
    {
        // Akses currentSeason melalui timeManager
        switch (currentSeason)
        {

            case Season.Rain:
                rainChance = 0.80f;
                break;

            case Season.Dry:
                rainChance = 0.1f; // Dry season doesn't have rain chance
                break;

            default:
                rainChance = 0f;  // Nilai default jika tidak ada yang cocok
                break;
        }

        CheckForRain();
    }

    public void CheckForRain()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        if (randomValue <= rainChance)
        {
            TimeManager.Instance.isRain = true;
            Debug.Log("Hujan turun bang");
            //nonactifkan hujan saat di dalam rumah
            SmoothCameraFollow.Instance.EnterHouse(true);
        }
        else
        {
            TimeManager.Instance.isRain = false;
            //nonactifkan hujan saat di dalam rumah
            SmoothCameraFollow.Instance.EnterHouse(true);
            Debug.Log("Tidak turun hujan");
        }
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
            dailyLuck = 0f;
        }
        else if (randomValue < 0.7f)  // 40% peluang untuk mendapatkan angka antara 1 dan 3
        {
            dailyLuck = UnityEngine.Random.Range(1f, 2f);  // Nilai acak antara 1 dan 2
        }
        else  // 30% peluang untuk mendapatkan 3
        {
            dailyLuck = 3f;
        }

    }

    public float GetDayLuck()
    {
        return dailyLuck;
    }



    private void UpdateSeason()
    {
        // Mengganti musim secara berurutan setiap kali fungsi ini dipanggil
        currentSeason = (Season)(((int)currentSeason + 1) % Enum.GetValues(typeof(Season)).Length);
        Debug.Log("Season updated to: " + currentSeason);
        OnSeasonChanged?.Invoke();

    }

    public void HitungWaktu(int totalHari)
    {
        int hariDalamMinggu = 7;
        int hariDalamBulan = 28;
        int hariDalamTahun = 336;

        // Tahun ke-berapa (mulai dari 1)
        tahun = (totalHari / hariDalamTahun) + 1;

        // Sisa hari setelah dihitung tahun
        int sisaHari = totalHari % hariDalamTahun;

        // Bulan ke-berapa (mulai dari 1)
        tahun = (sisaHari / hariDalamBulan) + 1;

        sisaHari %= hariDalamBulan;

        // Minggu ke-berapa (mulai dari 1)
        minggu = (sisaHari / hariDalamMinggu) + 1;

        sisaHari %= hariDalamMinggu;

        // Hari ke-berapa dalam minggu (1–7)
        hari = (sisaHari == 0 && totalHari > 0) ? 7 : (sisaHari == 0 ? 1 : sisaHari);

        // Tanggal (1–28)
        date = ((totalHari - 1) % 28) + 1;

        Debug.Log($"Tanggal: {date}, Hari: {hari}, Minggu: {minggu}, Bulan: {bulan}, Tahun: {tahun}, TotalHari: {totalHari}");
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

    public Season GetCurrentSeason()
    {
        return currentSeason;
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
    public void AdvanceAllTreeGrowth()
    {
        Debug.Log("Hari baru! Memeriksa semua pohon untuk pertumbuhan...");

        // Temukan SEMUA pohon yang ada di scene saat ini.
        TreeBehavior[] allTreesInScene = FindObjectsOfType<TreeBehavior>();

        // Buat list baru untuk menampung pohon yang masih bisa tumbuh.
        List<TreeBehavior> growingTrees = new List<TreeBehavior>();

        // Saring pohon-pohon tersebut.
        foreach (TreeBehavior tree in allTreesInScene)
        {
            // Cek apakah tahapnya BUKAN tahap terakhir.
            if (tree.currentStage != GrowthTree.MaturePlant)
            {
                growingTrees.Add(tree);
            }
        }

        Debug.Log($"Ditemukan {growingTrees.Count} pohon yang masih bisa tumbuh.");

        // Jalankan logika pertumbuhan pada pohon yang sudah disaring.
        foreach (TreeBehavior treeToGrow in growingTrees)
        {
            // Panggil fungsi di dalam skrip TreeBehavior untuk menumbuhkannya
            treeToGrow.PertumbuhanPohon();
        }
    }


    //[Header("logika reset sesuatu setiap hari")]



}
