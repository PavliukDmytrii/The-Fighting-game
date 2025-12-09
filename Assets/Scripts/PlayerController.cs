using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public float jumpCooldown = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Component References")]
    public Transform spriteVisualsTransform; 

    [Header("Crouch Settings")]
    public float crouchColliderHeight = 1.0f;
    public float crouchColliderOffsetY = -0.5f;
    
    [Header("Player Index")]
    public int playerIndex = 0;

    // --- Variables ---
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
    
    // PUBLIC variables for other scripts to see
    public bool isBlocking = false; 
    public bool isLockControl = false; // <--- This fixes your first error

    // --- Storage ---
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector3 originalSpritePosition; 

    // --- Inputs ---
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction crouchAction;
    private InputAction blockAction; 
    
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 20;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>(); 
        coll = GetComponent<CapsuleCollider2D>();
        controls = new InputSystem_Actions();

        if (coll != null)
        {
            originalColliderSize = coll.size;
            originalColliderOffset = coll.offset;
        }

        if (spriteVisualsTransform != null)
        {
            originalSpritePosition = spriteVisualsTransform.localPosition;
        }

        if (playerIndex == 0)
        {
            moveAction = controls.Player.Move;
            jumpAction = controls.Player.Jump;
            attackAction = controls.Player.Attack;
            crouchAction = controls.Player.Crouch;
            blockAction = controls.Player.Block; 
            controls.Player.Enable();
            isFacingRight = true; 
        }
        else 
        {
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            attackAction = controls.Player2.Attack;
            crouchAction = controls.Player2.Crouch;
            blockAction = controls.Player2.Block; 
            controls.Player2.Enable();
            isFacingRight = false; 
        }
    }

    void OnEnable()
    {
        if (jumpAction != null) jumpAction.performed += OnJump;
        if (attackAction != null) attackAction.performed += OnAttack;
        if (crouchAction != null) { crouchAction.performed += OnCrouch; crouchAction.canceled += OnCrouchRelease; }
        if (blockAction != null) { blockAction.performed += OnBlock; blockAction.canceled += OnBlockRelease; }
    }

    void OnDisable()
    {
        if (jumpAction != null) jumpAction.performed -= OnJump;
        if (attackAction != null) attackAction.performed -= OnAttack;
        if (crouchAction != null) { crouchAction.performed -= OnCrouch; crouchAction.canceled -= OnCrouchRelease; }
        if (blockAction != null) { blockAction.performed -= OnBlock; blockAction.canceled -= OnBlockRelease; }

        controls.Player.Disable();
        controls.Player2.Disable();
    }

    void Update()
    {
        // 1. If locked, stop everything and RETURN
        if (isLockControl) 
        {
            moveInput = Vector2.zero;
            // Update animator to show we stopped
            if (anim != null)
            {
                anim.SetFloat("Speed", 0);
                anim.SetBool("isWalkingBackwards", false);
            }
            return; 
        }

        // 2. Read Input
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        
        if (isCrouching || isBlocking)
        {
            moveInput.x = 0;
        }

        // 3. Safety Check: Stop crouching if falling
        if (!isGrounded && isCrouching)
        {
            StopCrouching(); // <--- This fixes your second error
        }

        // 4. Update Animator
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(moveInput.x));

            bool isWalkingBackwards = false;
            if (isFacingRight && moveInput.x < -0.1f) isWalkingBackwards = true;
            else if (!isFacingRight && moveInput.x > 0.1f) isWalkingBackwards = true;

            anim.SetBool("isWalkingBackwards", isWalkingBackwards);
            anim.SetBool("isCrouching", isCrouching);
            anim.SetBool("isBlocking", isBlocking); 
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

    // --- Helper Functions ---

    public void TakeHit()
    {
        if (isLockControl) return;
        if (anim != null) anim.SetTrigger("Hit");
        StartCoroutine(StunCoroutine(0.4f));
    }

    public void WinGame()
    {
        SetControlLock(true);
        if (anim != null) anim.SetTrigger("Victory");
    }

    public void LoseGame()
    {
        SetControlLock(true);
        rb.simulated = false; 
        if (anim != null) anim.SetTrigger("Defeat");
    }

    public void SetControlLock(bool isLocked)
    {
        isLockControl = isLocked;
        if (isLocked)
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero; 
        }
    }

    private void StopCrouching()
    {
        isCrouching = false;
        if (coll != null)
        {
            coll.size = originalColliderSize;
            coll.offset = originalColliderOffset;
        }
        if (spriteVisualsTransform != null)
        {
            spriteVisualsTransform.localPosition = originalSpritePosition;
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isLockControl = true;
        yield return new WaitForSeconds(duration);
        // Unlock only if game isn't over
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Victory") && 
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Defeat"))
        {
            isLockControl = false;
        }
    }

    // --- Input Callbacks ---

    private void OnBlock(InputAction.CallbackContext context)
    {
        if (isGrounded && !isLockControl) isBlocking = true;
    }

    private void OnBlockRelease(InputAction.CallbackContext context)
    {
        isBlocking = false;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (isGrounded && !isLockControl)
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
        StopCrouching();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && canJump && !isCrouching && !isBlocking && !isLockControl)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; 
            StartCoroutine(JumpCooldown()); 
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (anim != null && !isCrouching && isGrounded && !isBlocking && !isLockControl)
        {
            anim.SetTrigger("Attack");

            if (attackPoint != null)
            {
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
                foreach (Collider2D enemy in hitEnemies)
                {
                    enemy.GetComponent<Health>().TakeDamage(attackDamage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
}