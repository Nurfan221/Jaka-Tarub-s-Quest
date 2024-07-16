using UnityEngine;
using UnityEngine.UI;

public class InventoryAutoSizer : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public RectTransform contentRectTransform;

    private void Awake()
    {
        UpdateInventorySize();
    }

    private void UpdateInventorySize()
    {
        // Hitung lebar dan tinggi total dari semua slot di dalam inventory
        float totalWidth = gridLayout.cellSize.x * gridLayout.constraintCount;
        float totalHeight = gridLayout.cellSize.y * Mathf.CeilToInt((float)transform.childCount / gridLayout.constraintCount);

        // Tambahkan margin atau ruang tambahan jika diperlukan
        // Misalnya, Anda dapat menambahkan beberapa nilai tetap untuk margin
        totalWidth += 20f;
        totalHeight += 20f;

        // Terapkan ukuran yang dihitung ke komponen RectTransform dari content inventory
        contentRectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    // Panggil method UpdateInventorySize() ketika ada perubahan dalam inventory
    // Misalnya, saat slot ditambahkan atau dihapus
    public void OnInventoryChange()
    {
        UpdateInventorySize();
    }

    public void OnSlotMovedFromSlotCook()
    {
        // Panggil method UpdateInventorySize() ketika slot dipindahkan dari SlotCook ke inventory
        UpdateInventorySize();
    }
}
