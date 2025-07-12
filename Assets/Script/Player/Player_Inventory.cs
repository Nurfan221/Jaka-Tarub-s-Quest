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




 

     public bool inventoryOpened;
     public bool inventoryClosed;
    //[SerializeField] MiniGameHewanUI miniGameHewanUI;

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

            ItemPool.Instance.AddItem(itemData);
            MechanicController.Instance.HandleUpdateInventory();
            Destroy(other.gameObject);
            
        }
        else if(other.CompareTag("Animal"))
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
                quickSlotList[0] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
            else if (quickSlotList[1].itemName == "Empty")
            {
                quickSlotList[1] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
            else
            {

                // Kembalikan item lama di slot 0 ke inventaris
                ItemPool.Instance.AddItem(quickSlotList[1]);
                // Pasang item baru di slot 0
                quickSlotList[0] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
        }

        // JIKA ITEM ADALAH ITEM GUNA (HEAL/BUFF)
        else if(itemTemplate.types == ItemType.Melee_Combat || itemTemplate.types == ItemType.Ranged_Combat)
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
                // Kembalikan item lama di slot 0 ke inventaris
                ItemPool.Instance.AddItem(equipmentList[1]);
                // Pasang item baru di slot 0
                equipmentList[0] = itemToEquip;
                stats.inventory.Remove(itemToEquip);
            }
        }
        else
        {
            Debug.Log($"Item '{itemToEquip.itemName}' tidak bisa dipasang.");
            return;
        }

        Debug.Log($"Item '{itemToEquip.itemName}' berhasil dipasang.");
        MechanicController.Instance.InventoryUI.RefreshInventoryItems();
        MechanicController.Instance.InventoryUI.UpdateSixItemDisplay();
        PlayerUI.Instance.UpdateItemUseUI();
        PlayerUI.Instance.UpdateEquippedWeaponUI();


    }


    // Add item to quick slot according index (0,1)
    //public void AddQuickSlot(Item item, int index)
    //{
    //    // Memastikan item ada dalam stats.itemList dan bukan "Empty"
    //    if (!stats.itemList.Exists(x => x.itemName == item.itemName) && item.itemName != "Empty")
    //        return;

    //    // Jika slot yang dipilih sudah terisi, kembalikan item sebelumnya ke stats.itemList
    //    //if (stats.quickSlots[index] != null && stats.quickSlots[index] != stats.emptyItem && stats.quickSlots[index].itemName != "Empty" && stats.quickSlots[index].stackCount > 0)
    //    {
    //        stats.itemList.Add(stats.quickSlots[index]);
    //        Debug.Log("menambahkan item ke inventory");
    //    }


    //    // Menambahkan item baru ke stats.quickSlots pada index yang sesuai
    //    stats.quickSlots[index] = item;

    //    // Menghapus item yang baru ditambahkan dari stats.itemList
    //    stats.itemList.Remove(item);

    //    // Refresh UI inventory dan memperbarui tampilan item
    //    inventoryUI.RefreshInventoryItems();
    //    inventoryUI.UpdateSixItemDisplay();

    //    // Memperbarui UI dengan item baru di slot yang sesuai
    //    //inventoryUI.SetActiveItem(index + 2, item); // Menyesuaikan indeks untuk tampilan UI
    //    print(item.itemName + " equipped");
    //}


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
   

    

}
