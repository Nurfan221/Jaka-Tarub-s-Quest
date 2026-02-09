using System.Collections.Generic;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{






    public bool inventoryOpened;
    public bool inventoryClosed;
    //[SerializeField] MiniGameHewanUI miniGameHewanUI;

    [SerializeField] ParticleSystem healParticle;


    private PlayerController stats;
    private PlayerData_SO playerData;

    private void Awake()
    {

        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance;
            playerData = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }

        //Instance = this;
        //pA = GetComponent<Player_Action>();

        //// Making sure everything in it is a clone
        //List<Item> newList = new List<Item>();
        //foreach (Item item in stats.itemList)
        //{
        //    newList.Add(Instantiate(item));
        //}
        //stats.itemList.Clear();
        //stats.itemList = newList;

        //stats.emptyItem = Instantiate(stats.emptyItem);
    }

    private void Start()
    {

        //inventoryButton.onClick.AddListener(ToggleInventory); // Add listener to button
        //closeInventoryButton.onClick.AddListener(CloseInventory); // Add listener to close button

        // Pastikan switchWeaponImage tidak null

        //stats.equippedItemData[0].itemName = stats.emptyItemTemplate.itemName;
        //stats.equippedItemData[1].itemName = stats.emptyItemTemplate.itemName;
        //stats.itemUseData[0].itemName = stats.emptyItemTemplate.itemName;
        //stats.itemUseData[1].itemName = stats.emptyItemTemplate.itemName;
    }

    private void Update()
    {






    }











    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang disentuh memiliki tag "ItemDrop"
        if (other.CompareTag("ItemDrop"))
        {
            Debug.Log("terdeteksi item drop");
            // Ambil data dari PrefabItemBehavior jika ada
            ItemDropInteractable itemDropInteractable = other.GetComponent<ItemDropInteractable>();
            ItemData itemData = itemDropInteractable.itemdata;

            // Di dalam CookUI / Result Button Listener
            bool isSuccess = ItemPool.Instance.AddItem(itemData);

            if (isSuccess)
            {
                // Hapus item dari tungku
                MechanicController.Instance.HandleUpdateInventory();
                Destroy(other.gameObject);
            }
            else
            {
                // Jangan hapus, biarkan di tungku
                Debug.Log("Tas penuh, item tetap di tungku.");
            }

        }
        else if (other.CompareTag("Animal"))
        {
            Debug.Log("animal terdeteksi");
            AnimalBehavior animalBehavior = other.GetComponent<AnimalBehavior>();
            if (animalBehavior != null && animalBehavior.isAnimalEvent)
            {
                //miniGameHewanUI.Open(other.gameObject);

            }

            //AnimalBehavior animalBehavior = other.GetComponent<AnimalBehavior>();
            //animalBehavior.DropItem();
        }

    }







    public void EquipItem(ItemData itemToEquip)
    {
        // Bagian ini sudah benar
        if (!stats.inventory.Contains(itemToEquip))
        {
            Debug.LogWarning($"Mencoba memasang '{itemToEquip.itemName}' yang tidak ada di inventaris.");
            return;
        }

        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToEquip.itemName, itemToEquip.quality);
        if (itemTemplate == null) return;


        // JIKA ITEM ADALAH SENJATA
        if (itemTemplate.types == ItemType.Heal || itemTemplate.types == ItemType.Buff)
        {
            Debug.Log($"Item '{itemToEquip.itemName}' adalah Item Guna. Memasang ke slot quick-use.");

            List<ItemData> quickSlotList = stats.itemUseData; // Gunakan nama yang jelas

            // Logika untuk Quick Slot mungkin berbeda, misalnya hanya ada 2 slot
            if (quickSlotList[0].itemName == "Empty")
            {
                //Debug.Log($"Memasang '{itemToEquip.itemName}' ke slot 0.");
                quickSlotList[0] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
            else if (quickSlotList[1].itemName == "Empty")
            {
                //Debug.Log($"Memasang '{itemToEquip.itemName}' ke slot 1.");
                quickSlotList[1] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
            else
            {
                //Debug.Log($"Memasang gagal Quick slots penuh. Mengganti item di slot 0 dengan '{itemToEquip.itemName}'.");
                // Kembalikan item lama di slot 0 ke inventaris
                // Di dalam CookUI / Result Button Listener
                bool isSuccess = ItemPool.Instance.AddItem(quickSlotList[0]);
                // Pasang item baru di slot 0

                if (isSuccess)
                {
                    // Hapus item dari tungku
                    quickSlotList[0] = itemToEquip;
                    stats.inventory.Remove(itemToEquip);
                }
                else
                {
                    // Jangan hapus, biarkan di tungku
                    Debug.Log("Tas penuh, item tetap di tungku.");
                    // Opsional: Munculkan teks "Tas Penuh!"
                }

            }
        }

        // JIKA ITEM ADALAH ITEM GUNA (HEAL/BUFF)
        else if (itemTemplate.IsInType(ItemType.Melee_Combat) || itemTemplate.IsInType(ItemType.Ranged_Combat))
        {
            Debug.Log($"Item '{itemToEquip.itemName}' adalah Senjata. Memasang ke slot combat.");

            List<ItemData> equipmentList = stats.equippedItemData; // Gunakan nama yang jelas

            if (equipmentList[0].itemName == "Empty")
            {

                equipmentList[0] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
            else if (equipmentList[1].itemName == "Empty")
            {
                equipmentList[1] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
            else
            {
                // Di dalam CookUI / Result Button Listener
                bool isSuccess = ItemPool.Instance.AddItem(equipmentList[0]);

                if (isSuccess)
                {
                    // Pasang item baru di slot 0
                    equipmentList[0] = itemToEquip;
                    stats.inventory.Remove(itemToEquip);
                }
                else
                {
                    // Jangan hapus, biarkan di tungku
                    Debug.Log("Tas penuh, item tetap di tungku.");
                }

            }
        }
        else
        {
            Debug.Log($"Item '{itemToEquip.itemName}' tidak bisa dipasang.");
            return;
        }
        if (itemToEquip.itemName == "Cangkul")
        {
            TutorialManager.Instance.TriggerTutorial("tutorial_cangkul");
        }
        Debug.Log($"Item '{itemToEquip.itemName}' berhasil dipasang.");
        MechanicController.Instance.HandleUpdateInventory();
        PlayerUI.Instance.UpdateItemUseUI();
        PlayerUI.Instance.UpdateEquippedWeaponUI();


    }





    // Use which quick slot (1,2)
    public void UseQuickSlot()
    {
        int which;
        if (stats.itemUse1)
        {
            which = 0;
        }
        else
        {
            which = 1;
        }
        // Making sure there is an item in the quick slot
        ItemData item = stats.itemUseData[which];
        if (item == null || item.itemName == "Empty")
        {
            print("No item bish");
            return;
        }

        // Using them from inventory
        print("using quick slot " + (which));
        stats.itemUseData[which].count--;
        MechanicController.Instance.HandleUpdateInventory();

        Item itemUseTemplate = ItemPool.Instance.GetItemWithQuality(item.itemName, item.quality);

        // Have its effect
        BuffScrollController.Instance.GetBuff(itemUseTemplate);

        if (stats.itemUseData[which].count == 0)
        {
            stats.itemUseData[which] = playerData.emptyItemTemplate;
        }

        MechanicController.Instance.InventoryUI.RefreshInventoryItems();
        PlayerUI.Instance.UpdateItemUseUI();
    }


}







