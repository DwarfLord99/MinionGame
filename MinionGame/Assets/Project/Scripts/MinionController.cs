using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinionController : MonoBehaviour
{
    [Header("Minion Stats")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float followRange = 5f;

    [Header("Minion Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;


    private InputAction minionSweep;
    private InputAction minionSend;
    private InputAction minionRecall;

    private bool isSweeping = false;

    void Start()
    {
        minionSweep = InputSystem.actions.FindAction("Sweep");
        minionSend = InputSystem.actions.FindAction("Send");
        minionRecall = InputSystem.actions.FindAction("Recall");
    }

    void Update()
    {
        // Calculate distance between minion and player within a specified range
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is within follow range
        if (distanceToPlayer > followRange && !isSweeping)
        {
            // Move towards player
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

            // Update animator parameters
            Vector3 moveDirection = (playerTransform.position - transform.position).normalized;
            animator.SetFloat("Speed", moveDirection.magnitude * moveSpeed);

            // Rotate minion to face player
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        if (distanceToPlayer <= followRange && playerController.playerSpeed <= 0.0f && !isSweeping)
        {
            // Stop moving when within range
            rb.linearVelocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        }
    }

    void FixedUpdate()
    {
        if(minionSweep.inProgress)
        {
            HandleSweep();
        }
        else if(minionSend.inProgress && minionRecall.inProgress)
        {
            HandleMouseSweep();
        }
        else
        {
            isSweeping = false;
        }
    }

    private void HandleMouseSweep()
    {
        // Raycast from mouse position to ground plane
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = (hitPoint - transform.position).normalized;
            Vector3 velocity = new Vector3(direction.x, 0f, direction.z) * moveSpeed;
            rb.linearVelocity = velocity;

            // Update animator parameters
            animator.SetFloat("Speed", velocity.magnitude);

            // Rotate minion to face movement direction
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }

            isSweeping = true;
        }
    }

    private void HandleSweep()
    {
        Vector2 inputVector = minionSweep.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        Vector3 velocity = moveDirection * moveSpeed;

        rb.linearVelocity = velocity;

        // Update animator parameters
        animator.SetFloat("Speed", velocity.magnitude);

        // Rotate minion to face movement direction if moving
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }

        isSweeping = true;
    }
}
