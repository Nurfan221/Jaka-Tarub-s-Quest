using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BanditState
{
    Idle,
    Patroli,
    KejarTarget,
    Serang,
    Kabur
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
    public Rigidbody2D rb; // Tambahkan referensi Rigidbody2D
    public List<Vector2> rutePatroli = new List<Vector2>(4);
    public Vector2 movementDirection;
    public Vector2 lastDirection;
    private Coroutine roamingCoroutine;


    [Header("Logika Serangan")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    public float wanderRadius = 4;
    public int attackDamage = 10;
    public bool attackInProgress = false;
    public bool isTakingDamage = false;

    public Transform zonaSerangTransform;

    [Header("Komponen Serangan")]

    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)
    public float hitboxRadius = 1.0f;
    public Transform currentTarget;

    [Header("Animator Parts")]
    public Animator spriteBadan;
    public List<Animator> layerAnimators = new List<Animator>();


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Pastikan ambil komponen fisik
        roamingCoroutine = StartCoroutine(ThinkProcess());
        StartCoroutine(DetectTargetRoutine());
    }

    private void Update()
    {
        UpdateZonaSerangPosition();
        // Kita hanya jalankan ini kalau tipe Agresif dan PUNYA target
        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);


            if (distance > attackRange)
            {


                currentState = BanditState.KejarTarget;
                isMoving = true;
            }
            else
            {

                currentState = BanditState.Serang;
                isMoving = false;

                // Tambahkan REM TANGAN biar gak meluncur (sliding) saat mukul
                rb.linearVelocity = Vector2.zero;
                JalankanLogikaSerangan();
            }

            // Jika target lari terlalu jauh (misal detectionRadius + 3 meter)
            if (distance > detectionRadius + 3f)
            {
                // ??? (Tulis logika menyerah di sini: Reset target, Reset state, Mulai roaming lagi)
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
    private IEnumerator ThinkProcess()
    {

        while (true) // Loop selamanya
        {
           
            BanditState nextAction = GetRandomNextState(currentState);

            if (nextAction == BanditState.Patroli) yield return StartCoroutine(DoWandering());
          
            else yield return StartCoroutine(DoIdle());
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

        // 1. BERSIHKAN RUTE LAMA (Wajib!)
        rutePatroli.Clear();

        // 2. TENTUKAN BERAPA TITIK
        int jumlahTitik = Random.Range(1, 4); // Minimal 1 titik, maksimal 3

        // 3. TENTUKAN TITIK AWAL ACUAN (Gunakan posisi FISIK saat ini, bukan Direction!)
        Vector2 titikAcuan = rb.position;

        // 4. BUAT RUTE BARU
        for (int i = 0; i < jumlahTitik; i++)
        {
            // Cari titik baru berdasarkan titik acuan terakhir
            Vector2 titikBaru = GetRandomPoint(titikAcuan);
            rutePatroli.Add(titikBaru);

            // Titik acuan digeser ke titik baru, biar jalurnya nyambung (A -> B -> C)
            titikAcuan = titikBaru;
        }

        isMoving = true;

        // 5. JALANKAN FISIKA
        foreach (var rute in rutePatroli)
        {
            yield return StartCoroutine(MoveToTargetWithPhysics(rute, moveSpeed));
        }

        isMoving = false;

        // (Opsional) Play Animasi Idle sebentar sebelum mikir lagi
        if (spriteBadan != null) spriteBadan.Play("Idle");
        yield return new WaitForSeconds(1f);
    }
    private IEnumerator MoveToTargetWithPhysics(Vector2 targetPosition, float speed)
    {
        float stuckTimer = 0f;
        float timeToConsiderStuck = 1.0f;
        Vector2 previousFramePosition = rb.position;

        while (Vector2.Distance(rb.position, targetPosition) > 0.5f)
        {
            // 1. HITUNG ARAH YANG DIINGINKAN (INTENTION)
            Vector2 directionToTarget = (targetPosition - rb.position).normalized;

            // --- PERBAIKAN DISINI ---
            // Kita paksa variabel ini berisi arah "Niat", bukan hasil fisika.
            // Walaupun menabrak tembok, directionToTarget tetap bernilai 1 (tetap berusaha maju).
            this.movementDirection = directionToTarget;

            // 2. DORONG FISIKA
            rb.linearVelocity = directionToTarget * speed;

            yield return null;

            // 3. LOGIKA STUCK (Tetap pakai posisi asli untuk mendeteksi macet)
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
                    break; // Keluar loop
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            previousFramePosition = currentPos;
        }

        // SELESAI
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
        if (spriteBadan == null) return;
        if (isTakingDamage) return;

        // --- KEMBALI MENGGUNAKAN VARIABEL GLOBAL ---
        // Karena movementDirection sekarang diisi oleh "directionToTarget" di coroutine,
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
        Debug.Log($"Bandit sedang mencari target di sekitar...");
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
            Debug.Log($"Bandit melihat target baru: {potentialTarget.name}");
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



        Vector3 orbitPosition = centerPoint + (Vector3)(directionToLook * attackRange);

        // Terapkan Posisi
        zonaSerangTransform.position = orbitPosition;

        float angle = Mathf.Atan2(directionToLook.y, directionToLook.x) * Mathf.Rad2Deg;
        zonaSerangTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void JalankanLogikaSerangan()
    {
        if (currentTarget != null)
        {
            // Cek Cooldown
            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            // Stop gerakan agar tidak "sliding" saat memukul
            isMoving = false;
            rb.linearVelocity = Vector2.zero; // Pastikan fisik berhenti total

            // Jalankan Animasi
            // HAPUS PerformAttack() dari sini agar damage tidak keluar duluan!
            PlayActionAnimation_NoWait("Pedang");

            Debug.Log($"Bandit memulai animasi serangan ke {currentTarget.name}");
        }
    }

    public void PlayActionAnimation_NoWait(string actionType)
    {
        if (spriteBadan == null) return;

        // Flag ini berguna jika anda punya logika untuk mencegah gerak saat attackInProgress
        // attackInProgress = true; 

        string triggerName = actionType;

        // --- PERBAIKAN LOGIKA ARAH ---
        // Jangan pakai posisi transform, pakai 'lastDirection' dari script movement
        // Asumsi: lastDirection.x dan lastDirection.y bernilai -1, 0, atau 1

        if (Mathf.Abs(lastDirection.y) > Mathf.Abs(lastDirection.x)) // Lebih dominan vertikal
        {
            if (lastDirection.y > 0) triggerName += "Atas";
            else triggerName += "Bawah";
        }
        else // Lebih dominan horizontal (atau diam)
        {
            if (lastDirection.x > 0) triggerName += "Kanan";
            else triggerName += "Kiri"; // Default ke Kiri jika x < 0
        }

        // Fallback jika lastDirection (0,0) -> Default Kanan/Bawah tergantung preferensi
        if (lastDirection == Vector2.zero) triggerName += "Bawah";

        // Trigger Animasi
        spriteBadan.SetTrigger(triggerName);

        foreach (Animator anim in layerAnimators)
        {
            if (anim != null) anim.SetTrigger(triggerName);
        }

        // PENTING: Posisikan zonaSerangTransform mengikuti arah serangan
        UpdateAttackPosition();
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

    // FUNGSI INI DIPANGGIL OLEH ANIMATION EVENT DI UNITY
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
                    playerHealth.TakeDamage(attackDamage, transform.position);
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
        // Pastikan target masih ada. Kalau null, return (keluar).
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // Jika jarak masih jauh (lebih besar dari attackRange), kita kejar!
        if (distance > attackRange)
        {
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                SetMovementDirection(direction);
                UpdateAnimationParameters();
            }
            else
            {
                SetMovementDirection(new Vector2(0, direction.y));
                UpdateAnimationParameters();
            }

            // Rumus: Arah * Kecepatan (moveSpeed)
            rb.linearVelocity = direction * moveSpeed; // ??? (Isi rumus velocity di sini)

            isMoving = true;
        }
        else
        {
            // Matikan mesin
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
            spriteBadan.Play("Idle");

            // (Opsional) Play animasi Idle jika mau
        }
    }
    public void SetMovementDirection(Vector2 direction)
    {
        this.movementDirection = direction;
        if (direction.magnitude > 0.1f)
        {
            this.lastDirection = direction.normalized; // Simpan arah yang sudah dinormalisasi
        }

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
