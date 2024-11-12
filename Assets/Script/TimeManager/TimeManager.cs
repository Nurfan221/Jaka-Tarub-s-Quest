using UnityEngine;
using UnityEngine.Events;
using System;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private FarmTile farmTile;
    [Header("Date & Time settings")]
    public int totalHari = 1;
    public int hari = 1;
    public int date = 1;
    public int minggu = 1;
    public int bulan = 1;
    public int tahun = 1;
    public Days currentDay = Days.Mon;
    public Season currentSeason = Season.Summer; 
    
    [Header("Logika Waktu")]
    public int secondsIncrease = 10;  
    public float tickInterval = 1f;
    private float tickTimer = 0f;
    public int minutes = 0;
    public int hour = 0;

       public static event UnityAction OnTimeChanged;

    private void Update()
    {
        tickTimer += Time.deltaTime;
        
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            AdvanceTime();
        }
    }

    private void AdvanceTime()
    {
        minutes += secondsIncrease;
        
        if (minutes >= 60)
        {
            minutes -= 60;
            hour++;
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
        currentDay = (Days)((totalHari % 7 == 0) ? 7 : totalHari % 7);
         // Tentukan probabilitas hujan berdasarkan musim
        weatherManager.SetRainChance();
        // Cek apakah hujan terjadi
        weatherManager.CheckForRain();

        farmTile.CheckTile();

        farmTile.ResetWateredTiles();
    }

    private void UpdateSeason()
    {
        // Mengganti musim secara berurutan setiap kali fungsi ini dipanggil
        currentSeason = (Season)(((int)currentSeason + 1) % Enum.GetValues(typeof(Season)).Length);
        Debug.Log("Season updated to: " + currentSeason);


    }

    public void HitungWaktu(int totalHari)
    {
        int hariDalamBulan = 28;
        int bulanDalamTahun = 12;
        int hariDalamMinggu = 7;
        
        tahun = totalHari / (hariDalamBulan * bulanDalamTahun);
        int sisaHari = totalHari % (hariDalamBulan * bulanDalamTahun);

        bulan = sisaHari / hariDalamBulan ;
        sisaHari %= hariDalamBulan;

        minggu = sisaHari / hariDalamMinggu ;

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
        Summer = 0,
        Rain = 1,
        Dry = 2
    }
}
