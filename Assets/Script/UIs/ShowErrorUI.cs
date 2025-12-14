using System.Collections;
using TMPro;
using UnityEngine;

public class ShowErrorUI : MonoBehaviour
{

    [Header("UI References")]
    public GameObject errorPanel;
    public TMP_Text errorText;
    public CanvasGroup errorCanvasGroup;

    private Coroutine errorCoroutine;


    private void Start()
    {
        gameObject.SetActive(false);

    }
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

    // Mengganti nama dan parameter dari 'ShowErrorUICoroutine'
    private IEnumerator ShowErrorCoroutine(string messageToDisplay, float showDuration, float fadeDuration)
    {
        // Pastikan semua referensi UI sudah di-assign di Inspector
        if (errorPanel == null || errorText == null || errorCanvasGroup == null)
        {
            Debug.LogError("ShowErrorUI tidak disetup dengan benar di Inspector!");
            yield break; // Hentikan coroutine jika setup salah
        }

        Debug.Log($"munculkan error {messageToDisplay}");
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
        if (errorPanel != null)
            errorCanvasGroup.alpha = 0f;
        // Tidak perlu StopCoroutine di sini, karena fungsi ini
        // dipanggil di *akhir* coroutine. Cukup set ke null.
        errorCoroutine = null;
        gameObject.SetActive(false);
    }
}