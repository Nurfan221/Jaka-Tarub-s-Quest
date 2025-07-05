using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Player_Inventory : MonoBehaviour
{
//public static Player_Inventory Instance { get; private set; } // Ubah sedikit menjadi property agar lebih aman

    Player_Action pA;

  

    [SerializeField] GameObject normalAttackHitArea; //Referensi image hitbox
    [SerializeField] private Button switchWeaponImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    [SerializeField] private Button switchUseItemImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    [SerializeField] PintuManager pintuManager;
    [SerializeField] private BuffScrollController buffScrollController;
    [SerializeField] private Player_Action playerAction;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private SpesialSkillWeapon spesialSkillWeapon;


    public bool meleeOrRanged = true;
    public bool itemUse1 = true;

 

     public bool inventoryOpened;
     public bool inventoryClosed;
    [SerializeField] MiniGameHewanUI miniGameHewanUI;

    [SerializeField] ParticleSystem healParticle;


    private PlayerData_SO stats;

    private void Awake()
    {

        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
        //Instance = this;
        pA = GetComponent<Player_Action>();

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
        if (switchWeaponImage != null)
        {
            // Tambahkan event listener untuk klik pada Image
            Button button = switchWeaponImage.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ToggleWeapon);
            }
            else
            {
                Debug.LogError("Image component does not have a Button component attached.");
            }
        }
        else
        {
            Debug.LogError("SwitchWeaponImage is not assigned.");
        }


        if (switchUseItemImage != null)
        {
            // Tambahkan event listener untuk klik pada Image
            Button button = switchUseItemImage.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ToggleUseItem);
            }
            else
            {
                Debug.LogError("Image component does not have a Button component attached.");
            }
        }
        else
        {
            Debug.LogError("switchUseItemImage is not assigned.");
        }

        stats.equippedItemData[0].itemName = stats.emptyItemTemplate.itemName;
        stats.equippedItemData[1].itemName = stats.emptyItemTemplate.itemName;
        stats.itemUseData[0].itemName = stats.emptyItemTemplate.itemName;
        stats.itemUseData[1].itemName = stats.emptyItemTemplate.itemName;
    }

   private void Update()
    {
        

            UpdateEquippedWeaponUI();
        UpdateItemUseUI();
            // inventoryUI.UpdateSixItemDisplay();

             

           
    }


   



    public void SwapItems(int id1, int id2)
    {
        if (id1 < 0 || id1 >= stats.itemList.Count || id2 < 0 || id2 >= stats.itemList.Count)
            return; // Pastikan ID valid

        Item tempItem = stats.itemList[id1];
        stats.itemList[id1] = stats.itemList[id2];
        stats.itemList[id2] = tempItem;

        // Opsional: Anda bisa menambahkan logika untuk mengupdate status item jika diperlukan
    }

   

    public void AddItem(Item item)
    {
        item = Instantiate(item); // Clone item agar tidak mempengaruhi data global

        // Cari item yang bisa di-stack
        //Item existingItem = stats.itemList.Find(x => x.itemName == item.itemName && x.stackCount < x.maxStackCount);

        //if (existingItem != null)
        //{
        //    //int availableSpace = existingItem.maxStackCount - existingItem.stackCount;
        //    //int amountToAdd = Mathf.Min(availableSpace, item.stackCount);

        //    //existingItem.stackCount += amountToAdd;
        //    //item.stackCount -= amountToAdd;

        //    //// Jika stack penuh, ubah `isStackable` menjadi false
        //    //if (existingItem.stackCount >= existingItem.maxStackCount)
        //    //{
        //    //    existingItem.isStackable = false;
        //    //}

        //    // Jika masih ada sisa item, tambahkan ke slot baru jika ada ruang
        //    //if (item.stackCount > 0)
        //    //{
        //    //    if (stats.itemList.Count < stats.maxItem)
        //    //    {
        //    //        stats.itemList.Add(item);
        //    //    }
        //    //    else
        //    //    {
        //    //        Debug.LogWarning("Inventory penuh! Item tidak bisa ditambahkan.");
        //    //        return;
        //    //    }
        //    //}
        //}
        //else
        //{
        //    // Jika tidak ada stack yang bisa diisi, tambahkan sebagai slot baru jika ada ruang
        //    if (stats.itemList.Count < stats.maxItem)
        //    {
        //        stats.itemList.Add(item);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("Inventory penuh! Item tidak bisa ditambahkan.");
        //        return;
        //    }
        //}

        // Update UI
        inventoryUI.UpdateInventoryUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang disentuh memiliki tag "ItemDrop"
        if (other.CompareTag("ItemDrop"))
        {
            Debug.Log("terdeteksi item drop");
            // Ambil data dari PrefabItemBehavior jika ada
            PrefabItemBehavior prefabItem = other.GetComponent<PrefabItemBehavior>();

            if (stats.itemList.Count < stats.maxItem)
            {
                if (prefabItem != null)
                {
                    string itemName = prefabItem.namePrefab;
                    float itemHealth = prefabItem.health;

                    // Cari item di ItemPool berdasarkan namePrefab dari PrefabItemBehavior
                    Item itemToAdd = ItemPool.Instance.GetItem(itemName);

                    if (itemToAdd != null)
                    {
                        int prevCount = stats.itemList.Count; // Hitung jumlah item sebelum AddItem

                        // Buat clone item agar tidak merusak data asli di ItemPool
                        itemToAdd = Instantiate(itemToAdd);
                        itemToAdd.health = itemHealth; // Atur nilai health dari item menggunakan health dari PrefabItemBehavior

                        Debug.Log("nyawa item itu adalah : " + itemHealth);
                        // Tambahkan item ke inventory
                        AddItem(itemToAdd);

                        // Jika jumlah item di inventory bertambah, berarti item berhasil dimasukkan
                        if (stats.itemList.Count > prevCount ||
                            (itemToAdd.isStackable && stats.itemList.Exists(x => x.itemName == itemToAdd.itemName)))
                        {
                            Debug.Log($"{itemName} berhasil ditambahkan ke inventory. Menghancurkan item drop.");
                            Destroy(other.gameObject); // Hancurkan item drop dari world
                        }
                        else
                        {
                            Debug.LogWarning($"{itemName} tidak ditambahkan ke inventory karena penuh.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Item dengan nama {itemName} tidak ditemukan di ItemPool.");
                    }
                }
                else
                {
                    Debug.LogWarning("Komponen PrefabItemBehavior tidak ditemukan di objek dengan tag 'ItemDrop'.");
                }
            }
        }
        else if(other.CompareTag("Animal"))
        {
            Debug.Log("animal terdeteksi");
            AnimalBehavior animalBehavior = other.GetComponent<AnimalBehavior>();
            if (animalBehavior != null && animalBehavior.isAnimalEvent)
            {
                miniGameHewanUI.Open(other.gameObject);
                
            }

            //AnimalBehavior animalBehavior = other.GetComponent<AnimalBehavior>();
            //animalBehavior.DropItem();
        }
        //else if(other.CompareTag("Pintu"))
        //{
        //    foreach (var pintuLokasi in pintuManager.pintuArray)
        //    {
        //        if(pintuLokasi.pintuIn.name == other.name)
        //        {
        //            GameObject pintu = pintuLokasi.pintuIn;
        //            pintuManager.EnterArea(pintu);
        //        }else if (pintuLokasi.pintuOut.name == other.name)
        //        {
        //            GameObject pintu = pintuLokasi.pintuOut;
        //            pintuManager.EnterArea(pintu);
        //        }
        //    }
        //}    
    }





    public void RemoveItem(Item item)
    {
        item = Instantiate(item);
        if (item.isStackable)
        {
            //stats.itemList.Find(x => x.itemName == item.itemName).stackCount--;
            //if (stats.itemList.Find(x => x.itemName == item.itemName).stackCount <= 0)
            //    stats.itemList.Remove(stats.itemList.Find(x => x.itemName == item.itemName));
        }
        else
            stats.itemList.Remove(stats.itemList.Find(x => x.itemName == item.itemName));
    }

    public Item FindItemInInventory(string name)
    {
        if (stats.itemList.Exists(x => x.itemName == name))
        {
            return stats.itemList.Find(x => x.itemName == name);
        }
        return ItemPool.Instance.GetItem("Empty");
    }

    public void EquipItem(ItemData itemToEquip)
    {
        // Pastikan item yang mau dipasang ada di inventaris utama
        if (!stats.inventory.Contains(itemToEquip))
        {
            Debug.LogWarning($"Mencoba memasang '{itemToEquip.itemName}' yang tidak ada di inventaris.");
            return;
        }

        // Dapatkan template SO dari item untuk info (seperti tipe item)
        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToEquip.itemName, itemToEquip.quality);
        if (itemTemplate == null) return;

        // Tentukan list equipment mana yang akan digunakan berdasarkan tipe item
        // Ini membuat fungsi Anda lebih fleksibel di masa depan
        List<ItemData> targetEquipmentList;
        if (itemTemplate.types == ItemType.Melee_Combat || itemTemplate.types == ItemType.Ranged_Combat)
        {
            targetEquipmentList = stats.equippedItemData;
        }
        else if (itemTemplate.types == ItemType.Heal || itemTemplate.types == ItemType.Buff)
        {
            targetEquipmentList = stats.itemUseData;
        }
        else
        {
            Debug.Log($"Item '{itemToEquip.itemName}' tidak bisa dipasang.");
            return; // Keluar jika item tidak bisa dipasang
        }

        // --- INI LOGIKA INTI YANG LEBIH BERSIH ---

        // Cek apakah slot pertama (indeks 0) kosong
        if (targetEquipmentList[0].itemName == "Empty") // Lebih aman membandingkan nama
        {
            // Jika kosong, langsung pasang di slot 0
            targetEquipmentList[0] = itemToEquip;
            stats.inventory.Remove(itemToEquip); // Hapus dari inventaris
        }
        // Jika slot pertama sudah terisi, coba cek slot kedua (indeks 1)
        else if (targetEquipmentList[1].itemName == "Empty")
        {
            // Jika kosong, langsung pasang di slot 1
            targetEquipmentList[1] = itemToEquip;
            stats.inventory.Remove(itemToEquip);
        }
        // Jika kedua slot sudah terisi...
        else
        {
            // Kembalikan item yang ada di slot pertama ke inventaris
            ItemPool.Instance.AddItem(itemToEquip);


            // Pasang item baru di slot pertama
            targetEquipmentList[0] = itemToEquip;
            stats.inventory.Remove(itemToEquip);
        }

        Debug.Log($"Item '{itemToEquip.itemName}' berhasil dipasang.");

        MechanicController.Instance.InventoryUI.RefreshInventoryItems();
        MechanicController.Instance.InventoryUI.UpdateSixItemDisplay();
    }


    // Add item to quick slot according index (0,1)
    public void AddQuickSlot(Item item, int index)
    {
        // Memastikan item ada dalam stats.itemList dan bukan "Empty"
        if (!stats.itemList.Exists(x => x.itemName == item.itemName) && item.itemName != "Empty")
            return;

        // Jika slot yang dipilih sudah terisi, kembalikan item sebelumnya ke stats.itemList
        //if (stats.quickSlots[index] != null && stats.quickSlots[index] != stats.emptyItem && stats.quickSlots[index].itemName != "Empty" && stats.quickSlots[index].stackCount > 0)
        {
            stats.itemList.Add(stats.quickSlots[index]);
            Debug.Log("menambahkan item ke inventory");
        }


        // Menambahkan item baru ke stats.quickSlots pada index yang sesuai
        stats.quickSlots[index] = item;

        // Menghapus item yang baru ditambahkan dari stats.itemList
        stats.itemList.Remove(item);

        // Refresh UI inventory dan memperbarui tampilan item
        inventoryUI.RefreshInventoryItems();
        inventoryUI.UpdateSixItemDisplay();

        // Memperbarui UI dengan item baru di slot yang sesuai
        //inventoryUI.SetActiveItem(index + 2, item); // Menyesuaikan indeks untuk tampilan UI
        print(item.itemName + " equipped");
    }


    // Use which quick slot (1,2)
    public void UseQuickSlot(int which)
    {
        // Making sure there is an item in the quick slot
        Item item = stats.quickSlots[which];
        if (item == null || item.itemName == "Empty")
        {
            print("No item bish");
            return;
        }

        // Using them from inventory
        print("using quick slot " + (which));
        //item.stackCount--;
        
        

        // Have its effect
        switch (item.types)
        {
            //case ItemType.Heal:
            //    // Heal Player
            //    print("HEALED");
            //    healParticle.Play();
            //    inventoryUI.jumlahQuickItem1.text = item.stackCount.ToString();
            //    buffScrollController.GetBuff(item);
            //    break;

            //case ItemType.Buff:
            //    // Buff player
            //    inventoryUI.jumlahQuickItem2.text = item.stackCount.ToString();
            //    buffScrollController.GetBuff(item);
            //    break;

            default: break;
        }

        //if (item.stackCount <= 0)
        //{
            
        //    AddQuickSlot(stats.emptyItem, which);
        //}
    }

    

      // Fungsi untuk mengganti senjata
    public void ToggleWeapon()
    {
        meleeOrRanged = !meleeOrRanged;
        UpdateEquippedWeaponUI();
        spesialSkillWeapon.UseWeaponSkill(stats.equippedWeapon, false);
        Debug.Log("Weapon Toggle");
        
    }
    // Fungsi untuk mengganti item yang dapat digunakan
    public void ToggleUseItem()
    {
        itemUse1 = !itemUse1;
        UpdateItemUseUI();
        Debug.Log("Toggle uhuyyyyyyy");
    }

    private void UpdateEquippedWeaponUI()
    {
        // Mengecek apakah ada senjata di slot melee/ranged (slot 0)
        if (meleeOrRanged && stats.equippedCombat[0] != null)
        {
            stats.equippedWeapon = stats.equippedCombat[0];
            
            if (playerUI != null && playerUI.equippedUI != null)
            {
                playerUI.equippedUI.sprite = stats.equippedWeapon.sprite;

                

            }
        }
        // Mengecek apakah ada senjata di slot kedua (slot 1)
        else if (stats.equippedCombat[1] != null)
        {
            stats.equippedWeapon = stats.equippedCombat[1];
            if (playerUI != null && playerUI.equippedUI != null)
            {
                playerUI.equippedUI.sprite = stats.equippedWeapon.sprite;


               
            }
        }
        
         // Jika slot 0 (melee/ranged) kosong, set default sprite
       if (stats.equippedCombat[0] == null)
        {
            //Debug.Log("Item is not equipped in slot 0");
            if (playerUI != null && playerUI.equippedUI != null)
            {
                if (playerAction != null)
                    {
                        // Sekarang Anda bisa mengakses metode atau properti di Player_Action
                        //playerAction.buttonAttack.gameObject.SetActive(false);  // Memanggil metode di Player_Action

                }
                
            }
        }
        // Jika slot 1 kosong, set default sprite
        else if (stats.equippedCombat[1] == null)
        {
            //Debug.Log("Item is not equipped in slot 1");
            if (playerUI != null && playerUI.equippedUI != null)
            {
                 if (playerAction != null)
                    {
                        // Sekarang Anda bisa mengakses metode atau properti di Player_Action
                        //playerAction.buttonUse.gameObject.SetActive(false);  // Memanggil metode di Player_Action
                    }
            }
        }
        //playerUI.UpdateCapacityBar(stats.equippedWeapon); 

    }

    private void UpdateItemUseUI()
    {
        if (itemUse1 && stats.quickSlots[0] != null)
        {
            stats.equippedItem = stats.quickSlots[0];
            if (playerUI != null && playerUI.itemUseUI != null)
            {
                playerUI.itemUseUI.sprite = stats.equippedItem.sprite;
            }
        }
        else if (stats.quickSlots[1] != null)
        {
            stats.equippedItem = stats.quickSlots[1];
            if (playerUI != null && playerUI.itemUseUI != null)
            {
                playerUI.itemUseUI.sprite = stats.equippedItem.sprite;
            }
        }
    }

}
