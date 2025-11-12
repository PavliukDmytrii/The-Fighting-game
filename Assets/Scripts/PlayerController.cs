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
    private bool isFacingRight = true; 
    private bool canJump = true; 

    // --- We need to store the specific actions ---
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction; // <-- ADD THIS

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
            attackAction = controls.Player.Attack; // <-- ADD THIS
            controls.Player.Enable();
            isFacingRight = true; 
        }
        else // if playerIndex is 1 (or anything else)
        {
            // Player 2 (Dee Jay)
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            attackAction = controls.Player2.Attack; // <-- ADD THIS
            controls.Player2.Enable();
            isFacingRight = false; 
        }
    }

    void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
        }
        if (attackAction != null) // <-- ADD THIS
        {
            attackAction.performed += OnAttack;
        }
    }

    void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
        }
        if (attackAction != null) // <-- ADD THIS
        {
            attackAction.performed -= OnAttack;
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
            // Send speed to animator
            anim.SetFloat("Speed", Mathf.Abs(moveInput.x));

            // Check for walking backwards
            bool isWalkingBackwards = false;
            if (isFacingRight && moveInput.x < -0.1f) 
            {
                isWalkingBackwards = true;
            }
            else if (!isFacingRight && moveInput.x > 0.1f) 
            {
                isWalkingBackwards = true;
            }
            anim.SetBool("isWalkingBackwards", isWalkingBackwards);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; 
            StartCoroutine(JumpCooldown()); 
        }
    }
    
    // --- ADD THIS NEW FUNCTION ---
    private void OnAttack(InputAction.CallbackContext context)
    {
        // When the attack button is pressed, send the "Attack" trigger to the Animator.
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
    }
    
    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
}