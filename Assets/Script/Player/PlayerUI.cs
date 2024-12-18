using UnityEngine;
using UnityEngine.UI; // Tambahkan ini untuk Button
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;

    public Image dashUI;
    public Image specialAttackUI;
    public TMP_Text promptText;
    public Button promptButton; // Tambahkan ini, Button untuk membungkus promptText
    public Image healthUI;
    public Image staminaUI;
    public Image equippedUI;
    public Image itemUseUI;
    public Button actionInputButton;
    public GameObject inventoryUI;
    public TMP_Text currentQuestText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;

            // Aktifkan atau nonaktifkan tombol berdasarkan apakah teks kosong
            if (string.IsNullOrEmpty(text))
            {
                promptButton.gameObject.SetActive(false);
            }
            else
            {
                promptButton.gameObject.SetActive(true);
            }
        }
    }
}
