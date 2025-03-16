using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;  // Referensi ke TimeManager
    [SerializeField] private ClockManager clockManager;

    public float rainChance = 0f;

    void Start()
    {
        // Set initial rain chance
        SetRainChance();
    }

    void Update()
    {
        // Optionally update rain chance each frame, if needed
    }

    public void SetRainChance()
    {
        // Akses currentSeason melalui timeManager
        switch (timeManager.currentSeason)
        {

            case TimeManager.Season.Rain:
                rainChance = 0.80f;
                break;

            case TimeManager.Season.Dry:
                rainChance = 0.1f; // Dry season doesn't have rain chance
                break;

            default:
                rainChance = 0f;  // Nilai default jika tidak ada yang cocok
                break;
        }
    }

    public void CheckForRain()
    {
        float randomValue = Random.Range(0f, 1f);

        if (randomValue <= rainChance)
        {
            Debug.Log("Turun hujan yeeeayyyy");
        }
        else
        {
            Debug.Log("Tidak turun hujan");
        }
    }
}
