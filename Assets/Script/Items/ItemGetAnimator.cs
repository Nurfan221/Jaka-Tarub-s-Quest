using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class ItemGetAnimator : MonoBehaviour
{
    // Variabel publik yang bisa diatur di Inspector
    public float moveDuration = 0.5f;
    public float overshootDistance = 20f;
    public float fadeDuration = 0.5f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Simpan posisi awal slot item
        startPosition = rectTransform.anchoredPosition;
    }

    // Fungsi ini akan dipanggil untuk memulai animasi
    public void PlayItemGetAnimation()
    {
        StartCoroutine(AnimateSequence());
    }

    private IEnumerator AnimateSequence()
    {
        // 1. Animasi dari Kiri ke Tengah (dengan overshoot)
        float elapsedTime = 0f;
        Vector2 initialPos = new Vector2(startPosition.x - 500f, startPosition.y);
        Vector2 targetPos = startPosition;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            // Gunakan ease-out untuk gerakan yang lebih alami
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            rectTransform.anchoredPosition = Vector2.Lerp(initialPos, targetPos, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPos; // Pastikan mencapai target yang tepat

        // 2. Animasi Overshoot dan Kembali
        elapsedTime = 0f;
        while (elapsedTime < 0.2f) // Durasi pendek untuk pantulan
        {
            float t = elapsedTime / 0.2f;
            rectTransform.anchoredPosition = Vector2.Lerp(targetPos, targetPos + new Vector2(overshootDistance, 0), t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            float t = elapsedTime / 0.2f;
            rectTransform.anchoredPosition = Vector2.Lerp(targetPos + new Vector2(overshootDistance, 0), targetPos, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // 3. Animasi Memudar dan Bergerak ke Bawah
        elapsedTime = 0f;
        Vector2 fadeStartPos = rectTransform.anchoredPosition;
        Vector2 fadeEndPos = fadeStartPos - new Vector2(0, 100); // Bergerak 100 unit ke bawah

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            rectTransform.anchoredPosition = Vector2.Lerp(fadeStartPos, fadeEndPos, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Matikan objek setelah animasi selesai
        Destroy(gameObject);
    }
}