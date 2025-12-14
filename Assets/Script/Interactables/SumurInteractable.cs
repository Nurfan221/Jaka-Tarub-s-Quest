using UnityEngine;

public class SumurInteractable : Interactable
{

    public Item itemInteractable;
    private PlayerController stats;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void Interact()
    {
        Debug.Log("Mengisi ulang alat...");


        // Cek apakah item yang dipegang pemain adalah item yang benar (misal: Penyiram Tanaman)
        if (stats.equipped1 == true)
        {
            // Jika ya, fokus pada item di slot pertama [0]
            ItemData itemDiSlotPertama = stats.equippedItemData[0];

            // Cek apakah namanya cocok
            if (itemDiSlotPertama.itemName == itemInteractable.itemName)
            {
                // Lakukan aksi untuk slot pertama...
                Debug.Log("Item di slot pertama cocok! Mengisi ulang...");
                Item item = ItemPool.Instance.GetItemWithQuality(itemDiSlotPertama.itemName, itemDiSlotPertama.quality);
                stats.equippedItemData[0].itemHealth = item.maxhealth;
                PlayerUI.Instance.UpdateEquippedWeaponUI();
            }
        }
        else
        {
            // Fokus pada item di slot kedua [1]
            ItemData itemDiSlotKedua = stats.equippedItemData[1];

            // Cek apakah namanya cocok
            if (itemDiSlotKedua.itemName == itemInteractable.itemName)
            {
                Debug.Log("Item di slot pertama cocok! Mengisi ulang...");
                Item item = ItemPool.Instance.GetItemWithQuality(itemDiSlotKedua.itemName, itemDiSlotKedua.quality);
                stats.equippedItemData[1].itemHealth = item.maxhealth;
                PlayerUI.Instance.UpdateEquippedWeaponUI();
            }
        }
    }
}
