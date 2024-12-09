using System.Collections;
using System.Collections.Generic; // Diperlukan jika menggunakan List
using UnityEngine;

public class AnimalBehavior : MonoBehaviour
{
    public enum Jenis
    {
        None,
        HewanBuas,
        HewanBuruan
    }

    public string namaHewan;
    public float health;
    public Jenis jenisHewan;

  
    //element 0 harus selalu di isi dengan prefab daging
    //element 1 harus selalu di isi dengan animalSkin
    //element 3 harus selalu di isi dengan animalBone
    //element 4 dan seterusnya boleh di isi dengan barang spesial milik hewan yang akan di drop

    public GameObject[] dropitems;

    // Logika menentukan jumlah minimal dan maksimal dari item yang akan dijatuhkan
    public int minNormalItem = 1;  // Jumlah minimum batu
    public int maxNormalItem = 2;  // Jumlah maksimum batu
    public int minSpecialItem = 0; // Jumlah item spesial seperti copper, iron, atau gold
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
        StartCoroutine(AnimalMovement());
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




    private IEnumerator AnimalMovement()
    {
        while (true)
        {
            if (!isEating && !isSleeping)
            {
                // Tentukan arah pergerakan hewan (kanan atau kiri)
                moveDirection = Random.Range(0, 2) == 0 ? Vector2.left : Vector2.right;

                isMoving = true;
                yield return MoveForDuration();
                isMoving = false;
            }
            else if (isEating)
            {
                // Hewan buruan sedang makan
                Debug.Log($"{namaHewan} sedang makan.");
                yield return new WaitForSeconds(eatDuration); // Durasi makan
                isEating = false;
            }
            else if (isSleeping)
            {
                // Hewan buas sedang tidur
                Debug.Log($"{namaHewan} sedang tidur.");
                yield return new WaitForSeconds(sleepDuration); // Durasi tidur
                isSleeping = false;
            }

            // Menunggu sebelum kelinci bergerak lagi
            yield return new WaitForSeconds(2f);
        }
    }

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
}
