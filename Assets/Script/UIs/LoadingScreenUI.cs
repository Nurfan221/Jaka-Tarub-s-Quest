using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    private Coroutine moveCoroutine;

    [SerializeField] string[] tips;
    [Header("UI Animation")]
    public RectTransform bgTransform; // Gunakan RectTransform untuk UI
    public string achievementText;

    [Header("Pengaturan Animasi")]
    public float animationSpeed; // Kecepatan animasi
    public float endPosition_Y = 0f; // Posisi Y akhir (biasanya di tengah)
    public float startPosition_Y = 600f; // Posisi Y awal (di luar layar atas)

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
    public void ShowLoading(bool achievement, string textLoading)
    {
        // Hentikan coroutine loop gambar (Loading Spinner)
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // Hentikan coroutine gerakan background (PENTING: Agar tidak bentrok)
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        tipsText.text = "Tips: \n" + tips[Random.Range(0, tips.Length)];

        // Reset posisi dulu sebelum mengaktifkan UI agar tidak terlihat 'glitch' di posisi lama
        // Kita default-kan ke atas dulu, nanti PlayLoadingAnimation yang menentukan turun atau tidak
        if (bgTransform != null)
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.anchoredPosition.x, startPosition_Y);
        }

        transform.GetChild(0).gameObject.SetActive(true);
        GameController.Instance.ShowPersistentUI(false);
        GameController.Instance.PauseGame();

        // Mulai coroutine utama
        animationCoroutine = StartCoroutine(PlayLoadingAnimation(achievement, textLoading));
    }

    public IEnumerator SetLoadingandTimer(bool achievement, string achievementText = "")
    {
        this.achievementText = achievementText;
        Debug.Log("[SetLoadingandTimer] Memulai loading screen..." + achievementText);
        isAnimating = true;
        ShowLoading(achievement, achievementText);
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
        // Setup teks
        TMP_Text loadingText = bgTransform.GetChild(0).GetComponent<TMP_Text>();
        if (loadingText != null)
        {
            loadingText.text = textLoading;
        }

        if (achievement)
        {
            // Jika True: Jalankan animasi turun
            StartAnimation(textLoading);
        }
        else
        {
            // Jika False:
            // Pastikan tidak ada animasi turun yang berjalan (Safety double check)
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);

            // PAKSA posisi tetap di ATAS (startPosition_Y)
            if (bgTransform != null)
            {
                bgTransform.anchoredPosition = new Vector2(bgTransform.anchoredPosition.x, startPosition_Y);
            }
        }

        // Loop animasi loading spinner (gambar berputar/berubah)
        while (true)
        {
            if (loadingImages.Length > 0 && loadingImageTransform != null)
            {
                Image imageloadingScreen = loadingImageTransform.GetComponent<Image>();
                if (imageloadingScreen != null)
                {
                    imageloadingScreen.sprite = loadingImages[currentFrame];
                    currentFrame = (currentFrame + 1) % loadingImages.Length;
                }
            }
            yield return new WaitForSecondsRealtime(frameRate);
        }
    }




    public void StartAnimation(string textLoading)
    {
        Debug.Log("Memulai animasi turun...");
        TMP_Text loadingText = bgTransform.GetChild(0).GetComponent<TMP_Text>();
        loadingText.text = textLoading;

        // Pastikan posisi di awal (atas) sebelum turun
        if (bgTransform != null)
        {
            bgTransform.anchoredPosition = new Vector2(bgTransform.anchoredPosition.x, startPosition_Y);
        }

        // SIMPAN coroutine-nya agar bisa di-stop nanti
        moveCoroutine = StartCoroutine(AnimateDownCoroutine());
    }

    private IEnumerator AnimateDownCoroutine()
    {

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
