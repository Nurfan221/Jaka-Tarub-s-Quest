using UnityEngine;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // Pastikan SoundManager ditemukan
        SoundManager soundManager = SoundManager.Instance;
        if (soundManager == null)
        {
            Debug.LogError("SoundManager tidak ditemukan!");
            return;
        }

        // Pastikan slider terhubung sebelum digunakan
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
            SetMusicVolume(volumeSlider.value); // Set initial volume
        }
        else
        {
            Debug.LogError("Slider tidak ditemukan!");
        }
    }

    public void SetMusicVolume(float volume)
    {
        SoundManager.Instance.SetMusicVolume(volume);
    }
}
