using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems; 

public class ItemTrashHandler : MonoBehaviour, IDropHandler // Implementasikan interface IDropHandler
{
    public GameObject itemPrefab; // Prefab item yang akan di-drop
                                  // HANYA jika sebuah objek di-drop di atas GameObject yang memiliki script ini.
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        ItemDragandDrop draggedItemScript = draggedObject.GetComponent<ItemDragandDrop>();

        if (draggedItemScript != null)
        {
            // Ambil data terbaru langsung dari PlayerController menggunakan index.
            ItemData currentItemData = PlayerController.Instance.HandleGetItem(draggedItemScript.index);
            if (currentItemData == null) return; // Item mungkin sudah tidak ada, hentikan.

            // Dapatkan definisi Item (termasuk prefab dropItem) dari ItemPool.
            Item itemDefinition = ItemPool.Instance.GetItemWithQuality(currentItemData.itemName, currentItemData.quality);
            if (itemDefinition == null || itemDefinition.dropItem == null) return; // Tidak ada prefab untuk didrop.

            draggedItemScript.MarkAsDroppedSuccessfully();

            GameObject itemPrefab = itemDefinition.dropItem; // Sekarang ini dijamin punya nilai
            ItemPool.Instance.DropItem(itemDefinition.itemName, transform.position + new Vector3(0, 0.5f, 0), itemPrefab);
            // Lanjutkan sisa logika Anda...
            // ...
        }
    }


}