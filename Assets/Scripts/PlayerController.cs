using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // --- Your original variables ---
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    [Tooltip("How long to wait (in seconds) before you can jump again.")]
    public float jumpCooldown = 0.2f;

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
    
    // This is now the "master" direction. It will NOT change unless the player is flipped (e.g., by crossing up)
    private bool isFacingRight = true; 
    
    private bool canJump = true; 

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
            // Player 1 (Ryu)
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            controls.Player.Enable();
            isFacingRight = true; // Player 1 starts facing right
        }
        else // if playerIndex is 1 (or anything else)
        {
            // Player 2 (Dee Jay)
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            controls.Player2.Enable();
            isFacingRight = false; // Player 2 starts facing left
        }
    }

    // OnEnable/OnDisable are the same
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
            // --- THIS IS THE NEW ANIMATION LOGIC ---

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
        // Physics movement is the same
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- REMOVED FlipCheck() ---
        // The character no longer auto-flips.
    }

    // OnJump and JumpCooldown are the same
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; 
            StartCoroutine(JumpCooldown()); 
        }
    }
    
    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    // --- REMOVED FlipCheck() and Flip() ---
    // We no longer use these functions.
}