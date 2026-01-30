using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private float interactMaxViewportDistance = 0.75f;
    [SerializeField] private float interactMaxAngle = 60f;
    [SerializeField] private LayerMask interactMask = ~0;

    private PlayerCharacterController player;
    private Camera viewCamera;

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
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (cameraTransform == null || player == null)
        {
            return;
        }

        IInteractable bestInteractable = FindBestInteractable();
        if (bestInteractable != null)
        {
            bestInteractable.Interact(player);
        }
    }

    private IInteractable FindBestInteractable()
    {
        Vector3 camPos = cameraTransform.position;
        Vector3 camForward = cameraTransform.forward;

        Collider[] hits = Physics.OverlapSphere(
            camPos,
            interactDistance,
            interactMask,
            QueryTriggerInteraction.Collide
        );

        IInteractable best = null;
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
                best = interactable;
                bestViewportDist = viewportDist;
                bestWorldDist = worldDist;
                bestAngle = angle;
            }
        }

        return best;
    }
}
