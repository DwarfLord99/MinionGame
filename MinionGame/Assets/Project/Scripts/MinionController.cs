using System;
using System.Collections;
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
    [SerializeField] private Transform minionPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float returnDelay = 0.5f;

    private InputAction minionSweep;
    private InputAction minionSend;
    private InputAction minionRecall;

    private bool isSweeping = false;
    private bool isReturningDelay = false;

    void Start()
    {
        minionSweep = InputSystem.actions.FindAction("Sweep");
        minionSend = InputSystem.actions.FindAction("Send");
        minionRecall = InputSystem.actions.FindAction("Recall");
    }

    void Update()
    {
        
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
            if(isSweeping)
            {
                // Sweep just ended, start return delay
                StartCoroutine(ReturnDelay());
            }
            isSweeping = false;

            HandleFollowAndReturn();
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
        animator.SetFloat("Speed", 1f);

        // Rotate minion to face movement direction if moving
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }

        isSweeping = true;
    }

    IEnumerator ReturnDelay()
    {
        isReturningDelay = true;
        rb.linearVelocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);
        yield return new WaitForSeconds(returnDelay);
        isReturningDelay = false;
    }

    private void HandleFollowAndReturn()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (!isReturningDelay)
        {
            if (playerController.playerSpeed > 0.0f)
            {
                // Calculate a point behind the player
                Vector3 behindPlayer = playerTransform.position - playerTransform.forward * followRange;

                // Move towards the point behind the player
                Vector3 targetPos = Vector3.MoveTowards(transform.position, behindPlayer, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(targetPos);

                // Update animator parameters
                Vector3 moveDirection = (behindPlayer - transform.position).normalized;
                animator.SetFloat("Speed", 1f);

                // Rotate minion to face movement direction
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                }
            }
            else
            {
                // Return to minion point when player is not moving
                Vector3 targetPos = Vector3.MoveTowards(transform.position, minionPoint.position, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(targetPos);
                animator.SetFloat("Speed", 1f);

                // Rotate minion to face movement direction
                Vector3 moveDirection = (minionPoint.position - transform.position).normalized;
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                }

                if (Vector3.Distance(transform.position, minionPoint.position) < 0.1f)
                {
                    animator.SetFloat("Speed", 0f);
                }
            }
        }
    }

}
