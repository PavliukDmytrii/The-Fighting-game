using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

    [Header("Component References")]
    [Tooltip("Drag your child 'SpriteVisuals' object here.")]
    public Transform spriteVisualsTransform; 

    [Header("Crouch Settings")]
    public float crouchColliderHeight = 1.0f;
    public float crouchColliderOffsetY = -0.5f;
    
    [Header("Player Index")]
    [Tooltip("Set this to 0 for Player 1 (uses 'Player' map), or 1 for Player 2 (uses 'Player2' map)")]
    public int playerIndex = 0;

    // --- Private variables ---
    private bool isGrounded;
    private Rigidbody2D rb;
    private Animator anim;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    private CapsuleCollider2D coll; 

    // --- Player State ---
    private bool isFacingRight = true; 
    private bool canJump = true; 
    private bool isCrouching = false;
    private bool isBlocking = false; // Tracks block state

    // --- Collider Original Shape ---
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector3 originalSpritePosition; 

    // --- Input Actions ---
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction crouchAction;
    private InputAction blockAction; // Action for blocking

    void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>(); 
        coll = GetComponent<CapsuleCollider2D>();
        controls = new InputSystem_Actions();

        // Store original collider/sprite values
        originalColliderSize = coll.size;
        originalColliderOffset = coll.offset;
        if (spriteVisualsTransform != null)
        {
            originalSpritePosition = spriteVisualsTransform.localPosition;
        }

        // --- Set up input based on playerIndex ---
        if (playerIndex == 0)
        {
            // Player 1 (Ryu)
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            attackAction = controls.Player.Attack;
            crouchAction = controls.Player.Crouch;
            blockAction = controls.Player.Block; // Gets the 'Block' action
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
            blockAction = controls.Player2.Block; // Gets the 'Block' action
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
        if (attackAction != null)
        {
            attackAction.performed += OnAttack;
        }
        if (crouchAction != null)
        {
            crouchAction.performed += OnCrouch; 
            crouchAction.canceled += OnCrouchRelease; 
        }
        // Listen for the block button being pressed or released
        if (blockAction != null)
        {
            blockAction.performed += OnBlock; 
            blockAction.canceled += OnBlockRelease; 
        }
    }

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
        // Stop listening for block
        if (blockAction != null)
        {
            blockAction.performed -= OnBlock;
            blockAction.canceled -= OnBlockRelease;
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
        
        // Stop horizontal movement if crouching OR blocking
        if (isCrouching || isBlocking)
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
            anim.SetBool("isBlocking", isBlocking); // Tell the animator we are blocking
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (anim != null)
        {
            anim.SetBool("isJumping", !isGrounded);
        }
    }

    // --- This function is called when you press 'Q' (Ryu) or 'PgDown' (Dee Jay) ---
    private void OnBlock(InputAction.CallbackContext context)
    {
        // Only block if on ground and not crouching
        if (isGrounded && !isCrouching)
        {
            isBlocking = true;
        }
    }

    // --- This function is called when you release the block button ---
    private void OnBlockRelease(InputAction.CallbackContext context)
    {
        isBlocking = false;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isCrouching = true;
            coll.size = new Vector2(originalColliderSize.x, crouchColliderHeight);
            coll.offset = new Vector2(originalColliderOffset.x, crouchColliderOffsetY);
            if (spriteVisualsTransform != null)
            {
                spriteVisualsTransform.localPosition = new Vector3(originalSpritePosition.x, -0.2f, originalSpritePosition.z);
            }
        }
    }

    private void OnCrouchRelease(InputAction.CallbackContext context)
    {
        isCrouching = false;
        coll.size = originalColliderSize;
        coll.offset = originalColliderOffset;
        if (spriteVisualsTransform != null)
        {
            spriteVisualsTransform.localPosition = originalSpritePosition;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Can't jump if blocking
        if (isGrounded && canJump && !isCrouching && !isBlocking)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; 
            StartCoroutine(JumpCooldown()); 
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // Can't attack if blocking
        if (anim != null && !isCrouching && isGrounded && !isBlocking)
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