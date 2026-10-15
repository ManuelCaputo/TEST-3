using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference sprintAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference crouchAction;

    [Header("Camera")]
    [Tooltip("Leave empty to use the Main Camera automatically.")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] float turnSpeed = 720f; // degrees per second

    [Header("Animation")]
    [SerializeField] float dampTime = 0.1f;

    [Header("Movement Speeds (units per second)")]
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float crouchSpeed = 1.2f;

    Animator animator;

    static readonly int MoveX = Animator.StringToHash("MoveX");
    static readonly int MoveY = Animator.StringToHash("MoveY");
    static readonly int JumpTrigger = Animator.StringToHash("Jump");
    static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false; // the script moves the character instead

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
        jumpAction.action.Disable();
        crouchAction.action.Disable();
    }

    void Update()
    {
        // Turn the character to face the direction the camera is looking (yaw only)
        if (cameraTransform != null)
        {
            Quaternion target = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        bool crouching = crouchAction.action.IsPressed();
        bool sprinting = sprintAction.action.IsPressed() && !crouching;

        // Animation: walk = magnitude 1, run = magnitude 2
        Vector2 animMove = sprinting ? input * 2f : input;
        animator.SetFloat(MoveX, animMove.x, dampTime, Time.deltaTime);
        animator.SetFloat(MoveY, animMove.y, dampTime, Time.deltaTime);

        // Actual movement, relative to the direction the character (and camera) faces
        float speed = crouching ? crouchSpeed : (sprinting ? runSpeed : walkSpeed);
        Vector3 direction = new Vector3(input.x, 0f, input.y);
        transform.Translate(direction * speed * Time.deltaTime, Space.Self);

        // Jump (one-shot trigger)
        if (jumpAction.action.WasPressedThisFrame())
            animator.SetTrigger(JumpTrigger);

        // Crouch (hold to crouch)
        animator.SetBool(IsCrouching, crouching);
    }
}