using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using static MainQuest1_Controller;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

// Enum bantuan untuk membuat kode lebih mudah dibaca


public class Player_Action : MonoBehaviour
{
    private enum FarmActionType { Hoe, Water, UsePesticide }
    public Animator animator; // Deklarasikan Animator
    public SpriteRenderer hitBoxRenderer;


    private Coroutine stopAnimCoroutine;




    #region COMBAT
    [Header("COMBAT")]
    public bool combatMode = false;
    public bool canAttack = true;
    [SerializeField] GameObject normalAttackHitArea;
    [SerializeField] GameObject specialAttackHitArea;
    public Vector3 faceDirection;
    public Vector3 playerPosition;

    float specialAttackTimer;
    public bool canSpecialAttack = true;

    //[SerializeField] GameObject swordFX;
    //[SerializeField] GameObject swordAOEFX;
    //[SerializeField] ParticleSystem swordParticle;
    //[SerializeField] ParticleSystem swordAOEParticle;
    //[SerializeField] ParticleSystem tombakParticle;

    [SerializeField] ParticleSystem buffParticle;

    float damageMult = 1;
    #endregion

    #region QUICK_SLOTS
    [Header("QUICK SLOTS")]
    [SerializeField] float quickSlotCD = 2f;
    float[] quickSlotsTimer = new float[2];
    bool[] canQuickSlots = new bool[2];
    #endregion

    #region INTERACTS
    [Header("INTERACTS")]

    [SerializeField] private FarmTile farmTile;
    // [SerializeField] private plantSeed plantSeed;




    bool canInteract = false;
    Interactable interactable;

    [SerializeField] private Transform face; // Hubungkan di inspector
                                             //[SerializeField] private TreeBehavior treeBehavior;

    #endregion

    private PlayerData_SO stats;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }

    void Start()
    {
        //if (buttonAttack != null)
        //{
        //    buttonAttack.onClick.AddListener(OnAttackButtonClick);
        //}

        //if (specialAttack != null)
        //{
        //    specialAttack.onClick.AddListener(OnSpecialAttackButtonClick);
        //}

        //if (buttonUse != null)
        //{
        //    buttonUse.onClick.AddListener(OnUseButtonClick);
        //}



        //if (playerUI != null)
        //{
        //    if (playerUI.actionInputButton != null)
        //    {
        //        playerUI.actionInputButton.onClick.AddListener(OnActionInputButtonClick);
        //    }
        //    else
        //    {
        //        Debug.LogError("actionInputButton is null in PlayerUI");
        //    }
        //}
        //else
        //{
        //    Debug.LogError("PlayerUI not found");
        //}


    }

   



    // cek apakah player bersentuhan dengan tanaman




    #region UI_HELPER
    // Handling Quick slot cooldown after using
    void HandleQuickSLotUI(int i)
    {
        if (!canQuickSlots[i])
        {
            quickSlotsTimer[i] += Time.deltaTime;
            // PlayerUI.Instance.quickSlotsUI_HUD[i].fillAmount = quickSlotsTimer[i] / quickSlotCD;
            if (quickSlotsTimer[i] > quickSlotCD)
            {
                quickSlotsTimer[i] = 0;
                canQuickSlots[i] = true;
            }
        }
    }

    // Handling the spiral animation for UIs
    
    #endregion

    // Helper function for checking interactables nearby
    


    public void OnActionInputButtonClick()
    {
        if (canInteract)
        {
            interactable.BaseInteract();
            Debug.Log("actionInputButton on click");
        }
    }



    #region COMBAT_ACTIONS

    public void OnAttackButtonClick()
    {
        if (combatMode && canAttack)
        {
            Attack();
        }
    }

    public void OnSpecialAttackButtonClick()
    {
        if (combatMode && canAttack && canSpecialAttack)
        {
            canSpecialAttack = false;
            SpecialAttack();
            //menggunakan spesial skill
            //StartCoroutine(HandleUICD(PlayerUI.Instance.specialAttackUI, Player_Inventory.Instance.equippedWeapon.SpecialAttackCD));
        }
    }
   

    //private void OnUseButtonClick()
    //{
    //    Player_Inventory player_inventory = FindObjectOfType<Player_Inventory>();

    //    // Pastikan player_inventory ada
    //    if (player_inventory != null)
    //    {
    //        // Cek apakah quickSlot[0] berisi item dan itemUse1 adalah true
    //        if (player_inventory.quickSlots[0] != null && player_inventory.itemUse1)
    //        {
    //            // Gunakan quick slot 1
    //            player_inventory.UseQuickSlot(0);
    //            //menggunakan item use

    //            //StartCoroutine(HandleUICD(PlayerUI.Instance.itemUseUI, quickSlotCD));
    //            //Debug.Log("Menggunakan item dari quick slot 1");
    //        }
    //        // Jika quickSlot[0] kosong atau itemUse1 false, cek quickSlot[1]
    //        else if (player_inventory.quickSlots[1] != null)
    //        {
    //            // Gunakan quick slot 2
    //            player_inventory.UseQuickSlot(1);
    //            //menggunakan item use 
    //            //StartCoroutine(HandleUICD(PlayerUI.Instance.itemUseUI, quickSlotCD));
    //            //Debug.Log("Menggunakan item dari quick slot 2");
    //        }
    //        else
    //        {
    //            Debug.Log("Tidak ada item yang bisa digunakan.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("Player_Inventory tidak ditemukan.");
    //    }
    //}


   



    public void ActivateHitboxAndPlayAction(string actionType, int damage, float howLong = 1f, bool AOE = false)
    {
        // 🔹 Pastikan animasi dipanggil lebih dulu
        PlayActionAnimation(actionType);

        GameObject activeHitbox = AOE ? specialAttackHitArea : normalAttackHitArea;
        activeHitbox.transform.position = face.position;
        activeHitbox.name = damage.ToString();

        activeHitbox.SetActive(true);
        StartCoroutine(DeactivateHitboxAfterAnimation(activeHitbox, howLong));

        //logika menambahkan damage buff ke dalaam damage senjata
        //if (buffScrollController.isBuffDamage)
        //{
        //    damage += buffScrollController.jumlahBuffDamage;
        //}

        Debug.Log("Damage berjumlah : " + damage);

        DetectAndDamageObjects(actionType, damage);
    }



    // Coroutine untuk menonaktifkan hitbox setelah animasi selesai
    private IEnumerator DeactivateHitboxAfterAnimation(GameObject hitbox, float duration)
    {
        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }


    private void DetectAndDamageObjects(string actionType, int damage)
    {
        // Define the damage actions for each attack type
        Dictionary<string, List<System.Action<GameObject>>> damageActions = new Dictionary<string, List<System.Action<GameObject>>>()
    {
        { "Kapak", new List<System.Action<GameObject>>
            {
                obj => obj.GetComponent<TreeBehavior>()?.TakeDamage(damage),
                obj => obj.GetComponent<AkarPohon>()?.TakeDamage(damage)
            } },
        { "PickAxe", new List<System.Action<GameObject>> { obj => obj.GetComponent<StoneBehavior>()?.TakeDamage(damage) } },
        { "Sabit", new List<System.Action<GameObject>> { obj => obj.GetComponent<PlantSeed>()?.Harvest() } },
        { "Sword", new List<System.Action<GameObject>>
            {
                obj => obj.GetComponent<Enemy_Health>()?.TakeDamage(damage),
                obj => obj.GetComponent<AnimalBehavior>()?.TakeDamage(damage)
            }
        }
    };

        // Find objects within the hitbox radius
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(face.position, 0.5f);
        foreach (Collider2D obj in hitObjects)
        {
            // Get the tags associated with the current actionType
            List<string> targetTags = GetTargetTags(actionType);

            // Check if the object's tag matches any of the actionType's tags
            foreach (string targetTag in targetTags)
            {
                if (obj.CompareTag(targetTag))
                {
                    Debug.Log($"{obj.name} terkena {actionType} dengan damage: {damage}");

                    // Perform all actions associated with the current actionType
                    if (damageActions.ContainsKey(actionType))
                    {
                        foreach (var action in damageActions[actionType])
                        {
                            action.Invoke(obj.gameObject);
                        }
                    }
                }
            }
        }
    }

    private List<string> GetTargetTags(string actionType)
    {
        // Return the appropriate tags based on actionType
        if (actionType == "Kapak") return new List<string> { "Tree", "AkarPohon" };
        if (actionType == "PickAxe") return new List<string> { "Stone" };
        if (actionType == "Sabit") return new List<string> { "Plant" };
        if (actionType == "Sword") return new List<string> { "Bandit", "Animal" }; // Multiple tags for Sword

        return new List<string>(); // Return empty list if no match
    }





    public void PlayActionAnimation(string actionType)
    {
        if (animator == null)
        {
            Debug.LogError("Animator belum di-assign!");
            return;
        }

        // Pastikan animasi tidak terganggu sebelum selesai
        StartCoroutine(WaitForAnimation(actionType));
    }

    private IEnumerator WaitForAnimation(string actionType)
    {
        // Tentukan arah berdasarkan posisi face
        string triggerName = actionType;

        if (faceDirection.y > 0.5f) triggerName += "Atas";
        else if (faceDirection.y < -0.5f) triggerName += "Bawah";
        else if (faceDirection.x > 0.5f) triggerName += "Kanan";
        else if (faceDirection.x < -0.5f) triggerName += "Kiri";

        animator.SetTrigger(triggerName);

        // Tunggu sampai animasi selesai sebelum mengubah state ke Idle
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.ResetTrigger(triggerName); // Reset Trigger setelah animasi selesai
    }





    public void Attack()
    {
        Debug.Log("tombol attack di tekan");
        Item itemToAttack = ItemPool.Instance.GetItemWithQuality(stats.equippedWeaponTemplate.itemName, stats.equippedWeaponTemplate.quality);
        if (itemToAttack.itemName == "Empty")
            return;

        if (itemToAttack.types == ItemType.Melee_Combat)
        {
            // Memanggil suara pedang ketika serangan normal dengan pedang
            //if (SoundManager.Instance != null)
            //    SoundManager.Instance.PlaySound("Sword");

            print("melee normal attacking");
            PlayerUI.Instance.TakeCapacityBar(itemToAttack);
            //PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina);
            //playerHealth.SpendMaxCurrentStamina(itemToAttack.UseStamina);
            Debug.Log("nama item yang sedang di pakai" + itemToAttack.itemName);
            switch (itemToAttack.itemName)
            {
                //case "Tombak Berburu":
                //case "Halberd":
                //    if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
                //    {
                //        ActivateHitbox(itemToAttack.Damage, itemToAttack.AreaOfEffect);
                //    }
                //break;

                case "Kapak":
                    if (PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina))
                    {
                        Debug.Log("Menyerang menggunakan" + itemToAttack.itemName);

                        //Debug.Log($"Damage: {itemToAttack.Damage}");
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;
                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);
                       
                        //PlayActionAnimation(itemToAttack.itemName);

                        //Debug.Log("Kapak dijalankan dengan hitbox.");
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }
                    break;

                case "PickAxe":
                    if (PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina))
                    {
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;

                        //Debug.Log($"Damage: {itemToAttack.Damage}");
                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);
                       
                        //Debug.Log("PickAxe dijalankan dengan hitbox.");
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }
                    break;

                case "Sabit":
                    if (PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina))
                    {
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;
                        //Debug.Log($"Damage: {itemToAttack.Damage}");

                        //Memanggil ActiveHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);
                       
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }

                    break;
                case "Sword":
                    if (PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina))
                    {
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;

                        //Debug.Log($"Damage: {itemToAttack.Damage}");
                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);

                        //Debug.Log("PickAxe dijalankan dengan hitbox.");
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }

                    break;
                case "Cangkul":
                    if (PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina))
                    {
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;
                        PlayActionAnimation(itemToAttack.itemName);
                       
                        farmTile.HoeTile(playerPosition, faceDirection);
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }
                    //print("Mengapak tanah");

                    break;
                case "PenyiramTanaman":
                    if (PlayerController.Instance.HandleDrainStamina(itemToAttack.UseStamina))
                    {
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;
                        PlayActionAnimation(itemToAttack.itemName);
                        //ActivateHitboxAndPlayAction(itemToAttack.itemName, 0, 0.5f, true);
                        FarmTile.Instance.WaterTile(playerPosition, faceDirection);
                        //farmTile.HoeTile(playerPosition, faceDirection);
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }
                    //print("Mengapak tanah");

                    break;



                    //default:
                    //    ActivateHitbox(itemToAttack.Damage, itemToAttack.AreaOfEffect);
                    //    break;
            }
            StartCoroutine(ActivateAttack(.5f));
        }
        else if (itemToAttack.itemName == "Batu")
        {
            print("throwing rock");
            // throw rock
            StartCoroutine(ShootProjectile(itemToAttack.RangedWeapon_ProjectilePrefab, itemToAttack.Damage));
            // check if rock depleted after use then remove as equipped then remove from inventory
            //if (stats.equippedWeapon.stackCount == 1)
            //{
            //    //stats.EquipItem(ItemPool.Instance.GetItem("Empty"));
            //}

            // minus rock count
            //stats.RemoveItem(ItemPool.Instance.GetItem("Batu"));

            StartCoroutine(ActivateAttack(.5f));
        }
        else if (itemToAttack.types == ItemType.Ranged_Combat)
        {
            // Check for arrow first
            if (stats.itemList.Exists(x => x.itemName == "Anak Panah"))
            {
                print("shooting arrow");
                // Shoot arrow if possible
                StartCoroutine(ShootProjectile(itemToAttack.RangedWeapon_ProjectilePrefab, itemToAttack.Damage));
                // minus arrow count
                //stats.RemoveItem(ItemPool.Instance.GetItem("Anak Panah"));
            }
            else
            {
                print("no arrow bish");
            }
            StartCoroutine(ActivateAttack(1));
        }
    }




    // Diasumsikan fungsi ini ada di dalam skrip seperti Player_Action.cs

    public void SpecialAttack()
    {
        Debug.Log("Special Skill dipicu!");

        // Ambil data item yang sedang aktif dari "Papan Pengumuman"
        ItemData activeWeaponData = PlayerController.Instance.playerData.equippedWeaponTemplate;
        if (activeWeaponData.itemName == "Empty") return;

        // Dapatkan "Katalog Produk" (ItemSO) dari database
        Item itemTemplate = ItemPool.Instance.GetItemWithQuality(activeWeaponData.itemName, activeWeaponData.quality);
        if (itemTemplate == null) return;

        // Cek stamina SEBELUM melakukan aksi apapun
        if (!PlayerController.Instance.HandleDrainStamina(itemTemplate.UseStamina))
        {
            Debug.Log("Stamina tidak cukup untuk menggunakan skill!");
            return;
        }

        // --- DELEGASI TUGAS BERDASARKAN TIPE ITEM ---

        // Apakah ini alat untuk bertani?
        if (itemTemplate.IsInType(ItemType.Pestisida)) // Prioritaskan tipe yang paling spesifik
        {
            HandleFarmingAction(itemTemplate, FarmActionType.UsePesticide);
        }
        else if (itemTemplate.IsInType(ItemType.PenyiramTanaman))
        {
            HandleFarmingAction(itemTemplate, FarmActionType.Water);
        }
        else if (itemTemplate.IsInType(ItemType.Cangkul))
        {
            HandleFarmingAction(itemTemplate, FarmActionType.Hoe);
        }
        // Apakah ini senjata untuk bertarung?
        else if (itemTemplate.IsInType(ItemType.Melee_Combat))
        {
            HandleCombatAction(itemTemplate, false);
        }
        else if (itemTemplate.IsInType(ItemType.Ranged_Combat))
        {
            HandleCombatAction(itemTemplate, true);
        }
        else
        {
            Debug.Log($"Item '{itemTemplate.itemName}' tidak memiliki special attack.");
        }

        // Kurangi durability/kapasitas alat jika ada
        // playerUI.TakeCapacityBar(activeWeaponData);
    }

    // --- FUNGSI-FUNGSI BANTUAN YANG LEBIH FOKUS ---

    // Satu fungsi untuk menangani semua aksi bertani
    private void HandleFarmingAction(Item tool, FarmActionType actionType)
    {
        Debug.Log($"Melakukan aksi bertani: {actionType}");

        // Dapatkan posisi dan arah sekali saja
        Vector3 playerPosition = transform.position;
        Vector2 faceDirection = face.localPosition.normalized;

        PlayActionAnimation(tool.itemName); // Mainkan animasi

        // Gunakan switch untuk menjalankan logika yang tepat
        switch (actionType)
        {
            case FarmActionType.Hoe:
                FarmTile.Instance.HoeTile(playerPosition, faceDirection);
                break;
            case FarmActionType.Water:
                ActivateHitboxAndPlayAction(tool.itemName, 0, 0.5f, true);
                FarmTile.Instance.WaterTile(playerPosition, faceDirection);
                break;
            case FarmActionType.UsePesticide:
                // Logika untuk pestisida...
                break;
        }
    }

    // Satu fungsi untuk menangani semua aksi pertarungan
    private void HandleCombatAction(Item weapon, bool isRanged)
    {
        Debug.Log($"Melakukan aksi pertarungan dengan: {weapon.itemName}");
        PlayActionAnimation(weapon.itemName);

        if (isRanged)
        {
            // Logika untuk menembakkan proyektil
            // if (PlayerInventory.Instance.HasItem("Anak Panah")) { ... }
            StartCoroutine(ShootProjectile(weapon.RangedWeapon_ProjectilePrefab, weapon.Damage, 0f));
        }
        else
        {
            // Logika untuk serangan melee
            StartCoroutine(ActivateAttack(1f));
        }
    }


    private void WaterNearbyPlants()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1.5f); // Adjust radius as needed
        foreach (var hitCollider in hitColliders)
        {
            Seed seed = hitCollider.GetComponent<Seed>();
            if (seed != null)
            {
                // seed.Siram();
            }
        }
    }

    IEnumerator ActivateAttack(float dur)
    {
        canAttack = false;
        yield return new WaitForSeconds(dur);
        canAttack = true;
    }

    #region WEAPON_SPECIFIC
    //IEnumerator StartBuff_PedangRen(float dur)
    //{
    //    damageMult *= 2;
    //    yield return new WaitForSeconds(dur);
    //    damageMult /= 2;
    //}

    IEnumerator ShootProjectile(GameObject prefab, int damage, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        // Check where to aim using mouse
        Vector2 aimPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Shoot prefab to that general area
        Vector2 rotation = aimPos - (Vector2)transform.position;
        float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        GameObject projectile = Instantiate(prefab, transform.position, Quaternion.Euler(0, 0, rot));
        projectile.name = damage.ToString();
        //GameObject projectile = ObjectPooler.Instance.SpawnFromPool("Bullet", transform.position, Quaternion.Euler(0, 0, rot));
        //projectile.GetComponent<BulletLogic>().SetBullet(bulletSpd, bulletdamage);
        projectile.GetComponent<Rigidbody2D>().AddForce(rotation.normalized * 10, ForceMode2D.Impulse);
        yield return null;
    }
    #endregion

    #endregion


}