using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Inventory : MonoBehaviour
{
    public static Player_Inventory Instance; // Access this class by its Instance
    Player_Action pA;

    public List<Item> itemList;

    public int maxItem = 18;

    [SerializeField] GameObject normalAttackHitArea; //Referensi image hitbox
    [SerializeField] private Button switchWeaponImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    [SerializeField] private Button switchUseItemImage; // Referensi ke Image yang digunakan untuk mengganti senjata
    [SerializeField] public ContohFlipCard contohFlipCard;
    [SerializeField] PintuManager pintuManager;

    public bool meleeOrRanged = true;
    public bool itemUse1 = true;

    [Header("UI ELEMENTS")]
    public Item emptyItem;
    public List<Item> equippedCombat = new List<Item>(2);
    public Item equippedWeapon;
    public Item equippedItem;
    public List<Item> quickSlots = new List<Item>(2);

    InventoryUI inventoryUI;
    Player_Action playerAction;
     public bool inventoryOpened;
     public bool inventoryClosed;
    [SerializeField] MiniGameHewanUI miniGameHewanUI;

    [SerializeField] ParticleSystem healParticle;
    public Button inventoryButton;  // Drag and drop the button in the inspector
    public Button closeInventoryButton;  // Drag and drop the close button in the inspector

   

    private void Awake()
    {
        Instance = this;
        pA = GetComponent<Player_Action>();

        // Making sure everything in it is a clone
        List<Item> newList = new List<Item>();
        foreach (Item item in itemList)
        {
            newList.Add(Instantiate(item));
        }
        itemList.Clear();
        itemList = newList;

        emptyItem = Instantiate(emptyItem);
    }

    private void Start()
    {
        inventoryUI = PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>();
        Player_Action playerAction = FindObjectOfType<Player_Action>();
        inventoryButton.onClick.AddListener(ToggleInventory); // Add listener to button
        closeInventoryButton.onClick.AddListener(CloseInventory); // Add listener to close button

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
    }

   private void Update()
    {
        

            UpdateEquippedWeaponUI();
            UpdateItemUseUI();
            // inventoryUI.UpdateSixItemDisplay();

             

           
    }


    private void ToggleInventory()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        inventoryOpened = !inventoryOpened;
        inventoryClosed = !inventoryOpened; // Update inventoryClosed
        PlayerUI.Instance.inventoryUI.SetActive(inventoryOpened);
        contohFlipCard.IfClose();

        GameController.Instance.ShowPersistentUI(false);

        if (inventoryOpened)
        {
            GameController.Instance.PauseGame();
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihAnggur"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihBawangMerah"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihBawangPutih"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihBungaMatahari"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihCabai"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihDaunBawang"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihJagung"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihKentang"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihKubis"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihLabu"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihLabuHijau"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihLobak"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihPadi"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihPaprikaHijau"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihPaprikaKuning"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihPaprikaMerah"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihStrawberry"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihTimun"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihTomat"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihUbiMerah"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihUbiUngu"));
            //Instance.AddItem(ItemPool.Instance.GetItem("BenihWortel"));





            inventoryUI.UpdateInventoryUI(); // Update UI when inventory is opened
        }
        else
        {
            GameController.Instance.ResumeGame();
        }
    }

    public void AddItem(string namaItem)
    {
        Instance.AddItem(ItemPool.Instance.GetItem(namaItem));
    }

    public void SwapItems(int id1, int id2)
    {
        if (id1 < 0 || id1 >= itemList.Count || id2 < 0 || id2 >= itemList.Count)
            return; // Pastikan ID valid

        Item tempItem = itemList[id1];
        itemList[id1] = itemList[id2];
        itemList[id2] = tempItem;

        // Opsional: Anda bisa menambahkan logika untuk mengupdate status item jika diperlukan
    }

    private void CloseInventory()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("Click");

        inventoryOpened = false;
        inventoryClosed = true;
        PlayerUI.Instance.inventoryUI.SetActive(false);
        GameController.Instance.ShowPersistentUI(true);
        GameController.Instance.ResumeGame();
    }

    public void AddItem(Item item)
    {
        item = Instantiate(item); // Clone item agar tidak mempengaruhi data global

        // Cari item yang bisa di-stack
        Item existingItem = itemList.Find(x => x.itemName == item.itemName && x.stackCount < x.maxStackCount);

        if (existingItem != null)
        {
            int availableSpace = existingItem.maxStackCount - existingItem.stackCount;
            int amountToAdd = Mathf.Min(availableSpace, item.stackCount);

            existingItem.stackCount += amountToAdd;
            item.stackCount -= amountToAdd;

            // Jika stack penuh, ubah `isStackable` menjadi false
            if (existingItem.stackCount >= existingItem.maxStackCount)
            {
                existingItem.isStackable = false;
            }

            // Jika masih ada sisa item, tambahkan ke slot baru jika ada ruang
            if (item.stackCount > 0)
            {
                if (itemList.Count < maxItem)
                {
                    itemList.Add(item);
                }
                else
                {
                    Debug.LogWarning("Inventory penuh! Item tidak bisa ditambahkan.");
                    return;
                }
            }
        }
        else
        {
            // Jika tidak ada stack yang bisa diisi, tambahkan sebagai slot baru jika ada ruang
            if (itemList.Count < maxItem)
            {
                itemList.Add(item);
            }
            else
            {
                Debug.LogWarning("Inventory penuh! Item tidak bisa ditambahkan.");
                return;
            }
        }

        // Update UI
        PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>().UpdateInventoryUI();
    }








    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang disentuh memiliki tag "ItemDrop"
        if (other.CompareTag("ItemDrop"))
        {
            Debug.Log("terdeteksi item drop");
            // Ambil data dari PrefabItemBehavior jika ada
            PrefabItemBehavior prefabItem = other.GetComponent<PrefabItemBehavior>();

            if (itemList.Count < maxItem)
            {
                if (prefabItem != null)
                {
                    string itemName = prefabItem.namePrefab;
                    float itemHealth = prefabItem.health;

                    // Cari item di ItemPool berdasarkan namePrefab dari PrefabItemBehavior
                    Item itemToAdd = ItemPool.Instance.GetItem(itemName);

                    if (itemToAdd != null)
                    {
                        int prevCount = itemList.Count; // Hitung jumlah item sebelum AddItem

                        // Buat clone item agar tidak merusak data asli di ItemPool
                        itemToAdd = Instantiate(itemToAdd);
                        itemToAdd.health = itemHealth; // Atur nilai health dari item menggunakan health dari PrefabItemBehavior

                        Debug.Log("nyawa item itu adalah : " + itemHealth);
                        // Tambahkan item ke inventory
                        AddItem(itemToAdd);

                        // Jika jumlah item di inventory bertambah, berarti item berhasil dimasukkan
                        if (itemList.Count > prevCount ||
                            (itemToAdd.isStackable && itemList.Exists(x => x.itemName == itemToAdd.itemName)))
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
            itemList.Find(x => x.itemName == item.itemName).stackCount--;
            if (itemList.Find(x => x.itemName == item.itemName).stackCount <= 0)
                itemList.Remove(itemList.Find(x => x.itemName == item.itemName));
        }
        else
            itemList.Remove(itemList.Find(x => x.itemName == item.itemName));
    }

    public Item FindItemInInventory(string name)
    {
        if (itemList.Exists(x => x.itemName == name))
        {
            return itemList.Find(x => x.itemName == name);
        }
        return ItemPool.Instance.GetItem("Empty");
    }

    public void EquipItem(Item item, int index)
    {
        if (!itemList.Exists(x => x.itemName == item.itemName) && item.itemName != "Empty")
            return;

        equippedCombat[index] = item;
        PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>().SetActiveItem(index, item);
        if (item.itemName != "Empty")
        {
            PlayerUI.Instance.UpdateCapacityBar(item);
        }
        print(item.itemName + " equipped");
    }

    // Add item to quick slot according index (0,1)
    public void AddQuickSlot(Item item, int index)
    {
        if (!itemList.Exists(x => x.itemName == item.itemName) && item.itemName != "Empty")
            return;

        switch (item.types)
        {
            case ItemType.Heal:
            case ItemType.Buff:
                break;
            default: break;
        }

        quickSlots[index] = item;
        PlayerUI.Instance.inventoryUI.GetComponent<InventoryUI>().SetActiveItem(index + 2, item);
        print(item.itemName + "equipped");
    }

    // Use which quick slot (1,2)
    public void UseQuickSlot(int which)
    {
        // Making sure there is an item in the quick slot
        Item item = quickSlots[which - 1];
        if (item == null || item.itemName == "Empty")
        {
            print("No item bish");
            return;
        }

        // Using them from inventory
        print("using quick slot " + (which - 1));
        item = itemList.Find(x => x.itemName == item.itemName);
        item.stackCount--;
        if (item.stackCount <= 0)
        {
            // SoundManager.Instance.PlaySound("Eat");

            itemList.Remove(item);
            AddQuickSlot(emptyItem, which - 1);
        }

        // Have its effect
        switch (item.types)
        {
            case ItemType.Heal:
                // Heal Player
                print("HEALED");
                healParticle.Play();
                Player_Health.Instance.Heal(10);
                break;

            case ItemType.Buff:
                // Buff player
                print("BUFFED");
                break;

            default: break;
        }
    }

      // Fungsi untuk mengganti senjata
    public void ToggleWeapon()
    {
        meleeOrRanged = !meleeOrRanged;
        UpdateEquippedWeaponUI();
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
        if (meleeOrRanged && equippedCombat[0] != null)
        {
            equippedWeapon = equippedCombat[0];
            if (PlayerUI.Instance != null && PlayerUI.Instance.equippedUI != null)
            {
                PlayerUI.Instance.equippedUI.sprite = equippedWeapon.sprite;

                

            }
        }
        // Mengecek apakah ada senjata di slot kedua (slot 1)
        else if (equippedCombat[1] != null)
        {
            equippedWeapon = equippedCombat[1];
            if (PlayerUI.Instance != null && PlayerUI.Instance.equippedUI != null)
            {
                PlayerUI.Instance.equippedUI.sprite = equippedWeapon.sprite;


               
            }
        }
        
         // Jika slot 0 (melee/ranged) kosong, set default sprite
       if (equippedCombat[0] == null)
        {
            Debug.Log("Item is not equipped in slot 0");
            if (PlayerUI.Instance != null && PlayerUI.Instance.equippedUI != null)
            {
                if (playerAction != null)
                    {
                        // Sekarang Anda bisa mengakses metode atau properti di Player_Action
                        playerAction.buttonAttack.gameObject.SetActive(false);  // Memanggil metode di Player_Action

                }
                
            }
        }
        // Jika slot 1 kosong, set default sprite
        else if (equippedCombat[1] == null)
        {
            Debug.Log("Item is not equipped in slot 1");
            if (PlayerUI.Instance != null && PlayerUI.Instance.equippedUI != null)
            {
                 if (playerAction != null)
                    {
                        // Sekarang Anda bisa mengakses metode atau properti di Player_Action
                        playerAction.buttonUse.gameObject.SetActive(false);  // Memanggil metode di Player_Action
                    }
            }
        }
         
    }

    private void UpdateItemUseUI()
    {
        if (itemUse1 && quickSlots[0] != null)
        {
            equippedItem = quickSlots[0];
            if (PlayerUI.Instance != null && PlayerUI.Instance.itemUseUI != null)
            {
                PlayerUI.Instance.itemUseUI.sprite = equippedItem.sprite;
            }
        }
        else if (quickSlots[1] != null)
        {
            equippedItem = quickSlots[1];
            if (PlayerUI.Instance != null && PlayerUI.Instance.itemUseUI != null)
            {
                PlayerUI.Instance.itemUseUI.sprite = equippedItem.sprite;
            }
        }
    }

}
