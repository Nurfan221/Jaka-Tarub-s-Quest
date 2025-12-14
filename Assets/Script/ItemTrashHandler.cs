using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTrashHandler : MonoBehaviour, IDropHandler
{
    public ItemInteraction itemToPotentiallyTrash;

    private void Start()
    {
        // Daftarkan listener ke event popup sekali saja saat permainan dimulai
        if (QuantityPopupUI.Instance != null)
        {
            QuantityPopupUI.Instance.onConfirm.AddListener(HandleTrashConfirmation);
        }
    }

    private void OnDestroy()
    {
        // Selalu baik untuk melepaskan listener saat objek dihancurkan
        if (QuantityPopupUI.Instance != null)
        {
            QuantityPopupUI.Instance.onConfirm.RemoveListener(HandleTrashConfirmation);
        }
    }

    //Metode ini dipanggil oleh Unity Event System saat item dilepaskan di atas objek ini.

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDropSampah dipanggil pada Trash Area");
        // Dapatkan item yang sedang diseret dari manajer UI
        ItemInteraction draggedItem = MechanicController.Instance.HandleGetHeldItem();

        // Jika ada item yang valid sedang diseret
        if (draggedItem != null)
        {
            // Simpan item ini sementara, agar kita tahu apa yang harus dibuang saat popup dikonfirmasi
            itemToPotentiallyTrash = draggedItem;

            // Dapatkan data lengkap item untuk ditampilkan di popup
            ItemData itemData = PlayerController.Instance.HandleGetItem(draggedItem.index);
            if (itemData == null) return;

            Item itemSO = ItemPool.Instance.GetItemWithQuality(itemData.itemName, itemData.quality);
            if (itemSO == null) return;

            // Tampilkan popup dengan jumlah maksimum adalah jumlah item yang dimiliki
            QuantityPopupUI.Instance.Show(itemSO.sprite, 1, itemData.count);
        }
    }

    //Metode ini akan dipanggil secara otomatis saat pemain menekan "Confirm" di popup.

    private void HandleTrashConfirmation(int quantityToTrash)
    {
        // Pastikan kita tahu item mana yang akan dibuang
        if (itemToPotentiallyTrash != null)
        {
            Debug.Log($"Konfirmasi diterima! Membuang {quantityToTrash} buah dari item di index [{itemToPotentiallyTrash.index}]");

            // Panggil manajer data untuk menghapus item dengan jumlah tertentu
            //InventoryManager.Instance.RemoveItem(itemToPotentiallyTrash.index, quantityToTrash);
            MechanicController.Instance.HandleDropItemFromInventory(itemToPotentiallyTrash.index, quantityToTrash);

            // Beri tahu manajer UI untuk menyelesaikan proses drag dan me-refresh inventaris
            MechanicController.Instance.HandleSuccessfulDrop(-1); // Kirim -1 karena ini bukan swap

            // Reset item sementara setelah selesai
            itemToPotentiallyTrash = null;
        }
    }


}