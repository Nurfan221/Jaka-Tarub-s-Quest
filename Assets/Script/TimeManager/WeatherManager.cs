using UnityEngine;

public class WeatherManager : MonoBehaviour
{


    //public ParticleSystem rainParticle;
    //public bool isRain;

    public float rainChance = 0f;

   

    void Update()
    {
        // Optionally update rain chance each frame, if needed
    }
    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
    }


    private void HandleNewDay()
    {
        Debug.Log("WeatherManager menerima sinyal hari baru!");

        // Pastikan timeData_SO di TimeManager bersifat public atau memiliki getter.
        Season currentSeason = TimeManager.Instance.currentSeason;

        SetRainChance(currentSeason);
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
        float randomValue = Random.Range(0f, 1f);

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
}
