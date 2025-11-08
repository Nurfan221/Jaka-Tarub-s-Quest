using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowErrorUI : MonoBehaviour
{
    public static ShowErrorUI Instance { get; private set; }

    // --- PASTIKAN ANDA MENAMBAHKAN INI ---
    [Header("UI References")]
    public GameObject errorPanel;
    public TMP_Text errorText;
    public CanvasGroup errorCanvasGroup;
    // ------------------------------------------

    private Coroutine errorCoroutine;

    private void Awake()
    {
        // Logika Singleton standar
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        // --- PERBAIKAN 2: Pastikan UI Sembunyi Saat Awal ---
        // Ini untuk memastikan panel error tidak terlihat saat game pertama kali dimuat
        if (errorPanel != null)
        {
            if (errorCanvasGroup != null)
            {
                errorCanvasGroup.alpha = 0f;
            }
        }
    }

    // --- PERBAIKAN 3: Fungsi Publik yang Disederhanakan ---
    // Hapus 'StartErrorUI()' dan 'IEnumerator ShowErrorUI()' yang lama.
    // Ganti dengan satu fungsi publik yang jelas ini.
    // Fungsi ini akan menjadi satu-satunya cara untuk memanggil error.
    /// <summary>
    /// Menampilkan pesan error di UI.
    /// </summary>
    /// <param name="message">Pesan yang akan ditampilkan</param>
    /// <param name="showDuration">Berapa lama pesan terlihat (setelah fade in)</param>
    /// <param name="fadeDuration">Berapa lama untuk fade in/out</param>
    public void ShowError(string message, float showDuration = 2f, float fadeDuration = 0.5f)
    {
        // Jika sudah ada error yang sedang tampil, hentikan dulu
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
        }

        // Mulai coroutine baru dengan parameter yang diberikan
        errorCoroutine = StartCoroutine(ShowErrorCoroutine(message, showDuration, fadeDuration));
    }

    // --- PERBAIKAN 4: Coroutine Utama ---
    // Mengganti nama dan parameter dari 'ShowErrorUICoroutine'
    private IEnumerator ShowErrorCoroutine(string messageToDisplay, float showDuration, float fadeDuration)
    {
        // Pastikan semua referensi UI sudah di-assign di Inspector
        if (errorPanel == null || errorText == null || errorCanvasGroup == null)
        {
            Debug.LogError("ShowErrorUI tidak disetup dengan benar di Inspector!");
            yield break; // Hentikan coroutine jika setup salah
        }

        // --- PERBAIKAN 5: Gunakan Variabel yang Benar ---
        // Set teks dan aktifkan panel
        errorText.text = messageToDisplay; // Menggunakan parameter 'messageToDisplay'
        errorPanel.SetActive(true);
        errorCanvasGroup.alpha = 0f;

        // Fade In
        float timer = 0f;
        while (timer < fadeDuration)
        {
            // Cek jika panel (errorPanel) dinonaktifkan di tengah animasi
            if (!errorPanel.activeInHierarchy)
            {
                ResetErrorUI();
                yield break;
            }

            timer += Time.deltaTime;
            errorCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        errorCanvasGroup.alpha = 1f;

        // Tahan beberapa detik
        yield return new WaitForSeconds(showDuration);

        // Fade Out
        timer = 0f;
        while (timer < fadeDuration)
        {
            if (!errorPanel.activeInHierarchy)
            {
                ResetErrorUI();
                yield break;
            }

            timer += Time.deltaTime;
            errorCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        errorCanvasGroup.alpha = 0f;

        ResetErrorUI();
    }

    public void ResetErrorUI()
    {
        // --- PERBAIKAN 6: Gunakan Referensi yang Benar ---
        if (errorPanel != null)
            errorCanvasGroup.alpha = 0f;
        // Tidak perlu StopCoroutine di sini, karena fungsi ini
        // dipanggil di *akhir* coroutine. Cukup set ke null.
        errorCoroutine = null;
    }
}