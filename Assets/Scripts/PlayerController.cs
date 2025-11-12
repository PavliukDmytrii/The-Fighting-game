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

    // --- We need to store the specific actions ---
    private InputAction moveAction;
    private InputAction jumpAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();

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
            // *** YOU MUST CREATE this "Player2" map in your input asset ***
            moveAction = controls.Player2.Move;
            jumpAction = controls.Player2.Jump;
            controls.Player2.Enable();
        }
    }

    void OnEnable()
    {
        // Only register the jump action that we are actually using
        if (jumpAction != null)
        {
            jumpAction.performed += OnJump;
        }
    }

    void OnDisable()
    {
        // Only unregister the action we were using
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
        }

        // You could also just disable all maps
        controls.Player.Disable();
        controls.Player2.Disable();
    }

    void Update()
    {
        // Read value from the correct action
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}