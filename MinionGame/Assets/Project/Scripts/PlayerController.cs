using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Player Components")]
    [SerializeField] private Rigidbody rb;

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
    }
}
