using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string soundName;
        public AudioClip clip;
    }

    public static SoundManager Instance;

    public List<SoundEffect> soundEffects;
    public List<SoundEffect> bgmTracks;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sfxSource = gameObject.AddComponent<AudioSource>();
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
    }

    public void PlaySound(string soundName)
    {
        SoundEffect sfx = soundEffects.Find(s => s.soundName == soundName);
        if (sfx != null && sfx.clip != null)
        {
            sfxSource.PlayOneShot(sfx.clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    public void PlayBGM(string bgmName)
    {
        SoundEffect bgm = bgmTracks.Find(b => b.soundName == bgmName);
        if (bgm != null && bgm.clip != null && bgmSource.clip != bgm.clip)
        {
            bgmSource.clip = bgm.clip;
            bgmSource.Play();
        }
        else if (bgm == null)
        {
            Debug.LogWarning("BGM not found: " + bgmName);
        }
    }

    public void Stop(string soundName)
    {
        if (bgmSource.clip != null && bgmSource.isPlaying && bgmSource.clip.name == soundName)
        {
            bgmSource.Stop();
        }
        else if (sfxSource.isPlaying && sfxSource.clip != null && sfxSource.clip.name == soundName)
        {
            sfxSource.Stop();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    // Tambahkan metode untuk mengatur volume SFX jika diperlukan
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
