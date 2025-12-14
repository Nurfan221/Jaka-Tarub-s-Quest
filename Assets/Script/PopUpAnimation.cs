using UnityEngine;

public class PopUpAnimation : MonoBehaviour
{
    [Header("Pengaturan Animasi")]
    public float startYOffset = -2f; // Jarak muncul dari bawah
    public float animationDuration = 0.5f;
    public AnimationCurve popUpCurve;

    private Vector3 originalLocalPosition;
    private bool isAnimating = false;
    private float timer = 0f;

    void Start()
    {
        // Simpan posisi lokal asli (posisi normal di (0,0) relatif parent)
        originalLocalPosition = transform.localPosition;

        // Langsung sembunyikan ke bawah tanah saat game mulai
        ResetPosition();

        // Matikan script update ini agar hemat CPU sampai kamera melihatnya
        enabled = false;
    }


    // Dipanggil otomatis saat kamera melihat Sprite ini
    void OnBecameVisible()
    {
        // Nyalakan script ini agar fungsi Update() jalan
        enabled = true;

        // Mulai animasi
        TriggerPopUp();
    }

    // Dipanggil otomatis saat kamera tidak melihat Sprite ini
    void OnBecameInvisible()
    {
        // Matikan script ini (Update berhenti) -> HEMAT CPU
        enabled = false;

        // Reset posisi ke bawah lagi untuk persiapan nanti
        ResetPosition();
    }


    void ResetPosition()
    {
        // Pindahkan posisi lokal ke bawah (offset)
        transform.localPosition = new Vector3(originalLocalPosition.x, originalLocalPosition.y + startYOffset, originalLocalPosition.z);
        isAnimating = false;
        timer = 0f;
    }

    public void TriggerPopUp()
    {
        // Mulai animasi hanya jika belum jalan
        if (!isAnimating)
        {
            timer = 0f;
            isAnimating = true;
        }
    }

    void Update()
    {
        // Logika animasi (hanya jalan saat enabled = true)
        if (isAnimating)
        {
            timer += Time.deltaTime / animationDuration;

            // Hitung posisi baru
            float newY = Mathf.Lerp(originalLocalPosition.y + startYOffset, originalLocalPosition.y, popUpCurve.Evaluate(timer));

            // Terapkan ke localPosition (Child bergerak, Parent diam)
            transform.localPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z);

            // Jika animasi selesai
            if (timer >= 1f)
            {
                isAnimating = false;
                transform.localPosition = originalLocalPosition;

                // Opsional: Matikan enabled = false disini jika ingin super hemat,
                // tapi biarkan true jika Anda butuh animasi idle (goyang-goyang angin).
            }
        }
    }
}