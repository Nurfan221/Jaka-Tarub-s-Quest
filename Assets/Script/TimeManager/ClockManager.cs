using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{
    public static ClockManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI Date, Time, Season;

    [Header("System Settings")]
    public bool isIndoors = false; // Set true jika masuk rumah

    // Kita tidak mewarnai SpriteOverlay, tapi mewarnai Background Kamera Cahaya
    [Header("Light System")]
    public Camera lightCamera; // TARIK OBJEK "LightCamera" KE SINI!

    [Header("Color Settings (Multiply Logic)")]

    // Subuh: Sedikit gelap agak oranye
    public Color morningColor = new Color(0.8f, 0.8f, 0.7f, 1f);

    // Siang: PUTIH MUTLAK (Agar gambar game asli terlihat 100%)
    public Color noonColor = Color.white;

    // Sore: Agak Oranye Kemerahan
    public Color eveningColor = new Color(0.8f, 0.6f, 0.4f, 1f);

    // Malam: Biru Tua Gelap (Semakin gelap warnanya, semakin gelap gamenya)
    public Color nightColor = new Color(0.05f, 0.05f, 0.25f, 1f);

    // Hujan: Abu-abu kebiruan
    public Color rainColor = new Color(0.4f, 0.5f, 0.6f, 1f);

    private Color targetColor;

    [Header("Minimap Settings")]
    public RawImage minimapOverlay;
    public Color mapNightColor = new Color(0.36f, 0.39f, 0.69f, 1f);
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
        // Update Teks UI
        if (Date != null) Date.text = TimeManager.Instance.GetFormattedDate();
        if (Time != null) Time.text = TimeManager.Instance.GetFormattedTime();
        if (Season != null) Season.text = TimeManager.Instance.GetCurrentSeason().ToString();

        // Hitung Waktu Desimal
        int minutes = TimeManager.Instance.minutes;
        float hour = TimeManager.Instance.hour + (minutes / 60f);

        // LOGIKA WARNA (Targeting Background Camera)
        //if (isIndoors)
        //{
        //    // Di dalam rumah terang (Putih = Normal)
        //    targetColor = morningColor;
        //}
        //if
        //{

        //}
        bool isRaining = TimeManager.Instance.isRain;

        if (isRaining)
        {

             if (hour >= 17 && hour < 19) // Maghrib (Senja -> Malam)
            {
                float progress = Mathf.InverseLerp(16, 19, hour);
                targetColor = Color.Lerp(rainColor, nightColor, progress);
            }
            else // Malam Larut
            {
                targetColor = rainColor;
            }
        }
        else
        {
            // -- FASE WAKTU --
            if (hour >= 0 && hour < 4) // Tengah Malam - Subuh
            {
                targetColor = nightColor;
            }
            else if (hour >= 4 && hour < 6) // Subuh
            {
                float progress = Mathf.InverseLerp(4, 6, hour);
                targetColor = Color.Lerp(nightColor, morningColor, progress);
            }
            else if (hour >= 6 && hour < 10) // Pagi -> Siang
            {
                float progress = Mathf.InverseLerp(6, 10, hour);
                targetColor = Color.Lerp(morningColor, noonColor, progress);
            }
            else if (hour >= 10 && hour < 15) // Siang Bolong (Terang)
            {
                targetColor = noonColor;
            }
            else if (hour >= 15 && hour < 17) // Menuju Sore
            {
                float progress = Mathf.InverseLerp(15, 17, hour);
                targetColor = Color.Lerp(noonColor, eveningColor, progress);
            }
            else if (hour >= 17 && hour < 19) // Maghrib (Senja -> Malam)
            {
                float progress = Mathf.InverseLerp(17, 19, hour);
                targetColor = Color.Lerp(eveningColor, nightColor, progress);
            }
            else // Malam Larut
            {
                targetColor = nightColor;
            }
        }


        // TERAPKAN WARNA KE BACKGROUND CAMERA
        if (lightCamera != null)
        {
            lightCamera.backgroundColor = targetColor;
        }

        if (minimapOverlay != null)
        {
            Color miniMapColor = targetColor;

            // Jika Siang (Putih), Minimap harus Bening (Alpha 0) agar peta terlihat jelas
            if (targetColor == noonColor)
            {
                miniMapColor.a = 0.5f;
            }
            else
            {
                miniMapColor = mapNightColor;
                // Jika Malam/Sore, gunakan Opacity yang lebih rendah (misal 0.5)
                // Agar peta tetap terlihat walau sedang malam
                miniMapColor.a = 1f;
            }

            minimapOverlay.color = miniMapColor;
        }
    }
}