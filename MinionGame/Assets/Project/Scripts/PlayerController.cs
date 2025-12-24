using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Player Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public float playerSpeed = 0.0f;
    private float cameraRotSpeed = 720f;

    private InputAction moveAction;
    private InputAction targetAction;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        targetAction = InputSystem.actions.FindAction("Target");
    }

    void Update()
    {
        HandleTargeting();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        // Get camera direction
        Vector3 camForward = virtualCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = virtualCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDirection = camForward * inputVector.y + camRight * inputVector.x;
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

    private void HandleTargeting()
    {
        if (targetAction.triggered)
        {
            // Implement targeting logic here
            Debug.Log("Target action triggered");

            Vector3 forward = rb.transform.forward;
            forward.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(forward);
            StopAllCoroutines();
            StartCoroutine(RotateToTarget(targetRotation));
        }
    }

    IEnumerator RotateToTarget(Quaternion targetRotation)
    {
        while (Quaternion.Angle(virtualCamera.transform.rotation, targetRotation) > 0.1f)
        {
            virtualCamera.transform.rotation = Quaternion.RotateTowards(virtualCamera.transform.rotation, 
                targetRotation, cameraRotSpeed * Time.deltaTime);
        }
        yield return null;
    }
}
