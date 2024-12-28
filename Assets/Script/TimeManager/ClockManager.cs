using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI Date, Time, Season, Money;
    public Image weatherSprite;
    public Sprite[] weatherSprites;

    public Light sunlight;
    public float nightIntensity = 0.05f;  // Malam lebih gelap
    public float dayIntensity = 1.5f;     // Siang lebih terang
    public AnimationCurve dayNightCurve;  // Untuk transisi halus

    public Color morningColor = new Color(1f, 0.85f, 0.7f);  // Cahaya pagi (oranye lembut)
    public Color noonColor = new Color(1f, 1f, 0.95f);       // Cahaya siang (putih cerah)
    public Color eveningColor = new Color(1f, 0.7f, 0.4f);   // Cahaya sore (jingga)
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f);   // Cahaya malam (biru gelap)

    [SerializeField] private TimeManager timeManager;
    [SerializeField] GameEconomy gameEconomy;

    private void OnEnable()
    {
        TimeManager.OnTimeChanged += UpdateDateTime;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeChanged -= UpdateDateTime;
    }

    private void UpdateDateTime()
    {
        Date.text = timeManager.GetFormattedDate();
        Time.text = timeManager.GetFormattedTime();
        Season.text = timeManager.GetCurrentSeason();
        Money.text = gameEconomy.Money.ToString();

        float hour = timeManager.hour + (timeManager.minutes / 60f);

        // Skala waktu 24 jam ke rentang 0 - 1
        float t = hour / 24f;

        // Intensitas siang dan malam menggunakan Animation Curve
        float dayNightT = dayNightCurve.Evaluate(t);

        // Atur transisi warna berdasarkan waktu
        Color currentColor;

        if (hour >= 18 || hour < 4)  // Malam (18:00 - 04:00)
        {
            currentColor = nightColor;  // Tetap malam gelap tanpa transisi
        }
        else if (hour >= 4 && hour < 6)  // Subuh (04:00 - 06:00) 
        {
            float subuhProgress = Mathf.InverseLerp(4, 6, hour);  // Transisi dari gelap ke pagi
            currentColor = Color.Lerp(nightColor, morningColor, subuhProgress);
        }
        else if (hour >= 6 && hour < 12)  // Pagi (06:01 - 12:00)
        {
            float morningProgress = Mathf.InverseLerp(6, 12, hour);
            currentColor = Color.Lerp(morningColor, noonColor, morningProgress);
        }
        else if (hour >= 12 && hour < 15)  // Siang (12:01 - 15:00)
        {
            float noonProgress = Mathf.InverseLerp(12, 15, hour);
            currentColor = Color.Lerp(noonColor, eveningColor, noonProgress);
        }
        else  // Sore (15:01 - 18:00)
        {
            float eveningProgress = Mathf.InverseLerp(15, 18, hour);
            currentColor = Color.Lerp(eveningColor, nightColor, eveningProgress);
        }

        // Terapkan intensitas dan warna ke light
        sunlight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayNightT);
        sunlight.color = currentColor;

        // Atur ambient light dan fog agar sesuai suasana
        RenderSettings.ambientLight = Color.Lerp(nightColor, currentColor, dayNightT);
        RenderSettings.fogColor = Color.Lerp(new Color(0.03f, 0.03f, 0.08f), currentColor, dayNightT);
        RenderSettings.fogDensity = Mathf.Lerp(0.07f, 0.015f, dayNightT);
    }


}
