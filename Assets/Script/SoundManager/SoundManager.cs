using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;



public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [Header("Audio Sources (Wajib 2 Deck)")]
    public AudioSource sourceA; 
    public AudioSource sourceB; 
    public AudioSource sfxSource;

    [Header("Data Source")]
    public MusicLibrary audioLibrary; 

    private Dictionary<SoundName, SoundEffect> sfxDictionary;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "MainGameScene";
    private bool isSourceAPlaying = false;
    public float fadeDuration = 2f; // Durasi transisi (detik)
    public float gapDuration = 1.0f;  // LAMA HENING (Jeda antara lagu)
    [Range(0f, 1f)] public float musicMasterVolume = 0.3f; // Settingan kecil untuk background
    [Range(0f, 1f)] public float sfxMasterVolume = 1.0f;   // Settingan besar untuk efek


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Agar tidak hancur saat pindah dari Bootstrapper
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeDictionary();
    }

    private void OnEnable()
    {
        // Kita "Daftar" ke Event Unity: "Kabari saya kalau scene selesai loading"
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Jangan lupa "Un-daftar" saat objek mati agar tidak memory leak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fungsi ini OTOMATIS dipanggil Unity setiap kali pindah scene
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cek nama Scene yang baru saja dimuat
        if (scene.name == mainMenuSceneName)
        {
            PlayMusic(audioLibrary.mainMenuMusic, 0);
        }
        //else if (scene.name != "Bootstrapper")
        //{
        //    // Logic Gameplay Music
        //    bool isIndoors = scene.name.Contains("House"); // Sesuaikan logika kamu
        //    CheckGameplayMusic(isIndoors);
        //}
        else if (scene.name == gameplaySceneName)
        {
            // Logic Gameplay Music
            bool isIndoors = true; // selalu true karena setiap load gameplay scene adalah di dalam rumah
            CheckGameplayMusic(isIndoors);
        }
    }

    private void InitializeDictionary()
    {
        sfxDictionary = new Dictionary<SoundName, SoundEffect>();

        // Ambil data dari LIBRARY, bukan dari List lokal
        if (audioLibrary != null && audioLibrary.sfxList != null)
        {
            foreach (var sfx in audioLibrary.sfxList)
            {
                // Parsing String ke Enum
                if (System.Enum.TryParse(sfx.soundName, out SoundName resultEnum))
                {
                    if (!sfxDictionary.ContainsKey(resultEnum))
                    {
                        sfxDictionary.Add(resultEnum, sfx);
                    }
                }
            }
        }
    }

    public void PlaySound(SoundName name, bool varyPitch = false)
    {
        if (sfxDictionary.ContainsKey(name))
        {
            SoundEffect sfx = sfxDictionary[name];

            if (varyPitch) sfxSource.pitch = Random.Range(0.85f, 1.15f);
            else sfxSource.pitch = 1f;


            float finalVolume = sfx.volume * sfxMasterVolume;
            sfxSource.PlayOneShot(sfx.clip, finalVolume);
            Debug.Log("Playing SFX: " + name.ToString());
        }
        else
        {
            Debug.LogWarning("SFX not found in dictionary: " + name.ToString());
        }
    }
    public void CheckGameplayMusic(bool isIndoors, float delay = 0f)
    {
        Season currentSeason = TimeManager.Instance.currentSeason;
        WeatherType currentWeather = TimeManager.Instance.isRain ? WeatherType.Raining : WeatherType.Sunny;

        // Ambil klip dari Library
        AudioClip clipToPlay = audioLibrary.GetClip(currentSeason, currentWeather, isIndoors);

        if (clipToPlay != null)
        {
            // Teruskan delay ke fungsi PlayMusic
            PlayMusic(clipToPlay, delay);
        }
    }
    public void PlayMusic(AudioClip newClip, float delay)
    {
        // Tentukan siapa yang aktif sekarang, siapa yang selanjutnya
        AudioSource activeSource = isSourceAPlaying ? sourceA : sourceB;
        AudioSource nextSource = isSourceAPlaying ? sourceB : sourceA;

        // Cek: Kalau lagunya SAMA, jangan di-restart (biar gak aneh)
        if (activeSource.clip == newClip && activeSource.isPlaying) return;

        // Hentikan crossfade sebelumnya jika ada (biar gak tabrakan)
        StopAllCoroutines();

        // Mulai proses transisi halus
        StartCoroutine(FadeRoutine(activeSource, nextSource, newClip, delay));
    }

    private IEnumerator FadeRoutine(AudioSource outgoing, AudioSource incoming, AudioClip newClip, float delay)
    {
        if (delay > 0)
        {
            // Script akan "tidur" di sini selama sekian detik
            // Musik lama tetap main, musik baru belum mulai.
            yield return new WaitForSeconds(delay);
        }
        // Setup Deck Baru (Incoming)
        incoming.clip = newClip;
        incoming.volume = 0f; // Mulai dari bisu
        incoming.Play();

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Turun dari Max ke 0
            outgoing.volume = Mathf.Lerp(musicMasterVolume, 0f, progress);

            yield return null;
        }

        // Pastikan lagu lama benar-benar mati & stop
        outgoing.volume = 0f;
        outgoing.Stop();


        // Di sini suasana akan sunyi senyap selama 1 detik (sesuai settingan)
        if (gapDuration > 0)
        {
            yield return new WaitForSeconds(gapDuration);
        }


        // Setup lagu baru
        incoming.clip = newClip;
        incoming.volume = 0f; // Mulai dari bisu
        incoming.Play();

        timer = 0f; // Reset timer untuk dipakai lagi!

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Naik dari 0 ke Max
            incoming.volume = Mathf.Lerp(0f, musicMasterVolume, progress);

            yield return null;
        }

        // Finalisasi volume lagu baru
        incoming.volume = musicMasterVolume; // Pastikan mentok di limit music

        // Tukar status Deck aktif
        isSourceAPlaying = !isSourceAPlaying;
    }
}