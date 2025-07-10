using UnityEngine;

public class WeatherManager : MonoBehaviour
{

    [SerializeField] private TimeManager timeManager;  // Referensi ke TimeManager
    [SerializeField] private ClockManager clockManager;
    public ParticleSystem rainParticle;
    public bool isRain;

    public float rainChance = 0f;

    void Start()
    {
        // Set initial rain chance
        //SetRainChance();
        TimeManager.Instance.RegisterWeather(this);
    }

    private void OnDestroy()
    {
        TimeManager.Instance.UnregisterWeather(this);
    }

    void Update()
    {
        // Optionally update rain chance each frame, if needed
    }

    //public void SetRainChance()
    //{
    //    // Akses currentSeason melalui timeManager
    //    switch (timeManager.timeData_SO.currentSeason)
    //    {

    //        case TimeManager.Season.Rain:
    //            rainChance = 0.80f;
    //            break;

    //        case TimeManager.Season.Dry:
    //            rainChance = 0.1f; // Dry season doesn't have rain chance
    //            break;

    //        default:
    //            rainChance = 0f;  // Nilai default jika tidak ada yang cocok
    //            break;
    //    }
    //}

    public void CheckForRain()
    {
        float randomValue = Random.Range(0f, 1f);

        if (randomValue <= rainChance)
        {
            isRain = true;
            rainParticle.Play();
        }
        else
        {
            isRain = false;
            rainParticle.Stop();
            Debug.Log("Tidak turun hujan");
        }
    }
}
