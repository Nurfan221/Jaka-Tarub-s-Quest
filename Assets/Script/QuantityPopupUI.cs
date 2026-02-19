using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Event khusus yang akan mengirimkan jumlah (int) saat dikonfirmasi.
[System.Serializable]
public class UnityIntEvent : UnityEvent<int> { }

public class QuantityPopupUI : MonoBehaviour
{

    private static QuantityPopupUI _instance;
    [Header("Komponen UI")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemCountText;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button maxButton;
    [SerializeField] private Button minButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    public TMP_Text nameItem;

    [Header("Events untuk Komunikasi")]
    public UnityIntEvent onConfirm;
    public UnityEvent onCancel;

    private int currentAmount;
    private int maxAmount;





    public static QuantityPopupUI Instance
    {
        get
        {
            if (_instance == null)
            {
                // FindObjectOfType(true) akan mencari objek bahkan yang tidak aktif
                _instance = FindObjectOfType<QuantityPopupUI>(true);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }


        plusButton.onClick.AddListener(() => UpdateAmount(1));
        minusButton.onClick.AddListener(() => UpdateAmount(-1));
        maxButton.onClick.AddListener(SetToMax);
        minButton.onClick.AddListener(SetToMin); 
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);

        //gameObject.SetActive(false); // Pastikan popup tidak aktif saat awal
    }

    public void Show(Item itemUse, int initialAmount, int maxPossibleAmount)
    {
        gameObject.transform.SetAsLastSibling();
        Debug.Log("Showing QuantityPopupUI with sprite: " + itemUse.itemName + ", initialAmount: " + initialAmount + ", maxPossibleAmount: " + maxPossibleAmount);
        gameObject.SetActive(true);
        itemImage.sprite = itemUse.sprite;
        maxAmount = maxPossibleAmount;
        nameItem.text = itemUse.itemName;
        currentAmount = Mathf.Clamp(initialAmount, 1, maxAmount);
        UpdateText();
    }

    private void UpdateAmount(int change)
    {
        // Prediksi dulu: "Kalau ditambah, jadinya berapa?"
        int nextAmount = currentAmount + change;

        // Cek apakah prediksi tersebut MELANGGAR batas?

        //  Melebihi batas maksimal (Bahan kurang)
        if (nextAmount > maxAmount)
        {
            // Tampilkan pesan error SESUAI keinginan Anda
            PlayerUI.Instance.ShowErrorUI("Item di inventory anda tidak cukup untuk membuat lebih banyak!");

            // STOP di sini. Jangan update angka. Biarkan tetap di angka maksimal.
            return;
        }

        //  Kurang dari 1 (Minimal buat 1)
        if (nextAmount < 1)
        {
            // Tidak perlu error heboh, cukup return agar tidak jadi 0 atau negatif.
            return;
        }

        // Jika lolos pengecekan di atas, berarti AMAN. Update nilainya.
        currentAmount = nextAmount;

        //  Update Teks UI
        UpdateText();
    }

    private void SetToMax()
    {
        currentAmount = maxAmount;
        UpdateText();
    }

    private void SetToMin()
    {
        currentAmount = 1;
        UpdateText();
    }

    private void UpdateText()
    {
        itemCountText.text = currentAmount.ToString();
    }

    private void Confirm()
    {
        onConfirm.Invoke(currentAmount);
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        onCancel.Invoke();
        gameObject.SetActive(false);
    }
}