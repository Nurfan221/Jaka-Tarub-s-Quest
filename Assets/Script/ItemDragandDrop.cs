using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Gunakan ini jika Anda memakai TextMeshPro

public class ItemDragandDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // --- Variabel Publik ---
    // Index item ini di dalam List data inventaris. Diatur oleh skrip UI Manager.
    public int index;

    [Header("Komponen UI (Hubungkan di Inspector)")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI countText;

    // --- Variabel Statis ---
    // Referensi statis ke item yang SEDANG diseret.
    // Ini memudahkan skrip lain (seperti TrashZone) untuk mengetahui item mana yang aktif.
    public static ItemDragandDrop itemBeingDragged;

    // --- Variabel Privat ---
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Ambil komponen CanvasGroup untuk mengatur transparansi dan interaksi.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

  

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemIcon.sprite == null) return;

        // Atur item ini sebagai item yang sedang diseret
        itemBeingDragged = this;

        // Tampilkan dan atur DragIcon
        MechanicController.Instance.InventoryUI.dragIcon.sprite = this.itemIcon.sprite;
        MechanicController.Instance.InventoryUI.dragIcon.gameObject.SetActive(true);

        // HANYA sembunyikan item asli secara visual. JANGAN sentuh blocksRaycasts.
        // Item ini harus tetap bisa dideteksi oleh OnDrop.
        canvasGroup.alpha = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Gerakkan DragIcon
        MechanicController.Instance.InventoryUI.dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // OnEndDrag hanya berjalan jika drop GAGAL (tidak mengenai target valid).
        // Jika itemBeingDragged belum di-reset oleh OnDrop, berarti drop gagal.
        if (itemBeingDragged != null)
        {
            // Kembalikan item asli menjadi terlihat
            canvasGroup.alpha = 1;

            // Sembunyikan DragIcon
            MechanicController.Instance.InventoryUI.dragIcon.gameObject.SetActive(false);

            // Reset referensi
            itemBeingDragged = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Metode ini sekarang akan terpanggil dengan benar!
        ItemDragandDrop draggedItem = ItemDragandDrop.itemBeingDragged;
        Debug.Log($"OnDrop dipanggil pada item index {this.index}. Item yang diseret adalah: {(draggedItem != null ? draggedItem.index.ToString() : "null")}");

        if (draggedItem != null && draggedItem != this)
        {
            Debug.Log($"OnDrop terpicu! Menukar item {draggedItem.index} dengan {this.index}");

            // Panggil manajer untuk menukar DATA
            MechanicController.Instance.HandleSwapItems(draggedItem.index, this.index);

            // Sembunyikan DragIcon karena operasi selesai
            MechanicController.Instance.InventoryUI.dragIcon.gameObject.SetActive(false);

            // Panggil refresh untuk menggambar ulang UI
            MechanicController.Instance.HandleUpdateInventory();

            // Reset referensi statis
            ItemDragandDrop.itemBeingDragged = null;
        }
    }
}