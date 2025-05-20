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
    public Color rainColor = new Color(0.4f, 0.4f, 0.6f); // pencahayaan saat hujan 
    public Color currentColor; // Atur transisi warna berdasarkan waktu 

    [SerializeField] private TimeManager timeManager;
    [SerializeField] GameEconomy gameEconomy;
    [SerializeField] WeatherManager weatherManager;

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
        Money.text = gameEconomy.money.ToString();

        float hour = timeManager.hour + (timeManager.minutes / 60f);

        float t = hour / 24f;
        float dayNightT = dayNightCurve.Evaluate(t);

        // CEK DULU: Kalau hujan, pakai nightColor terus!
        if (weatherManager.isRain)
        {
            //Debug.Log("hujan");
            currentColor = rainColor;
        }
        else
        {
            if (hour >= 18 || hour < 4)
            {
                currentColor = nightColor;
            }
            else if (hour >= 4 && hour < 6)
            {
                float subuhProgress = Mathf.InverseLerp(4, 6, hour);
                currentColor = Color.Lerp(nightColor, morningColor, subuhProgress);
            }
            else if (hour >= 6 && hour < 12)
            {
                float morningProgress = Mathf.InverseLerp(6, 12, hour);
                currentColor = Color.Lerp(morningColor, noonColor, morningProgress);
            }
            else if (hour >= 12 && hour < 15)
            {
                float noonProgress = Mathf.InverseLerp(12, 15, hour);
                currentColor = Color.Lerp(noonColor, eveningColor, noonProgress);
            }
            else
            {
                float eveningProgress = Mathf.InverseLerp(15, 18, hour);
                currentColor = Color.Lerp(eveningColor, nightColor, eveningProgress);
            }
        }

        sunlight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayNightT);
        sunlight.color = currentColor;

        RenderSettings.ambientLight = Color.Lerp(nightColor, currentColor, dayNightT);
        RenderSettings.fogColor = Color.Lerp(new Color(0.03f, 0.03f, 0.08f), currentColor, dayNightT);
        RenderSettings.fogDensity = Mathf.Lerp(0.07f, 0.015f, dayNightT);
    }



}
