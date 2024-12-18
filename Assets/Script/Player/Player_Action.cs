using System.Collections;
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
    private Animator animator; // Deklarasikan Animator
    [SerializeField] private Animator toolsAnimator;
    public SpriteRenderer hitBoxRenderer;

    private Coroutine stopAnimCoroutine;




    #region COMBAT
    [Header("COMBAT")]
    public bool combatMode = false;
    public bool canAttack = true;
    [SerializeField] GameObject normalAttackHitArea;
    [SerializeField] GameObject specialAttackHitArea;

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
        animator = GetComponent<Animator>(); // Inisialisasi animator dengan komponen Animator yang ada pada GameObject
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



    public void ActivateHitbox(int damage, string weaponType, float howLong = 1f, bool AOE = false)
    {
        GameObject activeHitbox = AOE ? specialAttackHitArea : normalAttackHitArea;

        // Tetapkan posisi hitbox ke posisi face
        activeHitbox.transform.position = face.position;

        // Tetapkan nama hitbox untuk menyimpan informasi damage
        activeHitbox.name = damage.ToString();

        // Cek arah wajah dan tentukan animasi berdasarkan arah
        Vector2 faceDirection = face.localPosition.normalized; // Ambil arah wajah dari posisi lokal face
        UpdateAnimation(faceDirection); // Memanggil fungsi untuk mengatur animasi

        // Jalankan animasi tools
        activeHitbox.SetActive(true);
        StartCoroutine(RunToolsAnimationAndDeactivateHitbox(weaponType, faceDirection, activeHitbox, howLong));



        // Cek apakah mengenai objek dengan tag "Tree" atau "Stone"
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(face.position, activeHitbox.transform.localScale.x / 2);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Tree") && weaponType == "Kapak")
            {

                Debug.Log($"Pohon terkena serangan dengan damage: {damage}");
                TreeBehavior tree = obj.GetComponent<TreeBehavior>();
                if (tree != null)
                {
                    tree.TakeDamage(damage);
                }

            }
            else if (obj.CompareTag("Stone") && weaponType == "PickAxe")
            {
                Debug.Log($"Batu terkena serangan dengan damage : {damage}");
                StoneBehavior stone = obj.GetComponent<StoneBehavior>();
                if (stone != null)
                {
                    stone.TakeDamage(damage);
                }
            }
            else if (obj.CompareTag("Plant") && weaponType == "Sabit")
            {
                Debug.Log($"Rumput terkena damage dan akan dipanen");
                PlantSeed plantSeed = obj.GetComponent<PlantSeed>();
                if (plantSeed != null)
                {
                    plantSeed.Harvest();
                }
            }
            else if (obj.CompareTag("Animal"))
            {
                Debug.Log($"Hewan terkena serangan dengan damage: {damage}");
                AnimalBehavior animal = obj.GetComponent<AnimalBehavior>();
                if (animal != null)
                {
                    animal.TakeDamage(damage);  // Memanggil method TakeDamage untuk memberikan damage
                }
            }
            else if (obj.CompareTag("ObjekDestroy") && weaponType == "Kapak")
            {
                // Cek apakah objek memiliki script PrefabItemBehavior
                PrefabItemBehavior prefabItem = obj.GetComponent<PrefabItemBehavior>();
                Debug.Log("ini bernyawa : " + prefabItem.health);
                if (prefabItem != null) // Jika PrefabItemBehavior ada di objek tersebut
                {
                    // Cari item di ItemPool berdasarkan namePrefab dari PrefabItemBehavior
                    Item itemData = ItemPool.Instance.items.Find(item => item.itemName == prefabItem.namePrefab);
                    itemData.health = prefabItem.health;
                    if (itemData != null)
                    {
                        GameObject itemDropPrefab = itemData.prefabItem; // Ambil prefab item dari itemData
                        if (itemDropPrefab != null)
                        {
                            PrefabItemBehavior afterDrop = itemDropPrefab.GetComponent<PrefabItemBehavior>();
                            if(afterDrop != null)
                            {
                                afterDrop.health = prefabItem.health;
                            }
                            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                            // Drop item di posisi objek
                            ItemPool.Instance.DropItem(itemData.itemName, obj.transform.position + offset, itemDropPrefab);
                            //ItemPool.Instance.DropItem(getah.name, transform.position + offset, getah);
                        }
                        else
                        {
                            Debug.LogWarning($"Prefab item untuk {itemData.itemName} tidak ditemukan.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Item dengan nama {prefabItem.namePrefab} tidak ditemukan di ItemPool.");
                    }

                    // Hancurkan objek setelah item di-drop
                    Destroy(obj.gameObject);
                }
                else
                {
                    Debug.LogWarning("ObjekDestroy tidak memiliki script PrefabItemBehavior.");
                }
            }

        }

        // Aktifkan hitbox untuk durasi animasi
        //StartCoroutine(ActivatingHitbox(activeHitbox, animationDuration));
    }

    private IEnumerator RunToolsAnimationAndDeactivateHitbox(string weaponType, Vector2 direction, GameObject activeHitbox, float howLong)
    {
        // Jalankan animasi tool
        float animationDuration = ToolsAnimation(weaponType, direction);

        // Tunggu selama animasi selesai
        yield return new WaitForSeconds(animationDuration);

        // Setelah animasi selesai, nonaktifkan hitbox
        activeHitbox.SetActive(false);
    }


    private float ToolsAnimation(string weaponType, Vector2 direction)
    {
        float animationDuration = 0.4f; // Durasi animasi

        if (weaponType == "PickAxe")
        {
            // Pastikan animasi sebelumnya dimatikan sebelum memulai animasi baru
            if (Vector2.Dot(direction, Vector2.up) > 0.9f) // Mengarah ke atas
            {
                toolsAnimator.SetBool("PickAxeUp", true);
                toolsAnimator.SetBool("PickAxeDown", false);
                toolsAnimator.SetBool("PickAxeRight", false);
                toolsAnimator.SetBool("PickAxeLeft", false);
            }
            else if (Vector2.Dot(direction, Vector2.down) > 0.9f) // Mengarah ke bawah
            {
                toolsAnimator.SetBool("PickAxeUp", false);
                toolsAnimator.SetBool("PickAxeDown", true);
                toolsAnimator.SetBool("PickAxeRight", false);
                toolsAnimator.SetBool("PickAxeLeft", false);
            }
            else if (Vector2.Dot(direction, Vector2.right) > 0.9f) // Mengarah ke kanan
            {
                toolsAnimator.SetBool("PickAxeUp", false);
                toolsAnimator.SetBool("PickAxeDown", false);
                toolsAnimator.SetBool("PickAxeRight", true);
                toolsAnimator.SetBool("PickAxeLeft", false);
                hitBoxRenderer.flipX = false; // Tidak perlu membalikkan sprite, biarkan default
            }
            else if (Vector2.Dot(direction, Vector2.left) > 0.9f) // Mengarah ke kiri
            {
                toolsAnimator.SetBool("PickAxeUp", false);
                toolsAnimator.SetBool("PickAxeDown", false);
                toolsAnimator.SetBool("PickAxeRight", false);
                toolsAnimator.SetBool("PickAxeLeft", true);

                //ubah arah animasi
                hitBoxRenderer.flipX = true;

            }
            return animationDuration; // Mengembalikan durasi animasi untuk menunggu sebelum melanjutkan
        }
        else if (weaponType == "Sword")
        {
            Debug.Log("animasi sword di jalankan");
            if (Vector2.Dot(direction, Vector2.up) > 0.9f) // Mengarah ke atas
            {
                toolsAnimator.SetBool("SwordUp", true);
                toolsAnimator.SetBool("SwordDown", false);
                toolsAnimator.SetBool("SwordRight", false);
                toolsAnimator.SetBool("SwordLeft", false);
                hitBoxRenderer.flipY = true;
                Debug.Log("animasi sword di jalankan atas");
            }
            else if (Vector2.Dot(direction, Vector2.down) > 0.9f) // Mengarah ke bawah
            {
                toolsAnimator.SetBool("SwordUp", false);
                toolsAnimator.SetBool("SwordDown", true);
                toolsAnimator.SetBool("SwordRight", false);
                toolsAnimator.SetBool("SwordLeft", false);
                hitBoxRenderer.flipY = false;
                Debug.Log("animasi sword di jalankan bawah");
            }
            else if (Vector2.Dot(direction, Vector2.right) > 0.9f) // Mengarah ke kanan
            {
                toolsAnimator.SetBool("SwordUp", false);
                toolsAnimator.SetBool("SwordDown", false);
                toolsAnimator.SetBool("SwordRight", true);
                toolsAnimator.SetBool("SwordLeft", false);
                hitBoxRenderer.flipX = false; // Tidak perlu membalikkan sprite, biarkan default
                Debug.Log("animasi sword di jalankan kanan");
            }
            else if (Vector2.Dot(direction, Vector2.left) > 0.9f) // Mengarah ke kiri
            {
                toolsAnimator.SetBool("SwordUp", false);
                toolsAnimator.SetBool("SwordDown", false);
                toolsAnimator.SetBool("SwordRight", false);
                toolsAnimator.SetBool("SwordLeft", true);

                //ubah arah animasi
                hitBoxRenderer.flipX = true;
                Debug.Log("animasi sword di jalankan kiri");

            }
            return animationDuration; // Mengembalikan durasi animasi untuk menunggu sebelum melanjutkan
        }
        return 0;
    }

    // Fungsi untuk mengatur animasi berdasarkan arah wajah
    private void UpdateAnimation(Vector2 direction)
    {
        Debug.Log("logika animation di jalankan ");
        Debug.Log("direction: " + direction);
        float animationDuration = 0.4f; // Perpanjang durasi agar lebih mudah terlihat

        // Cancel the previous animation stop coroutine if it exists
        if (stopAnimCoroutine != null)
        {
            StopCoroutine(stopAnimCoroutine);
        }

        // Play animation based on the direction
        if (Vector2.Dot(direction, Vector2.up) > 0.9f) // Mengarah ke atas
        {
            Debug.Log("atas");
            animator.SetBool("ActionTop", true);
            animator.SetBool("ActionDown", false);
            animator.SetBool("ActionLeft", false);
            animator.SetBool("ActionRight", false);
            stopAnimCoroutine = StartCoroutine(StopAnimationAfterDelay(animationDuration));
        }
        else if (Vector2.Dot(direction, Vector2.down) > 0.9f) // Mengarah ke bawah
        {
            Debug.Log("bawah");
            animator.SetBool("ActionTop", false);
            animator.SetBool("ActionDown", true);
            animator.SetBool("ActionLeft", false);
            animator.SetBool("ActionRight", false);
            stopAnimCoroutine = StartCoroutine(StopAnimationAfterDelay(animationDuration));
        }
        else if (Vector2.Dot(direction, Vector2.left) > 0.9f) // Mengarah ke kiri
        {
            Debug.Log("kiri");
            animator.SetBool("ActionTop", false);
            animator.SetBool("ActionDown", false);
            animator.SetBool("ActionLeft", true);
            animator.SetBool("ActionRight", false);
            stopAnimCoroutine = StartCoroutine(StopAnimationAfterDelay(animationDuration));
        }
        else if (Vector2.Dot(direction, Vector2.right) > 0.9f) // Mengarah ke kanan
        {
            Debug.Log("kanan");
            animator.SetBool("ActionTop", false);
            animator.SetBool("ActionDown", false);
            animator.SetBool("ActionLeft", false);
            animator.SetBool("ActionRight", true);
            stopAnimCoroutine = StartCoroutine(StopAnimationAfterDelay(animationDuration));
        }
    }

    private IEnumerator StopAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool("ActionTop", false);
        animator.SetBool("ActionDown", false);
        animator.SetBool("ActionLeft", false);
        animator.SetBool("ActionRight", false);
        toolsAnimator.SetBool("SwordDown", false);
        toolsAnimator.SetBool("PickAxeDown", false);
        toolsAnimator.SetBool("PickAxeRight", false);
        toolsAnimator.SetBool("PickAxeLeft", false);
        toolsAnimator.SetBool("SwordUp", false);
        toolsAnimator.SetBool("SwordDown", false);
        toolsAnimator.SetBool("SwordRight", false);
        toolsAnimator.SetBool("SwordLeft", false);
    }






    public void Attack()
    {
        Item itemToAttack = Player_Inventory.Instance.equippedWeapon;
        if (itemToAttack.itemName == "Empty")
            return;

        if (itemToAttack.type == ItemType.Melee_Combat)
        {
            // Memanggil suara pedang ketika serangan normal dengan pedang
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("Sword");

            print("melee normal attacking");
            Debug.Log("nama item yang sedan di pakai" + itemToAttack.itemName);
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

                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitbox(itemToAttack.Damage, "Kapak");

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
                        Debug.Log($"Damage: {itemToAttack.Damage}");

                        // Memanggil ActivateHitbox tanpa parameter area
                        ActivateHitbox(itemToAttack.Damage, "PickAxe");

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
                        Debug.Log($"Damage: {itemToAttack.Damage}");

                        //Memanggil ActiveHitbox tanpa parameter area
                        ActivateHitbox(itemToAttack.Damage, "Sabit");

                    }
                    else
                    {
                        Debug.Log("Stamina tidak mencukupi untuk menyerang.");
                    }

                    break;
                case "Sword":
                    if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
                    {
                        Debug.Log($"Damage: {itemToAttack.Damage}");

                        //Memanggil ActiveHitbox tanpa parameter area
                        ActivateHitbox(itemToAttack.Damage, "Sword");

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

        if (Player_Health.Instance.SpendStamina(itemToAttack.SpecialAttackStamina))
        {
            StartCoroutine(HandleSpecialAttackCD(itemToAttack.SpecialAttackCD));
            if (itemToAttack.itemName == "Penyiram Tanaman")
            {


                // SoundManager.Instance.PlaySound("Siram");
                print("watering plants");

                Vector3 playerPosition = transform.position; // Posisi pemain

                // Ambil arah dari posisi face
                Vector3 faceDirection = face.localPosition.normalized;

                // Panggil HoeTile menggunakan playerPosition dan arah face
                farmTile.WaterTile(playerPosition, faceDirection);

            }
            else if (itemToAttack.itemName == "Cangkul")
            {
                print("mencangkul tanah");
                Vector3 playerPosition = transform.position; // Posisi pemain

                // Ambil arah dari posisi face
                Vector3 faceDirection = face.localPosition.normalized;

                // Panggil HoeTile menggunakan playerPosition dan arah face
                farmTile.HoeTile(playerPosition, faceDirection);
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