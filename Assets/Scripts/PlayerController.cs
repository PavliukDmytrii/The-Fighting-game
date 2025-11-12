using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // --- Your original variables ---
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // --- NEW ---
    [Tooltip("Set this to 0 for Player 1 (uses 'Player' map), or 1 for Player 2 (uses 'Player2' map)")]
    public int playerIndex = 0; // 0 = Player 1, 1 = Player 2

    // --- Private variables ---
    private bool isGrounded;
    private Rigidbody2D rb;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    private Animator anim;
    
    private bool isFacingRight = true; // <-- ADD THIS: Start by facing right

    // --- We need to store the specific actions ---
    private InputAction moveAction;
    private InputAction jumpAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();
        anim = GetComponent<Animator>();

        if (playerIndex == 0)
        {
            // Player 1 uses the "Player" action map
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            controls.Player.Enable();
        }
        else // if playerIndex is 1 (or anything else)
        {
            // Player 2 uses the "Player2" action map
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            controls.Player2.Enable();
        }
        
        // --- ADD THIS: Make sure Player 1 (Ryu) starts facing right ---
        // (You'll need to check if your Player 2 (Dee Jay) starts facing left)
        if (playerIndex == 1)
        {
            isFacingRight = false; // Player 2 starts facing left
        }
    }

    void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
        }
    }

    void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
        }
        controls.Player.Disable();
        controls.Player2.Disable();
    }

    void Update()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- ADD THIS: Call the Flip function ---
        FlipCheck();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // --- ADD THIS: New function to flip the character ---
    private void FlipCheck()
    {
        // If moving right but facing left
        if (moveInput.x > 0 && !isFacingRight)
        {
            Flip();
        }
        // If moving left but facing right
        else if (moveInput.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    // --- ADD THIS: New function that does the flip ---
    private void Flip()
    {
        // Switch the way the player is labelled as facing
        isFacingRight = !isFacingRight;

        // Get the current scale
        Vector3 localScale = transform.localScale;
        
        // Flip the x-axis
        localScale.x *= -1f;

        // Apply the new scale
        transform.localScale = localScale;
    }
}