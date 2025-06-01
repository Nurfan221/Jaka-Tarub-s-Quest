using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Tambahkan untuk mengakses komponen Image

public class ButtonSlider : MonoBehaviour
{
    public Sprite[] imagesSlider;  // Gambar-gambar untuk animasi slider
    public bool sliderValue = true;  // Nilai slider, bisa dipakai untuk kontrol lainnya
    public float frameRate = 0.01f; // Waktu per frame (kecepatan animasi)
    private Image imageComponent;  // Komponen Image untuk mengganti gambar
    private int currentFrame = 0; // Indeks frame saat ini
    public Button buttonSlider; // Button untuk memulai animasi
    private Coroutine currentCoroutine;  // Menyimpan coroutine yang sedang berjalan

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Mengambil komponen Image dari objek buttonSlider (pastikan buttonSlider memiliki komponen Image)
        imageComponent = buttonSlider.GetComponent<Image>();

        // Menambahkan listener pada tombol hanya sekali
        buttonSlider.onClick.AddListener(OnButtonClick);
    }

    // Fungsi yang akan dipanggil ketika tombol ditekan
    private void OnButtonClick()
    {
        // Jika ada coroutine yang sedang berjalan, hentikan dulu
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Mulai animasi sesuai dengan nilai sliderValue
        if (sliderValue)
        {
            currentCoroutine = StartCoroutine(AnimationTrue());
        }
        else
        {
            currentCoroutine = StartCoroutine(AnimationFalse());
        }
    }

    private IEnumerator AnimationTrue()
    {
        // Loop untuk melalui setiap frame pada array imagesSlider
        for (int i = 0; i < imagesSlider.Length; i++)
        {
            imageComponent.sprite = imagesSlider[i]; // Mengubah sprite pada Image
            yield return new WaitForSeconds(frameRate); // Menunggu sesuai dengan frameRate
        }

        // Opsi: reset frame jika diperlukan
        currentFrame = 0; // Reset ke frame pertama jika diperlukan
        sliderValue = false; // Mengatur nilai sliderValue setelah animasi selesai
    }

    public IEnumerator AnimationFalse()
    {
        // Loop melalui setiap frame di array imagesSlider dari belakang
        for (int i = imagesSlider.Length - 1; i >= 0; i--)
        {
            imageComponent.sprite = imagesSlider[i]; // Mengubah sprite pada Image
            yield return new WaitForSeconds(frameRate); // Menunggu sesuai dengan frameRate
        }

        // Opsi: reset frame jika diperlukan
        currentFrame = 0; // Reset ke frame pertama jika diperlukan
        sliderValue = true; // Mengatur nilai sliderValue setelah animasi selesai
    }
}
