using System;
using UnityEngine;

// Enum untuk Hari dan Musim (jika belum ada di file lain)
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
/// <summary>
/// ScriptableObject untuk menyimpan semua data terkait waktu dan tanggal dalam game.
/// Data ini akan tersimpan di dalam project sebagai aset.
/// </summary>
[CreateAssetMenu(fileName = "TimeData", menuName = "Data/Time Data")]
public class TimeData_SO : ScriptableObject
{
    [Header("Date & Time Settings")]
    public int totalHari = 1;
    public int hari = 1;
    public int date = 1;
    public int minggu = 1;
    public int bulan = 1;
    public int tahun = 1;
    public Days currentDay = Days.Mon;
    public Season currentSeason = Season.Rain;
    public float dailyLuck;
    public bool isRain;

    /// <summary>
    /// Fungsi untuk mereset semua data waktu ke kondisi awal.
    /// Berguna saat memulai permainan baru.
    /// </summary>
    public void ResetData()
    {
        totalHari = 1;
        hari = 1;
        date = 1;
        minggu = 1;
        bulan = 1;
        tahun = 1;
        currentDay = Days.Mon;
        currentSeason = Season.Rain;
        dailyLuck = UnityEngine.Random.Range(0f, 1f); // Contoh reset keberuntungan
    }
}