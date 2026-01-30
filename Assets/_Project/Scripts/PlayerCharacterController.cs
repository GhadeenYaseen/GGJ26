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
    [SerializeField] private float seatedYawClamp = 120f;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 sittingCameraLocalOffset = new Vector3(-0.179000005f, 1.19000006f, 0.460999995f);
    [SerializeField] private float cameraOffsetBlendSpeed = 6f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isSittingParam = "IsSitting";
    [SerializeField] private float speedDampTime = 0.1f;

    [Header("Sitting")]
    [SerializeField] private KeyCode sitToggleKey = KeyCode.C;
    [SerializeField] private float sitSnapSpeed = 10f;
    [SerializeField] private float sitRotateSpeed = 540f;
    [SerializeField] private Vector3 sitAlignOffset = new Vector3(-0.0700000003f, 0.0860000029f, 0.0170000009f);
    [SerializeField] private Vector3 sitPreOffset = Vector3.zero;
    [SerializeField] private float sitYawOffset = 0f;
    [SerializeField] private bool snapToSeatOnSit = true;
    [SerializeField] private bool lockToSeatWhileSitting = true;
    [SerializeField] private string standPromptMessage = "Press {key} to stand";
    [SerializeField] private float standPromptDuration = 2f;

    [Header("Dialogue")]
    [SerializeField] private DialogueUI dialogueUI;

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
    private int isSittingHash;
    private bool hasSpeedParam;
    private bool hasIsMovingParam;
    private bool hasIsSittingParam;
    private float stepTimer;
    private float pitch;
    private float seatedYaw;
    private bool isSitting;
    private Transform sitTarget;
    private Vector3 standingCameraLocalPos;
    private Vector3 sitLockedPosition;
    private Quaternion sitLockedRotation;
    private bool originalApplyRootMotion;
    private PlayerInteractor interactor;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        interactor = GetComponent<PlayerInteractor>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            originalApplyRootMotion = animator.applyRootMotion;
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

            if (!string.IsNullOrWhiteSpace(isSittingParam))
            {
                isSittingHash = Animator.StringToHash(isSittingParam);
                hasIsSittingParam = HasAnimatorParameter(animator, isSittingHash);
            }
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (cameraTransform != null)
        {
            standingCameraLocalPos = cameraTransform.localPosition;
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
        UpdateCameraOffset();

        if (Input.GetKeyDown(sitToggleKey))
        {
            if (isSitting)
            {
                StandUp();
            }
            else
            {
                SitDown(transform);
            }
        }

        if (isSitting)
        {
            UpdateSitPose();
            UpdateAnimator(false, 0f);
            UpdateAudio(false, Vector3.zero);
            return;
        }

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

        if (hasIsSittingParam)
        {
            animator.SetBool(isSittingHash, isSitting);
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

        if (!isSitting)
        {
            transform.Rotate(Vector3.up, mouseX, Space.World);
            seatedYaw = 0f;
        }
        else
        {
            seatedYaw = Mathf.Clamp(seatedYaw + mouseX, -seatedYawClamp, seatedYawClamp);
        }

        if (cameraTransform == null)
        {
            return;
        }

        pitch = Mathf.Clamp(pitch + mouseY, -pitchClamp, pitchClamp);
        float yaw = isSitting ? seatedYaw : 0f;
        cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateCameraOffset()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 targetPos = isSitting ? sittingCameraLocalOffset : standingCameraLocalPos;
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPos,
            cameraOffsetBlendSpeed * Time.deltaTime
        );
    }

    public void SitDown(Transform target)
    {
        CloseAllDialogue();

        isSitting = true;
        sitTarget = target;
        velocity = Vector3.zero;
        GetSitTargetPose(out sitLockedPosition, out sitLockedRotation);

        if (interactor != null && !string.IsNullOrWhiteSpace(standPromptMessage))
        {
            string keyLabel = sitToggleKey.ToString();
            string message = standPromptMessage.Replace("{key}", keyLabel);
            interactor.ShowTemporaryPrompt(message, standPromptDuration);
        }

        if (animator != null)
        {
            originalApplyRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = false;
        }

        if (sitTarget != null && snapToSeatOnSit)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(sitLockedPosition, sitLockedRotation);
            controller.enabled = true;
        }
    }

    public void StandUp()
    {
        isSitting = false;
        sitTarget = null;

        if (animator != null)
        {
            animator.applyRootMotion = originalApplyRootMotion;
        }

        CloseAllDialogue();
    }

    private void CloseAllDialogue()
    {
        if (dialogueUI != null)
        {
            dialogueUI.CloseDialogue();
            return;
        }

        DialogueUI[] allDialogue = Resources.FindObjectsOfTypeAll<DialogueUI>();
        foreach (DialogueUI ui in allDialogue)
        {
            if (ui == null)
            {
                continue;
            }

            if (!ui.gameObject.scene.IsValid())
            {
                continue;
            }

            ui.CloseDialogue();
        }
    }

    private void UpdateSitPose()
    {
        if (sitTarget == null)
        {
            return;
        }

        if (lockToSeatWhileSitting)
        {
            transform.SetPositionAndRotation(sitLockedPosition, sitLockedRotation);
            return;
        }

        GetSitTargetPose(out Vector3 targetPos, out Quaternion targetRot);
        transform.position = Vector3.Lerp(transform.position, targetPos, sitSnapSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, sitRotateSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (isSitting && lockToSeatWhileSitting && sitTarget != null)
        {
            transform.SetPositionAndRotation(sitLockedPosition, sitLockedRotation);
        }
    }

    private void GetSitTargetPose(out Vector3 targetPos, out Quaternion targetRot)
    {
        Vector3 totalOffset = sitAlignOffset + sitPreOffset;
        targetPos = sitTarget != null ? sitTarget.TransformPoint(totalOffset) : transform.position;
        float yaw = sitYawOffset;
        Quaternion yawOffset = Quaternion.Euler(0f, yaw, 0f);
        targetRot = sitTarget != null ? sitTarget.rotation * yawOffset : transform.rotation;
    }
}
