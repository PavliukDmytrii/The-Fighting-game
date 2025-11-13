using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Required for Coroutines (like the jump cooldown)

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Header("Jump Cooldown")]
    [Tooltip("How long to wait (in seconds) before you can jump again.")]
    public float jumpCooldown = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Player Index")]
    [Tooltip("Set this to 0 for Player 1 (uses 'Player' map), or 1 for Player 2 (uses 'Player2' map)")]
    public int playerIndex = 0; // 0 = Player 1, 1 = Player 2

    // --- Private variables ---
    private bool isGrounded;
    private Rigidbody2D rb;
    private Animator anim;
    private InputSystem_Actions controls;
    private Vector2 moveInput;

    // --- Player State ---
    private bool isFacingRight = true; 
    private bool canJump = true; 

    // --- Input Actions ---
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controls = new InputSystem_Actions();

        // --- Set up input based on playerIndex ---
        if (playerIndex == 0)
        {
            // Player 1 (Ryu)
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            attackAction = controls.Player.Attack;
            controls.Player.Enable();
            isFacingRight = true; // Player 1 starts facing right
        }
        else // if playerIndex is 1 (or anything else)
        {
            // Player 2 (Dee Jay)
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            attackAction = controls.Player2.Attack;
            controls.Player2.Enable();
            isFacingRight = false; // Player 2 starts facing left
        }
    }

    void OnEnable()
    {
        // Subscribe to the "performed" event for jumping and attacking
        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
        }
        if (attackAction != null)
        {
            attackAction.performed += OnAttack;
        }
    }

    void OnDisable()
    {
        // Unsubscribe to avoid errors
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
        }
        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
        }

        // Disable the action maps
        controls.Player.Disable();
        controls.Player2.Disable();
    }

    void Update()
    {
        // Read movement input every frame
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        // --- Animation Logic ---
        if (anim != null)
        {
            // 1. Send the absolute speed (for idle vs. moving)
            // Mathf.Abs() gets the positive value, e.g. -1 becomes 1
            anim.SetFloat("Speed", Mathf.Abs(moveInput.x));

            // 2. Check if we are walking backward
            bool isWalkingBackwards = false;
            
            // Check for "backward" conditions:
            if (isFacingRight && moveInput.x < -0.1f) // Facing right, but moving left
            {
                isWalkingBackwards = true;
            }
            else if (!isFacingRight && moveInput.x > 0.1f) // Facing left, but moving right
            {
                isWalkingBackwards = true;
            }

            // 3. Send this true/false value to the Animator
            anim.SetBool("isWalkingBackwards", isWalkingBackwards);
        }
    }

    void FixedUpdate()
    {
        // Apply physics-based movement
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Called when the Jump button is pressed
        if (isGrounded && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; // Instantly disable jumping
            StartCoroutine(JumpCooldown()); // Start the timer to re-enable it
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // Called when the Attack button is pressed
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
    }

    private IEnumerator JumpCooldown()
    {
        // This is a timer (Coroutine) that re-enables jumping
        yield return new WaitForSeconds(jumpCooldown);
        
        // After waiting, allow the player to jump again
        canJump = true;
    }
}