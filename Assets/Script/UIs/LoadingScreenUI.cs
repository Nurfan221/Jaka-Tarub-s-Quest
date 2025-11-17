using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] TMP_Text tipsText;
    public Sprite[] loadingImages;

    public Transform loadingImageTransform;
    public float frameRate = 0.1f; // Waktu per frame (kecepatan animasi)
    private int currentFrame = 0; // Indeks frame saat ini

    private Coroutine animationCoroutine;


    [SerializeField] string[] tips;
    [Header("UI Animation")]
    public RectTransform bgTransform; // Gunakan RectTransform untuk UI

    [Header("Pengaturan Animasi")]
    public float animationSpeed; // Kecepatan animasi
    public float endPosition_Y = 0f; // Posisi Y akhir (biasanya di tengah)
    public float startPosition_Y = 500f; // Posisi Y awal (di luar layar atas)

    public bool isAnimating = false;

    public static event UnityAction OnFinishedLoadingScreen;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    private void Update()
    {


    }
    public void ShowLoading(bool achievement,string textLoading)
    {
        // Hentikan coroutine lama jika ada sebelum memulai yang baru
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        tipsText.text = "Tips: \n" + tips[Random.Range(0, tips.Length)];
        transform.GetChild(0).gameObject.SetActive(true);
        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();

        // Mulai coroutine yang baru

        animationCoroutine = StartCoroutine(PlayLoadingAnimation(achievement, textLoading));

    }

    public IEnumerator SetLoadingandTimer(bool achievement)
    {
        isAnimating = true;
        string textLoading = "Chapter-1, Selesai";
        ShowLoading(achievement, textLoading);
        yield return new WaitForSecondsRealtime(1.5f); // Jeda minimal 1.5 detik agar tips terbaca
        LoadingScreenUI.Instance.HideLoading();
    }

    public void HideLoading()
    {
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
        StartCoroutine(HideLoadingCoroutine());
    }

    private IEnumerator HideLoadingCoroutine()
    {
        OnFinishedLoadingScreen?.Invoke();
        isAnimating = false;

        // --- DEBUGGING DIMULAI DI SINI ---
        Debug.Log($"[Hide Coroutine] Memulai. Time.timeScale saat ini adalah: {Time.timeScale}");
        Debug.Log($"[Hide Coroutine] Mengecek kondisi while: currentFrame ({currentFrame}) < loadingImages.Length - 1 ({loadingImages.Length - 1})");

        // Tunggu hingga frame terakhir tampil
        int loopCounter = 0; // Pelacak untuk mencegah log spam tak terbatas
        while (currentFrame < loadingImages.Length - 1)
        {
            // Log ini hanya akan muncul beberapa kali jika loop berjalan normal,
            // tapi akan muncul TERUS MENERUS jika terjebak.
            if (loopCounter < 100) // Batasi log agar tidak crash
            {
                Debug.LogWarning($"[Hide Coroutine] TERJEBAK di dalam while loop! Frame ke-{loopCounter}. currentFrame masih {currentFrame}. Menunggu frame berikutnya...");
                loopCounter++;
            }

            // Periksa apakah game di-pause, ini adalah kemungkinan penyebabnya
            if (Time.timeScale == 0f && loopCounter == 10)
            {
                Debug.LogError("[Hide Coroutine] FATAL: Time.timeScale adalah 0! Coroutine tidak akan pernah bisa melanjutkan dari sini.");
            }

            yield return null;
        }

        Debug.Log($"[Hide Coroutine] Berhasil keluar dari while loop. currentFrame sekarang {currentFrame}.");
        // --- DEBUGGING SELESAI ---

        // Tunggu sedikit untuk memastikan frame terakhir tampil
        // Ganti ke WaitForSecondsRealtime agar tidak terpengaruh oleh Time.timeScale
        yield return new WaitForSecondsRealtime(frameRate);

        if (animationCoroutine != null)
        {
            Debug.Log("[Hide Coroutine] Menghentikan animationCoroutine...");
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        Debug.Log("[Hide Coroutine] Hiding loading screen SEKARANG!");
        transform.GetChild(0).gameObject.SetActive(false);
    }




    //IEnumerator LoadingScene(int i)
    //{
    //    Debug.Log("LOADING SCENE");

    //    string loading = "Loading...";
    //    StartCoroutine(PlayLoadingAnimation()); // Mulai animasi
    //    yield return new WaitForSeconds(i);
    //    transform.GetChild(0).gameObject.SetActive(false);
    //    Debug.Log("DONE");
    //}

    private IEnumerator PlayLoadingAnimation(bool achievement, string textLoading)
    {
        if (achievement)
        {
            StartAnimation(textLoading);
        }

        while (true) // Loop tanpa batas (animasi berulang)
        {
            if (loadingImages.Length > 0) // Pastikan array sprite tidak kosong
            {
                Image imageloadingScreen = loadingImageTransform.GetComponent<Image>();
                imageloadingScreen.sprite = loadingImages[currentFrame]; // Setel sprite saat ini
                currentFrame = (currentFrame + 1) % loadingImages.Length; // Pindah ke frame berikutnya (loop)
            }
            yield return new WaitForSecondsRealtime(frameRate); // Tunggu sebelum beralih ke frame berikutnya
        }
    }




    public void StartAnimation(string textLoading) // Ubah nama fungsi ini agar tidak bentrok dengan Start()
    {
        // Hentikan coroutine lama jika ada
        //StopAllCoroutines();
        TMP_Text loadingText = bgTransform.GetComponent<TMP_Text>();

        // Atur posisi awal
        if (bgTransform != null)
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.anchoredPosition.x, startPosition_Y);
        }

        // Mulai coroutine animasi
        StartCoroutine(AnimateDownCoroutine());
    }

    private IEnumerator AnimateDownCoroutine()
    {
        Debug.Log("Memulai animasi turun...");
        Vector2 targetPosition = new Vector2(bgTransform.anchoredPosition.x, endPosition_Y);

        while (Vector2.Distance(bgTransform.anchoredPosition, targetPosition) > 0.1f)
        {
            // Gunakan Time.unscaledDeltaTime yang mengabaikan Time.timeScale
            bgTransform.anchoredPosition = Vector2.Lerp(
                bgTransform.anchoredPosition,
                targetPosition,
                Time.unscaledDeltaTime * animationSpeed
            );

            yield return null; // Tunggu satu frame
        }

        // Posisikan secara tepat di posisi akhir
        bgTransform.anchoredPosition = targetPosition;

        Debug.Log("Animasi selesai!");
    }


}
