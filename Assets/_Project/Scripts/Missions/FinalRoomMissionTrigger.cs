using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FinalRoomMissionTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private string playerTag = "Player";

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera missionCamera;
    [SerializeField] private int missionCameraPriority = 20;
    [SerializeField] private Camera raycastCamera;

    [Header("Selection")]
    [SerializeField] private Transform[] selectableTargets;
    [SerializeField] private LayerMask selectionMask = ~0;
    [SerializeField] private float selectionDistance = 6f;
    [SerializeField] private KeyCode selectKey = KeyCode.Return;

    [Header("Disable Player")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;
    [SerializeField] private CharacterController characterController;

    private readonly HashSet<Collider> selectableColliders = new HashSet<Collider>();
    private readonly HashSet<OutlineHighlight> selectableHighlights = new HashSet<OutlineHighlight>();
    private OutlineHighlight currentHighlight;
    private bool missionActive;
    private int previousCameraPriority;
    private bool hasPreviousPriority;

    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }

        if (raycastCamera == null)
        {
            raycastCamera = Camera.main;
        }

        BuildSelectableCache();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || !other.CompareTag(playerTag))
        {
            return;
        }

        Activate();
    }

    private void Update()
    {
        if (!missionActive)
        {
            return;
        }

        if (raycastCamera == null)
        {
            raycastCamera = Camera.main;
            if (raycastCamera == null)
            {
                return;
            }
        }

        Ray ray = raycastCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, selectionDistance, selectionMask, QueryTriggerInteraction.Collide))
        {
            OutlineHighlight highlight = GetHighlightFromHit(hit.collider);
            SetHighlight(highlight);

            if (highlight != null && Input.GetKeyDown(selectKey))
            {
                Debug.Log($"Final mission selected: {highlight.name}");
            }
        }
        else
        {
            SetHighlight(null);
        }
    }

    public void Activate()
    {
        if (missionActive)
        {
            return;
        }

        Debug.Log($"FinalRoomMissionTrigger: Activated by player.", this);
        missionActive = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (missionCamera != null)
        {
            previousCameraPriority = missionCamera.Priority;
            hasPreviousPriority = true;
            missionCamera.Priority = missionCameraPriority;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (componentsToDisable != null)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                if (componentsToDisable[i] != null)
                {
                    componentsToDisable[i].enabled = false;
                }
            }
        }
    }

    private void BuildSelectableCache()
    {
        selectableColliders.Clear();
        selectableHighlights.Clear();

        if (selectableTargets == null)
        {
            return;
        }

        foreach (Transform target in selectableTargets)
        {
            if (target == null)
            {
                continue;
            }

            Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                if (collider != null)
                {
                    selectableColliders.Add(collider);
                }
            }

            OutlineHighlight highlight = target.GetComponentInChildren<OutlineHighlight>(true);
            if (highlight != null)
            {
                selectableHighlights.Add(highlight);
            }
        }
    }

    private OutlineHighlight GetHighlightFromHit(Collider hitCollider)
    {
        if (hitCollider == null || !selectableColliders.Contains(hitCollider))
        {
            return null;
        }

        OutlineHighlight highlight = hitCollider.GetComponentInParent<OutlineHighlight>();
        if (highlight != null && selectableHighlights.Contains(highlight))
        {
            return highlight;
        }

        return null;
    }

    private void SetHighlight(OutlineHighlight highlight)
    {
        if (currentHighlight == highlight)
        {
            return;
        }

        if (currentHighlight != null)
        {
            currentHighlight.SetHighlighted(false);
        }

        currentHighlight = highlight;

        if (currentHighlight != null)
        {
            currentHighlight.SetHighlighted(true);
        }
    }
}
