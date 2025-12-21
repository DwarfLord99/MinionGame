using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Player Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;

    public float playerSpeed = 0.0f;

    private InputAction moveAction;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        Vector3 velocity = moveDirection * moveSpeed;

        rb.linearVelocity = velocity;
        playerSpeed = velocity.magnitude;

        // Update animator parameters
        animator.SetFloat("Speed", velocity.magnitude);

        // Rotate player to face movement direction if moving
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }
}
