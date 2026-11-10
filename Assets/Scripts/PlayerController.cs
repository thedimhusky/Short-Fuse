using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // REFERENCES
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference bombAction;
    private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // BOMB REFERENCES
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform bombSpawnPoint;

    // PLAYER INPUT
    private float moveInput;
    private bool jumpRequested = false;
    private bool grounded;

    // MOVEMENT SETTINGS
    [Range(1f, 12f)]
    [SerializeField] private float moveSpeed = 6f;

    [Range(1f, 100f)]
    [SerializeField] private float acceleration = 40f;

    private float currentSpeed;

    [Range(1f, 20f)]
    [SerializeField] private float jumpForce = 10f;

    [Range(0.05f, 0.5f)]
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.3f;

    // SETUP

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        currentBombs = maxBombs;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();


        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;

        bombAction.action.Enable();
        bombAction.action.performed += OnBomb;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();


        jumpAction.action.performed -= OnJump;
        jumpAction.action.Disable();

        
        bombAction.action.performed -= OnBomb;
        bombAction.action.Disable();
    }


    // INPUT
    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>().x;
    }

    // MOVEMENT
    private void FixedUpdate()
    {
        CheckGround();
        HandleAcceleration();
        HandleMovement();
        HandleJump();
    }

    // HANDLE ACCELLERATION
    private void HandleAcceleration()
    {
        // Set "Target speed" (Ideal speed if it were in vacuum)
        float targetSpeed = moveInput * moveSpeed;

        float currentAcceleration = CalculateAcceleration();

        // Calculate currentspeed based on target speed but with acceleration damnpening)
        currentSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, currentAcceleration * Time.fixedDeltaTime);
    }


    // HANDLE MOVEMENT
    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    // HANDLE JUMPING
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

    private void HandleJump()
    {
        if (jumpRequested && grounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        jumpRequested = false;
    }

    // CHECK GROUNDING

    private void CheckGround()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    // CALCULATE ACCELERATION
    private float CalculateAcceleration()
    {
        if (grounded) // check if grounded
        {
            return acceleration; // just use normal acceleration
        }

        return acceleration * airControl; // if airborne, use acceleration times by a percentage (30%)
    }

    // BOMBS
    private void OnBomb(InputAction.CallbackContext context)
    {
        Instantiate(
            bombPrefab,
            bombSpawnPoint.position,
            Quaternion.identity
        );

        currentBombs--;

        Debug.Log("Bombs remaining: " + currentBombs);
    }

    private void PlaceBomb()
    {
        if (currentBombs <= 0)
        {
            return;
        }

        Instantiate(
            bombPrefab,
            bombSpawnPoint.position,
            Quaternion.identity
        );

        currentBombs--;

        Debug.Log("Bombs remaining: " + currentBombs);
    }

    [Range(1, 10)]
    [SerializeField] private int maxBombs = 3;

    private int currentBombs;



    // GIZMOS AND DEBUGS

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
