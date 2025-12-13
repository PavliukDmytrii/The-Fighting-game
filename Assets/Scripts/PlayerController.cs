using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public float jumpCooldown = 0.2f;

[Header("Ryu Visual Fixes")]
    public float ryuVictoryHeight = 0f;
    public float ryuDefeatHeight = -0.25f;

    [Header("Dee Jay Visual Fixes")]
    public float deeJayVictoryHeight = 0f;
    public float deeJayDefeatHeight = 0f;
    [Header("Combat Settings")]
    public float attackDuration = 0.3f;
    public int attackDamage = 20;
    public float attackRange = 0.5f;
    public Transform attackPoint;
    public LayerMask enemyLayers;

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
    private bool isAttacking = false; // <-- NEW: Tracks if we are attacking
    
    // PUBLIC variables
    public bool isBlocking = false; 
    public bool isLockControl = false; 

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
        // 1. Locked (Intro/Damage/Death)
        if (isLockControl) 
        {
            moveInput = Vector2.zero;
            if (anim != null) { anim.SetFloat("Speed", 0); anim.SetBool("isWalkingBackwards", false); }
            return; 
        }

        // 2. Read Input
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        
        // 3. STOP movement if Crouching OR Blocking OR Attacking <-- CHANGED HERE
        if (isCrouching || isBlocking || isAttacking)
        {
            moveInput.x = 0;
        }

        // 4. Safety Check (Stop crouching if falling)
        if (!isGrounded && isCrouching)
        {
            StopCrouching();
        }

        // 5. Update Animator
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
        
        if (anim != null) 
        {
            anim.SetTrigger("Hit");
        }
        
        StopCoroutine("StunCoroutine"); 
        StartCoroutine(StunCoroutine(0.4f)); 
    }

public void WinGame()
    {
        SetControlLock(true);
        if (rb != null) rb.linearVelocity = Vector2.zero; 

        if (anim != null) 
        {
            anim.SetTrigger("Victory");
        }

        if (spriteVisualsTransform != null)
        {
            if (playerIndex == 0)
            {
                spriteVisualsTransform.localPosition = new Vector3(originalSpritePosition.x, ryuVictoryHeight, originalSpritePosition.z);
            }
            else if (playerIndex == 1)
            {
                spriteVisualsTransform.localPosition = new Vector3(originalSpritePosition.x, deeJayVictoryHeight, originalSpritePosition.z);
            }
        }
    }

    public void LoseGame()
    {
        SetControlLock(true);
        rb.simulated = false; 
        
        if (anim != null) 
        {
            anim.SetTrigger("Defeat");
        }

        if (spriteVisualsTransform != null)
        {
            if (playerIndex == 0)
            {
                spriteVisualsTransform.localPosition = new Vector3(originalSpritePosition.x, ryuDefeatHeight, originalSpritePosition.z);
            }
            else if (playerIndex == 1)
            {
                spriteVisualsTransform.localPosition = new Vector3(originalSpritePosition.x, deeJayDefeatHeight, originalSpritePosition.z);
            }
        }
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
        if (coll != null) { coll.size = originalColliderSize; coll.offset = originalColliderOffset; }
        if (spriteVisualsTransform != null) { spriteVisualsTransform.localPosition = originalSpritePosition; }
    }

    // --- Coroutines ---

    private IEnumerator StunCoroutine(float duration)
    {
        isLockControl = true;
        yield return new WaitForSeconds(duration);
        if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Victory") && 
            !anim.GetCurrentAnimatorStateInfo(0).IsName("Defeat"))
        {
            isLockControl = false;
        }
    }

    // --- NEW: Stops movement for a short time when attacking ---
    private IEnumerator AttackMovementLock()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    // --- Inputs ---

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
                spriteVisualsTransform.localPosition = new Vector3(originalSpritePosition.x, 0f, originalSpritePosition.z);
        }
    }

    private void OnCrouchRelease(InputAction.CallbackContext context)
    {
        StopCrouching();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Cannot jump if attacking
        if (isGrounded && canJump && !isCrouching && !isBlocking && !isLockControl && !isAttacking)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false; 
            StartCoroutine(JumpCooldown()); 
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // Cannot attack if already attacking
        if (anim != null && !isCrouching && isGrounded && !isBlocking && !isLockControl && !isAttacking)
        {
            anim.SetTrigger("Attack");

            // --- NEW: Start the freeze timer ---
            StartCoroutine(AttackMovementLock());

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


    public void ResetState()
    {
        isLockControl = false;

        // returt phisics 
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        if (spriteVisualsTransform != null)
        {
            spriteVisualsTransform.localPosition = originalSpritePosition;
        }

        // reset anim
        if (anim != null)
        {
            anim.Rebind(); 
            anim.Update(0f);
        }

        isCrouching = false;
        isBlocking = false;
        isAttacking = false;
        StopAllCoroutines(); 
    }
}
