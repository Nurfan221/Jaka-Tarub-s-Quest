using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    Rigidbody2D rb;
    public Vector2 movement;


    //[SerializeField] Transform sprite;
    [SerializeField] Transform face;
    [SerializeField] Transform circleBoundary; // Lingkaran di sekitar player

    public bool isMoving;

    #region SPEEDS
    [Header("SPEEDS")]
    float moveSpd;
    [SerializeField] float walkSpd = 5f;
    [SerializeField] float runSpd = 9f;
    #endregion

    #region DASH
    [Header("DASH")]
    [SerializeField] ParticleSystem dashParticle;
    [SerializeField] float dashStamina = 40f;
    [SerializeField] float dashDistance = 5;
    [SerializeField] float dashForce = 100;
    bool justDash = false;
    bool dashing = false;
    #endregion

    bool noMovement = false;

    public Button dashButton; // Drag and drop the dash button in the inspector
    public Joystick movementJoystick; // Drag and drop the joystick in the inspector
    [SerializeField] private RectTransform joystickHandle; // Drag and drop the joystick handle in the inspector
    [SerializeField] private RectTransform joystickBackground; // Drag and drop the joystick background in the inspector
    float inputThreshold = 0.1f; // Batas minimal deteksi gerakan joystick
    private Vector2 lastDirection = Vector2.up; // simpan nilai default dari face
    private float radius; // Radius lingkaran boundary
    public Vector2 moveDir { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Hitung radius lingkaran berdasarkan skala lokal dari circleBoundary
        radius = circleBoundary.localScale.x / 2f;

        // Add listener to the dash button
        dashButton.onClick.AddListener(TriggerDash);
    }

    void Update()
    {
        if (GameController.Instance.enablePlayerInput)
        {
            PlayerInput();
        }
        else
        {
            movement = Vector2.zero;
        }

        isMoving = movement != Vector2.zero;

        PlayerUI.Instance.dashUI.color = new(1, 1, 1, Player_Health.Instance.stamina < dashStamina ? .5f : 1);

        UpdateFacePosition(); // Perbarui posisi face berdasarkan lingkaran
        //UpdateSpriteDirection(); // Update arah sprite (kanan atau kiri)
        moveSpd = walkSpd;
        moveDir = movement;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void PlayerInput()
    {
        // Input dari joystick
        movement.x = movementJoystick.Direction.x;
        movement.y = movementJoystick.Direction.y;
        movement = movement.normalized; // Normalisasi input

        // Simpan arah terakhir jika ada input
        if (movement != Vector2.zero)
        {
            lastDirection = movement;
        }
    }

    private void UpdateFacePosition()
    {
        // Jika ada input, tempatkan face di tepi lingkaran sesuai arah
        if (movement != Vector2.zero)
        {
            Vector3 newFacePosition = new Vector3(movement.x, movement.y, 0) * radius;
            face.localPosition = newFacePosition;

            // Tentukan animasi berdasarkan arah input
            //UpdateAnimation(movement);
        }
        else
        {
            // Jika tidak ada input, face tetap di posisi terakhir
            Vector3 lastFacePosition = new Vector3(lastDirection.x, lastDirection.y, 0) * radius;
            face.localPosition = lastFacePosition;

            // Tentukan animasi berdasarkan arah sebelumnya
            //UpdateAnimation(lastDirection);
        }
    }

    



    void HandleMovement()
    {
        if (!noMovement)
        {
            if (dashing)
            {
                StartCoroutine(StartDashing(transform.position));
            }
            else
            {
                rb.linearVelocity = new Vector2(movement.x * moveSpd, movement.y * moveSpd);

                // Stops player if no input given
                if (movement == Vector2.zero)
                    rb.linearVelocity = Vector2.zero;
            }
        }
    }

    IEnumerator StartDashing(Vector2 startPos)
    {
        dashParticle.Play();
        justDash = true;
        noMovement = true;
        Vector2 targetDir = (face.position - transform.position).normalized;
        float startTime = Time.time;

        while (Vector2.Distance(startPos, transform.position) < dashDistance && Time.time < startTime + 1)
        {
            rb.AddForce(dashForce * Time.deltaTime * targetDir, ForceMode2D.Impulse);
            PlayerUI.Instance.dashUI.fillAmount = Vector2.Distance(startPos, transform.position) / dashDistance;
            yield return null;
        }
        PlayerUI.Instance.dashUI.fillAmount = 1;
        rb.linearVelocity = Vector2.zero;
        noMovement = false;
        justDash = false;
        dashing = false;
    }

    private void TriggerDash()
    {
        if (!justDash)
        {
            if (Player_Health.Instance.SpendStamina(dashStamina))
                dashing = true;
        }
    }
}