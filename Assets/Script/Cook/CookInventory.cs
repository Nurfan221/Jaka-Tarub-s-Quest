using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CookInventory : MonoBehaviour, IDropHandler
{
     [SerializeField] DropCookSlot dropCookSlot;
    public void OnDrop(PointerEventData eventData)
    {
        // Mendapatkan DragCook dari event data
        DragCook dragCook = eventData.pointerDrag.GetComponent<DragCook>();

        // Jika yang di-drop adalah DragCook
        if (dragCook != null)
        {
            // Mendapatkan RectTransform item
            RectTransform itemRectTransform = dragCook.GetComponent<RectTransform>();

            // Mencari tahu parent dari item yang di-drop
            DropCookSlot previousSlot = itemRectTransform.GetComponentInParent<DropCookSlot>();
            
            // Jika sebelumnya berada di SlotCook1 atau SlotCook2
            if (previousSlot != null && (previousSlot.slotName == "SlotCook1" || previousSlot.slotName == "SlotCook2"))
            {
                // Mengembalikan item ke inventory
                // Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(dragCook.itemName));
                previousSlot.item = null;
                itemRectTransform.SetParent(transform);
                itemRectTransform.anchoredPosition = Vector3.zero;

                // // Jika item yang kembali ke inventory adalah DagingPedas, maka kita harus membatalkan pembuatannya
                // if (dragCook.itemName == "DagingPedas")
                // {
                //     // Hapus DagingPedas dari hasilCook
                //     Destroy(previousSlot.hasilCook.transform.GetChild(0).gameObject);
                //     // Bersihkan daftar onClick dari button hasilCook
                //     previousSlot.hasilCook.GetComponent<Button>().onClick.RemoveAllListeners();
                // }

                // cekk 
                dropCookSlot.CancelCook();
                dropCookSlot.CreateCook();

                

                return;
            }

            // Jika target drop adalah DropCookSlot (SlotCook1 atau SlotCook2)
            DropCookSlot dropSlot = eventData.pointerEnter.GetComponent<DropCookSlot>();
            if (dropSlot != null && (dropSlot.slotName == "SlotCook1" || dropSlot.slotName == "SlotCook2"))
            {
                // Menghapus item dari inventory
                Player_Inventory.Instance.RemoveItem(ItemPool.Instance.GetItem(dragCook.itemName));
                // Mengatur item ke inventory
                itemRectTransform.SetParent(dropSlot.transform);
                itemRectTransform.anchoredPosition = Vector3.zero;
            }
        }
    }
}