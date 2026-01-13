using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISaveable
{
    [Header("Player State (Runtime)")]
    public int health;
    public int currentHealthCap;
    public float stamina;
    public float currentStaminaCap;
    public bool isInGrief;
    public bool equipped1;
    public bool itemUse1;
    public float currentGriefPenalty;
    public int healingQuestsCompleted;
    public List<ItemData> inventory = new List<ItemData>();
    public List<ItemData> equippedItemData = new List<ItemData>(2);
    public List<ItemData> itemUseData = new List<ItemData>(2);
    public float currentFatiguePenalty;
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
    private void OnEnable()
    {
        // Berlangganan ke event saat objek aktif
        TimeManager.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        // Selalu berhenti berlangganan saat objek nonaktif untuk menghindari error
        TimeManager.OnDayChanged -= HandleNewDay;
    }

    public void HandleNewDay()
    {
        HandleReverseHealthandStamina();


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

    public object CaptureState()
    {
        Debug.Log($"[SAVE-CAPTURE] PlayerController menangkap {inventory.Count} item di inventaris.");
        //Transform playerTransform = ActivePlayer.transform;
        Vector3 playerPosition = ActivePlayer.transform.position;
        Debug.Log($"[LOAD-RESTORE] PlayerController me-restore posisi player ke {playerPosition}.");
        // Buat "formulir" baru
        return new PlayerSaveData
        {
            // Isi semua data dari kondisi saat ini

            position = playerPosition,
            health = this.health,
            currentHealthCap = this.currentHealthCap,
            stamina = this.stamina,
            currentStaminaCap = this.currentStaminaCap,
            isInGrief = this.isInGrief,
            currentGriefPenalty = this.currentGriefPenalty,
            healingQuestsCompleted = this.healingQuestsCompleted,
            currentFatiguePenalty = this.currentFatiguePenalty,

            // Isi semua list inventory dan equipment
            inventory = this.inventory,
            equippedItemData = this.equippedItemData,
            itemUseData = this.itemUseData,
            equipped1 = this.equipped1,
            itemUse1 = this.itemUse1,
            coins = GameEconomy.Instance.coins
        };
    }

    public void RestoreState(object state)
    {
        // Terima "formulir" dan ubah ke tipe yang benar
        PlayerSaveData data = (PlayerSaveData)state;

        // Terapkan semua data kembali ke pemain
        this.health = data.health;
        this.currentHealthCap = data.currentHealthCap;
        this.stamina = data.stamina;
        this.currentStaminaCap = data.currentStaminaCap;
        this.isInGrief = data.isInGrief;
        this.currentGriefPenalty = data.currentGriefPenalty;
        this.healingQuestsCompleted = data.healingQuestsCompleted;
        this.currentFatiguePenalty = data.currentFatiguePenalty;

        // Kembalikan semua list inventory dan equipment
        this.inventory = data.inventory;
        this.equippedItemData = data.equippedItemData;
        this.itemUseData = data.itemUseData;
        this.equipped1 = data.equipped1;
        this.itemUse1 = data.itemUse1;
        GameEconomy.Instance.coins = data.coins;

        Transform playerTransform = ActivePlayer.transform;
        playerTransform.position = data.position;
        Debug.Log($"[LOAD-RESTORE] PlayerController me-restore posisi player ke {data.position}.");


        Debug.Log($"[LOAD-RESTORE] PlayerController me-restore {this.inventory.Count} item ke inventaris.");

        // PENTING: Setelah me-restore data, perbarui UI
        // PlayerUI.Instance.UpdateAllUI();
    }

    public void StartPlayerPosition(Vector2 startPosition)
    {
        ActivePlayer.transform.position = startPosition;
        Debug.Log($"Player dimulai di posisi: {startPosition}");
    }
    public void InitializeForNewGame()
    {
        Debug.Log("Menginisialisasi Player untuk game baru dari PlayerData_SO...");

        if (playerData == null)
        {
            Debug.LogError("PlayerData_SO belum diatur di Inspector PlayerController!", this.gameObject);
            return;
        }

        // Atur semua nilai progres ke nilai awal/default dari blueprint
        this.health = playerData.maxHealth;
        this.currentHealthCap = playerData.maxHealth;
        this.stamina = playerData.maxStamina;
        this.currentStaminaCap = playerData.maxStamina;

        // Atur status-status lain ke kondisi awal
        this.isInGrief = false; // Contoh, duka dimulai dari false
        this.currentGriefPenalty = 0;
        this.healingQuestsCompleted = 0;
        this.currentFatiguePenalty = 0;

        //  Kosongkan semua list
        this.inventory.Clear();
        this.equippedItemData.Clear();
        this.itemUseData.Clear();

        //  (Opsional) Isi slot equipment dengan item kosong jika diperlukan
        //    Ini memastikan list memiliki 2 elemen 'kosong' daripada benar-benar kosong.
        for (int i = 0; i < 2; i++)
        {
            if (playerData.emptyItemTemplate != null)
            {
                equippedItemData.Add(playerData.emptyItemTemplate);
                itemUseData.Add(playerData.emptyItemTemplate);
            }
        }

        Debug.Log("Inisialisasi Pemain Selesai.");
    }


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
        if (ActivePlayer.Health.SpendStamina(useStamina))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HandleDrainStamina(float useStamina)
    {
        Debug.Log("HandleDrainStamina called with useStamina: " + useStamina);
        if (ActivePlayer.Health.DrainStamina(useStamina))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void HandleReverseHealthandStamina()
    {

        if (ActivePlayer.Health != null)
        {
            ActivePlayer.Health.ReverseHealthandStamina();
        }
        else
        {
            Debug.LogError("player Heal kosong bang");
        }
    }


    public void HandleEquipItem(ItemData item)
    {
        ActivePlayer.Inventory.EquipItem(item);
    }
    public void HandleSetActiveItem(int slot, ItemData item)
    {
        MechanicController.Instance.InventoryUI.SetActiveItem(slot, item);
    }



    public void MoveItem(List<ItemData> sourceList, List<ItemData> targetList, ItemData itemToMove, int amountToMove)
    {
        // Dapatkan data template dari database
        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(itemToMove.itemName, itemToMove.quality);
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
            ItemData newSlot = new ItemData(itemTemplate.itemName, amountForNewSlot, itemToMove.quality, itemToMove.itemHealth);
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

    public ItemData HandleGetItem(int index)
    {
        // Pastikan ada player aktif dan data inventory ada
        if (ActivePlayer == null || playerData == null)
        {
            return null;
        }

        // Pastikan indeks yang diminta berada dalam jangkauan list inventory.
        if (index < 0 || index >= inventory.Count)
        {
            Debug.LogWarning($"Percobaan mengambil item pada indeks di luar jangkauan: {index}");
            return null; // Kembalikan null karena indeks tidak valid
        }

        // Langsung ambil dan kembalikan ItemData pada indeks yang benar.
        // Tidak perlu menggunakan loop.
        ItemData itemData = inventory[index];
        return itemData;
    }

    public Vector3 HandleGetPlayerPosition()
    {
        // Pastikan ada player aktif
        if (ActivePlayer != null)
        {
            // Kembalikan nilai dari properti transform.position, yang bertipe Vector3
            return ActivePlayer.Movement.face.transform.position;
        }
        else
        {
            Debug.LogError("Tidak ada Player aktif untuk mendapatkan posisi.");
            // Kembalikan posisi default (0,0,0) jika tidak ada player
            return Vector3.zero;
        }
    }

    public void HandlePlayerIsGreaf()
    {

        if (ActivePlayer.Health != null)
        {
            ActivePlayer.Health.StartGrief();
            Debug.Log("Status grief telah diatur ke true untuk player aktif.");
        }
        else
        {
            Debug.LogError("Tidak ada Player aktif untuk mengatur status grief.");
        }
    }

    public void HandlePlayerPingsan()
    {
        ActivePlayer.Health.PlayerPingsan();
    }



    public void HandlePlayAnimation(string nameAnimation)
    {
        ActivePlayer.Player_Anim.PlayAnimation(nameAnimation);

    }




    public void HandleButtonUseItem()
    {
        ActivePlayer.Inventory.UseQuickSlot();
    }

    public void HandleHealPlayer(int healthAmount, int staminaAmount)
    {
        ActivePlayer.Health.Heal(healthAmount, staminaAmount);
    }


}