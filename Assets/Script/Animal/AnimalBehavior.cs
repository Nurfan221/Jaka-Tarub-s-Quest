using System.Collections;
using System.Collections.Generic; // Diperlukan jika menggunakan List
using Unity.VisualScripting;
using UnityEngine;

public class AnimalBehavior : MonoBehaviour
{

    [System.Serializable]
    public class AnimationState
    {
        public string stateName;         // Nama state (Idle, Jalan, Makan)
        public string[] availableStates; // Array animasi yang bisa dituju
    }

    public Sprite[] animalIdle;


    public AnimationState[] animationStates; // Array semua animasi dan transisinya
    public string currentState = "Idle";  // Set awal ke Idle
    public float jedaAnimasi = 3f;
    public bool isMoving;
    public float moveSpeed;
    private Vector2 lastCollisionPoint; // Menyimpan posisi objek yang disentuh
    private bool isAvoiding = false;    // Menandakan apakah hewan sedang menghindar
    public bool isAnimalEvent;
    private Coroutine currentMovementCoroutine;




    //[Header("Animasi")]
    [SerializeField] private Animator animalAnimator;
    public SpriteRenderer animalRenderer;


    public string namaHewan;
    public float health;
    public int maxHealth;



    //element 0 harus selalu di isi dengan prefab daging
    //element 1 harus selalu di isi dengan animalSkin
    //element 3 harus selalu di isi dengan animalBone
    //element 4 dan seterusnya boleh di isi dengan barang spesial milik hewan yang akan di drop

    public GameObject[] dropitems;
    // Logika menentukan jumlah minimal dan maksimal dari item yang akan dijatuhkan
    public int minNormalItem = 1;  // Jumlah minimum normal item
    public int maxNormalItem = 2;  // Jumlah maksimum normal item
    public int minSpecialItem = 0; // Jumlah item spesial seperti copper, iron, atau gold
    public int maxSpecialItem = 1;


    private void Start()
    {
        // Mulai Coroutine untuk memilih dan menjalankan animasi secara otomatis
        StartCoroutine(PlayRandomAnimationPeriodically());

        health = maxHealth;
    }

    private IEnumerator PlayRandomAnimationPeriodically()
    {

        // Jika bukan jam tidur, jalankan animasi acak
        while (true)  // Loop terus-menerus untuk animasi acak
        {
            // Pilih animasi acak berdasarkan currentState
            string nextState = GetRandomAnimationForCurrentState();

            // Jalankan animasi yang terpilih
            TransitionTo(nextState);

            // Tunggu selama 5 detik sebelum memilih animasi baru
            yield return new WaitForSeconds(5f);
        }
    }
    


    private string GetRandomAnimationForCurrentState()
    {
        // Cari state animasi yang sedang aktif
        AnimationState state = System.Array.Find(animationStates, s => s.stateName == currentState);

        // Jika ada animasi yang tersedia untuk state ini
        if (state != null && state.availableStates.Length > 0)
        {
            // Pilih animasi acak dari availableStates
            int randomIndex = Random.Range(0, state.availableStates.Length);
            return state.availableStates[randomIndex];
        }


        // Jika tidak ada animasi yang tersedia, kembalikan animasi default
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
        // Jalankan animasi sementara (nextState) terlebih dahulu
        animalAnimator.Play(nextState);
        //Debug.Log("Playing animation: " + nextState);

        // Tunggu hingga animasi selesai
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0);
        });

        // Setelah animasi sementara selesai, jalankan animasi target
        if (nextState == "Duduk")
        {
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0);
            });
            animalAnimator.Play("Rebahan");
            currentState = "Rebahan";
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = animalAnimator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.normalizedTime >= 1 && !animalAnimator.IsInTransition(0);
            });
            animalAnimator.Play("TidurNyenyak");
            currentState = "TidurNyenyak";
        }
        else if(nextState == "Rebahan")
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

        //Debug.Log("Transitioned to: " + currentState);
    }

    private IEnumerator AnimalMovement(string currentAnimation)
    {
        //Debug.Log("animal mencarai jalan");
        // Tentukan arah pergerakan berdasarkan animasi
        Vector2 randomDirection = Vector2.zero;

        // Cek apakah animasi yang dipilih adalah JalanKanan atau JalanKiri
        if (currentAnimation == "JalanKanan")
        {
            randomDirection = new Vector2(1, Random.Range(-1f, 1f)); // Kiri dengan variasi atas/bawah
            //Debug.Log("Moving Left");
            animalRenderer.flipX = true;
        }
        else if (currentAnimation == "JalanKiri")
        {
            randomDirection = new Vector2(-1, Random.Range(-1f, 1f)); // Kanan dengan variasi atas/bawah
            //Debug.Log("Moving Right");
            animalRenderer.flipX = false;
        }


        // Tentukan posisi target berdasarkan arah pergerakan yang sudah dipilih
        Vector2 targetPosition = (Vector2)transform.position + randomDirection * 3f; // Target posisi gerakan
        //Debug.Log("Target Position: " + targetPosition);

        // Mulai pergerakan menuju target
        isMoving = true;
        yield return MoveForDuration(targetPosition);  // Gerak menuju posisi target
        isMoving = false;

        // Setelah bergerak, beri waktu untuk makan atau tidur
        yield return new WaitForSeconds(5); // Waktu tunggu sebelum kembali ke aktivitas lain
    }

    private IEnumerator MoveForDuration(Vector2 targetPosition)
    {
        Vector2 startPosition = transform.position;

        // Cek jarak target
        //Debug.Log("Starting to move. Current position: " + startPosition + ", Target position: " + targetPosition);

        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)  // Selama jarak masih ada
        {
            // Gerakkan objek dengan kecepatan yang sudah diatur
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;  // Tunggu frame berikutnya
        }

        //Debug.Log("Movement finished!");
        // Setelah bergerak, beri waktu untuk makan atau tidur
        yield return new WaitForSeconds(3f); // Waktu tunggu sebelum kembali ke aktivitas lain
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Tree") || collision.gameObject.CompareTag("Stone")|| collision.gameObject.CompareTag("Animal")|| collision.gameObject.CompareTag("Tebing"))
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
        // Menghitung arah untuk menjauh dari objek yang memiliki tag "Environment"
        Vector2 directionAwayFromObstacle = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

        // Tentukan posisi target untuk bergerak menjauhi objek
        Vector2 targetPosition = (Vector2)transform.position + directionAwayFromObstacle * 5f;  // Menjauh sejauh 5 unit dari objek

        // Gerakkan hewan ke arah target
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)  // Selama jarak masih ada
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;  // Tunggu frame berikutnya
        }

        // Setelah bergerak menjauhi objek, kembalikan status dan lanjutkan pergerakan acak
        isAvoiding = false;
        StartCoroutine(AnimalMovement(currentState));  // Mulai pergerakan acak lagi setelah menghindar
    }

    public void DropItem()
    {
        int normalItemCount = Random.Range(minNormalItem, maxNormalItem + 1);
        DropItemsByType(0, Mathf.Min(3, dropitems.Length), normalItemCount);
        if (dropitems.Length >2) // Jika ada special items
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
        // Pastikan rentang indeks valid
        if (startIndex < 0 || endIndex > dropitems.Length || startIndex >= endIndex)
        {
            Debug.LogError($"Invalid index range: startIndex={startIndex}, endIndex={endIndex}, arrayLength={dropitems.Length}");
            return;
        }

        for (int i = 0; i < itemCount; i++)
        {
            //Debug.Log("for untuk perulangan drop item");
            int randomIndex = Random.Range(startIndex, endIndex);
            GameObject itemToDrop = dropitems[randomIndex];
            if (itemToDrop != null)
            {
                Debug.Log("nama item yang di drop adalah : " + itemToDrop.name);
                //Instantiate(itemToDrop, transform.position, Quaternion.identity);
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                ItemPool.Instance.DropItem(itemToDrop.name, transform.position + offset, itemToDrop);
            }
            else
            {
                Debug.LogWarning($"Item at index {randomIndex} is null.");
            }
        }
        Destroy(gameObject);
    }

    public void TakeDamage(float damage )
    {
        Debug.Log("take Damage" + damage);
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth); // Pastikan tidak kurang dari 0 atau lebih dari maxHealth

        if (health <= 0)
        {
            Die();
        }else
        {
            Run();
        }


    }

    public void Run()
    {
        float runSpeed = 5f;  // Kecepatan lari lebih cepat dari kecepatan normal
        Vector2 currentPosition = transform.position; // Posisi awal

        // Pilih tiga titik acak untuk lari
        Vector2 point1 = GetRandomPoint(currentPosition);
        Vector2 point2 = GetRandomPoint(point1);
        Vector2 point3 = GetRandomPoint(point2);

        // Menjalankan animasi lari
        currentState = "Lari";
        animalAnimator.Play("Lari");

        // Mulai pergerakan menuju titik-titik acak secara bertahap
        currentMovementCoroutine = StartCoroutine(AnimalRunMovement(point1, runSpeed, point2, point3));
    }

    private IEnumerator AnimalRunMovement(Vector2 point1, float speed, Vector2 point2, Vector2 point3)
    {
        // Tentukan arah animasi berdasarkan perbandingan posisi point1 dengan posisi saat ini
        SetRunningAnimation(point1);

        // Gerak menuju titik pertama
        yield return MoveToTarget(point1, speed);

        // Tentukan arah animasi untuk point2
        SetRunningAnimation(point2);

        // Gerak menuju titik kedua setelah mencapai titik pertama
        yield return MoveToTarget(point2, speed);

        // Tentukan arah animasi untuk point3
        SetRunningAnimation(point3);

        // Gerak menuju titik ketiga setelah mencapai titik kedua
        yield return MoveToTarget(point3, speed);

        // Setelah mencapai titik ketiga, kembali ke animasi normal
        yield return new WaitForSeconds(3f); // Misalnya berhenti berlari selama 3 detik
        currentState = "Idle";
        animalAnimator.Play("Idle");

        // Mulai Coroutine untuk memilih dan menjalankan animasi secara otomatis
        StartCoroutine(PlayRandomAnimationPeriodically());
    }


    private void SetRunningAnimation(Vector2 targetPosition)
    {
        // Hentikan animasi sebelumnya dengan memulai animasi baru (lari)
        StopPreviousAnimation();

        // Tentukan arah berdasarkan posisi target dibandingkan dengan posisi hewan
        if (targetPosition.x > transform.position.x) // Target di kanan
        {
            animalRenderer.flipX = true;  // Menunjukkan animasi lari ke kanan
            currentState = "JalanKanan";  // Ganti state ke JalanKanan
            animalAnimator.Play("JalanKanan");  // Mainkan animasi JalanKanan
        }
        else if (targetPosition.x < transform.position.x) // Target di kiri
        {
            animalRenderer.flipX = false;   // Menunjukkan animasi lari ke kiri
            currentState = "JalanKiri";   // Ganti state ke JalanKiri
            animalAnimator.Play("JalanKiri");  // Mainkan animasi JalanKiri
        }
    }

    private void StopPreviousAnimation()
    {
        // Menghentikan animasi sebelumnya
        // Anda bisa menggunakan SetTrigger atau SetBool untuk memastikan animasi sebelumnya dihentikan dengan transisi yang halus
        animalAnimator.Play("Idle");  // Sebagai contoh, hentikan animasi sebelumnya dan setel ke "Idle" sebelum berlari
    }



    private IEnumerator MoveToTarget(Vector2 targetPosition, float speed)
    {
        // Gerakkan hewan menuju posisi target
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;  // Tunggu frame berikutnya
        }
    }

    private Vector2 GetRandomPoint(Vector2 lastPosition)
    {
        // Pilih titik acak dalam jarak tertentu dari posisi terakhir
        float randomX = Random.Range(-5f, 5f);  // Jarak acak dalam sumbu X
        float randomY = Random.Range(-5f, 5f);  // Jarak acak dalam sumbu Y
        return lastPosition + new Vector2(randomX, randomY);  // Titik acak berdasarkan posisi terakhir
    }

    void Die()
    {
        DropItem();
        Debug.Log(gameObject.name + " has died.");
        gameObject.SetActive(false);
    }


}