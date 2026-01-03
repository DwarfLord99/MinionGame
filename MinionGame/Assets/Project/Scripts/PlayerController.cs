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

    private CinemachineTransposer transposer;
    private Vector3 initialFollowOffset;

    private InputAction moveAction;
    private InputAction targetAction;

    private Coroutine cameraLerp;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        targetAction = InputSystem.actions.FindAction("Target");

        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        initialFollowOffset = transposer.m_FollowOffset;

        if(transposer != null)
        {
            transposer.m_FollowOffset = new Vector3(0, 6, -8);
        }
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

    IEnumerator LerpCameraOffset(Vector3 targetOffset, float duration = 0.2f)
    {
        Vector3 startOffset = transposer.m_FollowOffset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transposer.m_FollowOffset = Vector3.Lerp(startOffset, targetOffset, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transposer.m_FollowOffset = targetOffset;
    }

    IEnumerator RotateToTarget(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, cameraRotSpeed * Time.deltaTime);
            yield return null;
        }

        // Now reset the camera behind the player
        if (transposer != null)
        {
            // Get the current offset's distance (XZ plane) and height (Y)
            float distance = new Vector2(transposer.m_FollowOffset.x, transposer.m_FollowOffset.z).magnitude;
            float height = transposer.m_FollowOffset.y;

            // Calculate the new offset so the camera is behind the player
            Vector3 newOffset = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(0, 0, -distance);
            newOffset.y = height;

            if (cameraLerp != null)
            {
                StopCoroutine(cameraLerp);
            }

            cameraLerp = StartCoroutine(LerpCameraOffset(newOffset));
        }
    }
}
