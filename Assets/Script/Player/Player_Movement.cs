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
    public float walkSpd = 5f;

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
        CircleCollider2D circleCollider = circleBoundary.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            radius = circleCollider.radius * circleBoundary.lossyScale.x; // Pastikan menggunakan lossyscale
        }
        else
        {
            Debug.LogError("CircleBoundary tidak memiliki CircleCollider2D!");
        }


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
        if (movement != Vector2.zero) // Jika bergerak, simpan arah terakhir
        {
            lastDirection = movement.normalized;
        }

        // Hitung sudut berdasarkan arah terakhir
        float angle = Mathf.Atan2(lastDirection.y, lastDirection.x);

        // Kalibrasi posisi `face` agar selalu tepat di tepi lingkaran
        float offset = 0.1f; // Koreksi jika face kurang menyentuh boundary
        float faceX = Mathf.Cos(angle) * (radius + offset);
        float faceY = Mathf.Sin(angle) * (radius + offset);

        // Atur posisi `face`
        face.localPosition = new Vector3(faceX, faceY, 0);
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