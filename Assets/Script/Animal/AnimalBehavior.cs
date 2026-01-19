using System;
using System.Collections;
using UnityEngine;

public enum AnimalType { Pasif, Agresif, isQuest }
public enum AnimalState { Idle, Wandering, Eating, Sleeping, Mengejar, Attack, Run, Duduk}

[System.Serializable]
public class StateTransition
{
    public AnimalState currentState;
    // Dari state ini, boleh pindah ke mana saja?
    public AnimalState[] possibleNextStates;
}

public class AnimalBehavior : MonoBehaviour
{
    [Header("Animasi dan Transisi")]
    public StateTransition[] transitions;
    [Header("Sensor Mata")]
    public float detectionRadius = 5f; // Tetap pakai float biar ringan
    public LayerMask targetLayer;

    public AnimalState currentState = AnimalState.Idle;
    public float jedaAnimasi = 3f;
    public bool isMoving;
    public bool isAnimalQuest;
    public float moveSpeed;
    private bool isAvoiding = false;
    public bool isAnimalEvent;
    private Coroutine currentMovementCoroutine;
    public Rigidbody2D rb; // Tambahkan referensi Rigidbody2D
    public Vector2 movementDirection;
    public Vector2 lastDirection;

    [Header("Tipe Perilaku Hewan")]
    public AnimalType tipeHewan = AnimalType.Pasif;
    public Transform currentTarget;
    public string itemTriggerName;

    [Header("Logika Serangan")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    public float wanderRadius = 4;
    public int attackDamage = 10;

    [Header("Komponen Serangan")]
    public Transform zonaSerangTransform;

    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)
    public float hitboxRadius = 1.0f;

    [Header("Animasi")]
    [SerializeField] private Animator animalAnimator;
    public SpriteRenderer animalRenderer;

    [Header("Logika Deteksi")]
    // Variable untuk mencegah spam coroutine (Wajib ada di Class scope)
    private Coroutine roamingCoroutine;


    public string namaHewan;
    public float health;
    public int maxHealth;


    public Item[] dropitems;
    public int minNormalItem = 1;
    public int maxNormalItem = 2;
    public int minSpecialItem = 0;
    public int maxSpecialItem = 1;

    public static event Action<AnimalBehavior> OnAnimalDied;
    public static event Action OnAnimalPickItem;

    // Tambahkan referensi ke Player jika ingin harimau isQuest langsung mengejar player
    [Header("Quest Behavior")]
    public Transform playerTransform; // Pastikan ini di-assign di Inspector jika diperlukan

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Dapatkan referensi Rigidbody2D
    }
    private void Start()
    {
        health = maxHealth;

        if (tipeHewan == AnimalType.Pasif)
        {
            roamingCoroutine = StartCoroutine(ThinkProcess());
        }
        else if (tipeHewan == AnimalType.Agresif) // Tambahkan ini jika agresif butuh Idle awal
        {
            currentState = AnimalState.Idle;
            animalAnimator.Play("Idle");
            roamingCoroutine = StartCoroutine(ThinkProcess());
            StartCoroutine(DetectTargetRoutine());
        }
        else if (tipeHewan == AnimalType.isQuest)
        {
            StartCoroutine(DetectTargetRoutine());
            // Jika diawal sudah isQuest, langsung coba kejar player
            if (playerTransform == null)
            {
                playerTransform = GameObject.FindWithTag("Player")?.transform;
            }
            if (playerTransform != null)
            {
                currentTarget = playerTransform;
                currentState = AnimalState.Mengejar; // Akan mengejar player
                StopAllCoroutines(); // Hentikan perilaku pasif
                Debug.Log($"{namaHewan} (Quest) mulai mengikuti Player.");
            }
            else
            {
                currentState = AnimalState.Idle; // Jika tidak ada player, kembali idle
                animalAnimator.Play("Idle");
                roamingCoroutine = StartCoroutine(ThinkProcess());
                //StartCoroutine(PlayRandomAnimationPeriodically()); // Mungkin perlu perilaku acak jika tak ada player
            }
        }
    }

    private void Update()
    {

        if (currentTarget == null)
        {
            // Kalau dia lagi Idle/Wandering dan target null, itu normal (gak perlu direset).
            if (currentState == AnimalState.Mengejar || currentState == AnimalState.Attack)
            {
                Debug.Log("Waduh, target hilang saat dikejar! Reset dulu.");

              

                currentState = AnimalState.Idle;
                isMoving = false;
                animalAnimator.Play("Idle");


               
                rb.linearVelocity = Vector2.zero;

             
                if (roamingCoroutine == null)
                {
                    roamingCoroutine = StartCoroutine(ThinkProcess());
                }

            }

          
            return;
        }
        UpdateZonaSerangPosition();



     
        // Kita hanya jalankan ini kalau tipe Agresif dan PUNYA target
        if (tipeHewan == AnimalType.Agresif && currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);


            if (distance > attackRange)
            {
               

                currentState = AnimalState.Mengejar;
                isMoving = true;
            }
            else
            {
              
                currentState = AnimalState.Attack;
                isMoving = false;
                Debug.Log("Ah ada mangsa nih serang ahhhh > . ..");
                // Tambahkan REM TANGAN biar gak meluncur (sliding) saat mukul
                rb.linearVelocity = Vector2.zero;
                JalankanLogikaSerangan();
            }

            // Jika target lari terlalu jauh (misal detectionRadius + 3 meter)
            if (distance > detectionRadius + 3f)
            {
                // ??? (Tulis logika menyerah di sini: Reset target, Reset state, Mulai roaming lagi)
                currentTarget = null;
                currentState = AnimalState.Idle;
                animalAnimator.Play("Idle");
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
        if (currentState == AnimalState.Mengejar && currentTarget != null)
        {
            ChaseTargetFixedUpdate();
        }
    }


    // Coroutine baru untuk pergerakan yang lebih aman
    private IEnumerator MoveToTargetWithPhysics(Vector2 targetPosition, float speed)
    {
        float stuckTimer = 0f;
        float timeToConsiderStuck = 1.0f; // Jika 1 detik gak gerak, dianggap stuck
        Vector2 lastPosition = rb.position;

        // Loop gerakan
        while (Vector2.Distance(rb.position, targetPosition) > 0.5f) // Jarak toleransi sedikit diperbesar
        {
            Vector2 direction = (targetPosition - rb.position).normalized;
            rb.linearVelocity = direction * speed;

            // Hitung jarak perpindahan sejak frame terakhir
            float distanceMoved = Vector2.Distance(rb.position, lastPosition);

            // Jika geraknya sedikit sekali (hampir diam) padahal velocity ada
            if (distanceMoved < (speed * Time.deltaTime * 0.1f))
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > timeToConsiderStuck)
                {
                    Debug.Log($"{namaHewan} NYETUCK! Membatalkan jalur ini.");
                    rb.linearVelocity = Vector2.zero;
                    yield break; // kalau tidak bergerak berarti menabrak sesuatu. Keluar dari coroutine
                }
            }
            else
            {
                // Kalau gerak lancar, reset timer
                stuckTimer = 0f;
            }

            // Simpan posisi frame ini untuk dicek di frame depan
            lastPosition = rb.position;

            yield return null;
        }

        rb.linearVelocity = Vector2.zero; // Berhenti mulus
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMoving)
        {
            rb.linearVelocity = Vector2.zero; // Hentikan gerakan saat tabrakan
            isMoving = false; // Set isMoving ke false saat tabrakan
            Debug.Log($"{namaHewan}   Menabrak   {collision.gameObject.name} ganti kegiatan");

        }
    }

   


    public void ChangeAnimalType(AnimalType animalType)
    {
        tipeHewan = animalType;
        if (tipeHewan == AnimalType.isQuest)
        {
            //Debug.Log("Animal type changed to isQuest. Stopping current coroutines.");
            StopAllCoroutines(); // Hentikan semua coroutine sebelumnya

            // Set target ke player secara langsung saat diubah menjadi isQuest
            if (playerTransform == null)
            {
                playerTransform = GameObject.FindWithTag("Player")?.transform;
            }

            if (playerTransform != null)
            {
                currentTarget = playerTransform;
                currentState = AnimalState.Mengejar; // Mulai mengejar player
                //Debug.Log($"{namaHewan} (Quest) mulai mengikuti Player setelah diubah tipe.");
            }
            else
            {
                //Debug.LogWarning("Player Transform not found for isQuest animal.");
                currentState = AnimalState.Idle;
                animalAnimator.Play("Idle");
            }
        }
        else if (tipeHewan == AnimalType.Agresif)
        {
            // Jika diubah menjadi agresif, pastikan tidak ada target acak dan kembali ke idle
            currentTarget = null;
            currentState = AnimalState.Idle;
            animalAnimator.Play("Idle");
            StopAllCoroutines(); // Hentikan coroutine pasif jika ada
            // Agresif akan mencari target melalui OnTriggerEnter2D
        }
        else if (tipeHewan == AnimalType.Pasif)
        {
            currentTarget = null;
            currentState = AnimalState.Idle;
            StopAllCoroutines();
        }
    }

    //logika otak hewan
    private IEnumerator ThinkProcess()
    {
        
        while (true) // Loop selamanya
        {
            AnimalState nextAction = GetRandomNextState(currentState);

            if (nextAction == AnimalState.Wandering) yield return StartCoroutine(DoWandering());
            else if (nextAction == AnimalState.Eating) yield return StartCoroutine(DoEatingSequence());
            else if (nextAction == AnimalState.Sleeping) yield return  StartCoroutine(DoSleep()); // Implementasi tidur bisa ditambahkan nanti
            else if (nextAction == AnimalState.Duduk) yield return  StartCoroutine(DoSit());
            else yield return StartCoroutine(DoIdle());
        }
    }

    public AnimalState GetRandomNextState(AnimalState currentState)
    {
        foreach (var transition in transitions)
        {
            if (transition.currentState == currentState)
            {
                int randomIndex = UnityEngine.Random.Range(0, transition.possibleNextStates.Length);
                Debug.Log($"Next state for {currentState} is {transition.possibleNextStates[randomIndex]}");
                return transition.possibleNextStates[randomIndex];
            }
        }

        return AnimalState.Idle; // Default fallback
    }
    public IEnumerator DoIdle()
    {
        Debug.Log($"{namaHewan} memutuskan untuk beristirahat sejenak.");
        isMoving = false;
        animalAnimator.Play("Idle");
        float idleDuration = UnityEngine.Random.Range(4f, 6f);
        yield return new WaitForSeconds(idleDuration);
    }
    public IEnumerator DoWandering()
    {
        Debug.Log($"{namaHewan} mencoba jalan-jalan.");

        //tentukan seberapa jauh dia akan jalan
        float distanceToWander = UnityEngine.Random.Range((wanderRadius/2), wanderRadius);
        // Tentukan Tujuan Awal
        Vector2 currentPos = transform.position;
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * distanceToWander;
        Vector2 targetPosition = currentPos + randomOffset;

        isMoving = true;
        UpdateAnimationDirection(targetPosition); 

        // Kita tunggu sampai dia sampai ATAU dia menyerah (stuck)
        yield return StartCoroutine(MoveToTargetWithPhysics(targetPosition, moveSpeed));

       

        isMoving = false;
        animalAnimator.Play("Idle");

        // Istirahat
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
    }

    // Helper kecil biar code rapi
    private void UpdateAnimationDirection(Vector2 target)
    {
        if (target.x > transform.position.x)
        {
            animalRenderer.flipX = true;
            animalAnimator.Play("JalanKanan");
        }
        else
        {
            animalRenderer.flipX = false;
            animalAnimator.Play("JalanKiri");
        }
    }

    private IEnumerator DoEatingSequence()
    {
        currentState = AnimalState.Eating; // Kunci state biar gak diganggu


        yield return StartCoroutine(PlayAndWait("Makan"));

        animalAnimator.Play("Mengunyah");
        float durasiMakan = UnityEngine.Random.Range(2f, 4f); // Makan selama 3-6 detik
        yield return new WaitForSeconds(durasiMakan);

       

        // Setelah fungsi ini selesai, dia akan kembali ke ThinkProcess
        // dan memilih kegiatan baru (misal Idle atau Tidur)
        currentState = AnimalState.Idle;
    }

    public IEnumerator DoSleep()
    {
        currentState = AnimalState.Eating; // Kunci state biar gak diganggu

        yield return StartCoroutine(PlayAndWait("Duduk"));

        yield return StartCoroutine(PlayAndWait("Rebahan"));

        animalAnimator.Play("TidurNyenyak");
        float durasiMakan = UnityEngine.Random.Range(2f, 4f); // Tidur selama 3-6 detik
        yield return new WaitForSeconds(durasiMakan);



        currentState = AnimalState.Idle;
    }
    public IEnumerator DoSit()
    {
        currentState = AnimalState.Eating; // Kunci state biar gak diganggu

        yield return StartCoroutine(PlayAndWait("Duduk"));

        animalAnimator.Play("Rebahan");
        float durasiRebahan = UnityEngine.Random.Range(2f, 4f); // Makan selama 3-6 detik
        yield return new WaitForSeconds(durasiRebahan);

        yield return StartCoroutine(PlayAndWait("Berdiri"));




        currentState = AnimalState.Idle;
    }


    private IEnumerator PlayAndWait(string stateName)
    {
        animalAnimator.Play(stateName);

        // Tanpa ini, Unity masih membaca info animasi yang LAMA (sebelum diganti)
        yield return null;

        // Ambil informasi state yang sedang berjalan di Layer 0
        AnimatorStateInfo info = animalAnimator.GetCurrentAnimatorStateInfo(0);

        //Ambil durasi (length) animasi tersebut dan tunggu
        yield return new WaitForSeconds(info.length);
    }

    public void DropItem()
    {
        // Tentukan berapa kali drop
        int dropItemCount = UnityEngine.Random.Range(2, 4); // 2 atau 3 kali

        if (isAnimalQuest)
        {
            // Logika Quest (Tetap sama, biasanya fix index 0)
            if (dropitems.Length > 0)
            {
                // Drop Quest Item
                SpawnSingleItem(dropitems[0], 1);
            }
        }
        else
        {
            
            int limitNormal = Mathf.Min(3, dropitems.Length);
            DropItemsByType(dropItemCount, minNormalItem, maxNormalItem, 0, limitNormal);

            if (dropitems.Length > 3)
            {
                int specialItemCount = 1; // Biasanya item spesial cuma drop 1 kali loop

                DropItemsByType(specialItemCount, minSpecialItem, maxSpecialItem, 3, dropitems.Length);
            }
        }

    }

    // Helper kecil untuk spawn single item (biar kode Quest bersih)
    private void SpawnSingleItem(Item itemData, int count)
    {
        if (itemData == null) return;
        Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
        ItemPool.Instance.DropItem(itemData.itemName, itemData.health, itemData.quality, transform.position + offset, count);
    }


    private void DropItemsByType(int dropItemCount, int minItem, int maxItem, int startIndex, int endIndex)
    {
        // Safety Check: Pastikan array ada isinya
        if (dropitems == null || dropitems.Length == 0) return;

        // Kita kunci endIndex agar tidak error OutOfRange
        endIndex = Mathf.Min(endIndex, dropitems.Length);
    
        // Safety Check: Jika start lebih besar dari end, batalkan
        if (startIndex >= endIndex) return;

        for (int i = 0; i < dropItemCount; i++)
        {
            int indexRandomItem = 0;
            // Gunakan parameter startIndex dan endIndex, bukan angka manual (0, 3)
            if (startIndex == 0 && (endIndex - startIndex) >= 3)
            {
                int roll = UnityEngine.Random.Range(0, 100); // Acak angka 0 s/d 99

                if (roll < 50)
                {
                    indexRandomItem = 0; // 50% Peluang (0-49) -> Item Utama (Daging)
                }
                else if (roll < 80) // 50 + 30 = 80
                {
                    indexRandomItem = 1; // 30% Peluang (50-79) -> Item Kedua
                }
                else
                {
                    indexRandomItem = 2; // 20% Peluang (80-99) -> Item Ketiga (Langka)
                }
            }
            else
            {
                // Untuk item Spesial (Index 3++) atau jika item kurang dari 3, pakai acak rata
                indexRandomItem = UnityEngine.Random.Range(startIndex, endIndex);
            }

            // Tentukan jumlah tumpukan (Stack)
            int stackCount = UnityEngine.Random.Range(minItem, maxItem + 1);

            // Ambil Data Item
            ItemData sourceItem = new ItemData(dropitems[indexRandomItem].itemName, stackCount, dropitems[indexRandomItem].quality, 0);

            // Spawn Item
            ItemData itemToDrop = new ItemData(sourceItem.itemName, stackCount, sourceItem.quality, 0);
        
            if (itemToDrop != null)
            {
                Debug.Log($"Dropping {itemToDrop.itemName} (Index: {indexRandomItem})");
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                ItemPool.Instance.DropItem(itemToDrop.itemName, itemToDrop.itemHealth, itemToDrop.quality, transform.position + offset, itemToDrop.count);
            }
        }
    }


    public void TakeDamage(float damage)
    {
        Debug.Log("take Damage" + damage);
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            Die();
        }
        else if (tipeHewan == AnimalType.Pasif)
        {
            StopAllCoroutines();
            Run();
        }
    }

    public void Run()
    {
        float runSpeed = 5f;
        Vector2 currentPosition = transform.position;

        Vector2 point1 = GetRandomPoint(currentPosition);
        Vector2 point2 = GetRandomPoint(point1);
        Vector2 point3 = GetRandomPoint(point2);

        currentState = AnimalState.Run;
        

        currentMovementCoroutine = StartCoroutine(AnimalRunMovement(point1, runSpeed, point2, point3));
        isMoving = true;

        
    }

    private IEnumerator AnimalRunMovement(Vector2 point1, float speed, Vector2 point2, Vector2 point3)
    {
        SetRunningAnimation(point1);
        yield return MoveToTarget(point1, speed);

        SetRunningAnimation(point2);
        yield return MoveToTarget(point2, speed);

        SetRunningAnimation(point3);
        yield return MoveToTarget(point3, speed);

        yield return new WaitForSeconds(3f);
        currentState = AnimalState.Idle;
        animalAnimator.Play("Idle");

    }


    private void SetRunningAnimation(Vector2 targetPosition)
    {
        StopPreviousAnimation();

        if (targetPosition.x > transform.position.x)
        {
            animalRenderer.flipX = true; // Hadap Kanan
            animalAnimator.Play("JalanKanan"); // Atau "Jalan"
        }
        else
        {
            animalRenderer.flipX = false; // Hadap Kiri
            animalAnimator.Play("JalanKiri");
        }
    }

    private void StopPreviousAnimation()
    {
        animalAnimator.Play("Idle");
    }

    private IEnumerator MoveToTarget(Vector2 targetPosition, float speed)
    {
        // Ganti transform.position di sini
        while (Vector2.Distance(rb.position, targetPosition) > 0.1f)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.deltaTime);
            rb.MovePosition(newPosition);
            // Cek arah untuk menentukan animasi Kanan/Kiri
       
            yield return null;
        }
    }

    private Vector2 GetRandomPoint(Vector2 lastPosition)
    {
        float randomX = UnityEngine.Random.Range(-5f, 5f);
        float randomY = UnityEngine.Random.Range(-5f, 5f);
        return lastPosition + new Vector2(randomX, randomY);
    }

    void Die()
    {
        OnAnimalDied?.Invoke(this);
        DropItem();
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (tipeHewan == AnimalType.isQuest && currentTarget != null && currentTarget.CompareTag("ItemDrop"))
        {
            Debug.Log($"Harimau (Quest) sudah mengejar {currentTarget.name}. Mengabaikan collider lain: {other.name}");
            return; // Hentikan eksekusi seluruh fungsi OnTriggerEnter2D
        }
        // Jika hewan adalah isQuest
        if (tipeHewan == AnimalType.isQuest)
        {
            // Pertama, coba dapatkan komponen ItemDropInteractable dari objek yang masuk trigger
            ItemDropInteractable itemDrop = other.GetComponent<ItemDropInteractable>();

            // Cek apakah komponen ItemDropInteractable ada DAN tag-nya sesuai
            if (itemDrop != null && other.CompareTag("ItemDrop"))
            {
                // Jika itemDrop ada dan tag-nya sesuai, cek apakah itemName-nya "DagingDomba"
                if (itemDrop.itemdata.itemName == itemTriggerName)
                {
                    Debug.Log($"{namaHewan} (Quest) melihat item quest: {other.name}!");
                    currentTarget = other.transform; // Set target ke item drop
                    currentState = AnimalState.Mengejar; // Ubah state menjadi mengejar
                    StopAllCoroutines(); // Hentikan perilaku lain (misal mengikuti player)
                    return; // Penting: hentikan eksekusi lebih lanjut karena target sudah ditemukan
                }
                else
                {
                    // ItemDropInteractable ada, tag-nya sesuai, tapi bukan "DagingDomba".
                    // Harimau harus diam atau kembali ke perilaku sebelumnya jika ada.
                    Debug.Log($"{namaHewan} (Quest) mendeteksi item lain: {other.name}, tapi bukan 'DagingDombaSpesial'. Tetap diam.");
                    // Pastikan tidak ada target lain yang sedang dikejar jika ini terjadi
                    // Jika sebelumnya mengejar player, biarkan tetap mengejar player sampai daging domba muncul
                    // atau ubah currentTarget = null; currentState = AnimalState.Idle; jika ingin dia berhenti total.
                    // Untuk skenario "diam sampai ada item yang benar", kita harus berhenti mengejar player.
                    if (currentTarget != null && currentTarget.CompareTag("Player"))
                    {
                        currentTarget = null;
                        currentState = AnimalState.Idle;
                        animalAnimator.Play("Idle"); // Pastikan animasinya idle
                        StopAllCoroutines(); // Berhenti dari pengejaran player
                    }
                    return; // Penting: hentikan eksekusi lebih lanjut
                }
            }
            else if (other.CompareTag("Player")) // Jika bukan item drop yang relevan, cek apakah itu player
            {
                // Jika hewan isQuest mendeteksi Player
                // Jangan ubah target jika sudah fokus ke ItemDrop yang benar (sudah dihandle di awal fungsi)
                Debug.Log($"{namaHewan} (Quest) mendeteksi Player: {other.name}!");
                currentTarget = other.transform; // Set target ke player
                currentState = AnimalState.Mengejar; // Mulai mengejar player
                StopAllCoroutines(); // Hentikan perilaku acak jika ada
                return; // Penting: hentikan eksekusi lebih lanjut
            }
            else
            {
                // other tidak memiliki komponen ItemDropInteractable, atau tag-nya tidak "Untagged" atau "ItemDrop",
                // dan juga bukan "Player". Harimau harus diam.
                Debug.Log($"{namaHewan} (Quest) mendeteksi objek non-item quest: {other.name}. Tetap diam.");
                // Pastikan tidak ada target yang sedang dikejar (terutama player)
                if (currentTarget != null && currentTarget.CompareTag("Player"))
                {
                    currentTarget = null;
                    currentState = AnimalState.Idle;    
                    animalAnimator.Play("Idle");
                    StopAllCoroutines();
                }
                return; // Penting: hentikan eksekusi lebih lanjut
            }
        }

        // Jika bukan isQuest, atau isQuest tapi targetnya bukan ItemDrop, lanjutkan ke logika lain.
        if (tipeHewan == AnimalType.Pasif) return;

        // Jangan ubah target jika sudah ada target kecuali untuk kasus ItemDrop di atas.
        // Jika currentTarget sudah ItemDrop, jangan ganti ke Player/Animal lain.
        if (currentTarget != null && currentTarget.CompareTag("ItemDrop")) return;


        if (other.CompareTag("Animal") || other.CompareTag("Player"))
        {
            // Untuk AnimalType.isQuest, dia akan mengikuti player, bukan menyerang animal lain
            if (tipeHewan == AnimalType.isQuest && other.CompareTag("Player"))
            {
                Debug.Log($"{namaHewan} (Quest) mendeteksi Player: {other.name}!");
                currentTarget = other.transform; // Set target ke player
                currentState = AnimalState.Mengejar; // Mulai mengejar player
                StopAllCoroutines(); // Hentikan perilaku acak jika ada
            }
            else if (tipeHewan == AnimalType.Agresif) // Logika agresif hanya untuk tipe Agresif
            {
                AnimalBehavior otherAnimal = other.GetComponent<AnimalBehavior>();

                if (otherAnimal != null && otherAnimal != this && otherAnimal.tipeHewan == AnimalType.Pasif)
                {
                    Debug.Log($"{namaHewan} (Agresif) melihat mangsa: {other.name}!");
                    currentTarget = other.transform;
                    currentState = AnimalState.Mengejar;
                    StopAllCoroutines();
                }
                else if (other.CompareTag("Player")) // Agresif juga menyerang Player
                {
                    Debug.Log($"{namaHewan} (Agresif) melihat Player: {other.name}!");
                    currentTarget = other.transform;
                    currentState = AnimalState.Mengejar;
                    StopAllCoroutines();
                }
            }
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

    public void JalankanLogikaSerangan()
    {
        // Harimau dengan tipe AnimalType.isQuest TIDAK AKAN MENYERANG!
        if (tipeHewan == AnimalType.isQuest)
        {
            // Di sini Anda bisa tambahkan logika untuk event quest, misalnya:
            if (currentTarget.CompareTag("ItemDrop"))
            {
                Debug.Log("Harimau quest berhasil mencapai item drop!");
                Destroy(currentTarget.gameObject); // Ambil itemnya
                currentTarget = null;
                currentState = AnimalState.Idle;
            }
            return; // Hentikan fungsi jika hewan adalah quest
        }

     


        if (currentTarget != null && tipeHewan == AnimalType.Agresif)
        {
            Debug.Log($"{namaHewan} sedang memberikan damage ke {currentTarget.name}");

            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            // Jangan kasih damage disini! Biarkan Animation Event yang memanggil PerformAttack()
            animalAnimator.Play("Attack");

            // Stop gerakan saat menyerang biar gak meluncur (sliding)
            isMoving = false;
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

    public void PerformAttack()
    {
     

        // Kita cek area di sekitar zonaSerangTransform dengan radius hitboxRadius
        Collider2D[] hits = Physics2D.OverlapCircleAll(zonaSerangTransform.position, hitboxRadius, targetLayer);

        foreach (Collider2D hit in hits)
        {
            // Hindari memukul diri sendiri
            if (hit.gameObject == this.gameObject) continue;


            // Cek jika kena Player
            if (hit.CompareTag("Player"))
            {
                Player_Health playerHealth = hit.GetComponent<Player_Health>();
                if (playerHealth != null)
                {
                    // Berikan damage + efek knockback (posisi penyerang dikirim untuk arah mental)
                    playerHealth.TakeDamage(attackDamage, this.transform);
                    Debug.Log($"{namaHewan} berhasil menggigit Player!");
                }
            }
            // Cek jika kena Hewan Lain (Mangsa)
            else if (hit.CompareTag("Animal"))
            {
                AnimalBehavior prey = hit.GetComponent<AnimalBehavior>();
                // Pastikan hanya menyerang jika targetnya Pasif (atau musuh)
                if (prey != null && prey.tipeHewan == AnimalType.Pasif)
                {
                    prey.TakeDamage(attackDamage);
                    Debug.Log($"{namaHewan} berhasil menggigit {prey.namaHewan}!");
                }
            }
        }
    }

    private IEnumerator DetectTargetRoutine()
    {
        //  scan setiap 0.5 detik 
        WaitForSeconds scanInterval = new WaitForSeconds(0.5f);

        while (true)
        {
          
            if ((tipeHewan == AnimalType.Agresif || tipeHewan == AnimalType.isQuest) && currentTarget == null)
            {
                FindClosestTarget();
            }

            yield return scanInterval;
        }
    }

    private void FindClosestTarget()
    {
        Debug.Log($"{namaHewan} sedang mencari target di sekitar...");
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
            if (tipeHewan == AnimalType.Agresif)
            {
                if (hit.CompareTag("Player")) isValidTarget = true;
                else if (hit.CompareTag("Animal"))
                {
                    AnimalBehavior otherAnimal = hit.GetComponent<AnimalBehavior>();
                    // cari hewan tipe pasif saja
                    if (otherAnimal != null && otherAnimal.tipeHewan == AnimalType.Pasif)
                    {
                        isValidTarget = true;
                    }
                }else if (hit.CompareTag("Bandit"))
                {
                    isValidTarget = true;
                }
            }
            // Jika Saya Quest Cari Player saja (untuk diikuti)
            else if (tipeHewan == AnimalType.isQuest)
            {
                if (hit.CompareTag("Player")) isValidTarget = true;
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
            Debug.Log($"{namaHewan} melihat target baru: {potentialTarget.name}");
            currentTarget = potentialTarget;

            // Ubah state jadi mengejar
            currentState = AnimalState.Mengejar;

            // Hentikan jalan-jalan santai (Wandering)
            if (roamingCoroutine != null)
            {
                StopCoroutine(roamingCoroutine);
                roamingCoroutine = null;
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
                UpdateAnimationDirection(zonaSerangTransform.position);
            }
            else
            {
                SetMovementDirection(new Vector2(0, direction.y));
                UpdateAnimationDirection(zonaSerangTransform.position);
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
            animalAnimator.Play("Idle");

            // (Opsional) Play animasi Idle jika mau
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