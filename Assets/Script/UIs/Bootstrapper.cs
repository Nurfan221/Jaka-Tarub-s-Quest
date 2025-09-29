using UnityEngine;
using UnityEngine.SceneManagement;


public class Bootstrapper : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene Main Menu yang akan dimuat.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    void Start()
    {
        // Panggil fungsi untuk memuat scene Main Menu.
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
