using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    // Referensi Komponen Inti
    public Rigidbody2D rb;
    //public PlayerController controller; // Referensi ke "Otak"
    public Transform facePlayer;
    [Tooltip("Geser titik pusat lingkaran ke atas (agar sejajar badan, bukan kaki)")]
    public float verticalOffset = -2f; // Coba ubah angka ini di inspector (misal 0.5 atau 0.8)
    public float hitboxRadius = 1.0f;
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] public Transform face; // Untuk arah dash
    public float jarakOffsetSerang = 1.0f;

    // Variabel Status Internal
    public Vector2 movementDirection;
    public Vector2 lastDirection;
    public bool isDashing = false;
    private PlayerData_SO stats;
    public bool IsMoving { get; private set; } // Properti publik yang bisa dibaca skrip lain
    public bool ifDisturbed; // Menandai jika pemain sedang terganggu (misal terkena knockback)
    // Start berjalan sekali saat objek ini dibuat di scene baru.
    void Start()
    {

    }

    // OnDestroy berjalan tepat sebelum objek ini dihancurkan (misal saat pindah scene).

    private void Awake()
    {
        //controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }

    private void FixedUpdate()
    {
        // Jangan paksa velocity 0 di sini kalau mau ada fitur Knockback.
        if (!isDashing && !ifDisturbed)
        {
            HandleMovement();
        }
    }

    private void Update()
    {
        // Update status isMoving berdasarkan arah terakhir yang diberikan
        IsMoving = movementDirection.magnitude > 0.1f;
        // Update posisi objek "face" untuk menunjukkan arah
        UpdateFacePosition();
    }


    public void SetMovementDirection(Vector2 direction)
    {
        this.movementDirection = direction;
        if (direction.magnitude > 0.1f)
        {
            this.lastDirection = direction.normalized; // Simpan arah yang sudah dinormalisasi
        }

    }


    public void TriggerDash()
    {
        // Cek ke "Otak" apakah kita punya cukup stamina
        //&& Player_Health.Instance.SpendStamina(controller.dashStamina)
        if (!isDashing && PlayerController.Instance.HandleSpendStamina(PlayerController.Instance.playerData.dashStamina))
        {

            StartCoroutine(DashCoroutine());
        }
        else
        {
            return;
        }
    }



    private void HandleMovement()
    {
        // Gunakan kecepatan dari "Otak" (PlayerController)
        rb.linearVelocity = movementDirection * stats.walkSpd;


    }

    private void UpdateFacePosition()
    {
        // Pastikan referensi face ada
        if (face == null) return;

        // check lokasi direction
        if (movementDirection.magnitude > 0.1f)
        {
            // Karena kita hanya butuh arahnya, kita tetap normalisasi.
            Vector2 direction = movementDirection.normalized;

            // Atur posisi LOKAL dari 'face' berdasarkan arah tersebut.
            face.localPosition = direction * jarakOffsetSerang;
        }

        if (face != null)
        {
            Vector3 finalPosition = lastDirection * hitboxRadius;
            finalPosition.y += verticalOffset;

            face.localPosition = finalPosition;

            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            face.rotation = Quaternion.Euler(0, 0, angle);
        }



    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashParticle.Play();

        // Ambil data dash dari "Otak"
        float force = stats.dashForce;
        Vector2 dashDirection = lastDirection; // Dash ke arah hadap terakhir

        rb.linearVelocity = Vector2.zero; // Hentikan gerakan sebelumnya
        rb.AddForce(dashDirection * force, ForceMode2D.Impulse);

        // Tunggu sesaat selama durasi dash
        yield return new WaitForSeconds(0.3f); // Anda bisa membuat ini menjadi variabel di PlayerController

        isDashing = false;
    }

    public void Disturbed(float delayTime)
    {
        // Jika sudah disturbed, jangan ditumpuk (opsional)
        if (ifDisturbed) return;

        StartCoroutine(DisturbRoutine(delayTime));
    }

   
    private IEnumerator DisturbRoutine(float time)
    {
        ifDisturbed = true;
        IsMoving = false;

        // Paksa berhenti total agar tidak ada sisa momentum (sliding)
        rb.linearVelocity = Vector2.zero;

        Debug.Log("Player berhenti bergerak selama: " + time + " detik.");

        yield return new WaitForSeconds(time);

        ifDisturbed = false;
        IsMoving = movementDirection.magnitude > 0.1f;
        Debug.Log("Player bisa jalan lagi.");
    }

    // Taruh ini di script Player_Movement atau PlayerController Anda
    public void SetKnockbackStatus(bool status)
    {
        // Cukup ubah bool 'ifDisturbed' atau matikan input
        if (PlayerController.Instance != null)
        {
            // Jika status = true (kena mental), maka ifDisturbed = true (gak bisa gerak)
            PlayerController.Instance.ActivePlayer.Movement.ifDisturbed = status;

            // Opsional: Reset velocity biar gak licin kalau statusnya false (selesai knockback)
            if (status == false)
            {
                GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            }
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
        if (face != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(centerPoint, face.position);
            Gizmos.DrawWireSphere(face.position, 0.2f);
        }
    }
}