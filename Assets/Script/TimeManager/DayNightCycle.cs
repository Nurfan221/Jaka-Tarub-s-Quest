using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight; // Referensi ke Directional Light
    public float dayDuration = 30f; // Durasi satu hari dalam detik
    private float time;

    void Update()
    {
        // Menghitung waktu yang telah berlalu
        time += Time.deltaTime;

        // Normalisasi waktu ke antara 0 dan 1
        float normalizedTime = time / dayDuration;

        // Jika lebih dari 1, reset waktu
        if (normalizedTime >= 1f)
        {
            normalizedTime = 0f;
            time = 0f;
        }

        // Mengatur rotasi Directional Light berdasarkan waktu
        directionalLight.transform.localRotation = Quaternion.Euler(normalizedTime * 360f - 90f, 170f, 0f);

        // Mengatur intensitas cahaya untuk efek siang dan malam
        directionalLight.intensity = Mathf.Clamp01(1 - normalizedTime * 2); // Siang: 1, Malam: 0
    }
}
