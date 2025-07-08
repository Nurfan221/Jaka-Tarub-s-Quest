using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Singleton "Otak"
    public static PlayerController Instance { get; private set; }

    // Variabel untuk menyimpan koneksi ke "Tubuh" yang aktif saat ini.
    // Properti publik agar skrip lain bisa melihat, tapi hanya kelas ini yang bisa mengubah.
    // Sekarang ia hanya menyimpan SATU referensi ke "paket" Player yang aktif.
    public Player ActivePlayer { get; private set; }
    public Rigidbody2D ActivePlayerRigidbody { get; private set; }
    public Animator ActivePlayerAnimator { get; private set; }
    public Vector2 MovementDirection { get; private set; }

    public PlayerData_SO playerData;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Fungsi ini akan dipanggil oleh setiap Player_Movement baru yang muncul.
    public void RegisterPlayer(Player player)
    {
        this.ActivePlayer = player;
        Debug.Log($"PlayerController: Paket Player '{player.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterPlayer(Player player)
    {
        if (this.ActivePlayer == player)
        {
            this.ActivePlayer = null;
        }
    }

    // CONTOH FUNGSI PERINTAH

    public void HandleMovement(Vector2 direction)
    {
        // Cek apakah ada player aktif, lalu akses departemen gerakannya.
        if (ActivePlayer != null)
        {
            // "Manajer Hotel, tolong suruh departemen gerakan untuk bergerak."
            ActivePlayer.Movement.SetMovementDirection(direction);
        }
    }

    public void HandleDash()
    {
        ActivePlayer.Movement.TriggerDash();
    }

    public void HandleAttack()
    {
        if (ActivePlayer != null)
        {
            // "Manajer Hotel, tolong suruh departemen aksi untuk menyerang."
            //ActivePlayer.Action.PerformAttack();
        }
    }

    public bool HandleSpendStamina(float useStamina)
    {
        if(ActivePlayer.Health.SpendStamina(useStamina))
        {
            return true;
        }else
        {
            return false;
        }
    }

    public bool HandleDrainStamina(float useStamina)
    {
        if (ActivePlayer.Health.DrainStamina(useStamina))
        {
            return true;
        }else
        {
            return false;
        }
    }
    public void HandleReverseHealthandStamina()
    {
        ActivePlayer.Health.ReverseHealthandStamina();
    }


    public void HandleEquipItem(ItemData item)
    {
        ActivePlayer.Inventory.EquipItem(item);
    }
    public void HandleSetActiveItem(int slot, ItemData item)
    {
        MechanicController.Instance.InventoryUI.SetActiveItem(slot, item);
    }
    public void HandleUpdateCapacityBar(ItemData item)
    {
        PlayerUI.Instance.UpdateCapacityBar(item);
    }

   
    public void MoveItem(List<ItemData> sourceList, List<ItemData> targetList, ItemData itemToMove, int amountToMove)
    {
        // Dapatkan data template dari database
        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToMove.itemName, itemToMove.quality); ; ;
        if (itemTemplate == null || amountToMove <= 0) return;

        // Pastikan kita tidak memindahkan lebih dari yang kita miliki
        amountToMove = Mathf.Min(amountToMove, itemToMove.count);

        int amountSuccessfullyMoved = 0;

       
        // Coba tumpuk di slot yang sudah ada di list tujuan
        if (itemTemplate.isStackable)
        {
            foreach (ItemData slot in targetList)
            {
                if (slot.itemName == itemTemplate.itemName && slot.count < itemTemplate.maxStackCount)
                {
                    int availableSpace = itemTemplate.maxStackCount - slot.count;
                    int amountToAdd = Mathf.Min(availableSpace, amountToMove - amountSuccessfullyMoved);

                    slot.count += amountToAdd;
                    amountSuccessfullyMoved += amountToAdd;

                    if (amountSuccessfullyMoved >= amountToMove) break;
                }
            }
        }

        // Buat slot baru di list tujuan jika masih ada sisa
        int remainingToAdd = amountToMove - amountSuccessfullyMoved;
        int maxSlots = 24; // Anda perlu cara untuk mendapatkan maxSlots dari targetList

        while (remainingToAdd > 0 && targetList.Count < maxSlots)
        {
            int amountForNewSlot = Mathf.Min(remainingToAdd, itemTemplate.maxStackCount);
            ItemData newSlot = new ItemData(itemTemplate.itemName, amountForNewSlot, itemToMove.quality);
            targetList.Add(newSlot);

            amountSuccessfullyMoved += amountForNewSlot;
            remainingToAdd -= amountForNewSlot;
        }

      

        if (amountSuccessfullyMoved > 0)
        {
            // Kurangi jumlah dari slot asal
            itemToMove.count -= amountSuccessfullyMoved;

            // Jika jumlahnya menjadi 0 atau kurang, hapus item dari list asal
            if (itemToMove.count <= 0)
            {
                sourceList.Remove(itemToMove);
            }
        }

       MechanicController.Instance.InventoryUI.RefreshInventoryItems();
        MechanicController.Instance.InventoryUI.UpdateSixItemDisplay();
       
    }

    public void HandleAttackButton()
    {
        ActivePlayer.Action.OnAttackButtonClick();
    }

    public void HandleSpesialAttackButton()
    {
        ActivePlayer.Action.OnSpecialAttackButtonClick();
    }
    //public bool HandleSpendStamina(float floatStamina)
    //{
    //    ActivePlayer.Health.SpendStamina(floatStamina);
    //    //ActivePlayer.Health.SpendMaxCurrentStamina()
    //}


}