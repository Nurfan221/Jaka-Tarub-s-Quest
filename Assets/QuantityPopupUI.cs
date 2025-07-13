using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

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
    [SerializeField] private Button minButton; // --- TOMBOL BARU ---
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

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
        minButton.onClick.AddListener(SetToMin); // --- LISTENER BARU ---
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);

        //gameObject.SetActive(false); // Pastikan popup tidak aktif saat awal
    }

    public void Show(Sprite sprite, int initialAmount, int maxPossibleAmount)
    {
        Debug.Log("Showing QuantityPopupUI with sprite: " + sprite.name + ", initialAmount: " + initialAmount + ", maxPossibleAmount: " + maxPossibleAmount);
        gameObject.SetActive(true);
        itemImage.sprite = sprite;
        maxAmount = maxPossibleAmount;
        currentAmount = Mathf.Clamp(initialAmount, 1, maxAmount);
        UpdateText();
    }

    private void UpdateAmount(int change)
    {
        currentAmount = Mathf.Clamp(currentAmount + change, 1, maxAmount);
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

    private void Cancel()
    {
        onCancel.Invoke();
        gameObject.SetActive(false);
    }
}