using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference slowAction;

    [Header("View")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 0.05f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private bool lockCursor = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float shiftSlowMultiplier = 0.5f;
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float fallGravityMultiplier = 5f;
    [SerializeField] private float riseGravityMultiplier = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch;
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (cameraTransform != null)
        {
            pitch = cameraTransform.localEulerAngles.x;
            if (pitch > 180f)
            {
                pitch -= 360f;
            }
        }
    }

    private void OnEnable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
        }

        if (lookAction != null)
        {
            lookAction.action.Enable();
            lookAction.action.performed += OnLookPerformed;
            lookAction.action.canceled += OnLookCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.action.Enable();
            jumpAction.action.performed += OnJumpPerformed;
        }

        if (slowAction != null)
        {
            slowAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (moveAction != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }

        if (lookAction != null)
        {
            lookAction.action.performed -= OnLookPerformed;
            lookAction.action.canceled -= OnLookCanceled;
            lookAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
            jumpAction.action.Disable();
        }

        if (slowAction != null)
        {
            slowAction.action.Disable();
        }
    }

    private void Update()
    {
        ApplyLook();
    }

    private void FixedUpdate()
    {
        Transform moveReference = cameraTransform != null ? cameraTransform : transform;

        Vector3 flatForward = moveReference.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 flatRight = moveReference.right;
        flatRight.y = 0f;
        flatRight.Normalize();

        Vector3 moveDir = (flatRight * moveInput.x + flatForward * moveInput.y).normalized;
        bool slowHeld = slowAction != null && slowAction.action.IsPressed();
        float currentMoveSpeed = slowHeld ? moveSpeed * shiftSlowMultiplier : moveSpeed;
        Vector3 targetVelocity = moveDir * currentMoveSpeed;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !IsGrounded())
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (riseGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        if (jumpPressed && IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpPressed = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void ApplyLook()
    {
        if (cameraTransform == null)
        {
            return;
        }

        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            return false;
        }

        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
