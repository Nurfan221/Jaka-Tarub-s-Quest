using UnityEngine;

public class TorchLight : MonoBehaviour
{
    public Light torchLight;
    public float nightStart = 18f;
    public float nightEnd = 6f;
    private TimeManager timeManager;

    private void Start()
    {
        // Mencari TimeManager di scene saat runtime
        timeManager = FindObjectOfType<TimeManager>();

        // Jika TimeManager tidak ditemukan, tampilkan error
        if (timeManager == null)
        {
            Debug.LogError("TimeManager tidak ditemukan di scene!");
        }
        else
        {
            Debug.Log("time manager di temukan ");
            Debug.Log("sekarang jam  " + timeManager.hour) ;
        }
    }


    private void Update()
    {
        float hour = timeManager.hour + (timeManager.minutes / 60f);

        // Hidupkan lampu obor hanya di malam hari
        if (hour >= nightStart || hour < nightEnd)
        {
            torchLight.enabled = true;
        }
        else
        {
            torchLight.enabled = false;
        }
    }
}
