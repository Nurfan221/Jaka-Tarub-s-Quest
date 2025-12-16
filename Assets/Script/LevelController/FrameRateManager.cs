using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Matikan VSync lewat kodingan (biar pasti)
        QualitySettings.vSyncCount = 0;

        // Paksa target FPS ke 60 (atau 120 jika HP gaming)
        Application.targetFrameRate = 60;

        Debug.Log("FPS dipaksa ke 60!");
    }
}
