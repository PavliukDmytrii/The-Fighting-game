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

    // --- ADD THIS HEADER ---
    [Header("Crouch Settings")]
    [Tooltip("The height of the collider when crouching.")]
    public float crouchColliderHeight = 1.0f; // You will need to change this value
    [Tooltip("The Y-offset of the collider when crouching.")]
    public float crouchColliderOffsetY = -0.5f; // You will need to change this value

    [Header("Player Index")]
    [Tooltip("Set this to 0 for Player 1 (uses 'Player' map), or 1 for Player 2 (uses 'Player2' map)")]
    public int playerIndex = 0;

    // --- Private variables ---
    private bool isGrounded;
    private Rigidbody2D rb;
    private Animator anim;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    private CapsuleCollider2D coll; // <-- ADD THIS: To hold our collider

    // --- Player State ---
    private bool isFacingRight = true; 
    private bool canJump = true; 
    private bool isCrouching = false;

    // --- ADD THESE: To store our collider's original shape ---
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    // --- Input Actions ---
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction crouchAction;

    void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>(); // <-- ADD THIS: Get the collider
        controls = new InputSystem_Actions();

        // --- ADD THIS: Store the collider's original shape ---
        originalColliderSize = coll.size;
        originalColliderOffset = coll.offset;

        // --- Set up input based on playerIndex ---
        if (playerIndex == 0)
        {
            // Player 1 (Ryu)
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            attackAction = controls.Player.Attack;
            crouchAction = controls.Player.Crouch;
            controls.Player.Enable();
            isFacingRight = true; 
        }
        else // if playerIndex is 1 (or anything else)
        {
            // Player 2 (Dee Jay)
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            attackAction = controls.Player2.Attack;
            crouchAction = controls.Player2.Crouch;
            controls.Player2.Enable();
            isFacingRight = false; 
        }
    }

    // OnEnable is the same
    void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
        }
        if (attackAction != null)
        {
            attackAction.performed += OnAttack;
        }
        if (crouchAction != null)
        {
            crouchAction.performed += OnCrouch; 
            crouchAction.canceled += OnCrouchRelease; 
        }
    }

    // OnDisable is the same
    void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
        }
        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
        }
        if (crouchAction != null)
        {
            crouchAction.performed -= OnCrouch;
            crouchAction.canceled -= OnCrouchRelease;
        }
        controls.Player.Disable();
        controls.Player2.Disable();
    }

    // Update is the same
    void Update()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        
        if (isCrouching)
        {
            moveInput.x = 0;
        }
        
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput.x));

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
            anim.SetBool("isCrouching", isCrouching);
        }
    }

    // FixedUpdate is the same
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (anim != null)
        {
            anim.SetBool("isJumping", !isGrounded);
        }
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isCrouching = true;

            // --- ADD THIS ---
            // Set collider to crouch shape
            coll.size = new Vector2(originalColliderSize.x, crouchColliderHeight);
            coll.offset = new Vector2(originalColliderOffset.x, crouchColliderOffsetY);
        }
    }

    private void OnCrouchRelease(InputAction.CallbackContext context)
    {
        isCrouching = false;

        // --- ADD THIS ---
        // Set collider back to original shape
        coll.size = originalColliderSize;
        coll.offset = originalColliderOffset;
    }

    // OnJump is the same
    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && canJump && !isCrouching)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; 
            StartCoroutine(JumpCooldown()); 
        }
    }

    // OnAttack is the same
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (anim != null && !isCrouching && isGrounded)
        {
            anim.SetTrigger("Attack");
        }
    }

    // JumpCooldown is the same
    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
}