using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AnimalType { Pasif, Agresif, isQuest }

public class AnimalBehavior : MonoBehaviour
{

    [System.Serializable]
    public class AnimationState
    {
        public string stateName;
        public string[] availableStates;
    }

    public Sprite[] animalIdle;


    public AnimationState[] animationStates;
    public string currentState = "Idle";
    public float jedaAnimasi = 3f;
    public bool isMoving;
    public float moveSpeed;
    private Vector2 lastCollisionPoint;
    private bool isAvoiding = false;
    public bool isAnimalEvent;
    private Coroutine currentMovementCoroutine;

    [Header("Tipe Perilaku Hewan")]
    public AnimalType tipeHewan = AnimalType.Pasif;
    public Transform currentTarget;

    [Header("Logika Serangan")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    [Header("Komponen Serangan")]
    public Transform zonaSerangTransform;
    public float jarakOffsetSerang = 1.0f;

    [Header("Animasi")]
    [SerializeField] private Animator animalAnimator;
    public SpriteRenderer animalRenderer;

    [Header("Logika Deteksi")]
    public float detectionRadius;


    public string namaHewan;
    public float health;
    public int maxHealth;


    public GameObject[] dropitems;
    public int minNormalItem = 1;
    public int maxNormalItem = 2;
    public int minSpecialItem = 0;
    public int maxSpecialItem = 1;

    public static event Action<AnimalBehavior> OnAnimalDied;
    public static event Action OnAnimalPickItem;

    // Tambahkan referensi ke Player jika ingin harimau isQuest langsung mengejar player
    [Header("Quest Behavior")]
    public Transform playerTransform; // Pastikan ini di-assign di Inspector jika diperlukan

    private void Start()
    {
        health = maxHealth;

        if (tipeHewan == AnimalType.Pasif)
        {
            StartCoroutine(PlayRandomAnimationPeriodically());
        }
        else if (tipeHewan == AnimalType.Agresif) // Tambahkan ini jika agresif butuh Idle awal
        {
            currentState = "Idle";
            animalAnimator.Play("Idle");
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
                currentState = "Mengejar"; // Akan mengejar player
                StopAllCoroutines(); // Hentikan perilaku pasif
                Debug.Log($"{namaHewan} (Quest) mulai mengikuti Player.");
            }
            else
            {
                currentState = "Idle"; // Jika tidak ada player, kembali idle
                animalAnimator.Play("Idle");
                StartCoroutine(PlayRandomAnimationPeriodically()); // Mungkin perlu perilaku acak jika tak ada player
            }
        }
    }

    private void Update()
    {
        // Logika untuk AnimalType.Agresif dan AnimalType.isQuest
        if (currentTarget == null)
        {
            // Jika tidak ada target, dan bukan hewan pasif, kembali ke Idle
            if (tipeHewan != AnimalType.Pasif)
            {
                currentState = "Idle";
                if (animalAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") == false)
                {
                    animalAnimator.Play("Idle");
                }
                isMoving = false;
                // Jika ingin hewan isQuest kembali ke perilaku acak setelah kehilangan target,
                // bisa panggil StartCoroutine(PlayRandomAnimationPeriodically()); di sini.
            }
            return; // Hentikan Update jika tidak ada target
        }

        // Jika tipe hewan adalah isQuest dan targetnya bukan ItemDrop,
        // dia hanya akan mengikuti player tanpa menyerang.
        if (tipeHewan == AnimalType.isQuest && currentTarget.CompareTag("ItemDrop") == false)
        {
            // Pastikan currentTarget adalah player (jika Anda ingin selalu mengikuti player)
            if (currentTarget.CompareTag("Player"))
            {
                float distanceToPlayer = Vector2.Distance(transform.position, currentTarget.position);
                if (distanceToPlayer > attackRange) // Jika masih di luar jarak serang
                {
                    currentState = "Mengejar";
                    ChaseTarget();
                }
                else // Sudah dekat dengan player
                {
                    currentState = "Idle"; // Berhenti mengejar, tetap di tempat
                    isMoving = false;
                    animalAnimator.Play("Idle"); // Animasi idle
                }
            }
            else
            {
                // Jika isQuest punya target selain player/itemdrop (misal hewan lain), hapus targetnya.
                // Ini untuk memastikan isQuest hanya fokus ke player atau ItemDrop.
                currentTarget = null;
                currentState = "Idle";
                StartCoroutine(PlayRandomAnimationPeriodically());
            }
            return; // Hentikan Update untuk isQuest jika tidak ada ItemDrop
        }

        // Logika Agresif (hanya untuk AnimalType.Agresif) atau isQuest yang mengejar ItemDrop
        if (tipeHewan == AnimalType.Agresif || (tipeHewan == AnimalType.isQuest && currentTarget.CompareTag("ItemDrop")))
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange)
            {
                currentState = "Menyerang"; // Atau "MengambilItem" jika targetnya ItemDrop
            }
            else
            {
                currentState = "Mengejar";
            }

            switch (currentState)
            {
                case "Mengejar":
                    ChaseTarget();
                    break;
                case "Menyerang": // Ini akan dipanggil juga jika isQuest mencapai ItemDrop
                    // Untuk isQuest yang mencapai ItemDrop, kita akan "mengambil" itemnya
                    if (tipeHewan == AnimalType.isQuest && currentTarget.CompareTag("ItemDrop"))
                    {
                        Debug.Log($"{namaHewan} (Quest) telah mencapai item: {currentTarget.name}. Mengambilnya!");
                        OnAnimalPickItem?.Invoke(); // Panggil event jika ada yang mendengarkan
                        // Hancurkan item atau nonaktifkan
                        Destroy(currentTarget.gameObject);
                        currentTarget = null; // Hapus target
                        currentState = "Idle"; // Kembali ke idle setelah mengambil item
                        StartCoroutine(PlayRandomAnimationPeriodically()); // Lanjutkan perilaku pasif/acak
                    }
                    else // Ini untuk AnimalType.Agresif yang menyerang
                    {
                        JalankanLogikaSerangan();
                    }
                    isMoving = false;
                    animalAnimator.Play("Idle");
                    break;
            }
        }
    }

    private IEnumerator PlayRandomAnimationPeriodically()
    {
        while (true)
        {
            // Jangan jalankan animasi acak jika sedang mengejar atau bertipe quest (yang mengikuti player)
            if (currentTarget != null && (tipeHewan == AnimalType.Agresif || tipeHewan == AnimalType.isQuest))
            {
                yield return new WaitForSeconds(1f); // Cek lagi setelah 1 detik
                continue;
            }

            string nextState = GetRandomAnimationForCurrentState();
            TransitionTo(nextState);
            yield return new WaitForSeconds(5f);
        }
    }


    public void ChangeAnimalType(AnimalType animalType)
    {
        tipeHewan = animalType;
        if (tipeHewan == AnimalType.isQuest)
        {
            Debug.Log("Animal type changed to isQuest. Stopping current coroutines.");
            StopAllCoroutines(); // Hentikan semua coroutine sebelumnya

            // Set target ke player secara langsung saat diubah menjadi isQuest
            if (playerTransform == null)
            {
                playerTransform = GameObject.FindWithTag("Player")?.transform;
            }

            if (playerTransform != null)
            {
                currentTarget = playerTransform;
                currentState = "Mengejar"; // Mulai mengejar player
                Debug.Log($"{namaHewan} (Quest) mulai mengikuti Player setelah diubah tipe.");
            }
            else
            {
                Debug.LogWarning("Player Transform not found for isQuest animal.");
                currentState = "Idle";
                animalAnimator.Play("Idle");
                StartCoroutine(PlayRandomAnimationPeriodically());
            }
        }
        else if (tipeHewan == AnimalType.Agresif)
        {
            // Jika diubah menjadi agresif, pastikan tidak ada target acak dan kembali ke idle
            currentTarget = null;
            currentState = "Idle";
            animalAnimator.Play("Idle");
            StopAllCoroutines(); // Hentikan coroutine pasif jika ada
            // Agresif akan mencari target melalui OnTriggerEnter2D
        }
        else if (tipeHewan == AnimalType.Pasif)
        {
            currentTarget = null;
            currentState = "Idle";
            StopAllCoroutines();
            StartCoroutine(PlayRandomAnimationPeriodically());
        }
    }

    private string GetRandomAnimationForCurrentState()
    {
        AnimationState state = System.Array.Find(animationStates, s => s.stateName == currentState);
        if (state != null && state.availableStates.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, state.availableStates.Length);
            return state.availableStates[randomIndex];
        }
        return "Idle";
    }

    private void TransitionTo(string nextState)
    {
        if (animalAnimator != null)
        {
            StartCoroutine(PlayNextStateThenTransition(nextState));
        }
        else
        {
            Debug.LogWarning("Animator tidak ditemukan!");
        }
    }

    private IEnumerator PlayNextStateThenTransition(string nextState)
    {
        animalAnimator.Play(nextState);
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0);
        });

        if (nextState == "Duduk")
        {
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play("Rebahan");
            currentState = "Rebahan";
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play("TidurNyenyak");
            currentState = "TidurNyenyak";
        }
        else if (nextState == "Rebahan")
        {
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play("TidurNyenyak");
            currentState = ("TidurNyenyak");
        }
        else if (nextState == "Berdiri")
        {
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play("Idle");
            currentState = "Idle";
        }
        else if (nextState == "Makan")
        {
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play("Mengunyah");
            currentState = "Mengunyah";
        }
        else if (nextState == "JalanKanan" || nextState == "JalanKiri")
        {
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play(nextState);
            currentState = nextState;
            StartCoroutine(AnimalMovement(nextState));
        }
        else
        {
            yield return new WaitUntil(() => { AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0); return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0); });
            animalAnimator.Play(nextState);
            currentState = nextState;
        }
    }

    private IEnumerator AnimalMovement(string currentAnimation)
    {
        Vector2 randomDirection = Vector2.zero;
        if (currentAnimation == "JalanKanan")
        {
            randomDirection = new Vector2(1, UnityEngine.Random.Range(-1f, 1f));
            animalRenderer.flipX = true;
        }
        else if (currentAnimation == "JalanKiri")
        {
            randomDirection = new Vector2(-1, UnityEngine.Random.Range(-1f, 1f));
            animalRenderer.flipX = false;
        }

        Vector2 targetPosition = (Vector2)transform.position + randomDirection * 3f;
        isMoving = true;
        yield return MoveForDuration(targetPosition);
        isMoving = false;
        yield return new WaitForSeconds(5);
    }

    private IEnumerator MoveForDuration(Vector2 targetPosition)
    {
        Vector2 startPosition = transform.position;
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Tree") || collision.gameObject.CompareTag("Stone") || collision.gameObject.CompareTag("Animal") || collision.gameObject.CompareTag("Tebing"))
        {
            lastCollisionPoint = collision.transform.position;
            if (isMoving && !isAvoiding)
            {
                isAvoiding = true;
                if (currentMovementCoroutine != null)
                {
                    StopCoroutine(currentMovementCoroutine);
                }
                currentMovementCoroutine = StartCoroutine(MoveAwayFromObstacle(collision));
            }
        }
    }

    private IEnumerator MoveAwayFromObstacle(Collision2D collision)
    {
        Debug.Log("nabrak bang");
        Vector2 directionAwayFromObstacle = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;
        Vector2 targetPosition = (Vector2)transform.position + directionAwayFromObstacle * 5f;
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        isAvoiding = false;
        // Hanya melanjutkan gerakan acak jika tidak memiliki target aktif
        if (tipeHewan == AnimalType.Pasif && currentTarget == null)
        {
            StartCoroutine(AnimalMovement(currentState));
        }
        else if (currentTarget != null) // Jika ada target, kembali mengejar target
        {
            currentState = "Mengejar"; // Set ulang state ke mengejar
            // Tidak perlu memanggil ChaseTarget() di sini, akan otomatis di Update()
        }
    }

    public void DropItem()
    {
        int normalItemCount = UnityEngine.Random.Range(minNormalItem, maxNormalItem + 1);
        DropItemsByType(0, Mathf.Min(3, dropitems.Length), normalItemCount);
        if (dropitems.Length > 2)
        {
            int specialItemCount = UnityEngine.Random.Range(minSpecialItem, maxSpecialItem + 1);
            DropItemsByType(3, dropitems.Length, specialItemCount);
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

        for (int i = 0; i < itemCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(startIndex, endIndex);
            GameObject itemToDrop = dropitems[randomIndex];
            if (itemToDrop != null)
            {
                Debug.Log("nama item yang di drop adalah : " + itemToDrop.name);
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                ItemPool.Instance.DropItem(itemToDrop.name, transform.position + offset, itemToDrop);
            }
            else
            {
                Debug.LogWarning($"Item at index {randomIndex} is null.");
            }
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

        currentState = "Lari";
        animalAnimator.Play("Lari");

        currentMovementCoroutine = StartCoroutine(AnimalRunMovement(point1, runSpeed, point2, point3));
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
        currentState = "Idle";
        animalAnimator.Play("Idle");

        StartCoroutine(PlayRandomAnimationPeriodically());
    }


    private void SetRunningAnimation(Vector2 targetPosition)
    {
        StopPreviousAnimation();

        if (targetPosition.x > transform.position.x)
        {
            animalRenderer.flipX = true;
            currentState = "JalanKanan";
            animalAnimator.Play("JalanKanan");
        }
        else if (targetPosition.x < transform.position.x)
        {
            animalRenderer.flipX = false;
            currentState = "JalanKiri";
            animalAnimator.Play("JalanKiri");
        }
    }

    private void StopPreviousAnimation()
    {
        animalAnimator.Play("Idle");
    }

    private IEnumerator MoveToTarget(Vector2 targetPosition, float speed)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
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
                if (itemDrop.itemName == "DagingDomba")
                {
                    Debug.Log($"{namaHewan} (Quest) melihat item quest: {other.name}!");
                    currentTarget = other.transform; // Set target ke item drop
                    currentState = "Mengejar"; // Ubah state menjadi mengejar
                    StopAllCoroutines(); // Hentikan perilaku lain (misal mengikuti player)
                    return; // Penting: hentikan eksekusi lebih lanjut karena target sudah ditemukan
                }
                else
                {
                    // ItemDropInteractable ada, tag-nya sesuai, tapi bukan "DagingDomba".
                    // Harimau harus diam atau kembali ke perilaku sebelumnya jika ada.
                    Debug.Log($"{namaHewan} (Quest) mendeteksi item lain: {other.name}, tapi bukan 'DagingDomba'. Tetap diam.");
                    // Pastikan tidak ada target lain yang sedang dikejar jika ini terjadi
                    // Jika sebelumnya mengejar player, biarkan tetap mengejar player sampai daging domba muncul
                    // atau ubah currentTarget = null; currentState = "Idle"; jika ingin dia berhenti total.
                    // Untuk skenario "diam sampai ada item yang benar", kita harus berhenti mengejar player.
                    if (currentTarget != null && currentTarget.CompareTag("Player"))
                    {
                        currentTarget = null;
                        currentState = "Idle";
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
                currentState = "Mengejar"; // Mulai mengejar player
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
                    currentState = "Idle";
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
                currentState = "Mengejar"; // Mulai mengejar player
                StopAllCoroutines(); // Hentikan perilaku acak jika ada
            }
            else if (tipeHewan == AnimalType.Agresif) // Logika agresif hanya untuk tipe Agresif
            {
                AnimalBehavior otherAnimal = other.GetComponent<AnimalBehavior>();

                if (otherAnimal != null && otherAnimal != this && otherAnimal.tipeHewan == AnimalType.Pasif)
                {
                    Debug.Log($"{namaHewan} (Agresif) melihat mangsa: {other.name}!");
                    currentTarget = other.transform;
                    currentState = "Mengejar";
                    StopAllCoroutines();
                }
                else if (other.CompareTag("Player")) // Agresif juga menyerang Player
                {
                    Debug.Log($"{namaHewan} (Agresif) melihat Player: {other.name}!");
                    currentTarget = other.transform;
                    currentState = "Mengejar";
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
                currentState = "Idle";
                StartCoroutine(PlayRandomAnimationPeriodically());
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
                currentState = "Idle";

                // Untuk hewan isQuest, setelah kehilangan target (selain itemdrop),
                // mungkin kita ingin dia kembali mengejar player
                if (tipeHewan == AnimalType.isQuest && playerTransform != null)
                {
                    Debug.Log($"{namaHewan} (Quest) kembali fokus pada Player.");
                    currentTarget = playerTransform;
                    currentState = "Mengejar";
                }
                else // Jika bukan isQuest atau tidak ada player
                {
                    StartCoroutine(PlayRandomAnimationPeriodically());
                }
            }
            else
            {
                Debug.Log($"Target keluar dari trigger kecil, tapi masih dalam jangkauan deteksi. Pengejaran dilanjutkan.");
            }
        }
    }

    private void ChaseTarget()
    {
        isMoving = true;
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        if (direction.x > 0)
        {
            animalRenderer.flipX = true;
            animalAnimator.Play("JalanKanan");
        }
        else if (direction.x < 0)
        {
            animalRenderer.flipX = false;
            animalAnimator.Play("JalanKiri");
        }
        if (zonaSerangTransform != null && currentTarget != null)
        {
            Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
            zonaSerangTransform.localPosition = directionToTarget * jarakOffsetSerang;
        }
    }

    public void JalankanLogikaSerangan()
    {
        // Harimau dengan tipe AnimalType.isQuest TIDAK AKAN MENYERANG!
        if (tipeHewan == AnimalType.isQuest)
        {
            // Di sini Anda bisa tambahkan logika untuk event quest, misalnya:
            // if (currentTarget.CompareTag("ItemDrop"))
            // {
            //     Debug.Log("Harimau quest berhasil mencapai item drop!");
            //     Destroy(currentTarget.gameObject); // Ambil itemnya
            //     currentTarget = null;
            //     currentState = "Idle";
            //     StartCoroutine(PlayRandomAnimationPeriodically());
            // }
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
        if (zonaSerangTransform == null) return;
        if (animalRenderer.flipX)
        {
            zonaSerangTransform.localPosition = new Vector3(jarakOffsetSerang, 0, 0);
        }
        else
        {
            zonaSerangTransform.localPosition = new Vector3(-jarakOffsetSerang, 0, 0);
        }
    }
}