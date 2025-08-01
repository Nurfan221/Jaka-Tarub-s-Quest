using UnityEngine;
using UnityEngine.UI;

public class UIAnimationManager : MonoBehaviour
{
     public RectTransform bgTransform; // Gunakan RectTransform untuk UI

    [Header("Pengaturan Animasi")]
    public float animationSpeed = 5.0f; // Kecepatan animasi
    public float endPosition_Y = 0f; // Posisi Y akhir (biasanya di tengah)
    public float startPosition_Y = 500f; // Posisi Y awal (di luar layar atas)

    public bool isAnimating = false;
    [SerializeField] private Animator characterAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cek apakah bgTransform sudah diatur
        if (bgTransform != null)
        {
            // Atur posisi awal bgTransform di luar layar atas
            bgTransform.anchoredPosition = new Vector2(bgTransform.anchoredPosition.x, startPosition_Y);

            // Mulai animasi
            isAnimating = true;
        }
        else
        {
            Debug.LogError("bgTransform belum diatur di Inspector!");
        }

        StartCharacterAnimation();

    }

    public void StartCharacterAnimation()
    {
        if (characterAnimator != null)
        {
            characterAnimator.Play("JalanKanan");
            Debug.Log("Animasi 'JalanKanan' diputar!");
        }
        else
        {
            Debug.LogError("Referensi Animator di UIAnimationManager tidak ditemukan!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Jika animasi tidak berjalan, hentikan Update
        if (!isAnimating)
        {
            return;
        }

        // Tentukan posisi target akhir
        Vector2 targetPosition = new Vector2(bgTransform.anchoredPosition.x, endPosition_Y);

        // Gerakkan bgTransform secara bertahap menuju posisi target
        bgTransform.anchoredPosition = Vector2.Lerp(
            bgTransform.anchoredPosition, // Posisi saat ini
            targetPosition,               // Posisi yang ingin dituju
            Time.deltaTime * animationSpeed  // Kecepatan gerakan
        );

        // Cek apakah animasi sudah mendekati posisi akhir
        if (Vector2.Distance(bgTransform.anchoredPosition, targetPosition) < 0.1f)
        {
            // Posisikan secara tepat di posisi akhir
            bgTransform.anchoredPosition = targetPosition;

            // Hentikan animasi
            isAnimating = false;

            Debug.Log("Animasi selesai!");
        }
    }


}
