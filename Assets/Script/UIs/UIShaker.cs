using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIShaker : MonoBehaviour
{
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 10f;
    public float colorFlashDuration = 0.2f;
    public Color flashColor = Color.red;

    private Vector3 originalPos;
    private Color originalColor;
    private Image imageComponent;
    private Coroutine shakeRoutine;

    void Awake()
    {
        imageComponent = GetComponent<Image>();

        if (imageComponent != null)
            originalColor = imageComponent.color;
    }

    public void Shake()
    {
        originalPos = transform.localPosition;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        float elapsed = 0f;

        // Jalankan coroutine perubahan warna juga
        if (imageComponent != null)
            StartCoroutine(FlashRed());

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeRoutine = null;
    }



    private IEnumerator FlashRed()
    {
        Image[] allImages = GetComponentsInChildren<Image>(true); // Termasuk anak-anak tersembunyi
        Color[] originalColors = new Color[allImages.Length];

        for (int i = 0; i < allImages.Length; i++)
            originalColors[i] = allImages[i].color;

        // Ganti semua warna jadi merah
        foreach (var img in allImages)
            img.color = flashColor;

        yield return new WaitForSeconds(colorFlashDuration);

        // Kembalikan semua warna
        for (int i = 0; i < allImages.Length; i++)
            allImages[i].color = originalColors[i];
    }

}
