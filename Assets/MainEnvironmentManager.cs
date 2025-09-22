using UnityEngine;

public class MainEnvironmentManager : MonoBehaviour
{
    public static MainEnvironmentManager Instance { get; private set; }

    // Referensi ke manajer spesifik (ini adalah "radio" Anda)
    // Anda bisa menyeret skrip EnvironmentManager dari objek lain ke sini di Inspector
    public EnvironmentManager batuManager;
    public TreesManager pohonManager;
    public EnvironmentManager kuburanManager;
    public EnvironmentManager tumbuhanManager;
    public PlantContainer plantContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(this.gameObject); jika diperlukan
        }
    }



}