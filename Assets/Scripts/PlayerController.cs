using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody2D rb;
    private InputSystem_Actions controls; 
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Jump.performed += OnJump;
    }

    void OnDisable()
    {
        controls.Player.Disable();
        controls.Player.Jump.performed -= OnJump;
    }

    void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
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
