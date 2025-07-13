using UnityEngine;
using UnityEngine.EventSystems;

// Nama baru: ItemInteraction
// Mengimplementasikan semua interface yang dibutuhkan untuk memulai dan menerima drag.
public class ItemInteraction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // Index item ini di dalam List data, diatur oleh RefreshInventoryItems.
    public int index;

    // Komponen CanvasGroup untuk mengatur transparansi item asli di grid.
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Ambil komponen saat dibuat, tambahkan jika belum ada.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Dipanggil oleh sistem saat jari/mouse mulai menggeser objek ini.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Panggil manajer untuk memulai proses drag.
        // Kirim 'this' agar manajer tahu item mana yang menjadi pemicunya.
        MechanicController.Instance.HandleInitiateDrag(this);
    }

    /// <summary>
    /// Dipanggil oleh sistem selama jari/mouse bergerak saat menyeret.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // Perbarui posisi ikon drag melalui manajer.
       MechanicController.Instance.HandleUpdateDragPosition(eventData.position);
    }

    /// <summary>
    /// Dipanggil oleh sistem saat jari/mouse dilepaskan setelah menyeret.
    /// Metode ini akan berjalan jika drop GAGAL (tidak mengenai target valid).
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Beri tahu manajer untuk membatalkan proses drag.
        MechanicController.Instance.HandleCancelDrag();
    }

    /// <summary>
    /// Dipanggil oleh sistem jika ada objek lain yang dilepaskan DI ATAS objek ini.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // Beri tahu manajer bahwa drop berhasil di atas item ini.
        // Kirim index dari item ini sebagai target.
       MechanicController.Instance.HandleSuccessfulDrop(this.index);
    }

    // --- Metode Helper untuk Manajer UI ---

    /// <summary>
    /// Mengatur transparansi item ini. Dipanggil oleh Manajer UI.
    /// </summary>
    public void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }
}