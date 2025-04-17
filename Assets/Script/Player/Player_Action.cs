using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Player_Action : MonoBehaviour
{
    public static Player_Action Instance;
     private Player_Inventory player_Inventory;
    [SerializeField] PlayerUI playerUI;
    [SerializeField] Player_Health playerHealth;
    public Animator animator; // Deklarasikan Animator
    [SerializeField] private Animator toolsAnimator;
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
    bool canSpecialAttack = true;

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

    public Button buttonAttack;
    public Button specialAttack;

    public Button buttonUse;


    bool canInteract = false;
    Interactable interactable;

    [SerializeField] private Transform face; // Hubungkan di inspector
    //[SerializeField] private TreeBehavior treeBehavior;

    #endregion


    void Start()
    {
        if (buttonAttack != null)
        {
            buttonAttack.onClick.AddListener(OnAttackButtonClick);
        }

        if (specialAttack != null)
        {
            specialAttack.onClick.AddListener(OnSpecialAttackButtonClick);
        }

        if (buttonUse != null)
        {
            buttonUse.onClick.AddListener(OnUseButtonClick);
        }



        if (playerUI != null)
        {
            if (playerUI.actionInputButton != null)
            {
                playerUI.actionInputButton.onClick.AddListener(OnActionInputButtonClick);
            }
            else
            {
                Debug.LogError("actionInputButton is null in PlayerUI");
            }
        }
        else
        {
            Debug.LogError("PlayerUI not found");
        }


    }

    private void Awake()
    {
        Instance = this;

        //toolsAnimator = GetComponent<Animator>();
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
    IEnumerator HandleUICD(Image image, float cd)
    {
        float startTime = Time.time;
        while (Time.time < startTime + cd)
        {
            image.fillAmount = (Time.time - startTime) / cd;
            yield return null;
        }
        image.fillAmount = 1;
    }
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

    private void OnAttackButtonClick()
    {
        if (combatMode && canAttack)
        {
            Attack();
        }
    }

    private void OnSpecialAttackButtonClick()
    {
        if (combatMode && canAttack && canSpecialAttack)
        {
            canSpecialAttack = false;
            SpecialAttack();
            StartCoroutine(HandleUICD(PlayerUI.Instance.specialAttackUI, Player_Inventory.Instance.equippedWeapon.SpecialAttackCD));
        }
    }

    private void OnUseButtonClick()
    {
        Player_Inventory player_inventory = FindObjectOfType<Player_Inventory>();

        // Pastikan player_inventory ada
        if (player_inventory != null)
        {
            // Cek apakah quickSlot[0] berisi item dan itemUse1 adalah true
            if (player_inventory.quickSlots[0] != null && player_inventory.itemUse1)
            {
                // Gunakan quick slot 1
                Player_Inventory.Instance.UseQuickSlot(1);
                StartCoroutine(HandleUICD(PlayerUI.Instance.itemUseUI, quickSlotCD));
                Debug.Log("Menggunakan item dari quick slot 1");
            }
            // Jika quickSlot[0] kosong atau itemUse1 false, cek quickSlot[1]
            else if (player_inventory.quickSlots[1] != null)
            {
                // Gunakan quick slot 2
                Player_Inventory.Instance.UseQuickSlot(2);
                StartCoroutine(HandleUICD(PlayerUI.Instance.itemUseUI, quickSlotCD));
                Debug.Log("Menggunakan item dari quick slot 2");
            }
            else
            {
                Debug.Log("Tidak ada item yang bisa digunakan.");
            }
        }
        else
        {
            Debug.LogError("Player_Inventory tidak ditemukan.");
        }
    }


    IEnumerator HandleSpecialAttackCD(float dur)
    {
        yield return new WaitForSeconds(dur);
        canSpecialAttack = true;
    }



    public void ActivateHitboxAndPlayAction(string actionType, int damage, float howLong = 1f, bool AOE = false)
    {
        // 🔹 Pastikan animasi dipanggil lebih dulu
        PlayActionAnimation(actionType);

        GameObject activeHitbox = AOE ? specialAttackHitArea : normalAttackHitArea;
        activeHitbox.transform.position = face.position;
        activeHitbox.name = damage.ToString();

        activeHitbox.SetActive(true);
        StartCoroutine(DeactivateHitboxAfterAnimation(activeHitbox, howLong));

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
        Dictionary<string, System.Action<GameObject>> damageActions = new Dictionary<string, System.Action<GameObject>>()
        {
            { "Kapak", obj => obj.GetComponent<TreeBehavior>()?.TakeDamage(damage) },
            { "PickAxe", obj => obj.GetComponent<StoneBehavior>()?.TakeDamage(damage) },
            { "Sabit", obj => obj.GetComponent<PlantSeed>()?.Harvest() },
            {"Sword", obj => obj.GetComponent<Enemy_Health>()?.TakeDamage(damage) }
        };

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(face.position, 0.5f);
        foreach (Collider2D obj in hitObjects)
        {
            if (damageActions.ContainsKey(actionType) && obj.CompareTag(actionType == "Kapak" ? "Tree" :
                                                                       actionType == "PickAxe" ? "Stone" :
                                                                       actionType == "Sabit" ? "Plant" : 
                                                                       actionType == "Sword"? "Bandit": ""))
            {
                Debug.Log($"{obj.name} terkena {actionType} dengan damage: {damage}");
                damageActions[actionType]?.Invoke(obj.gameObject);
            }
        }
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
        Item itemToAttack = Player_Inventory.Instance.equippedWeapon;
        if (itemToAttack.itemName == "Empty")
            return;

        if (itemToAttack.type == ItemType.Melee_Combat)
        {
            // Memanggil suara pedang ketika serangan normal dengan pedang
            //if (SoundManager.Instance != null)
            //    SoundManager.Instance.PlaySound("Sword");

            print("melee normal attacking");
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
                    if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
                    {
                        Debug.Log($"Damage: {itemToAttack.Damage}");
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;
                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);
                        //PlayActionAnimation(itemToAttack.itemName);

                        Debug.Log("Kapak dijalankan dengan hitbox.");
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }
                    break;

                case "PickAxe":
                    if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
                    {
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;

                        Debug.Log($"Damage: {itemToAttack.Damage}");
                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);

                        Debug.Log("PickAxe dijalankan dengan hitbox.");
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }
                    break;

                case "Sabit":
                    if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
                    {
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;
                        Debug.Log($"Damage: {itemToAttack.Damage}");

                        //Memanggil ActiveHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);

                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }

                    break;
                case "Sword":
                    if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
                    {
                        //print("Mengapak tanah");
                        playerPosition = transform.position; // Posisi pemain

                        // Ambil arah dari posisi face
                        faceDirection = face.localPosition.normalized;

                        Debug.Log($"Damage: {itemToAttack.Damage}");
                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitboxAndPlayAction(itemToAttack.itemName, itemToAttack.Damage, 0.5f);

                        Debug.Log("PickAxe dijalankan dengan hitbox.");
                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }

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
            if (Player_Inventory.Instance.equippedWeapon.stackCount == 1)
            {
                Player_Inventory.Instance.EquipItem(ItemPool.Instance.GetItem("Empty"), 1);
            }

            // minus rock count
            Player_Inventory.Instance.RemoveItem(ItemPool.Instance.GetItem("Batu"));

            StartCoroutine(ActivateAttack(.5f));
        }
        else if (itemToAttack.type == ItemType.Ranged_Combat)
        {
            // Check for arrow first
            if (Player_Inventory.Instance.itemList.Exists(x => x.itemName == "Anak Panah"))
            {
                print("shooting arrow");
                // Shoot arrow if possible
                StartCoroutine(ShootProjectile(itemToAttack.RangedWeapon_ProjectilePrefab, itemToAttack.Damage));
                // minus arrow count
                Player_Inventory.Instance.RemoveItem(ItemPool.Instance.GetItem("Anak Panah"));
            }
            else
            {
                print("no arrow bish");
            }
            StartCoroutine(ActivateAttack(1));
        }
    }




    public void SpecialAttack()
    {
       
        Item itemToAttack = Player_Inventory.Instance.equippedWeapon;
        if (itemToAttack.itemName == "Empty")
            return;
        playerHealth.SpendStamina(itemToAttack.UseStamina);
        playerHealth.SpendMaxCurrentStamina(itemToAttack.UseStamina);
        if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
        {
            StartCoroutine(HandleSpecialAttackCD(itemToAttack.SpecialAttackCD));
            if (itemToAttack.itemName == "PenyiramTanaman")
            {


                // SoundManager.Instance.PlaySound("Siram");
                print("watering plants");

                 playerPosition = transform.position; // Posisi pemain

                // Ambil arah dari posisi face
                 faceDirection = face.localPosition.normalized;
                PlayActionAnimation(itemToAttack.itemName);

                // Panggil HoeTile menggunakan playerPosition dan arah face
                farmTile.WaterTile(playerPosition, faceDirection);

            }
            else if (itemToAttack.itemName == "Cangkul")
            {
                print("mencangkul tanah");
                playerPosition = transform.position; // Posisi pemain

                // Ambil arah dari posisi face
                faceDirection = face.localPosition.normalized;
                PlayActionAnimation(itemToAttack.itemName);
                // Panggil HoeTile menggunakan playerPosition dan arah face
                farmTile.HoeTile(playerPosition, faceDirection);
            }
            else if(itemToAttack.itemName == "Kapak")
            {
                print("Mengapak tanah");
                playerPosition = transform.position; // Posisi pemain

                // Ambil arah dari posisi face
                faceDirection = face.localPosition.normalized;
                PlayActionAnimation(itemToAttack.itemName);
                // Panggil HoeTile menggunakan playerPosition dan arah face
            }
            else if (itemToAttack.itemName == "Pedang Ren")
            {

                print("buffing");
                // Buff
                buffParticle.Play();
                StartCoroutine(StartBuff_PedangRen(30));
                StartCoroutine(ActivateAttack(1));

            }
            else if (itemToAttack.itemName == "Ranting Pohon")
            {
                print("special attacking with a stick");
                //ActivateHitbox(itemToAttack.Damage * 4, itemToAttack.AreaOfEffect, 1, true);
                StartCoroutine(ActivateAttack(1));

            }
            else if (itemToAttack.type == ItemType.Melee_Combat)
            {
                print("No Special Attack");
            }
            else if (itemToAttack.itemName == "Batu")
            {
                print("rock no special attack");
            }
            else if (itemToAttack.type == ItemType.Ranged_Combat)
            {
                print("bow special attack");
                for (int i = 0; i < 5; i++)
                {
                    // Check for arrow first
                    if (Player_Inventory.Instance.itemList.Exists(x => x.itemName == "Anak Panah"))
                    {
                        print("shooting arrow");
                        // Shoot arrow if possible
                        StartCoroutine(ShootProjectile(itemToAttack.RangedWeapon_ProjectilePrefab, itemToAttack.Damage, i * .1f));
                        // minus arrow count
                        Player_Inventory.Instance.RemoveItem(ItemPool.Instance.GetItem("Anak Panah"));
                    }
                    else
                    {
                        print("no arrow bish");
                    }
                }
                StartCoroutine(ActivateAttack(1));
            }
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
    IEnumerator StartBuff_PedangRen(float dur)
    {
        damageMult *= 2;
        yield return new WaitForSeconds(dur);
        damageMult /= 2;
    }

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