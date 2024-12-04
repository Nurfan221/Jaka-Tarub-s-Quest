using System.Collections;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class Player_Action : MonoBehaviour
{
    public static Player_Action Instance;

    #region KEYBINDINGS
    KeyCode actionInput = KeyCode.F;
    

    KeyCode quickSlot1 = KeyCode.Q;
    KeyCode quickSlot2 = KeyCode.E;

    #endregion

    #region COMBAT
    [Header("COMBAT")]
    public bool combatMode = false;
    public bool canAttack = true;
    [SerializeField] GameObject normalAttackHitArea;
    [SerializeField] GameObject specialAttackHitArea;

    float specialAttackTimer;
    bool canSpecialAttack = true;

    [SerializeField] GameObject swordFX;
    [SerializeField] GameObject swordAOEFX;
    [SerializeField] ParticleSystem swordParticle;
    [SerializeField] ParticleSystem swordAOEParticle;
    [SerializeField] ParticleSystem tombakParticle;

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
    [SerializeField] bool drawInteractCircle;
    [SerializeField] LayerMask interactablesLayer;
    [SerializeField] float interactsRadius = 2f;
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

        if (buttonUse!= null)
        {
            buttonUse.onClick.AddListener(OnUseButtonClick);
        }

      PlayerUI playerUI = FindObjectOfType<PlayerUI>();

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
    }

    void Update()
    {
        
        {
            #region INPUTS_ACTION
            // Main Action (Attacking)
            // Secondary Action (Sepcial Attacking)



           

            // Quick slots
            HandleQuickSLotUI(0);
            if (Input.GetKeyDown(quickSlot1) && canQuickSlots[0])
            {
                // quick slot 2
                if (Player_Inventory.Instance.quickSlots[0] != null)
                {
                    Player_Inventory.Instance.UseQuickSlot(1);
                    // StartCoroutine(HandleUICD(PlayerUI.Instance.quickSlotsUI_HUD[0], quickSlotCD));
                }
            }
            HandleQuickSLotUI(1);
            if (Input.GetKeyDown(quickSlot2) && canQuickSlots[1])
            {
                // quick slot 2
                if (Player_Inventory.Instance.quickSlots[1] != null)
                {
                    Player_Inventory.Instance.UseQuickSlot(2);
                    // StartCoroutine(HandleUICD(PlayerUI.Instance.quickSlotsUI_HUD[1], quickSlotCD));
                }
            }

            // Interact action (for interacting with environment)
            canInteract = CheckInteractables();
            // jadi logikanya cek dulu di sekitar ada yang bisa di interact gak
            if (Input.GetKeyDown(actionInput))
            {
                // terus kalo udah dicek, baru bisa pencet interact
                if (canInteract)
                {
                    interactable.BaseInteract();
                }
            }
            #endregion
        }
    }

    // cek apakah player bersentuhan dengan tanaman
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang disentuh memiliki tag "itemDrop"
        if (other.CompareTag("ItemDrop"))
        {
            // Ambil data item dari komponen ItemDrop
            ItemDropInteractable itemDrop = other.GetComponent<ItemDropInteractable>();
            if (itemDrop != null)
            {
                string itemName = itemDrop.itemName;
                // Tambahkan item ke inventory pemain
                Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(itemDrop.itemName));

                // Hancurkan item setelah diambil
                Destroy(other.gameObject);
            }
            else
            {
                Debug.LogWarning("Komponen ItemDrop tidak ditemukan di objek dengan tag 'itemDrop'.");
            }
        }
    }



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
    bool CheckInteractables()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, interactsRadius, Vector2.up, 0, interactablesLayer);
        if (hit.transform != null)
        {
            interactable = hit.transform.GetComponent<Interactable>();

            if (interactable != null)
            {
                if (interactable is PlantInteractable plantInteractable)
                {
                    // Check if the seed is ready to be watered
                    // if (plantInteractable.seedInteractable.siram)
                    // {
                    //     PlayerUI.Instance.promptText.text = " klik kanan untuk " + interactable.promptMessage;
                    // }
                     if (plantInteractable.plantSeed != null && !plantInteractable.plantSeed.isReadyToHarvest)
                        {
                            PlayerUI.Instance.promptText.text = interactable.promptMessage;
                        }
                        else
                        {
                            PlayerUI.Instance.promptText.text = "Tekan untuk " + interactable.promptMessage;
                        }

                }
                else
                {
                    PlayerUI.Instance.promptText.text = "Tekan untuk " + interactable.promptMessage;
                }

                return true;
            }
        }

        PlayerUI.Instance.promptText.text = string.Empty;
        return false;
    }

    public void OnActionInputButtonClick(){
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



    public void ActivateHitbox(int damage, string weaponType, float howLong = 0.5f, bool AOE = false)
    {
        // Tetapkan ukuran hitbox tetap
        float defaultHitboxSize = 1.5f; // Ubah sesuai kebutuhan

        GameObject activeHitbox = AOE ? specialAttackHitArea : normalAttackHitArea;

        // Tetapkan posisi hitbox ke posisi face
        activeHitbox.transform.position = face.position;

        // Ubah skala hitbox menggunakan ukuran default
        activeHitbox.transform.localScale = new Vector3(defaultHitboxSize, defaultHitboxSize, 1);

        // Tetapkan nama hitbox untuk menyimpan informasi damage
        activeHitbox.name = damage.ToString();

        // Cek apakah mengenai objek dengan tag "Tree" atau "Stone"
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(face.position, defaultHitboxSize / 2);
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
            else if (obj.CompareTag("Plant") && weaponType =="Sabit")
            {
                Debug.Log($"rumput terkena damage dan akan di panen");
                PlantSeed plantSeed = obj.GetComponent<PlantSeed>();
                if(plantSeed != null )
                {
                    plantSeed.Harvest();
                }
            }
        }

        // Aktifkan hitbox untuk durasi tertentu
        StartCoroutine(ActivatingHitbox(activeHitbox, howLong));
    }





    IEnumerator ActivatingHitbox(GameObject hitbox, float duration)
    {
        // Aktifkan hitbox
        hitbox.SetActive(true);

        // Tunggu selama durasi tertentu
        yield return new WaitForSeconds(duration);

        // Nonaktifkan hitbox
        hitbox.SetActive(false);
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

            }else if (itemToAttack.itemName == "Cangkul")
            {
                print("mencangkul tanah");
                Vector3 playerPosition = transform.position; // Posisi pemain

                // Ambil arah dari posisi face
                Vector3 faceDirection = face.localPosition.normalized;

                // Panggil HoeTile menggunakan playerPosition dan arah face
                farmTile.HoeTile(playerPosition, faceDirection);
            }else if (itemToAttack.itemName == "Pedang Ren")
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

    #region DEBUG
    private void OnDrawGizmos()
    {
        if (drawInteractCircle)
        {
            Gizmos.DrawWireSphere(transform.position, interactsRadius);
        }
    }
    #endregion
}