using System.Collections;
using System.Collections.Generic; // Diperlukan jika menggunakan List
using UnityEngine;

public class AnimalBehavior : MonoBehaviour
{
    [System.Serializable]
    public class AnimationState
    {
        public string stateName;         // Nama state (Idle, Jalan, Makan)
        public string[] availableStates; // Array animasi yang bisa dituju
    }

    
    public AnimationState[] animationStates; // Array semua animasi dan transisinya
    public string currentState = "Idle";  // Set awal ke Idle
    public float jedaAnimasi = 3f;
    public bool isMoving;
    public float moveSpeed;
    private Vector2 lastCollisionPoint; // Menyimpan posisi objek yang disentuh
    private bool isAvoiding = false;    // Menandakan apakah hewan sedang menghindar




    //[Header("Animasi")]
    [SerializeField] private Animator animalAnimator;
    public SpriteRenderer animalRenderer;


    public string namaHewan;
    public float health;
  


    //element 0 harus selalu di isi dengan prefab daging
    //element 1 harus selalu di isi dengan animalSkin
    //element 3 harus selalu di isi dengan animalBone
    //element 4 dan seterusnya boleh di isi dengan barang spesial milik hewan yang akan di drop

    public GameObject[] dropitems;


    private void Start()
    {
        // Mulai Coroutine untuk memilih dan menjalankan animasi secara otomatis
        StartCoroutine(PlayRandomAnimationPeriodically());
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Misalnya, jika hewan menyentuh objek dengan tag "Environment"
        if (other.CompareTag("Environment"))
        {
            //Debug.Log("Nabrak OYYY");

            // Menyimpan posisi objek yang disentuh
            lastCollisionPoint = other.transform.position;

            // Jika hewan sedang bergerak, langsung jalankan pergerakan menjauhi objek
            if (isMoving && !isAvoiding)
            {
                isAvoiding = true;
                StopCoroutine("AnimalMovement");  // Hentikan pergerakan yang sedang berlangsung
                StartCoroutine(MoveAwayFromObstacle(other));  // Mulai pergerakan menjauhi objek
            }
        }
    }

    private IEnumerator MoveAwayFromObstacle(Collider2D other)
    {
        // Menghitung arah untuk menjauh dari objek yang memiliki tag "Environment"
        Vector2 directionAwayFromObstacle = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;

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



}