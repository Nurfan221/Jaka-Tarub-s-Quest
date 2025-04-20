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
        while (true)  // Loop terus-menerus
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
            animalAnimator.Play("Rebahan");
            currentState = "Rebahan";
        }
        else if (nextState == "Berdiri")
        {
            animalAnimator.Play("Idle");
            currentState = "Idle";
        }
        else if (nextState == "Makan")
        {
            animalAnimator.Play("Mengunyah");
            currentState = "Mengunyah";
        }
        else if (nextState == "JalanKanan" || nextState == "JalanKiri")
        {
            animalAnimator.Play(nextState);
            currentState = nextState;
            StartCoroutine(AnimalMovement(nextState));
        }
        else
        {
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
        // Misalnya, jika hewan menyentuh objek dengan tag "Environment"
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Tree") || collision.gameObject.CompareTag("Stone"))
        {
            //Debug.Log("Nabrak OYYY");

            // Menyimpan posisi objek yang disentuh
            // Menyimpan posisi objek yang disentuh
            lastCollisionPoint = collision.transform.position;

            // Jika hewan sedang bergerak, langsung jalankan pergerakan menjauhi objek
            if (isMoving && !isAvoiding)
            {
                isAvoiding = true;
                StopCoroutine("AnimalMovement");  // Hentikan pergerakan yang sedang berlangsung
                StartCoroutine(MoveAwayFromObstacle(collision));  // Mulai pergerakan menjauhi objek
            }
        }
    }

    private IEnumerator MoveAwayFromObstacle(Collision2D collision)
    {
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

    public void TakeDamage(float damage)
    {
        Debug.Log("take Damage" + damage);
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth); // Pastikan tidak kurang dari 0 atau lebih dari maxHealth

        if (health <= 0)
        {
            Die();
        }


    }

    void Die()
    {
        DropItem();
        Debug.Log(gameObject.name + " has died.");
        gameObject.SetActive(false);
    }


}