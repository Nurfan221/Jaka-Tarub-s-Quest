using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    // --- Referensi Komponen Inti ---
    public Rigidbody2D rb;
    public PlayerController controller; // Referensi ke "Otak"
    public Transform facePlayer;
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] public Transform face; // Untuk arah dash
    public float jarakOffsetSerang = 1.0f;

    // --- Variabel Status Internal ---
    public Vector2 movementDirection;
    public Vector2 lastDirection;
    public bool isDashing = false;
    private PlayerData_SO stats;
    public bool IsMoving { get; private set; } // Properti publik yang bisa dibaca skrip lain

    // Start berjalan sekali saat objek ini dibuat di scene baru.
    void Start()
    {
        // "Halo Kantor Pusat, saya Karyawan baru di cabang ini. Ini data saya."
        
    }

    // OnDestroy berjalan tepat sebelum objek ini dihancurkan (misal saat pindah scene).
   
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
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
        // Hanya bergerak jika tidak sedang melakukan aksi lain seperti dash
        if (!isDashing)
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
        if (!isDashing && stats.stamina >stats.dashStamina  )
        {
            PlayerController.Instance.HandleSpendStamina(PlayerController.Instance.playerData.dashStamina);
            StartCoroutine(DashCoroutine());
        }else
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


    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashParticle.Play();

        // Ambil data dash dari "Otak"
        float force =stats.dashForce;
        Vector2 dashDirection = lastDirection; // Dash ke arah hadap terakhir

        rb.linearVelocity = Vector2.zero; // Hentikan gerakan sebelumnya
        rb.AddForce(dashDirection * force, ForceMode2D.Impulse);

        // Tunggu sesaat selama durasi dash
        yield return new WaitForSeconds(0.3f); // Anda bisa membuat ini menjadi variabel di PlayerController

        isDashing = false;
    }
}