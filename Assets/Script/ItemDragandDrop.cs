using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ItemDragandDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int index; // ID item yang disimpan

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    public ItemData itemData; // Data item yang di-drag
    public Item item; // Item yang di-drag
    private bool droppedOnValidSlot;

    //private void Start()
    //{
    //    GetItemFromInventory();
    //}
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent; // Simpan parent asli
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // PINDAHKAN KE SINI!
        // Dengan ini, data item selalu yang paling baru setiap kali drag dimulai.
        GetItemFromInventory();

        // Jika slotnya ternyata kosong, itemData akan null, dan kita hentikan proses drag.
        if (itemData == null)
        {
            eventData.pointerDrag = null; // Batalkan drag jika tidak ada item
            return;
        }

        droppedOnValidSlot = false;
        canvasGroup.alpha = 0.6f;
        transform.SetParent(transform.root);
    }
    public void OnDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f; // Memberikan efek transparansi saat di-drag
        rectTransform.anchoredPosition += eventData.delta; // Gerakkan item saat di-drag

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        transform.SetParent(originalParent); // Selalu kembalikan ke parent asli untuk mereset posisi jika tidak di-drop ke mana pun
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Reset posisi di dalam slotnya

        // **Logika Inti: Cek apakah item dibuang atau tidak**
        if (!droppedOnValidSlot)
        {
            // Jika penanda false, berarti item dibuang ke luar inventory
            Debug.Log("Item dibuang ke dunia game!");

            ItemData itemToDropData = PlayerController.Instance.HandleGetItem(index);
            if (itemToDropData == null) return; // Jika karena suatu hal item sudah tidak ada

            Item itemInDrag = ItemPool.Instance.GetItemWithQuality(itemToDropData.itemName, itemToDropData.quality);
            if (itemInDrag != null && itemInDrag.dropItem != null)
            {
                // Drop satu tumpukan item sebagai satu objek fisik

                ItemPool.Instance.DropItem(itemInDrag.itemName, transform.position + new Vector3(0, 0.5f, 0), itemInDrag.dropItem, itemToDropData.count);
                ItemPool.Instance.RemoveItemsFromInventory(itemData);

            }
            else
            {

                Debug.LogError("Item tidak ditemukan atau prefab untuk drop tidak ada!");
            }

            // 3. Hapus item dari data inventaris (PENTING!)
            // Anda perlu sebuah fungsi untuk menghapus item berdasarkan indexnya

            // 4. Perbarui UI inventaris
            MechanicController.Instance.HandleUpdateInventory();
        }
        // Jika droppedOnValidSlot adalah true, tidak terjadi apa-apa di sini karena logika swap sudah ditangani di OnDrop.
    }
    public void MarkAsDroppedSuccessfully()
    {
        droppedOnValidSlot = true;
    }
    public void OnDrop(PointerEventData eventData)
    {
        ItemDragandDrop draggedItem = eventData.pointerDrag.GetComponent<ItemDragandDrop>();
        if (draggedItem != null && draggedItem != this)
        {
            // Panggil metode baru untuk menandai drop berhasil
            draggedItem.MarkAsDroppedSuccessfully();

            // Lanjutkan logika tukar item
            MechanicController.Instance.HandleSwapItems(draggedItem.index, this.index);
            MechanicController.Instance.HandleUpdateInventory();
        }
    }

    public void GetItemFromInventory()
    {
        Debug.Log($"[DEBUG] Mengambil item dari inventaris dengan index: {index}");
        itemData = PlayerController.Instance.HandleGetItem(index);
        if (itemData != null)
        {
            // Hanya cari 'Item' jika 'itemData' valid
            item = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
        }
        else
        {
            // Pastikan 'item' juga null jika tidak ada data
            item = null;
        }
    }

}
