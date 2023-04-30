using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{

    public bool gravityEnabled = true;
    public bool canRun = true;
    public bool canJump = true;

    [SerializeField] private int playerIndex;

    [Header("Running")]
    [SerializeField] private float maxSpeed = 15;
    [SerializeField] private float acceleration = 500;
    [SerializeField] private float decceleration = 600;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 10;
    [SerializeField] private Vector2 overGroundBox = new Vector2(0.9f, 1);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float fallingGravityScaleMultiplier = 2;
    
    [Header("Animation")]
    [SerializeField] private Transform graphics;
    [SerializeField] private float tilt = 15;
    [SerializeField] private float tiltSpeed = 2;
    [SerializeField] private float squashAndStretchSpeed = 0.1f;
    [SerializeField] private Vector2 landSquash = new Vector2(0.8f, -0.8f);
    [SerializeField] private Vector2 jumpStretch = new Vector2(-0.8f, 0.8f);

    private float targetTilt = 0;
    private bool isTouchingGround = false;
    private bool isOverGround = false;

    // Input values
    private float movementInput;
    private float jumpInput;

    Rigidbody2D playerBody;
    PlayerInput playerInput;
    float originalGravityScale;

    void Start() {
        playerBody = GetComponent<Rigidbody2D>();

        originalGravityScale = playerBody.gravityScale;

        // InputDevice
        // foreach (InputDevice device in InputSystem.devices)
        // {
        //     if (device.deviceId == playerIndex)
        //     {
        //         targetDevice = device;
        //         break;
        //     }
        // }
        // playerInput = GetComponent<PlayerInput>();
        // InputUser.PerformPairingWithDevice(targetDevice, playerInput.user);
    }
    
    void Update() {
        Checks();

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        graphics.rotation = Quaternion.RotateTowards(graphics.rotation, targetRotation, tiltSpeed);

        graphics.localScale = Vector2.MoveTowards(graphics.localScale, Vector2.one, squashAndStretchSpeed);
    }

    void FixedUpdate() {
        if (canRun) Run();

        Gravity();

        if (jumpInput > 0 && isTouchingGround && isOverGround && canJump)
        {
            Jump();
        }
    }

    private void Run() {
        float targetSpeed = movementInput * maxSpeed;
        float speedDiff = targetSpeed - playerBody.velocity.x;
        float accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float moveForce = Mathf.Pow(Mathf.Abs(speedDiff) * accelerationRate, 0.96f) * Mathf.Sign(speedDiff);

        playerBody.AddForce(moveForce * Vector2.right * Time.fixedDeltaTime);
    }

    private void Jump() {
        playerBody.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
        graphics.localScale = Vector2.one + jumpStretch;
    }

    private void Gravity() {
        if (gravityEnabled)
        {
            if (playerBody.velocity.y < 0)
            {
                playerBody.gravityScale = originalGravityScale * fallingGravityScaleMultiplier;
            } else
            {
                playerBody.gravityScale = originalGravityScale;
            }
        } else
        {
            playerBody.gravityScale = 0;
        }
    }

    private void Checks() {
        isOverGround = Physics2D.OverlapBox(transform.position, overGroundBox, 0, groundLayer);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingGround = true;
            if (isOverGround)
            {
                graphics.localScale = Vector2.one + landSquash;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingGround = false;
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, overGroundBox);
    }

    // Input
    void OnMovement(InputValue value) {
        movementInput = value.Get<float>();
        targetTilt = -movementInput * tilt;
    }

    void OnJump(InputValue value) {
        jumpInput = value.Get<float>();
    }

}
