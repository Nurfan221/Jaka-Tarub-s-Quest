using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI Date, Time, Season, Week;

    public Image weatherSprite;
    public Sprite[] weatherSprites;

    private float startingRotation;

    public Light sunlight;
    public float nightIntensity;
    public float dayIntensity;
    public AnimationCurve dayNightCurve;

    // Warna untuk berbagai waktu dalam sehari
    public Color morningColor = new Color(1f, 0.85f, 0.7f);   // Warna pagi
    public Color noonColor = new Color(1f, 1f, 0.9f);         // Warna siang
    public Color eveningColor = new Color(1f, 0.6f, 0.4f);    // Warna sore
    public Color duskColor = new Color(0.5f, 0.3f, 0.3f);     // Warna maghrib (malam sedikit gelap)
    public Color midnightColor = new Color(0.2f, 0.2f, 0.35f); // Warna tengah malam
    
    [SerializeField] private TimeManager timeManager;

    private void Awake()
    {
        // startingRotation = ClockFace.localEulerAngles.z;
    }

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
        // Update UI teks dengan data dari TimeManager
        Date.text = timeManager.GetFormattedDate();
        Time.text = timeManager.GetFormattedTime();
        Season.text = timeManager.GetCurrentSeason();
        Week.text = timeManager.GetCurrentWeek();

        // Ubah intensitas cahaya dan warna berdasarkan waktu
        float t = (float)timeManager.hour / 24f;

        // Atur intensitas cahaya sesuai waktu
        float dayNightT = dayNightCurve.Evaluate(t);
        sunlight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayNightT);

        // Mengatur warna berdasarkan waktu
        Color currentColor;
        if (t < 0.25f)  // Tengah malam ke pagi
        {
            currentColor = Color.Lerp(midnightColor, morningColor, t * 4f);
        }
        else if (t < 0.5f)  // Pagi ke siang
        {
            currentColor = Color.Lerp(morningColor, noonColor, (t - 0.25f) * 4f);
        }
        else if (t < 0.75f)  // Siang ke sore
        {
            currentColor = Color.Lerp(noonColor, eveningColor, (t - 0.5f) * 4f);
        }
        else if (t < 0.9f)  // Sore ke maghrib
        {
            currentColor = Color.Lerp(eveningColor, duskColor, (t - 0.75f) * 6.67f);
        }
        else  // Maghrib ke tengah malam
        {
            currentColor = Color.Lerp(duskColor, midnightColor, (t - 0.9f) * 10f);
        }

        sunlight.color = currentColor;
    }
}
