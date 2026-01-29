using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform cameraTransform;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float pitchClamp = 80f;
    [SerializeField] private bool invertY = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private float speedDampTime = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float stepSpeedThreshold = 0.1f;
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private bool breatheWhileMoving = false;
    [SerializeField] private bool breatheWhileIdle = true;
    [SerializeField] private float breathingVolume = 0.6f;

    private CharacterController controller;
    private Vector3 velocity;
    private int speedHash;
    private int isMovingHash;
    private bool hasSpeedParam;
    private bool hasIsMovingParam;
    private float stepTimer;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            if (!string.IsNullOrWhiteSpace(speedParam))
            {
                speedHash = Animator.StringToHash(speedParam);
                hasSpeedParam = HasAnimatorParameter(animator, speedHash);
            }

            if (!string.IsNullOrWhiteSpace(isMovingParam))
            {
                isMovingHash = Animator.StringToHash(isMovingParam);
                hasIsMovingParam = HasAnimatorParameter(animator, isMovingHash);
            }
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (breathingSource != null && breathingClip != null)
        {
            breathingSource.loop = true;
            breathingSource.clip = breathingClip;
            breathingSource.volume = breathingVolume;
        }
    }

    private void Update()
    {
        HandleLook();

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 inputDirection = new Vector3(input.x, 0f, input.y);

        bool hasInput = inputDirection.sqrMagnitude > 0.001f;
        Vector3 moveDirection = hasInput
            ? (transform.right * inputDirection.x + transform.forward * inputDirection.z).normalized
            : Vector3.zero;

        Vector3 planarMove = moveDirection * (hasInput ? moveSpeed : 0f);

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 motion = new Vector3(planarMove.x, velocity.y, planarMove.z);
        controller.Move(motion * Time.deltaTime);

        UpdateAnimator(hasInput, planarMove.magnitude / Mathf.Max(0.001f, moveSpeed));
        UpdateAudio(hasInput, planarMove);
    }

    private void UpdateAnimator(bool hasInput, float normalizedSpeed)
    {
        if (animator == null)
        {
            return;
        }

        if (hasSpeedParam)
        {
            animator.SetFloat(speedHash, normalizedSpeed, speedDampTime, Time.deltaTime);
        }

        if (hasIsMovingParam)
        {
            animator.SetBool(isMovingHash, hasInput);
        }
    }

    private static bool HasAnimatorParameter(Animator targetAnimator, int nameHash)
    {
        foreach (AnimatorControllerParameter parameter in targetAnimator.parameters)
        {
            if (parameter.nameHash == nameHash)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateAudio(bool hasInput, Vector3 planarMove)
    {
        float speed = planarMove.magnitude;
        bool isMoving = hasInput && speed > stepSpeedThreshold && controller.isGrounded;

        if (movementSource != null && footstepClips != null && footstepClips.Length > 0)
        {
            if (isMoving)
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval)
                {
                    stepTimer = 0f;
                    int clipIndex = Random.Range(0, footstepClips.Length);
                    movementSource.PlayOneShot(footstepClips[clipIndex]);
                }
            }
            else
            {
                stepTimer = 0f;
            }
        }
        if (breathingSource != null && breathingClip != null)
        {
            bool shouldBreathe = (breatheWhileIdle && !isMoving) || (breatheWhileMoving && isMoving);
            if (shouldBreathe)
            {
                if (!breathingSource.isPlaying)
                {
                    breathingSource.Play();
                }
            }
            else if (breathingSource.isPlaying)
            {
                breathingSource.Stop();
            }
        }
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);

        transform.Rotate(Vector3.up, mouseX, Space.World);

        if (cameraTransform == null)
        {
            return;
        }

        pitch = Mathf.Clamp(pitch + mouseY, -pitchClamp, pitchClamp);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
