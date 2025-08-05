using UnityEngine;

public class TorchLight : MonoBehaviour
{
    public Light torchLight;
    public float nightStart = 18f;
    public float nightEnd = 6f;

    private void Start()
    {
        // Mencari TimeManager di scene saat runtime

       
    }


    private void Update()
    {
        float hour = TimeManager.Instance.hour;
        bool isRain = TimeManager.Instance.timeData_SO.isRain;

        // Hidupkan lampu obor hanya di malam hari
        if (hour >= nightStart || hour < nightEnd || isRain)
        {
            torchLight.enabled = true;
        }
        else
        {
            torchLight.enabled = false;
        }
    }
}
