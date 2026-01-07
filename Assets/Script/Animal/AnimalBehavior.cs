using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum AnimalType { Pasif, Agresif, isQuest }
public enum AnimalState { Idle, Wandering, Eating, Sleeping, Mengejar, Attack, Run}

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


    [Header("Komponen Serangan")]
    public Transform zonaSerangTransform;

    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)
    public float hitboxRadius = 1.0f;

    [Header("Animasi")]
    [SerializeField] private Animator animalAnimator;
    public SpriteRenderer animalRenderer;

    [Header("Logika Deteksi")]
    public float detectionRadius;
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
        }
        else if (tipeHewan == AnimalType.isQuest)
        {
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
        UpdateZonaSerangPosition();
        // Logika untuk AnimalType.Agresif dan AnimalType.isQuest
        if (currentTarget == null)
        {
            // Jika tadinya sedang mengejar/menyerang, dan tiba-tiba target hilang
            // Kita paksa reset state
            if (tipeHewan != AnimalType.Pasif)
            {
                currentState = AnimalState.Idle;

                if (!animalAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    animalAnimator.Play("Idle");
                }

                isMoving = false;

                // Hanya jalankan coroutine JIKA belum ada yang berjalan (null)
                if (roamingCoroutine == null)
                {

                    Debug.Log("eh cari kegiatan lain dong");
                    //roamingCoroutine = StartCoroutine(PlayRandomAnimationPeriodically());
                }
            }

            return; // Stop, jangan lanjut ke bawah
        }


        // Matikan mode jalan-jalan santai karena kita punya urusan (Mengejar/Menyerang)
        if (roamingCoroutine != null)
        {
            StopCoroutine(roamingCoroutine);
            roamingCoroutine = null;
        }

        // Jika tipe hewan adalah isQuest dan targetnya bukan ItemDrop,
        // dia hanya akan mengikuti player tanpa menyerang.
        //if (tipeHewan == AnimalType.isQuest && currentTarget.CompareTag("ItemDrop") == false)
        //{
        //    // Pastikan currentTarget adalah player (jika Anda ingin selalu mengikuti player)
        //    if (currentTarget.CompareTag("Player"))
        //    {
        //        float distanceToPlayer = Vector2.Distance(transform.position, currentTarget.position);
        //        if (distanceToPlayer > attackRange) // Jika masih di luar jarak serang
        //        {
        //            currentState = "Mengejar";
        //            ChaseTargetFixedUpdate();
        //        }
        //        else // Sudah dekat dengan player
        //        {
        //            currentState = AnimalState.Idle;// Berhenti mengejar, tetap di tempat
        //            isMoving = false;
        //            animalAnimator.Play("Idle"); // Animasi idle
        //        }
        //    }
        //    else
        //    {
        //        // Jika isQuest punya target selain player/itemdrop (misal hewan lain), hapus targetnya.
        //        // Ini untuk memastikan isQuest hanya fokus ke player atau ItemDrop.
        //        currentTarget = null;
        //        currentState = AnimalState.Idle;
        //        StartCoroutine(PlayRandomAnimationPeriodically());
        //    }
        //    return; // Hentikan Update untuk isQuest jika tidak ada ItemDrop
        //}

        // Logika Agresif (hanya untuk AnimalType.Agresif) atau isQuest yang mengejar ItemDrop
        if (tipeHewan == AnimalType.Agresif || (tipeHewan == AnimalType.isQuest && currentTarget.CompareTag("ItemDrop")))
        {

            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange)
            {
                currentState = AnimalState.Attack; // Atau "MengambilItem" jika targetnya ItemDrop
            }
            else
            {
                currentState = AnimalState.Mengejar;
            }

            switch (currentState)
            {
                case AnimalState.Mengejar:
                    ChaseTargetFixedUpdate();
                    break;
                case AnimalState.Attack: // Ini akan dipanggil juga jika isQuest mencapai ItemDrop
                    // Untuk isQuest yang mencapai ItemDrop, kita akan "mengambil" itemnya
                    if (tipeHewan == AnimalType.isQuest && currentTarget.CompareTag("ItemDrop"))
                    {
                        Debug.Log($"{namaHewan} (Quest) telah mencapai item: {currentTarget.name}. Mengambilnya!");
                        OnAnimalPickItem?.Invoke(); // Panggil event jika ada yang mendengarkan
                        // Hancurkan item atau nonaktifkan
                        Destroy(currentTarget.gameObject);
                        currentTarget = null; // Hapus target
                        currentState = AnimalState.Idle; // Kembali ke idle setelah mengambil item
                    }
                    else // Ini untuk AnimalType.Agresif yang menyerang
                    {
                        //JalankanLogikaSerangan();
                    }
                    isMoving = false;
                    animalAnimator.Play("Idle");
                    break;
            }
        }
    }

   

    // Coroutine baru untuk pergerakan yang lebih aman
    private IEnumerator MoveToTargetWithPhysics(Vector2 targetPosition, float speed)
    {
        while (Vector2.Distance(rb.position, targetPosition) > 0.1f)
        {
            // MovePosition() harus di FixedUpdate()
            // Jadi kita hanya perlu mengarahkan hewan di sini
            Vector2 direction = (targetPosition - rb.position).normalized;
            rb.linearVelocity = direction * speed;

            yield return null; // Tunggu satu frame
        }
        rb.linearVelocity = Vector2.zero; // Hentikan gerakan setelah sampai
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

    // FixedUpdate() baru untuk menangani fisika
    void FixedUpdate()
    {
        if (isMoving && currentTarget != null)
        {
            ChaseTargetFixedUpdate();
        }
        // Jika tidak mengejar, gerakan acak akan dihandle oleh coroutine MoveToTargetWithPhysics
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
            // 1. Pilih kegiatan selanjutnya secara acak
            AnimalState nextAction = GetRandomNextState(currentState);

            // 2. Lakukan kegiatan tersebut (Jalan-jalan, Makan, atau Diam)
            if (nextAction == AnimalState.Wandering) yield return StartCoroutine(DoWandering());
            else if (nextAction == AnimalState.Eating) yield return StartCoroutine(DoEatingSequence());
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
        Debug.Log($"{namaHewan} memutuskan untuk jalan-jalan santai.");


        // Ambil posisi sekarang
        Vector2 currentPos = transform.position;

        // Cari titik acak dalam lingkaran radius X
        // Random.insideUnitCircle akan menghasilkan Vector2 acak antara (-1,-1) sampai (1,1)
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;

        // Tentukan target akhir
        Vector2 targetPosition = currentPos + randomOffset;



        isMoving = true;

        // Cek arah untuk menentukan animasi Kanan/Kiri
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



        // Selama jarak ke target masih jauh (> 0.1f), teruslah bergerak
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            // Pindahkan posisi sedikit demi sedikit ke arah target
            // MoveTowards menjamin pergerakan yang mulus dan berhenti tepat di target
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // PENTING: Tunggu frame berikutnya, jangan macetkan game
            yield return null;
        }



        // Sudah sampai, hentikan animasi
        isMoving = false;
        animalAnimator.Play("Idle");

        Debug.Log($"{namaHewan} sampai di tujuan jalan-jalannya.");

        // Tambahan: Istirahat sebentar setelah capek jalan (misal 1-2 detik)
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
    }
    public float getRandomWalkAnimal()
    {
        return UnityEngine.Random.Range(0f, 4f);
    }
    private IEnumerator DoEatingSequence()
    {
        currentState = AnimalState.Eating; // Kunci state biar gak diganggu

        // Tahap 1: Menunduk (Persiapan)
        animalAnimator.Play("Makan");
        yield return new WaitForSeconds(1.5f); // Tunggu animasi nunduk

        // Tahap 2: Mengunyah (Looping)
        animalAnimator.Play("Mengunyah");
        float durasiMakan = UnityEngine.Random.Range(4f, 6f); // Makan selama 3-6 detik
        yield return new WaitForSeconds(durasiMakan);

       

        // Setelah fungsi ini selesai, dia akan kembali ke ThinkProcess
        // dan memilih kegiatan baru (misal Idle atau Tidur)
        currentState = AnimalState.Idle;
    }






    public void DropItem()
    {
        int normalItemCount = UnityEngine.Random.Range(minNormalItem, maxNormalItem + 1);

        if (isAnimalQuest)
        {
            // Drop item quest (biasanya 1)
            if (dropitems.Length > 0)
            {
                ItemData itemToDrop = new ItemData(dropitems[0].itemName, 1, dropitems[0].quality, dropitems[0].health);
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                ItemPool.Instance.DropItem(itemToDrop.itemName, itemToDrop.itemHealth, itemToDrop.quality, transform.position + offset, 1);
            }
        }
        else
        {
            // Drop item normal
            DropItemsByType(0, Mathf.Min(3, dropitems.Length), normalItemCount);

            // Jika punya item spesial, drop juga secara terpisah
            if (dropitems.Length > 3)
            {
                int specialItemCount = UnityEngine.Random.Range(minSpecialItem, maxSpecialItem + 1);
                DropItemsByType(3, dropitems.Length, specialItemCount);
            }
        }
    }


    private void DropItemsByType(int startIndex, int endIndex, int itemCount)
    {
        if (dropitems == null || dropitems.Length == 0)
        {
            Debug.LogWarning("Drop items array is empty or null.");
            return;
        }
        if (startIndex < 0 || endIndex > dropitems.Length || startIndex >= endIndex)
        {
            Debug.LogError($"Invalid index range: startIndex={startIndex}, endIndex={endIndex}, arrayLength={dropitems.Length}");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(startIndex, endIndex);
        ItemData itemToDrop = new ItemData(dropitems[randomIndex].itemName, 1, dropitems[randomIndex].quality, randomIndex);
        if (itemToDrop != null)
        {
            Debug.Log("nama item yang di drop adalah : " + itemToDrop.itemName);
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
            ItemPool.Instance.DropItem(itemToDrop.itemName, itemToDrop.itemHealth, itemToDrop.quality, transform.position + offset, itemCount);
        }
        else
        {
            Debug.LogWarning($"Item at index {randomIndex} is null.");
        }
        Destroy(gameObject);
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

    private void OnTriggerExit2D(Collider2D other)
    {
        // Jika yang keluar adalah target saat ini
        if (other.transform == currentTarget)
        {
            // Jika hewan adalah isQuest dan targetnya adalah ItemDrop yang baru diambil
            if (tipeHewan == AnimalType.isQuest && other.CompareTag("ItemDrop"))
            {
                Debug.Log($"{namaHewan} (Quest) sudah 'mengambil' ItemDrop. Kembali Idle.");
                currentTarget = null;
                currentState = AnimalState.Idle;
                return;
            }

            // Hitung jarak saat ini
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            // HANYA JIKA jaraknya lebih besar dari radius deteksi utama kita,
            // barulah kita anggap target benar-benar hilang.
            if (distanceToTarget > detectionRadius)
            {
                Debug.Log($"{namaHewan} kehilangan jejak targetnya karena terlalu jauh.");
                currentTarget = null;
                currentState = AnimalState.Idle;

                // Untuk hewan isQuest, setelah kehilangan target (selain itemdrop),
                // mungkin kita ingin dia kembali mengejar player
                if (tipeHewan == AnimalType.isQuest && playerTransform != null)
                {
                    Debug.Log($"{namaHewan} (Quest) kembali fokus pada Player.");
                    currentTarget = playerTransform;
                    currentState = AnimalState.Mengejar;
                }
                else // Jika bukan isQuest atau tidak ada player
                {
                }
            }
            else
            {
                Debug.Log($"Target keluar dari trigger kecil, tapi masih dalam jangkauan deteksi. Pengejaran dilanjutkan.");
            }
        }
    }

    private void ChaseTargetFixedUpdate()
    {
        movementDirection = (currentTarget.position - transform.position).normalized;
        rb.MovePosition(rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime);

        // Logika flip dan animasi tetap di sini
        if (movementDirection.x > 0)
        {
            animalRenderer.flipX = true;
            animalAnimator.Play("JalanKanan");
        }
        else if (movementDirection.x < 0)
        {
            animalRenderer.flipX = false;
            animalAnimator.Play("JalanKiri");
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

        // Logika serangan HANYA untuk AnimalType.Agresif
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        if (currentTarget != null)
        {
            Debug.Log($"{namaHewan} sedang memberikan damage ke {currentTarget.name}");

            if (currentTarget.CompareTag("Animal"))
            {
                AnimalBehavior targetBehavior = currentTarget.GetComponent<AnimalBehavior>();
                if (targetBehavior != null)
                {
                    targetBehavior.TakeDamage(10);
                }
            }
            else if (currentTarget.CompareTag("Player"))
            {
                Player_Health player_Health = currentTarget.GetComponent<Player_Health>();
                if (player_Health != null)
                {
                    player_Health.TakeDamage(20, zonaSerangTransform.position);
                    player_Health.CheckSekarat();
                }
            }
        }
    }

    private void UpdateZonaSerangPosition()
    {
        // Cek Null & Tipe Hewan
        if (zonaSerangTransform == null || currentTarget == null) return;
        if (tipeHewan != AnimalType.Agresif) return;

        // Update lastDirection jika hewan sedang bergerak
        //  zona serang tetap menghadap ke arah terakhir
        if (movementDirection.magnitude > 0.1f)
        {
            lastDirection = movementDirection.normalized;
        }

        // Cek safety agar tidak error jika lastDirection masih (0,0) di awal game
        if (lastDirection == Vector2.zero) return;

        // itung Posisi Zona Serang
        // Rumus: Arah Terakhir * Jarak Serang
        Vector3 finalPosition = lastDirection * attackRange;

        // Tambahkan Vertical Offset (Penting untuk game Top-Down agar hitbox pas di badan/kepala, bukan di kaki)
        finalPosition.y += verticalOffset;

        // Terapkan Posisi
        zonaSerangTransform.localPosition = finalPosition;

        //  Hitung Rotasi (Agar area serang memutar mengikuti arah hadap)
        float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        zonaSerangTransform.rotation = Quaternion.Euler(0, 0, angle);
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
    }
}