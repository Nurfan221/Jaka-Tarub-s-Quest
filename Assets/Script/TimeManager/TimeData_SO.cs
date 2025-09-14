using System;
using UnityEngine;

// Enum untuk Hari dan Musim (jika belum ada di file lain)

/// <summary>
/// ScriptableObject untuk menyimpan semua data terkait waktu dan tanggal dalam game.
/// Data ini akan tersimpan di dalam project sebagai aset.
/// </summary>
[CreateAssetMenu(fileName = "TimeData", menuName = "Data/Time Data")]
public class TimeData_SO : ScriptableObject
{

    /// <summary>
    /// Fungsi untuk mereset semua data waktu ke kondisi awal.
    /// Berguna saat memulai permainan baru.
    /// </summary>

}