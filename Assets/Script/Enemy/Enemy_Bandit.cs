using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BanditState
{
    Idle,
    Patroli,
    KejarTarget,
    Serang,
    Kabur,
    Mati
}
public class Enemy_Bandit : MonoBehaviour
{
    [Tooltip("Jarak hitbox dari pusat NPC")]
    public Transform hitboxTransform;
    [Tooltip("Geser titik pusat lingkaran ke atas (agar sejajar badan, bukan kaki)")]


    [Header("State Machine")]
    public BanditState currentState = BanditState.Idle;

    [Header("Sensor Mata")]
    public float detectionRadius = 5f; // Tetap pakai float biar ringan
    public LayerMask targetLayer;

    [Header("References")]
    public float jedaAnimasi = 3f;
    public bool isMoving;
    public float moveSpeed;
    public bool isDead;
    public Rigidbody2D rb; // Tambahkan referensi Rigidbody2D
    public List<Vector2> rutePatroli = new List<Vector2>(4);
    public Vector2 movementDirection;
    public Vector2 lastDirection;
    private Coroutine roamingCoroutine;
    public List<Item> lootTable = new List<Item>();
    public int minLoot = 1;
    public int maxLoot = 3;
    [Header("Death Settings")]
    public Collider2D myCollider;
    public GameObject spawnerReference;


    [Header("Logika Serangan")]
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    public float wanderRadius = 4;
    public int attackDamage = 10;
    public bool isTakingDamage = false;
    public bool isAttackReady = true;
    public bool isAttackInProgress = false;

    public Transform zonaSerangTransform;

    [Header("Komponen Serangan")]

    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)
    public float hitboxRadius = 1.0f;
    public Transform currentTarget;

    [Header("Animator Parts")]
    public Animator spriteBadan;
    public List<Animator> layerAnimators = new List<Animator>();

    
    [Header("Settings")]
    public float hearingDistance = 15f; // Jarak maksimal suara terdengar (sesuaikan dengan ukuran layar)

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Pastikan ambil komponen fisik
        roamingCoroutine = StartCoroutine(ThinkProcess());
        StartCoroutine(DetectTargetRoutine());
    }

    private void Update()
    {
        UpdateZonaSerangPosition();

        // Pastikan punya target & logic siap
        if (currentTarget != null && isAttackReady && !isDead)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);

            if (distance > hitboxRadius)
            {
                // jika target tidak null dan jarak lebih dari hitboxRadius kejar
                if (currentState != BanditState.KejarTarget)
                {
                    currentState = BanditState.KejarTarget;
                    isMoving = true;
                }
            }
            else
            {
                // Pastikan ubah state dulu sebelum serang
                currentState = BanditState.Serang;
                isMoving = false;
                roamingCoroutine = null; // Pastikan coroutine roaming dihentikan
                //  Matikan gerak fisik
                rb.linearVelocity = Vector2.zero;

              
                JalankanLogikaSerangan();
            }

            // Cek apakah target sudah jauh banget (lepas dari deteksi)
            if (distance > detectionRadius + 3f)
            {
                // Reset ke Idle / Patroli
                currentTarget = null;
                currentState = BanditState.Idle;
                spriteBadan.Play("Idle");
                isMoving = false;
                rb.linearVelocity = Vector2.zero;

                if (roamingCoroutine == null)
                {
                    roamingCoroutine = StartCoroutine(ThinkProcess());
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (currentState == BanditState.KejarTarget && currentTarget != null)
        {
            ChaseTargetFixedUpdate();
        }
    }

    private void LateUpdate()
    {
        SyncVisuals();
    }

    void SyncVisuals()
    {
        if (spriteBadan == null || layerAnimators.Count == 0) return;

        AnimatorStateInfo masterState = spriteBadan.GetCurrentAnimatorStateInfo(0);

        int masterHash = masterState.fullPathHash; // ID Animasi (misal: Run_Right)
        float masterTime = masterState.normalizedTime; // Waktu jalan (0.0 - 1.0)

        // terapkan ke semua SLAVE (Baju, dll)
        foreach (Animator anim in layerAnimators)
        {
            if (anim == null) continue;

            AnimatorStateInfo slaveState = anim.GetCurrentAnimatorStateInfo(0);

            // Cek apakah Slave berbeda dengan Master?
            // Beda State ATAU Beda Waktu lebih dari toleransi kecil
            bool isDifferentState = slaveState.fullPathHash != masterHash;
            bool isTimeDesynced = Mathf.Abs(slaveState.normalizedTime - masterTime) > 0.02f; // Toleransi 0.02 detik

            if (isDifferentState || isTimeDesynced)
            {
                // Paksa slave mainkan animasi Master di waktu yang sama persis
                anim.Play(masterHash, 0, masterTime);
            }
        }
    }
    private IEnumerator ThinkProcess()
    {
        while (true) // Loop selamanya
        {
         
            if (currentState == BanditState.KejarTarget || currentState == BanditState.Serang)
            {
                yield return null; // Tunggu 1 frame, lalu cek lagi (Looping idle)
                continue; 
            }

            BanditState nextAction = GetRandomNextState(currentState);
            currentState = nextAction;
            if (nextAction == BanditState.Patroli)
            {
                yield return StartCoroutine(DoWandering());
            }
            else
            {
                yield return StartCoroutine(DoIdle());
            }
        }
    }

    public BanditState GetRandomNextState(BanditState stateSaatIni)
    {
        int randomIndex = Random.Range(0, 2); // Hasilnya 0 atau 1

        if (randomIndex == 0) return BanditState.Idle;
        else return BanditState.Patroli;
    }
    public IEnumerator DoIdle()
    {
        Debug.Log($"Bandit memutuskan untuk beristirahat sejenak.");
        isMoving = false;
        spriteBadan.Play("Idle");
        float idleDuration = Random.Range(4f, 6f);
        yield return new WaitForSeconds(idleDuration);
    }
    public IEnumerator DoWandering()
    {
        Debug.Log($"Bandit memutuskan untuk Patroli.");

        rutePatroli.Clear();
        int jumlahTitik = Random.Range(1, 4);
        Vector2 titikAcuan = rb.position;

        for (int i = 0; i < jumlahTitik; i++)
        {
            Vector2 titikBaru = GetRandomPoint(titikAcuan);
            rutePatroli.Add(titikBaru);
            titikAcuan = titikBaru;
        }

        isMoving = true;

        foreach (var rute in rutePatroli)
        {
            if (currentState != BanditState.Patroli)
            {
                yield break; 
            }

            // Jalankan pergerakan
            yield return StartCoroutine(MoveToTargetWithPhysics(rute, moveSpeed));

            // Setelah sampai di satu titik, cek lagi (barangkali berubah pas di jalan)
            if (currentState != BanditState.Patroli)
            {
                yield break; 
            }
        }

        isMoving = false;

        // Cek lagi sebelum idle
        if (currentState == BanditState.Patroli)
        {
            if (spriteBadan != null) spriteBadan.Play("Idle");
            yield return new WaitForSeconds(1f);
        }
    }
    private IEnumerator MoveToTargetWithPhysics(Vector2 targetPosition, float speed)
    {
        float stuckTimer = 0f;
        float timeToConsiderStuck = 1.0f;
        Vector2 previousFramePosition = rb.position;

        while (Vector2.Distance(rb.position, targetPosition) > 0.5f)
        {

            if (currentState != BanditState.Patroli)
            {
                rb.linearVelocity = Vector2.zero; // Rem mendadak
                yield break; // Hentikan coroutine ini
            }
            
            //perhitungan arah yang diinginkan
            Vector2 directionToTarget = (targetPosition - rb.position).normalized;

            //  paksa variabel ini berisi arah "Niat", bukan hasil fisika.
            // Walaupun menabrak tembok, directionToTarget tetap bernilai 1 (tetap berusaha maju).
            this.movementDirection = directionToTarget;

            // dorongan fisika
            rb.linearVelocity = directionToTarget * speed;

            yield return null;

            // logika stak (Tetap pakai posisi asli untuk mendeteksi macet)
            Vector2 currentPos = rb.position;
            float distanceMoved = (currentPos - previousFramePosition).magnitude;

            // Panggil update animasi
            UpdateAnimationParameters();

            if (distanceMoved < (speed * Time.deltaTime * 0.1f))
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > timeToConsiderStuck)
                {
                    Debug.Log($"Bandit NYETUCK! Membatalkan jalur ini.");
                    rb.linearVelocity = Vector2.zero;
                    break;
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            previousFramePosition = currentPos;
        }

        rb.linearVelocity = Vector2.zero;
        this.movementDirection = Vector2.zero; // Reset ke 0 agar kembali Idle
        UpdateAnimationParameters();
    }

    private Vector2 GetRandomPoint(Vector2 lastPosition)
    {
        float randomX = Random.Range(-5f, 5f);
        float randomY = Random.Range(-5f, 5f);
        return lastPosition + new Vector2(randomX, randomY);
    }

    public void UpdateAnimationParameters()
    {
        if (currentState == BanditState.Mati || isDead) return;
        if (spriteBadan == null) return;
        if (isTakingDamage) return;
        if (isAttackInProgress) return;

      
        // nilainya akan stabil meskipun fisik musuh tertahan tembok.
        Vector2 inputGerak = this.movementDirection;

        // Gunakan threshold sangat kecil karena inputGerak adalah hasil normalized (pasti 0 atau 1)
        bool isMoving = inputGerak.magnitude > 0.01f;

        float speed = 0f;

        if (isMoving)
        {
            speed = 1f;

            // Input gerak sudah normalized dari coroutine, jadi aman langsung dipakai
            lastDirection = inputGerak;

            // Tidak perlu Normalisasi lagi karena directionToTarget sudah normalized
            SetAnimParameters(spriteBadan, inputGerak.x, inputGerak.y, speed);
        }
        else
        {
            speed = 0f;
            SetAnimParameters(spriteBadan, lastDirection.x, lastDirection.y, speed);
        }

        // Sinkronisasi ke Layer Animator lain (jika ada baju/senjata)
        foreach (Animator anim in layerAnimators)
        {
            if (anim == null) continue;

            anim.SetFloat("MoveX", spriteBadan.GetFloat("MoveX"));
            anim.SetFloat("MoveY", spriteBadan.GetFloat("MoveY"));
            anim.SetFloat("IdleX", spriteBadan.GetFloat("IdleX"));
            anim.SetFloat("IdleY", spriteBadan.GetFloat("IdleY"));
            anim.SetFloat("Speed", speed);
        }
    }

    void SetAnimParameters(Animator anim, float x, float y, float speed)
    {
        anim.SetFloat("MoveX", Mathf.Round(x));
        anim.SetFloat("MoveY", Mathf.Round(y));
        anim.SetFloat("IdleX", Mathf.Round(lastDirection.x)); // Pastikan idle direction juga terupdate
        anim.SetFloat("IdleY", Mathf.Round(lastDirection.y));
        anim.SetFloat("Speed", speed);
    }


    private IEnumerator DetectTargetRoutine()
    {
        //  scan setiap 0.5 detik 
        WaitForSeconds scanInterval = new WaitForSeconds(0.5f);

        while (true)
        {

            if ( currentTarget == null)
            {
                FindClosestTarget();
            }

            yield return scanInterval;
        }
    }

    private void FindClosestTarget()
    {
        // Cari semua objek di dalam lingkaran radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);

        float closestDistance = Mathf.Infinity;
        Transform potentialTarget = null;

        foreach (Collider2D hit in hits)
        {
            // Hindari mendeteksi diri sendiri
            if (hit.gameObject == this.gameObject) continue;

            bool isValidTarget = false;

            // Jika Saya Agresif Cari Player ATAU Hewan Pasif
            if (hit.CompareTag("Player") || hit.CompareTag("Animal"))
            {
                isValidTarget = true;
            }
            
           

            if (isValidTarget)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);

                // Jika ini lebih dekat dari kandidat sebelumnya, simpan ini
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    potentialTarget = hit.transform;
                }
            }
        }

        // Jika mata berhasil menemukan target yang valid
        if (potentialTarget != null)
        {
            currentTarget = potentialTarget;

            // Ubah state jadi mengejar
            currentState = BanditState.KejarTarget;

            // Hentikan jalan-jalan santai (Wandering)
            if (roamingCoroutine != null)
            {
                StopCoroutine(roamingCoroutine);
                roamingCoroutine = null;
            }
        }
    }
   
    private void UpdateZonaSerangPosition()
    {
        // Safety check
        if (zonaSerangTransform == null) return;

        // Tentukan Titik Pusat Rotasi (Misal: Dada/Kepala hewan)
        Vector3 centerPoint = transform.position + new Vector3(0, verticalOffset, 0);

        Vector2 directionToLook;

        //Jika punya target, pandangan (Zona Serang) KUNCI ke Target!
        if (currentTarget != null)
        {
            // Hitung arah dari Saya ke Musuh
            directionToLook = (currentTarget.position - transform.position).normalized;

            // Update lastDirection agar saat musuh mati, kita tetap menghadap ke sana
            lastDirection = directionToLook;
        }
        // Jika tidak ada target tapi sedang jalan, ikut arah jalan
        else if (movementDirection.magnitude > 0.1f)
        {
            directionToLook = movementDirection.normalized;
            lastDirection = directionToLook;
        }
        //  Jika diam dan tidak ada target, pakai arah terakhir
        else
        {
            // Pastikan tidak error jika lastDirection 0 (default kanan)
            if (lastDirection == Vector2.zero) lastDirection = Vector2.right;
            directionToLook = lastDirection;
        }



        Vector3 orbitPosition = centerPoint + (Vector3)(directionToLook * hitboxRadius);

        // Terapkan Posisi
        zonaSerangTransform.position = orbitPosition;

        float angle = Mathf.Atan2(directionToLook.y, directionToLook.x) * Mathf.Rad2Deg;
        zonaSerangTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void JalankanLogikaSerangan()
    {
      
        if (currentTarget != null && isAttackReady)
        {

            // Cek Cooldown
            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            // Stop gerakan agar tidak "sliding" saat memukul
            isMoving = false;
            isAttackReady = false;
            isAttackInProgress = true;
            rb.linearVelocity = Vector2.zero; // Pastikan fisik berhenti total
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            // Jalankan Animasi
            TriggerSound();
            PlayActionAnimation_NoWait("Sword");
            //spriteBadan.Play("SwordAtas");

        }
    }

    public void TriggerSound()
    {

        // Ambil posisi Kamera Utama
        Vector3 cameraPos = Camera.main.transform.position;
        // Ambil posisi Karakter ini
        Vector3 myPos = transform.position;

        // Hitung Jarak (Abaikan sumbu Z karena ini game 2D)
        cameraPos.z = 0;
        myPos.z = 0;

        float distance = Vector3.Distance(myPos, cameraPos);

        //  Kalau kejauhan, BATALKAN suara.
        if (distance > hearingDistance)
        {
            return; // Stop di sini, jangan mainkan suara
        }
        SoundManager.Instance.PlaySound(SoundName.SwordSfx);


    }

    public void PlayActionAnimation_NoWait(string actionType)
    {
        if (spriteBadan == null) return;

        // Reset speed agar tidak lari di tempat
        spriteBadan.SetFloat("Speed", 0f);

        // Tentukan Nama Trigger
        string triggerName = actionType;
        if (actionType != "Die")
        {
            // Hanya jalankan logika arah jika BUKAN animasi mati
            if (Mathf.Abs(lastDirection.y) > Mathf.Abs(lastDirection.x))
            {
                if (lastDirection.y > 0) triggerName += "Atas";
                else triggerName += "Bawah";
            }
            else
            {
                if (lastDirection.x > 0) triggerName += "Kanan";
                else triggerName += "Kiri";
            }
        }

    
        // Ini memastikan kondisi bersih di setiap serangan.
        spriteBadan.ResetTrigger(triggerName);
        foreach (Animator anim in layerAnimators)
        {
            if (anim != null) anim.ResetTrigger(triggerName);
        }

        spriteBadan.SetTrigger(triggerName);
        foreach (Animator anim in layerAnimators)
        {
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetTrigger(triggerName);
            }
        }

        UpdateAttackPosition();

        // Jangan andalkan Animation Event untuk reset status 'isAttackInProgress'
        if (actionType != "Die")
        {
            StartCoroutine(ResetAttackStateRoutine());
        }
    }

    private IEnumerator ResetAttackStateRoutine()
    {
        yield return null;

        // (Pastikan kita ambil durasi NextState jika sedang transisi, atau CurrentState)
        AnimatorStateInfo info = spriteBadan.GetCurrentAnimatorStateInfo(0);
        if (spriteBadan.IsInTransition(0))
        {
            info = spriteBadan.GetNextAnimatorStateInfo(0);
        }

        // Tunggu sampai animasi SELESAI
        yield return new WaitForSeconds(info.length);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // BARU lepaskan kunci logika
        isAttackInProgress = false;
        isAttackReady = true;

        // Update parameter sekali lagi agar langsung transisi ke Idle/Run
        UpdateAnimationParameters();
    }
    // Fungsi tambahan untuk memindah posisi hitbox (zonaSerangTransform)
    void UpdateAttackPosition()
    {
        if (zonaSerangTransform == null) return;

        // Jarak hitbox dari pusat badan
        float distance = 1.0f;

        // Pindahkan hitbox ke arah hadap terakhir
        zonaSerangTransform.localPosition = lastDirection * distance;
    }

    public void PerformAttack()
    {
        Debug.Log("Pedang mendarat! Mengecek hit...");

        Collider2D[] hits = Physics2D.OverlapCircleAll(zonaSerangTransform.position, hitboxRadius, targetLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            if (hit.CompareTag("Player"))
            {
                Player_Health playerHealth = hit.GetComponent<Player_Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage, this.transform);
                }
            }
            else if (hit.CompareTag("Animal"))
            {
                AnimalBehavior prey = hit.GetComponent<AnimalBehavior>();
                if (prey != null && prey.tipeHewan == AnimalType.Pasif)
                {
                    prey.TakeDamage(attackDamage);
                }
            }
        }
     
    }

  
    private void ChaseTargetFixedUpdate()
    {
        // Pastikan target masih ada
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }



        Vector2 direction = (currentTarget.position - transform.position).normalized;

        // Update arah hadap (Animation Parameters)
        if (Mathf.Abs(direction.x) > 0.1f)
            SetMovementDirection(direction); // Fungsi helper arah anda
        else
            SetMovementDirection(new Vector2(0, direction.y));

        UpdateAnimationParameters(); // Sinkronisasi animasi jalan

        // Gerakkan Fisik
        rb.linearVelocity = direction * moveSpeed;
        isMoving = true;
    }
    public void SetMovementDirection(Vector2 direction)
    {
        this.movementDirection = direction;
        if (direction.magnitude > 0.1f)
        {
            this.lastDirection = direction.normalized; // Simpan arah yang sudah dinormalisasi
        }

    }
    public void SetKnockbackStatus(bool status, Transform attackerPosition)
    {
        // Jika status true (sedang knockback), kita anggap seperti TakingDamage
        isTakingDamage = status;

        if (status == true)
        {
            // Matikan logic gerak 
            isMoving = false;

            // Jika dia sedang ancang-ancang nyerang, batalkan!
            isAttackInProgress = false;
            StopCoroutine("ResetAttackStateRoutine"); // Matikan timer serangan jika ada

          
            // Buka kunci posisi (FreezeAll) agar bisa didorong oleh Knockback!
            // Kembalikan ke FreezeRotation saja (agar tidak muter-muter)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Reset velocity (biar dorongannya fresh dari 0)
            rb.linearVelocity = Vector2.zero;

            KnockbackSystem knockback = GetComponent<KnockbackSystem>();

            if (knockback != null && attackerPosition != null)
            {
                knockback.PlayKnockback(attackerPosition, () =>
                {
                    Debug.Log("Knockback selesai via Callback");
                    isTakingDamage = false;
                    PlayerController.Instance.ActivePlayer.Movement.ifDisturbed = false;
                    GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                });
            }
        }
    }


    public void Die()
    {
        if (isDead) return; // Mencegah mati 2 kali
        isDead = true;
        currentState = BanditState.Mati;


        // Matikan Semua Logika & Fisika
        isMoving = false;
        isAttackReady = false;

        // Stop Coroutine Pikir & Jalan
        StopAllCoroutines();

        // Matikan Fisika (Biar tidak didorong-dorong lagi)
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // Kunci di tempat

        // Matikan Collider (Supaya player bisa jalan melewati mayat)
        if (myCollider != null) myCollider.enabled = false;
        else GetComponent<Collider2D>().enabled = false;

        
        // Pastikan Anda punya trigger "Die" di Animator Controller
        PlayActionAnimation_NoWait("Die");

        // Mulai Proses Menghilang (Tunggu 2 detik, lalu fade out)
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // Tunggu mayat tergeletak sebentar (misal 2 detik)
        yield return new WaitForSeconds(2f);

        float fadeDuration = 1.5f;
        float timer = 0f;

        // Kumpulkan semua sprite renderer (Badan + Baju + Celana + dll)
        List<SpriteRenderer> allSprites = new List<SpriteRenderer>();
        allSprites.Add(GetComponent<SpriteRenderer>()); // Badan utama
        allSprites.AddRange(GetComponentsInChildren<SpriteRenderer>()); // Anak-anaknya

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); // Hitung transparansi

            // Ubah alpha semua layer
            foreach (SpriteRenderer sr in allSprites)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = newAlpha;
                    sr.color = c;
                }
            }
            yield return null;
        }

        Enemy_Spawner spawner = spawnerReference.GetComponent<Enemy_Spawner>();
        if (spawner != null)
        {
            spawner.RemoveEnemyFromList(this.gameObject);
        }
        HandleDropItem();
        Destroy(gameObject);
    }

    public void HandleDropItem()
    {
        // Pastikan list tidak kosong
        if (lootTable == null || lootTable.Count == 0) return;

        int lootCount = Random.Range(minLoot, maxLoot + 1);

        for (int i = 0; i < lootCount; i++)
        {
            int selectedIndex = GetIndexByPercentage();

            // Ambil itemnya
            Item itemToDrop = lootTable[selectedIndex];

            // Buat Datanya
            ItemData itemDataDrop = new ItemData
            {
                itemName = itemToDrop.itemName,
                count = 1,
                quality = itemToDrop.quality,
                itemHealth = itemToDrop.maxhealth
            };

            Debug.Log($"Bandit drop index [{selectedIndex}]: {itemDataDrop.itemName}");

            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.1f, 0.5f));
            ItemPool.Instance.DropItem(
                itemDataDrop.itemName,
                itemDataDrop.itemHealth,
                itemDataDrop.quality,
                transform.position + offset,
                itemDataDrop.count
            );

        }
    }

    // Fungsi Logika Persentase (70% - 20% - 10%)
    private int GetIndexByPercentage()
    {
        // Acak angka 0 sampai 100
        int roll = Random.Range(0, 100);
        int chosenIndex = 0;

        // Jika roll 0-69 (70% peluang)
        if (roll < 70)
        {
            // Target: Index 0 atau 1
            // (Random.Range int itu param kedua exclusive, jadi tulis 2 biar dapet 0 atau 1)
            chosenIndex = Random.Range(0, 2);
        }
        // Jika roll 70-89 (20% peluang)
        else if (roll < 90)
        {
            // Target: Index 2, 3, atau 4
            // (Saya lebarkan dari index 2 supaya tidak ada item yang terlewat/bolong)
            chosenIndex = Random.Range(2, 5);
        }
        // Jika roll 90-99 (10% peluang)
        else
        {
            // Target: Index 5 ke atas (sampai habis)
            chosenIndex = Random.Range(5, lootTable.Count);
        }

     

        if (chosenIndex >= lootTable.Count)
        {
            // Jika index yang dipilih kegedean, paksa ambil item paling terakhir yang ada
            chosenIndex = lootTable.Count - 1;
        }

        return chosenIndex;
    }
    private void OnDrawGizmosSelected()
    {
        // Jika verticalOffset negatif, titik ini akan turun.
        Vector3 centerPoint = transform.position + new Vector3(0, verticalOffset, 0);

        // Gambar Lingkaran Lintasan (Kuning)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPoint, hitboxRadius);

        // Gambar Hitbox Aktual (Merah)
        if (zonaSerangTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(centerPoint, zonaSerangTransform.position);
            Gizmos.DrawWireSphere(zonaSerangTransform.position, 0.2f);
        }

        // Gambar Lingkaran Deteksi (Sensor Mata) - WARNA HIJAU
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPoint, detectionRadius);

        // Gambar Zona Serang (Hitbox) - WARNA MERAH (Dari kode Anda sebelumnya)
        if (zonaSerangTransform != null)
        {
            // Hitung titik tengah visual offset

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerPoint, hitboxRadius);
        }
    }
}
