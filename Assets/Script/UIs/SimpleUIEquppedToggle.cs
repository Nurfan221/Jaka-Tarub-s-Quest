using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Wajib ada untuk memanipulasi UI

public class SimpleUIEquppedToggle : MonoBehaviour
{
    public static SimpleUIEquppedToggle Instance { get; private set; }
    [Header("Pengaturan")]
    public RectTransform uiPanel; // Masukkan Panel UI yang mau digerakkan di sini
    public CanvasGroup uiCanvasGroup; // Drag komponen CanvasGroup ke sini
    public float durasiAnimasi = 0.5f;
    public Button closeButton; // Tombol untuk menutup UI
    public Button equippedButton; // Tombol untuk menutup UI dan memakai item
    public ItemData itemTemplate; // Item yang sedang dipakai

    [Header("Posisi")]
    public float posisiBawah = -500f; // Posisi sembunyi
    public float posisiAtas = 0f;     // Posisi muncul

    private bool sedangMuncul = false;   // Untuk mengecek status (sedang di atas atau di bawah?)
    private bool sedangGerak = false;    // Kunci pengaman biar tidak bisa di-spam

    [Header("Objek milik UIPanel")]
    public Image itemIcon;


    void Awake()
    {
        // Jika ada duplikat script ini di scene, hancurkan yang baru.
        // Biarkan yang lama tetap hidup sebagai "Raja" (Instance).
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

    }
    void Start()
    {
        // Saat game mulai, kita pastikan UI ada di posisi bawah dulu
        if (uiPanel != null)
        {
            Vector2 posAwal = uiPanel.anchoredPosition;
            posAwal.y = posisiBawah;
            uiPanel.anchoredPosition = posAwal;

            sedangMuncul = false;
            uiPanel.gameObject.SetActive(false); // Sembunyikan UI
        }
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            // Panggil fungsi TekanTombol tanpa itemData (null)
            TekanTombol(null);
        });
        equippedButton.onClick.RemoveAllListeners();
        equippedButton.onClick.AddListener(() =>
        {
            Debug.Log("Tombol Equipped ditekan!");
            // Panggil fungsi TekanTombol tanpa itemData (null)
            PlayerController.Instance.HandleEquipItem(itemTemplate);
            TekanTombol(null);
            MechanicController.Instance.HandleUpdateInventory();


        });
    }

    // Fungsi ini yang dipanggil oleh Tombol (Button)
    public void TekanTombol(ItemData itemData)
    {
        itemTemplate = itemData;
        // Cek Apakah sedang bergerak? Kalau iya, hentikan fungsi (abaikan klik).
        if (sedangGerak == true) return;

       
        // Cek logika Toggle (Saklar)
        if (sedangMuncul == true)
        {
            // Kalau sedang muncul, suruh TURUN ke posisiBawah
            StartCoroutine(GerakkanUI(posisiBawah));
        }
        else
        {
            UpdateItemLogic(itemData);

            // Kalau sedang sembunyi, suruh NAIK ke posisiAtas
            StartCoroutine(GerakkanUI(posisiAtas));
        }

        // Balik statusnya (Benar jadi Salah, Salah jadi Benar)
        sedangMuncul = !sedangMuncul;
    }

    public void UpdateItemLogic(ItemData itemData)
    {
        Item itemUse = ItemPool.Instance.GetItem(itemData.itemName);
        if (itemUse != null && itemIcon != null)
        {
            itemIcon.sprite = itemUse.sprite;
        }
    }

    IEnumerator GerakkanUI(float targetY)
    {
        // SETUP SEBELUM GERAK
        uiPanel.gameObject.SetActive(true); // Nyalakan objek
        sedangGerak = true;

        float waktuBerjalan = 0;
        Vector2 posisiAwal = uiPanel.anchoredPosition;
        Vector2 posisiTujuan = new Vector2(posisiAwal.x, targetY);

        // Tentukan Alpha Awal & Tujuan
        float startAlpha = uiCanvasGroup.alpha;

        // Jika targetnya ke ATAS (0), berarti mau Muncul (Alpha 1)
        // Jika targetnya ke BAWAH (-500), berarti mau Hilang (Alpha 0)
        float targetAlpha = (targetY == posisiAtas) ? 1f : 0f;

        //  LOOP GERAKAN & FADING
        while (waktuBerjalan < durasiAnimasi)
        {
            waktuBerjalan += Time.deltaTime;
            float t = waktuBerjalan / durasiAnimasi;
            t = t * t * (3f - 2f * t); // Smooth Step

            // Gerakkan Posisi
            uiPanel.anchoredPosition = Vector2.Lerp(posisiAwal, posisiTujuan, t);

            // Ubah Opacity (Alpha) secara bersamaan
            uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        //  FINALISASI (Pasca Gerak)
        uiPanel.anchoredPosition = posisiTujuan;
        uiCanvasGroup.alpha = targetAlpha;
        sedangGerak = false;

        // Cek Logika Akhir
        if (Mathf.Approximately(targetY, posisiBawah))
        {
            // Kalo sudah sampai bawah:
            uiCanvasGroup.blocksRaycasts = false; // Matikan interaksi (biar tombol ga kepencet hantu)
            uiPanel.gameObject.SetActive(false);  // Matikan total biar hemat performa
        }
        else
        {
            // Kalo sudah sampai atas:
            uiCanvasGroup.blocksRaycasts = true;  // Izinkan interaksi
        }
    }
}
