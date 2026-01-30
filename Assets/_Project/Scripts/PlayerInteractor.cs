using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private float interactMaxViewportDistance = 0.75f;
    [SerializeField] private float interactMaxAngle = 60f;
    [SerializeField] private LayerMask interactMask = ~0;
    [SerializeField] private bool requireCenterLook = true;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string defaultPromptMessage = "Press {key} to interact";

    private PlayerCharacterController player;
    private Camera viewCamera;
    private IInteractable currentInteractable;
    private OutlineHighlight currentHighlight;
    private float temporaryPromptTimer;
    private string temporaryPromptMessage;

    private void Awake()
    {
        player = GetComponent<PlayerCharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (cameraTransform != null)
        {
            viewCamera = cameraTransform.GetComponent<Camera>();
        }
        if (viewCamera == null && Camera.main != null)
        {
            viewCamera = Camera.main;
        }
    }

    private void Update()
    {
        UpdateHighlight();
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }

        if (temporaryPromptTimer > 0f)
        {
            temporaryPromptTimer -= Time.deltaTime;
            if (temporaryPromptTimer <= 0f)
            {
                temporaryPromptMessage = null;
                UpdatePrompt();
            }
        }
    }

    private void TryInteract()
    {
        if (player == null)
        {
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact(player);
        }
    }

    private void UpdateHighlight()
    {
        IInteractable interactable;
        OutlineHighlight highlight;

        if (requireCenterLook)
        {
            TryGetInteractableFromCenterRay(out interactable, out highlight);
        }
        else
        {
            FindBestInteractable(out interactable, out highlight);
        }

        if (currentHighlight != highlight)
        {
            if (currentHighlight != null)
            {
                currentHighlight.SetHighlighted(false);
            }

            if (highlight != null)
            {
                highlight.SetHighlighted(true);
            }

            currentHighlight = highlight;
        }

        currentInteractable = interactable;
        UpdatePrompt();
    }

    private void TryGetInteractableFromCenterRay(out IInteractable interactable, out OutlineHighlight highlight)
    {
        interactable = null;
        highlight = null;

        if (cameraTransform == null)
        {
            return;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                return;
            }

            Component interactableComponent = interactable as Component;
            highlight = interactableComponent != null
                ? interactableComponent.GetComponentInParent<OutlineHighlight>()
                : null;
        }
    }

    private void FindBestInteractable(out IInteractable bestInteractable, out OutlineHighlight bestHighlight)
    {
        bestInteractable = null;
        bestHighlight = null;

        if (cameraTransform == null)
        {
            return;
        }

        Vector3 camPos = cameraTransform.position;
        Vector3 camForward = cameraTransform.forward;

        Collider[] hits = Physics.OverlapSphere(
            camPos,
            interactDistance,
            interactMask,
            QueryTriggerInteraction.Collide
        );

        float bestViewportDist = float.MaxValue;
        float bestWorldDist = float.MaxValue;
        float bestAngle = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                continue;
            }

            Vector3 targetPos = hit.bounds.center;
            Vector3 toTarget = targetPos - camPos;
            float worldDist = toTarget.magnitude;
            if (worldDist < 0.001f)
            {
                continue;
            }

            float angle = Vector3.Angle(camForward, toTarget);
            if (angle > interactMaxAngle)
            {
                continue;
            }

            float viewportDist = angle;
            if (viewCamera != null)
            {
                Vector3 viewport = viewCamera.WorldToViewportPoint(targetPos);
                if (viewport.z <= 0f)
                {
                    continue;
                }
                if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
                {
                    continue;
                }

                Vector2 offset = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
                viewportDist = offset.magnitude;
                if (viewportDist > interactMaxViewportDistance)
                {
                    continue;
                }
            }

            bool isBetter = viewportDist < bestViewportDist
                || (Mathf.Approximately(viewportDist, bestViewportDist) && worldDist < bestWorldDist)
                || (viewCamera == null && angle < bestAngle);

            if (isBetter)
            {
                bestInteractable = interactable;
                Component interactableComponent = interactable as Component;
                bestHighlight = interactableComponent != null
                    ? interactableComponent.GetComponentInParent<OutlineHighlight>()
                    : null;
                bestViewportDist = viewportDist;
                bestWorldDist = worldDist;
                bestAngle = angle;
            }
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(temporaryPromptMessage) && temporaryPromptTimer > 0f)
        {
            promptText.text = temporaryPromptMessage;
            promptText.enabled = true;
            return;
        }

        bool shouldShow = currentInteractable != null;
        string prompt = shouldShow ? currentInteractable.GetPrompt(player) : string.Empty;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            prompt = defaultPromptMessage;
        }

        string keyLabel = interactKey.ToString();
        promptText.text = shouldShow ? prompt.Replace("{key}", keyLabel) : string.Empty;
        promptText.enabled = shouldShow;
    }

    public void ShowTemporaryPrompt(string message, float durationSeconds)
    {
        if (promptText == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        temporaryPromptMessage = message;
        temporaryPromptTimer = Mathf.Max(0.01f, durationSeconds);
        UpdatePrompt();
    }
}
