using UnityEngine;


public class UIBobbingAnimation : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Seret (drag) objek UI yang ingin dianimasikan ke sini.")]
    public Transform uiElement;

    [Header("Pengaturan Gerakan")]
    [Tooltip("Seberapa jauh (dalam piksel) UI akan bergerak naik dan turun dari posisi awalnya.")]
    public float amplitude = 10f; // Seberapa tinggi gerakannya

    [Tooltip("Seberapa cepat UI akan bergerak naik dan turun.")]
    public float speed = 2f; // Seberapa cepat gerakannya

    // Variabel privat untuk menyimpan posisi awal UI
    private Vector3 startPosition;

    void Start()
    {
        // Pengecekan keamanan jika uiElement belum diatur
        if (uiElement == null)
        {
            // Jika tidak diatur, gunakan transform dari objek ini sendiri
            uiElement = this.transform;
        }

        // Simpan posisi awal dari UI element saat game dimulai.
        startPosition = uiElement.position;
    }

    // Update dipanggil setiap frame, cocok untuk animasi yang halus
    void Update()
    {

        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;

        //    Posisi X dan Z tetap sama seperti posisi awal.
        Vector3 newPosition = new Vector3(
            startPosition.x,
            startPosition.y + yOffset,
            startPosition.z
        );

        // Terapkan posisi baru ke UI element.
        uiElement.position = newPosition;
    }
}
