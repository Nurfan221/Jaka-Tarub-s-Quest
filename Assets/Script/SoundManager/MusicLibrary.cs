using System.Collections.Generic;
using UnityEngine;

// Enum agar mudah memilih di Inspector
public enum WeatherType { Sunny, Raining }

// array untuk menampung musik per musim dan cuaca
[System.Serializable]
public class SeasonTrack
{
    public string name;
    public Season season;

    // UBAH DARI 'AudioClip' MENJADI 'AudioClip[]' (Array)
    [Header("Music Collections")]
    public AudioClip[] sunnyMusicList;
    public AudioClip[] rainMusicList;
}

[System.Serializable]
public class SoundEffect
{
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f; // Biar bisa atur volume per clip
}

[CreateAssetMenu(fileName = "GameMusicLibrary", menuName = "Audio/Music Library")]
public class MusicLibrary : ScriptableObject
{
    [Header("General")]
    public AudioClip mainMenuMusic;
    public AudioClip indoorMusic; // Musik default dalam ruangan
    public AudioClip[] nightMusicBackgrount;

    [Header("Seasonal Playlists")]
    public SeasonTrack[] seasonTracks; // Array untuk 4 musim

    // List ini yang kamu isi di Inspector
    [Header("SFX Collection")]
    public List<SoundEffect> sfxList; // Data SFX pindah ke sini!

    // Helper function untuk mencari lagu
    public AudioClip GetClip(Season season, WeatherType weather, bool isIndoors)
    {
        if (isIndoors) return indoorMusic;

        foreach (var track in seasonTracks)
        {
            if (track.season == season)
            {
                // Cek cuaca, lalu panggil fungsi acak
                if (weather == WeatherType.Raining)
                {
                    return GetRandomClip(track.rainMusicList);
                }
                else
                {
                    return GetRandomClip(track.sunnyMusicList);
                }
            }
        }
        return null;
    }

    // Fungsi Helper untuk mengambil 1 lagu acak dari sekumpulan lagu
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        // Cek keamanan: Kalau list kosong/null, jangan error
        if (clips == null || clips.Length == 0) return null;

        // Ambil angka acak dari 0 sampai jumlah lagu
        int randomIndex = Random.Range(0, clips.Length);

        return clips[randomIndex];
    }
}

