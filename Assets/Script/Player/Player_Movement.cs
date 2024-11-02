using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Movement : MonoBehaviour
{
    Rigidbody2D rb;
    Vector2 movement;

    #region KEYBINDINGS
    KeyCode runInput = KeyCode.LeftShift;
    #endregion

    [SerializeField] Transform sprite;
    [SerializeField] Transform face;
    [SerializeField] Transform hitboxes;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
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

        if (movement.x > 0)
        {
            hitboxes.eulerAngles = new(0, 0, 0);
            sprite.localScale = new(1, 1, 1);
        }
        else if (movement.x < 0)
        {
            hitboxes.eulerAngles = new(0, 180, 0);
            sprite.localScale = new(-1, 1, 1);
        }

        // Check joystick position to determine speed
        CheckJoystickPosition();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // Handle all player input regarding movement (axis, run, dash)
    void PlayerInput()
    {
        movement.x = movementJoystick.Direction.x;
        movement.y = movementJoystick.Direction.y;
        movement = movement.normalized;

        if (movement != Vector2.zero)
            face.localPosition = movement;
    }

    void CheckJoystickPosition()
    {
        // Calculate the distance between the joystick handle and the center of the joystick background
        float distance = Vector2.Distance(joystickHandle.anchoredPosition, Vector2.zero); // Vector2.zero is the center
        float maxDistance = joystickBackground.sizeDelta.x / 2; // Assuming the background is a circle

        // Check if the handle is near the edge of the joystick background
        if (distance >= maxDistance * 0.9f) // 0.9f to add a bit of buffer
        {
            moveSpd = runSpd;
            dashParticle.Play();
        }
        else
        {
            moveSpd = walkSpd;
         
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
                rb.velocity = new Vector2(movement.x * moveSpd, movement.y * moveSpd);

                // Stops player if no input given
                if (movement == Vector2.zero)
                    rb.velocity = Vector2.zero;
            }
        }
    }

    // Dashing until certain distance
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
        rb.velocity = Vector2.zero;
        noMovement = false;
        justDash = false;
        dashing = false;
    }

    // Method to trigger dash
    private void TriggerDash()
    {
        if (!justDash)
        {
            if (Player_Health.Instance.SpendStamina(dashStamina))
                dashing = true;
        }
    }
}