using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class Enemy_Bandit : MonoBehaviour
{
    // ===================== [ MOVEMENT SETTINGS ] ===================== //
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    private float moveDistance = 3f; // Jarak yang ditempuh setiap kali bergerak
    public bool isMoving;
    public bool isAvoiding = false;
    public bool isChasing = false;
    private bool isReturning = false; // Status untuk mencegah bandit keluar terus-menerus
    private int currentStep = 0; // Langkah saat ini dalam jalur NPC

    // ===================== [ COMPONENT REFERENCES ] ===================== //
    [Header("Component References")]
    public Rigidbody2D rb; // Rigidbody2D untuk physics-based movement
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public GameObject spawner; // Referensi ke Spawner
    private Collider2D spawnerCollider; // Collider dari Spawner
    public Transform hitboxTransform;
    public Transform target;

    // ===================== [ STATE VARIABLES ] ===================== //
    [Header("State Variables")]
    public bool isStory;
    public bool isAttacking;
    public bool isDead;
    public Vector2 lastDirection = Vector2.down; // Default menghadap bawah
    private Vector2 previousPosition;
    public Vector2 targetPosition;
    public Vector2 lastCollisionPoint; // Menyimpan posisi terakhir sentuhan dengan objek
    private Coroutine currentCoroutine;
    private Coroutine chaseCoroutine;

    // ===================== [ DROP ITEM SETTINGS ] ===================== //
    [Header("Drop Item Settings")]
    public GameObject[] dropitems; // Array item yang bisa dijatuhkan
                                   //element 0 harus selalu di isi dengan prefab item normal
                                   //element 1 harus selalu di isi dengan prefab item normal
                                   //element 3 harus selalu di isi dengan prefab item normal
                                   //element 4 dan seterusnya boleh di isi dengan barang spesial milik enemy yang akan di drop

    [Tooltip("Minimal dan maksimal item normal yang bisa dijatuhkan.")]
    public int minNormalItem = 1;
    public int maxNormalItem = 2;

    [Tooltip("Minimal dan maksimal item spesial yang bisa dijatuhkan.")]
    public int minSpecialItem = 0;
    public int maxSpecialItem = 1;

    // Drop rates untuk masing-masing item (0 = 70%, 1 = 50%, dst.)
    private float[] dropRates = { 0.7f, 0.5f, 0.3f, 0.1f, 0.05f };

    // ===================== [ MOVEMENT LOGIC ] ===================== //
    [Header("Movement Logic")]
    // Array arah pergerakan utama
    private Vector2[] moveDirections = new Vector2[]
    {
        new Vector2(-1, 0),  // Kiri
        new Vector2(1, 0),   // Kanan
        new Vector2(0, 1),   // Atas
        new Vector2(0, -1)   // Bawah
    };

    public Vector2[] pergerakanNPC; // Jalur pergerakan NPC
    public int jumlahPergerakanTarget = 5; // Jumlah langkah sebelum reset jalur

    private void Start()
    {
        if (hitboxTransform == null)
        {
            Debug.LogWarning("Hitbox kosong wak");
        }

        if (spawner != null)
        {
            spawnerCollider = spawner.GetComponent<Collider2D>();
            if (spawnerCollider == null)
            {
                Debug.LogError("Spawner tidak memiliki Collider2D! Pastikan menambahkan BoxCollider2D dan aktifkan 'isTrigger'.");
            }
        }
        else
        {
            Debug.LogError("Spawner tidak ditemukan! Pastikan bandit memiliki referensi ke Spawner.");
        }

        // Inisialisasi array pergerakan
        pergerakanNPC = new Vector2[jumlahPergerakanTarget];

        rb = GetComponent<Rigidbody2D>(); // Pastikan rb mendapatkan Rigidbody2D dari GameObject

        // Tentukan jalur pertama kali
        PushtoArrayPergerakanNPC();

        // Mulai proses pergerakan NPC
        StartCoroutine(FollowPath());
    }


    private void Update()
    {
        // Hitung arah gerakan dari posisi sebelumnya
        Vector2 movement = ((Vector2)transform.position - previousPosition).normalized;

        // Update animasi berdasarkan gerakan
        UpdateAnimation(movement);

        // Simpan posisi sekarang sebagai referensi untuk frame berikutnya
        previousPosition = transform.position;
        UpdateHitboxPosition();
    }


    // Fungsi untuk mendapatkan arah acak
    private Vector2 GetRandomDirection()
    {
        int randomIndex = Random.Range(0, moveDirections.Length);
        Vector2 chosenDirection = moveDirections[randomIndex];

        if (randomIndex == 0 || randomIndex == 1)
        {
            float verticalOffset = Random.Range(-0.5f, 0.5f);
            chosenDirection += new Vector2(0, verticalOffset);
        }
        else if (randomIndex == 2 || randomIndex == 3)
        {
            float horizontalOffset = Random.Range(-0.5f, 0.5f);
            chosenDirection += new Vector2(horizontalOffset, 0);
        }

        chosenDirection = chosenDirection.normalized;

        Debug.Log("Arah gerakan bandit: " + chosenDirection); // Debugging
        return chosenDirection;
    }


    // Fungsi untuk mengisi array pergerakan NPC
    private void PushtoArrayPergerakanNPC()
    {
        for (int i = 0; i < jumlahPergerakanTarget; i++)
        {
            pergerakanNPC[i] = GetRandomDirection();
        }
    }

    // Coroutine untuk mengikuti jalur yang sudah dibuat
    private IEnumerator FollowPath()
    {
        while (true)
        {
            if (currentStep >= pergerakanNPC.Length) // Jika sudah mencapai akhir jalur
            {
                yield return new WaitForSeconds(Random.Range(2f, 5f)); // Tunggu sebelum memilih jalur baru
                PushtoArrayPergerakanNPC(); // Buat jalur baru
                currentStep = 0; // Reset langkah ke awal
            }

            Vector2 direction = pergerakanNPC[currentStep]; // Ambil arah dari array
            targetPosition = (Vector2)transform.position + (direction * moveDistance); // Tentukan tujuan

            isMoving = true;
            yield return StartCoroutine(MoveToTarget(targetPosition)); // Bergerak ke target

            isMoving = false;
            
            yield return new WaitForSeconds(0.5f); // Tunggu satu detik sebelum lanjut ke titik selanjutnya
            currentStep++; // Lanjut ke langkah berikutnya dalam jalur
        }
    }

    // Fungsi untuk memindahkan NPC ke target
    private IEnumerator MoveToTarget(Vector2 targetPos)
    {
        while (Vector2.Distance(transform.position, targetPos) > 0.1f) // Jika belum sampai tujuan
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // Fungsi untuk mengatur animasi berdasarkan arah gerakan
    public void UpdateAnimation(Vector2 movement)
    {
        if (animator == null)
        {
            Debug.LogError("Animator belum di-assign!");
            return;
        }

        movement = movement.normalized;
        bool isMoving = movement != Vector2.zero;

        if (isMoving)
        {
            // Simpan arah terakhir saat bergerak, agar animasi idle mengikuti arah ini
            lastDirection = movement;

            // Set parameter untuk Blend Tree berjalan
            animator.SetFloat("MoveX", Mathf.Round(movement.x)); // -1, 0, 1
            animator.SetFloat("MoveY", Mathf.Round(movement.y));
        }
        else
        {
            // Gunakan arah terakhir saat idle
            animator.SetFloat("IdleX", Mathf.Round(lastDirection.x));
            animator.SetFloat("IdleY", Mathf.Round(lastDirection.y));
        }

        // Atur Speed untuk blend tree berjalan
        animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Tree") || collision.gameObject.CompareTag("Stone"))
        {
            Debug.Log("Nabrak Objek: " + collision.gameObject.name);

            // Jika bandit sedang mengejar player, hentikan pengejaran
            if (isChasing)
            {
                StopChasing(); // Hentikan pengejaran
                isChasing = false; // Set status tidak mengejar
                isMoving = true;
            }

            // Jika bandit masih bergerak dan belum menghindar
            if (isMoving && !isAvoiding)
            {
                isAvoiding = true;
                StopAllCoroutines(); // Hentikan semua coroutine agar bandit tidak terus mengejar

                Debug.Log("Nabrak masee berenti dulu ");

                // Jalankan fungsi menghindar
                StartCoroutine(DelayBeforeAvoiding(collision));
            }
        }
    }


    private IEnumerator DelayBeforeAvoiding(Collision2D collision)
    {
        yield return new WaitForSeconds(2f); // Bandit berhenti selama 2 detik

        Debug.Log("Mulai menghindar...");

        // Ambil arah menghindar
        Vector2 avoidanceDirection = GetAvoidanceDirection(collision.transform.position);

        // Isi array pergerakan dengan arah baru agar NPC menjauh
        for (int i = 0; i < pergerakanNPC.Length; i++)
        {
            pergerakanNPC[i] = avoidanceDirection;
        }

        // Reset langkah ke awal agar NPC langsung mengikuti jalur baru
        currentStep = 0;
        isAvoiding = false; // Selesai menghindar, bisa bergerak lagi

        // Mulai kembali coroutine pergerakan
        StartCoroutine(FollowPath());
    }


    private Vector2 GetAvoidanceDirection(Vector2 collisionPoint)
    {
        // Hitung arah menjauh dari titik tabrakan
        Vector2 avoidanceDirection = ((Vector2)transform.position - collisionPoint).normalized;

        // Pastikan NPC tidak diam, jika hasilnya nol, beri arah acak
        if (avoidanceDirection == Vector2.zero)
        {
            avoidanceDirection = GetRandomDirection();
        }

        Debug.Log("Arah menghindar: " + avoidanceDirection);
        return avoidanceDirection;
    }

  

    void UpdateHitboxPosition()
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;

            // Ambil ukuran Collider2D bandit (untuk memastikan hitbox berada di luar collider)
            Collider2D banditCollider = GetComponent<Collider2D>();
            if (banditCollider != null)
            {
                Vector2 banditSize = banditCollider.bounds.extents; // Ukuran setengah dari collider
                float offsetX = banditSize.x + 0.2f; // Offset ke depan
                float offsetY = banditSize.y + 0.2f; // Offset ke atas/bawah

                // Atur posisi hitbox di luar collider bandit
                Vector3 hitboxOffset = new Vector3(direction.x * offsetX, direction.y * offsetY, 0);
                hitboxTransform.localPosition = hitboxOffset;
            }
            else
            {
                Debug.LogWarning("Collider2D tidak ditemukan pada bandit!");
            }

            // Simpan arah terakhir hitbox
            lastDirection = direction;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError("SetTarget() dipanggil tapi newTarget NULL!");
            return;
        }

        Debug.Log("Bandit mulai mengejar target: " + newTarget.name);

        target = newTarget;
        isChasing = true;
        isReturning = false;

        StopAllCoroutines(); // Hentikan semua coroutine lain
        StartCoroutine(ChaseTarget());
    }

    public void StartChasing()
    {
        StopChasing(); // Hentikan coroutine sebelumnya jika ada
        if (!isAttacking) // Hanya mulai pengejaran jika tidak sedang menyerang
        {
            chaseCoroutine = StartCoroutine(ChaseTarget());
        }
    }


    public void StopChasing()
    {
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
            Debug.Log("Pengejaran dihentikan karena menabrak objek!");
        }
    }



    public IEnumerator ChaseTarget()
    {
        Debug.Log("ChaseTarget() dijalankan!");

        while (isChasing)
        {
            if (target != null && !isAttacking) // Cegah pergerakan saat menyerang
            {
                Debug.Log("Mengejar " + target.name + " ke " + target.position);
                Vector2 direction = (target.position - transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            }
            else if (isAttacking)
            {
                Debug.Log("Bandit sedang menyerang, hentikan pengejaran.");
                yield break; // Hentikan coroutine pengejaran
            }

            yield return null;
        }

        Debug.Log("Berhenti mengejar");
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) // Cek apakah bandit masih aktif
        {
            Debug.LogWarning("Bandit dinonaktifkan, tidak bisa menjalankan coroutine!");
            return;
        }

        if (collision.gameObject == spawner && !isReturning)
        {
            Debug.Log("Bandit keluar dari area! Kembali ke dalam.");
            isReturning = true;
            isChasing = false; // Hentikan pengejaran

            StopAllCoroutines(); // Hentikan semua gerakan sebelum kembali ke Spawner
            StartCoroutine(BackToSpawner()); // Jalankan coroutine dengan aman
        }
    }



    private IEnumerator BackToSpawner()
    {
        while (isReturning) // Loop hanya berjalan jika sedang kembali
        {
            transform.position = Vector2.MoveTowards(transform.position, spawner.transform.position, moveSpeed * Time.deltaTime);

            // Jika sudah dekat dengan spawner, hentikan kembali
            if (Vector2.Distance(transform.position, spawner.transform.position) < 0.1f)
            {
                isReturning = false;
                Debug.Log("Bandit kembali ke spawner!");
                yield break; // Keluar dari coroutine
            }

            yield return null;
        }
    }


    private void StopCurrentCoroutine()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
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

        if (lastDirection.y > 0.5f) triggerName += "Atas";
        else if (lastDirection.y < -0.5f) triggerName += "Bawah";
        else if (lastDirection.x > 0.5f) triggerName += "Kanan";
        else if (lastDirection.x < -0.5f) triggerName += "Kiri";

        animator.SetTrigger(triggerName);

        // Tunggu sampai animasi selesai sebelum mengubah state ke Idle
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.ResetTrigger(triggerName); // Reset Trigger setelah animasi selesai
    }


    public void DropItem()
    {

        //if (dropitems == null || dropitems.Length < 3)
        //{
        //    Debug.LogWarning("Drop items array is not properly configured. Ensure at least 3 items exist.");
        //    return;
        //}
        int normalItemCount = Random.Range(minNormalItem, maxNormalItem + 1);
        DropItemsByType(0, Mathf.Min(3, dropitems.Length), normalItemCount);
        if (dropitems.Length > 3) // Jika ada special items
        {
            int specialItemCount = Random.Range(minSpecialItem, maxSpecialItem + 1);
            DropItemsByType(3, dropitems.Length, specialItemCount);
        }
    }
    // Fungsi untuk menjatuhkan item berdasarkan rentang index tertentu
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
            int randomIndex = Random.Range(startIndex, endIndex);
            GameObject itemToDrop = dropitems[randomIndex];

            // Periksa apakah item ini lolos drop rate
            if (Random.value <= dropRates[randomIndex]) // Random.value menghasilkan angka antara 0-1
            {
                if (itemToDrop != null)
                {
                    Debug.Log("Item yang dijatuhkan: " + itemToDrop.name);
                    Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                    ItemPool.Instance.DropItem(itemToDrop.name, transform.position + offset, itemToDrop);
                }
            }
            else
            {
                Debug.Log("Item " + itemToDrop.name + " tidak jatuh karena gagal drop rate.");
            }
        }
        Destroy(gameObject);
    }

}
