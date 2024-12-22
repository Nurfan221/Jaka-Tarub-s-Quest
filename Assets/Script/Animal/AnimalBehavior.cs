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
    public string currentState;  // Set awal ke Idle


    public enum Jenis
    {
        None,
        HewanBuas,
        HewanBuruan
    }

    //[Header("Animasi")]
    [SerializeField] private Animator animalAnimator;
    public SpriteRenderer animalRenderer;


    public string namaHewan;
    public float health;
    public Jenis jenisHewan;

  
    //element 0 harus selalu di isi dengan prefab daging
    //element 1 harus selalu di isi dengan animalSkin
    //element 3 harus selalu di isi dengan animalBone
    //element 4 dan seterusnya boleh di isi dengan barang spesial milik hewan yang akan di drop

    public GameObject[] dropitems;

    // Logika menentukan jumlah minimal dan maksimal dari item yang akan dijatuhkan
    public int minNormalItem = 1;  // Jumlah minimum normal Item
    public int maxNormalItem = 2;  // Jumlah maksimum normal Item
    public int minSpecialItem = 0; // Jumlah item spesial seperti Tulang dll
    public int maxSpecialItem = 1;

    //public Transform circleCenter; // Pusat lingkaran
    public float circleRadius = 10f; // Radius lingkaran
    public float moveSpeed = 5f; // Kecepatan gerak hewan
    public float escapeDuration = 3f; // Waktu untuk berlari ke sana kemari sebelum keluar
    private bool isEscaping = false;

    // Untuk pergerakan hewan
    private bool isMoving = false;
    private float moveDuration = 2f; // Durasi pergerakan hewan
    private Vector2 moveDirection;

    // Untuk kegiatan makan dan tidur
    private bool isEating = false;
    private bool isSleeping = false;
    private float eatDuration = 3f; // Durasi makan
    private float sleepDuration = 5f; // Durasi tidur

    private void Start()
    {
        // Mulai pergerakan jika hewan tidak sedang tidur atau makan
        //StartCoroutine(AnimalMovement());

        currentState = "Idle";  // Set awal ke Idle
        PlayAnimation(currentState);

        // Mulai random animation setelah 3 detik dan setiap 5 detik
        InvokeRepeating(nameof(PlayRandomAnimation), 3f, 5f);
    }

    public void TakeDamage(int damage)
    {
        health -= Mathf.Min(damage, health);
        Debug.Log($"{namaHewan} terkena damage. Sisa HP: {health}");

        if (health <= 0)
        {
            DropItem();
            Destroy(gameObject); // Pastikan objek dihapus setelah drop
        }
        else if (jenisHewan == Jenis.HewanBuruan)
        {
            Kabur();
        }
    }


    public void Kabur()
    {
        Debug.Log("Arghhh terkena damage...");
        if (!isEscaping)
        {
            isEscaping = true;
            StartCoroutine(EscapeBehavior());
        }
    }

    private IEnumerator EscapeBehavior()
    {
        float elapsedTime = 0f;

        // Tahap 1: Berlari ke sana kemari
        while (elapsedTime < escapeDuration)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized; // Arah acak
            Vector2 targetPosition = (Vector2)transform.position + randomDirection * moveSpeed;

            // Gerakkan hewan ke posisi acak
            while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            elapsedTime += 0.5f; // Interval antara gerakan
        }

        // Setelah selesai bergerak, kelinci akan menghilang
        Debug.Log($"{namaHewan} telah kabur dan menghilang.");
        Destroy(gameObject);  // Menghilangkan objek setelah selesai kabur
    }

    private void DropItem()
    {
        if (dropitems == null || dropitems.Length < 3)
        {
            Debug.LogWarning("Drop items array is not properly configured. Ensure at least 3 items exist.");
            return;
        }

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
            int randomIndex = Random.Range(startIndex, endIndex);
            GameObject itemToDrop = dropitems[randomIndex];

            if (itemToDrop != null)
            {
                Debug.Log("nama item yang di drop adalah : " + itemToDrop.name);

                Instantiate(itemToDrop, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"Item at index {randomIndex} is null.");
            }
        }
    }




    // Pergerakan hewan saat animasi JalanKanan atau JalanKiri
    private IEnumerator AnimalMovement(string currentAnimation)
    {
        // Menunggu animasi selesai sebelum melanjutkan logika berikutnya
        yield return new WaitForEndOfFrame();  // Pastikan animasi sudah diputar pertama kali

        // Tentukan arah pergerakan berdasarkan animasi
        if (currentAnimation == "JalanKanan")
        {
            moveDirection = Vector2.left;  // Gerak ke kanan
        }
        else if (currentAnimation == "JalanKiri")
        {
            moveDirection = Vector2.right;   // Gerak ke kiri

        }

        // Mulai pergerakan
        isMoving = true;
        yield return MoveForDuration();  // Gerak selama durasi tertentu
        isMoving = false;

        // Setelah bergerak, animasi bisa dilanjutkan
        if (jenisHewan == Jenis.HewanBuruan)
        {
            isEating = true;
        }
        else if (jenisHewan == Jenis.HewanBuas)
        {
            isSleeping = true;
        }
    }



    // Fungsi untuk gerak selama durasi tertentu
    private IEnumerator MoveForDuration()
    {
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = (Vector2)startPosition + moveDirection * 5f; // Jarak pergerakan hewan

        // Gerakkan hewan ke kiri atau kanan
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Setelah bergerak, hewan akan berhenti untuk makan atau tidur
        if (jenisHewan == Jenis.HewanBuruan)
        {
            isEating = true;
        }
        else if (jenisHewan == Jenis.HewanBuas)
        {
            isSleeping = true;
        }

        // Setelah makan atau tidur, kelanjutan aktivitas
        yield return new WaitForSeconds(2f);
    }

    //private void OnDrawGizmos()
    //{
    //    if (circleCenter != null)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawWireSphere(circleCenter.position, circleRadius);
    //    }
    //}

    public void TransitionTo(string nextState)
    {
        // Cek apakah transisi diperbolehkan
        AnimationState state = System.Array.Find(animationStates, s => s.stateName == currentState);

        if (state != null && System.Array.Exists(state.availableStates, s => s == nextState))
        {
            PlayAnimation(nextState);
            currentState = nextState;
        }
        else
        {
            Debug.LogWarning($"Transisi dari {currentState} ke {nextState} tidak diperbolehkan.");
        }
    }

    private void PlayAnimation(string state)
    {
        // Hentikan animasi sebelumnya jika ada
        if (state == "JalanKiri")
        {
            animalRenderer.flipX = true;
        }
        else if (state == "JalanKanan")
        {
            animalRenderer.flipX = false;  // Set flipX ke false untuk JalanKanan
        }

        animalAnimator.Play(state);

        // Jika animasi adalah JalanKanan atau JalanKiri, mulai pergerakan
        if (state == "JalanKanan" || state == "JalanKiri")
        {
            // Pastikan animasi gerakan dimulai dan menunggu sampai selesai
            StartCoroutine(AnimalMovement(state));
        }
        else if (state == "Duduk")
        {
            StartCoroutine(TransitionToIdle("Rebahan", 0.8f));  // Delay 0.8 detik
        }
        else if (state == "Berdiri")
        {
            StartCoroutine(TransitionToIdle("Idle", 0.8f));  // Kembali ke Idle setelah 0.8 detik
        }
    }

    // Coroutine untuk delay transisi ke Idle
    private IEnumerator TransitionToIdle(string nextState, float delay)
    {
        yield return new WaitForSeconds(delay);
        animalAnimator.Play(nextState);
        currentState = nextState;
    }


    // Random Animation dari availableStates
    private void PlayRandomAnimation()
    {
        AnimationState state = System.Array.Find(animationStates, s => s.stateName == currentState);

        if (state != null && state.availableStates.Length > 0)
        {
            int randomIndex = Random.Range(0, state.availableStates.Length);
            string randomState = state.availableStates[randomIndex];

            TransitionTo(randomState);
        }
        else
        {
            Debug.LogWarning($"Tidak ada animasi yang tersedia untuk {currentState}");
        }
    }
}
