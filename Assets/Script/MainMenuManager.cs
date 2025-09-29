using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Tombol UI")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button quitGameButton;

    void Start()
    {
        // Pastikan semua tombol terhubung
        if (newGameButton == null || loadGameButton == null || quitGameButton == null)
        {
            Debug.LogError("Satu atau lebih tombol belum diatur di MainMenuManager!");
            return;
        }

        // Cek apakah file save ada untuk mengaktifkan/menonaktifkan tombol Load Game
        // Ini aman dilakukan karena SaveDataManager dijamin sudah ada.
        if (SaveDataManager.Instance.SaveFileExists())
        {
            loadGameButton.interactable = true;
        }
        else
        {
            loadGameButton.interactable = false;
        }

        // Hubungkan fungsi ke event OnClick dari setiap tombol
        newGameButton.onClick.AddListener(OnNewGameClicked);
        loadGameButton.onClick.AddListener(OnLoadGameClicked);
        quitGameButton.onClick.AddListener(OnQuitGameClicked);
    }

    public void PlayClickSound()
    {
        SoundManager.Instance.PlaySound("Click");
    }

    private void OnNewGameClicked()
    {
        // Panggil fungsi StartNewGame dari SaveDataManager.
        // Fungsi ini akan menghapus save lama dan memuat scene game.
        Debug.Log("Tombol New Game ditekan.");
        SaveDataManager.Instance.StartNewGame();
    }

    private void OnLoadGameClicked()
    {
        // Cukup panggil fungsi untuk memuat scene.
        // Nantinya, GameController di scene utama yang akan memuat datanya.
        Debug.Log("Tombol Load Game ditekan.");
        SaveDataManager.Instance.LoadGameScene();
    }

    private void OnQuitGameClicked()
    {
        Debug.Log("Tombol Quit Game ditekan.");
        Application.Quit();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("APPLICATION QUIT");
    }
}
