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
        }
        else if (distanceToPlayer <= followRange && !isSweeping)
        {
            // Stop moving when within range
            rb.linearVelocity = Vector3.zero;
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
            isSweeping = true;
        }
    }

    private void HandleSweep()
    {
        Vector2 inputVector = minionSweep.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        Vector3 velocity = moveDirection * moveSpeed;

        rb.linearVelocity = velocity;
        isSweeping = true;
    }
}
