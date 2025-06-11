using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] TMP_Text tipsText;
    public Sprite[] loadingImages;
    public Transform loadingImageTransform;
    public float frameRate = 0.3f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini

    private Coroutine animationCoroutine;


    [SerializeField] string[] tips;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }



    public void ShowLoading()
    {
        tipsText.text = "Tips: \n" + tips[Random.Range(0, tips.Length)];
        transform.GetChild(0).gameObject.SetActive(true);

        // Mulai animasi hanya sekali
        if (animationCoroutine == null)
            animationCoroutine = StartCoroutine(PlayLoadingAnimation());
    }


    public void HideLoading()
    {
        StartCoroutine(HideLoadingCoroutine());
    }

    private IEnumerator HideLoadingCoroutine()
    {
        // Tunggu hingga frame terakhir tampil
        while (currentFrame < loadingImages.Length - 1)
        {
            yield return null;
        }

        // Tunggu sedikit untuk memastikan frame terakhir tampil
        yield return new WaitForSeconds(frameRate);

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        transform.GetChild(0).gameObject.SetActive(false);
    }




    IEnumerator LoadingScene(int i)
    {
        Debug.Log("LOADING SCENE");

        string loading = "Loading...";
        StartCoroutine(PlayLoadingAnimation()); // Mulai animasi
        yield return new WaitForSeconds(i);
        transform.GetChild(0).gameObject.SetActive(false);
        Debug.Log("DONE");
    }

    private IEnumerator PlayLoadingAnimation()
    {
        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (loadingImages.Length > 0) // Pastikan array sprite tidak kosong
            {
                Image imageloadingScreen = loadingImageTransform.GetComponent<Image>();
                imageloadingScreen.sprite = loadingImages[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % loadingImages.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSeconds(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }
}
