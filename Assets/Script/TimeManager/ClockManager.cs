using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{

    public static ClockManager Instance { get; private set; }
    public TextMeshProUGUI Date, Time, Season;
    public Image weatherSprite;
    public Sprite[] weatherSprites;
    public bool isIndoors = true; // Apakah pemain sedang di dalam ruangan?

    public float nightIntensity = 0.05f;  // Malam lebih gelap
    public float dayIntensity = 1.5f;     // Siang lebih terang
    public AnimationCurve dayNightCurve;  // Untuk transisi halus

    [Header("Color Settings")]
    // Subuh: Alpha 100 (dari 255) -> 0.4f
    public Color morningColor = new Color(1f, 1f, 0.9f, 0.1f);

    // Siang: Alpha 0 (dari 255) -> 0f (Bening Total)
    public Color noonColor = new Color(1f, 1f, 1f, 0f);

    // Sore: Alpha 128 (dari 255) -> 0.5f (Setengah transparan)
    public Color eveningColor = new Color(0.7f, 0.4f, 0.2f, 0.3f);

    // Malam: Alpha 150 (dari 255) -> ~0.6f
    // Ini settingan yang kamu minta (agar tidak terlalu gelap)
    public Color nightColor = new Color(0.05f, 0.05f, 0.25f, 0.6f);

    // Hujan: Alpha 120 (dari 255) -> ~0.47f
    public Color rainColor = new Color(0.2f, 0.3f, 0.4f, 0.47f);

    private Color targetColor;

    [Header("The 'Mika Ajaib' Overlay")]
    // Masukkan Image UI yang menutupi layar di sini
    public SpriteRenderer darknessOverlay; // Masukkan objek Darkness_Cutout

    //[SerializeField] private TimeManager timeManager;
    //[SerializeField] GameEconomy gameEconomy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);

        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        //float progress = Mathf.InverseLerp(4, 6, 4);
        //targetColor = Color.Lerp(nightColor, morningColor, progress);
        //if (darknessOverlay != null)
        //{
        //    darknessOverlay.color = targetColor;
        //    //darknessOverlay.al
        //}
    }

    private void OnEnable()
    {
        TimeManager.OnTimeChanged += UpdateDateTime;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeChanged -= UpdateDateTime;
    }


    public void UpdateDateTime()
    {
        Date.text = TimeManager.Instance.GetFormattedDate();
        Time.text = TimeManager.Instance.GetFormattedTime();
        Season.text = TimeManager.Instance.GetCurrentSeason().ToString();

        // itung Waktu
        int minutes = TimeManager.Instance.minutes;
        float hour = TimeManager.Instance.hour + (minutes / 60f);

        // LOGIKA MIKA AJAIB (Overlay)
        if (isIndoors)
        {
            // Jika di dalam, PAKSA warna jadi Bening/Siang
            targetColor = noonColor;
        }else
        {
            bool isRaining = TimeManager.Instance.isRain;

            if (isRaining)
            {
                // Jika hujan, langsung pakai warna mendung (Mika abu-abu)
                targetColor = rainColor;
            }
            else
            {
                // Logika Transisi Waktu (Lerp Warna + Alpha sekaligus)

                if (hour >= 0 && hour < 4) // Tengah Malam - Subuh
                {
                    targetColor = nightColor;
                }
                else if (hour >= 4 && hour < 6) // Subuh (Gelap -> Oranye)
                {
                    float progress = Mathf.InverseLerp(4, 6, hour);
                    targetColor = Color.Lerp(nightColor, morningColor, progress);
                }
                else if (hour >= 6 && hour < 10) // Pagi (Oranye -> Bening/Siang)
                {
                    float progress = Mathf.InverseLerp(6, 10, hour);
                    targetColor = Color.Lerp(morningColor, noonColor, progress);
                }
                else if (hour >= 10 && hour < 15)
                {
                    targetColor = noonColor; // Alpha 0
                }

                // FASE 5: Menuju Sore (15:00 - 17:00)
                // Kita geser targetnya, jam 5 sore (17:00) harus sudah FULL Oranye
                else if (hour >= 15 && hour < 17)
                {
                    float progress = Mathf.InverseLerp(15, 17, hour);
                    targetColor = Color.Lerp(noonColor, eveningColor, progress);
                }

                // FASE 6: Senja / Maghrib (17:00 - 19:00)
                // INI KUNCINYA: Transisi ke malam dimulai SEJAK JAM 17:00
                else if (hour >= 17 && hour < 19)
                {
                    float progress = Mathf.InverseLerp(17, 19, hour);
                    targetColor = Color.Lerp(eveningColor, nightColor, progress);

                    // HASIL VISUAL:
                    // Jam 17:00 = Oranye
                    // Jam 18:00 = Campuran Oranye & Biru (Ungu/Remang Maghrib) -> PAS!
                    // Jam 19:00 = Biru Tua (Gelap)
                }

                // FASE 7: Malam Larut (19:00 - 24:00)
                else
                {
                    targetColor = nightColor;
                }
            }
        }


        // TERAPKAN KE MIKA (Image UI)
        if (darknessOverlay != null)
        {
            darknessOverlay.color = targetColor;
        }


    }



}
