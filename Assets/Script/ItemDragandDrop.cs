using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragandDrop : MonoBehaviour, IDragHandler, IEndDragHandler, IDropHandler
{
    public int itemID; // ID item yang disimpan

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent; // Simpan parent asli
    }

    public void OnDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f; // Memberikan efek transparansi saat di-drag
        rectTransform.anchoredPosition += eventData.delta; // Gerakkan item saat di-drag
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // Kembalikan transparansi saat selesai drag
        transform.SetParent(originalParent); // Kembalikan ke parent asli
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemDragandDrop droppedItem = eventData.pointerDrag.GetComponent<ItemDragandDrop>();
        if (droppedItem != null)
        {
            // Tukar posisi di dalam inventory
            int tempID = itemID; // Simpan itemID sementara
            Player_Inventory.Instance.SwapItems(droppedItem.itemID, itemID);
            
            // Update itemID untuk refleksi perubahan
            itemID = droppedItem.itemID; // Update itemID untuk item ini
            droppedItem.itemID = tempID; // Update itemID untuk item yang di-drag
            
            // Update UI untuk refleksi perubahan
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.UpdateInventoryUI();
            }
        }
    }
}
