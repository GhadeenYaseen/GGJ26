using System.Collections.Generic;
using Cinemachine;
using TMPro;
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
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private string instructionMessage = "Use Arrow Keys to choose, Enter to select";
    [SerializeField] private bool disableAfterSelection = true;

    [Header("Disable Player")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;
    [SerializeField] private CharacterController characterController;

    private readonly HashSet<Collider> selectableColliders = new HashSet<Collider>();
    private readonly HashSet<OutlineHighlight> selectableHighlights = new HashSet<OutlineHighlight>();
    private OutlineHighlight currentHighlight;
    private bool missionActive;
    private int previousCameraPriority;
    private bool hasPreviousPriority;
    private int currentIndex = -1;
    private OutlineHighlight[] targetHighlights;

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
        SetInstructionVisible(false);
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

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }

        if (Input.GetKeyDown(selectKey))
        {
            SelectCurrent();
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

        currentIndex = GetFirstValidIndex();
        SetHighlightByIndex(currentIndex);
        SetInstructionVisible(true);
    }

    private void BuildSelectableCache()
    {
        selectableColliders.Clear();
        selectableHighlights.Clear();

        if (selectableTargets == null)
        {
            return;
        }

        targetHighlights = new OutlineHighlight[selectableTargets.Length];

        for (int i = 0; i < selectableTargets.Length; i++)
        {
            Transform target = selectableTargets[i];
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
            targetHighlights[i] = highlight;
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

    private void MoveSelection(int direction)
    {
        if (selectableTargets == null || selectableTargets.Length == 0)
        {
            return;
        }

        int nextIndex = currentIndex;
        for (int i = 0; i < selectableTargets.Length; i++)
        {
            nextIndex = (nextIndex + direction + selectableTargets.Length) % selectableTargets.Length;
            if (selectableTargets[nextIndex] != null && targetHighlights[nextIndex] != null)
            {
                currentIndex = nextIndex;
                SetHighlightByIndex(currentIndex);
                return;
            }
        }
    }

    private void SetHighlightByIndex(int index)
    {
        if (index < 0 || selectableTargets == null || index >= selectableTargets.Length)
        {
            SetHighlight(null);
            return;
        }

        SetHighlight(targetHighlights != null ? targetHighlights[index] : null);
    }

    private int GetFirstValidIndex()
    {
        if (selectableTargets == null)
        {
            return -1;
        }

        for (int i = 0; i < selectableTargets.Length; i++)
        {
            if (selectableTargets[i] != null && targetHighlights != null && targetHighlights[i] != null)
            {
                return i;
            }
        }

        return -1;
    }

    private void SelectCurrent()
    {
        if (currentIndex < 0 || selectableTargets == null || currentIndex >= selectableTargets.Length)
        {
            return;
        }

        Transform target = selectableTargets[currentIndex];
        if (target == null)
        {
            return;
        }

        Debug.Log($"Final mission selected: {target.name}");
        FinalRoomSelectableAction action = target.GetComponentInParent<FinalRoomSelectableAction>();
        if (action != null)
        {
            action.ActivateAction();
        }
        else
        {
            Debug.LogWarning($"FinalRoomMissionTrigger: No FinalRoomSelectableAction on {target.name} or its parents.");
        }

        if (disableAfterSelection)
        {
            missionActive = false;
            SetHighlight(null);
            SetInstructionVisible(false);
        }
    }

    private void SetInstructionVisible(bool isVisible)
    {
        if (instructionText == null)
        {
            return;
        }

        instructionText.text = isVisible ? instructionMessage : string.Empty;
        instructionText.enabled = isVisible;
    }
}
