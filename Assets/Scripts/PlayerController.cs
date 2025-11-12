using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // <-- ADD THIS. We need it for the cooldown timer.

public class PlayerController : MonoBehaviour
{
    // --- Your original variables ---
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Tooltip("How long to wait (in seconds) before you can jump again.")]
    public float jumpCooldown = 0.2f; // <-- ADD THIS

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
    private bool isFacingRight = true;
    
    private bool canJump = true; // <-- ADD THIS: Tracks if we are allowed to jump.

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
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            controls.Player.Enable();
        }
        else // if playerIndex is 1 (or anything else)
        {
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            controls.Player2.Enable();
        }
        
        if (playerIndex == 1)
        {
            isFacingRight = false; 
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

        FlipCheck();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // --- MODIFIED THIS ---
        // Now it checks if you're grounded AND if the cooldown is over.
        if (isGrounded && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; // Instantly disable jumping
            StartCoroutine(JumpCooldown()); // Start the timer to re-enable it
        }
    }

    // --- ADD THIS NEW FUNCTION ---
    // This is a timer (Coroutine) that re-enables jumping.
    private IEnumerator JumpCooldown()
    {
        // Wait for the amount of time you set in jumpCooldown
        yield return new WaitForSeconds(jumpCooldown);
        
        // After waiting, allow the player to jump again
        canJump = true;
    }

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
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}