using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickMove : MonoBehaviour
{
    public Joystick movementJoystick;
    public float playerSpeed;
    public float sprintSpeed;
    public float sprintThreshold; // The threshold distance for triggering sprint
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 joystickDirection = movementJoystick.Direction;
        float distance = joystickDirection.magnitude;

        // Check if the joystick handle is near the edge to trigger sprint
        float currentSpeed = (distance >= sprintThreshold) ? sprintSpeed : playerSpeed;

        if (joystickDirection != Vector2.zero)
        {
            rb.linearVelocity = joystickDirection * currentSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
