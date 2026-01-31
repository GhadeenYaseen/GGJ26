using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FinalRoomSelectableAction : MonoBehaviour
{
    [Header("Shared Settings")]
    [SerializeField] private FinalRoomActionSettings sharedSettings;

    [Header("Duplicate Character")]
    [SerializeField] private GameObject sourceCharacter;
    [SerializeField] private Transform[] cloneSpawnPoints;
    [SerializeField] private Vector3[] cloneWorldPositions;
    [SerializeField] private float duplicateYawDegrees = 180f;
    [SerializeField] private bool useSpawnPoints = true;
    [SerializeField] private int cloneCount = 2;

    [Header("Environment Lighting")]
    [SerializeField] private bool disableAmbientLight = true;
    [SerializeField] private Color ambientOffColor = Color.black;
    [SerializeField] private float ambientOffIntensity = 0f;
    [SerializeField] private Light[] environmentLightsToDisable;

    [Header("Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraAttachDelay = 2f;
    [SerializeField] private string headTag = "Head";
    [SerializeField] private Vector3 cameraLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 cameraLocalEuler = Vector3.zero;
    [SerializeField] private GameObject[] objectsToActivateWithPrimaryCamera;
    [SerializeField] private CinemachineVirtualCamera secondaryVirtualCamera;
    [SerializeField] private float secondaryCameraDelay = 2f;
    [SerializeField] private bool disablePrimaryWhenSecondaryActive = false;
    [SerializeField] private GameObject[] objectsToActivateWithSecondaryCamera;
    [SerializeField] private int primaryCameraPriority = 20;
    [SerializeField] private int secondaryCameraPriority = 30;
    [SerializeField] private int primaryCameraPriorityWhenSecondaryActive = 0;

    [Header("Typing UI")]
    [TextArea(2, 6)]
    [SerializeField] private string selectionTextPrimary;
    [TextArea(2, 6)]
    [SerializeField] private string selectionTextSecondary;
    [SerializeField] private string selectionTextSeparator = "\n\n";
    [SerializeField] private Color primaryTextColor = new Color(1f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color secondaryTextColor = Color.white;
    [SerializeField] private TypewriterPanel typingPanel;

    [Header("Animator Override")]
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Deactivate Objects")]
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private GameObject[] selectableObjectsToDeactivate;
    [SerializeField] private FinalRoomMissionTrigger missionTrigger;

    [Header("Behavior")]
    [SerializeField] private bool playOnce = true;

    private bool hasPlayed;
    private readonly List<GameObject> spawnedClones = new List<GameObject>();
    private static CoroutineHost coroutineHost;
    private float ambientBeforeTransition;

    private sealed class CoroutineHost : MonoBehaviour { }

    private void Reset()
    {
        if (sourceCharacter == null)
        {
            sourceCharacter = gameObject;
        }
    }

    public void ActivateAction()
    {
        if (playOnce && hasPlayed)
        {
            return;
        }

        hasPlayed = true;
        if (MusicStateManager.Instance != null)
        {
            MusicStateManager.Instance.SetAfterSelect();
        }

        DisableEnvironmentLighting();
        spawnedClones.Clear();
        List<GameObject> clones = DuplicateClones();
        DeactivateObjects();
        DisableSelectables();

        if (clones.Count > 1)
        {
            StartCameraAttach(clones[1]);
        }
        else if (clones.Count > 0)
        {
            StartCameraAttach(clones[0]);
        }
    }

    private List<GameObject> DuplicateClones()
    {
        List<GameObject> clones = spawnedClones;
        if (sourceCharacter == null)
        {
            Debug.LogWarning($"{name}: Source character not assigned.");
            return clones;
        }

        int count = GetCloneCount();
        if (count <= 0)
        {
            return clones;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetClonePosition(i);
            Quaternion baseRot = sourceCharacter.transform.rotation;
            GameObject clone = Instantiate(sourceCharacter, spawnPos, baseRot);
            if (clone != null)
            {
                if (i == 0)
                {
                    clone.transform.rotation = baseRot * Quaternion.Euler(0f, GetDuplicateYawDegrees(), 0f);
                }
                else
                {
                    clone.transform.rotation = baseRot;
                    Vector3 scale = clone.transform.localScale;
                    scale.x = -Mathf.Abs(scale.x);
                    clone.transform.localScale = scale;
                }
                OutlineHighlight[] highlights = clone.GetComponentsInChildren<OutlineHighlight>(true);
                for (int h = 0; h < highlights.Length; h++)
                {
                    if (highlights[h] != null)
                    {
                        highlights[h].SetHighlighted(false);
                        highlights[h].enabled = false;
                    }
                }
                StripOutlineMaterials(clone);
                clones.Add(clone);
            }
        }

        RestartCloneAnimations(clones);
        return clones;
    }

    private void DisableEnvironmentLighting()
    {
        if (GetDisableAmbientLight())
        {
            RenderSettings.ambientLight = GetAmbientOffColor();
            RenderSettings.ambientIntensity = GetAmbientOffIntensity();
        }

        Light[] lights = GetEnvironmentLightsToDisable();
        if (lights == null)
        {
            return;
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].enabled = false;
            }
        }
    }

    private void DeactivateObjects()
    {
        GameObject[] objects = GetObjectsToDeactivate();
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }
    }

    private void DisableSelectables()
    {
        FinalRoomMissionTrigger trigger = GetMissionTrigger();
        if (trigger != null)
        {
            trigger.DisableSelectableTargets();
            return;
        }

        GameObject[] objects = GetSelectableObjectsToDeactivate();
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }
    }

    private float GetDuplicateYawDegrees()
    {
        return sharedSettings != null ? sharedSettings.DuplicateYawDegrees : duplicateYawDegrees;
    }

    private Transform[] GetCloneSpawnPoints()
    {
        return sharedSettings != null && sharedSettings.CloneSpawnPoints != null && sharedSettings.CloneSpawnPoints.Length > 0
            ? sharedSettings.CloneSpawnPoints
            : cloneSpawnPoints;
    }

    private Vector3[] GetCloneWorldPositions()
    {
        return sharedSettings != null && sharedSettings.CloneWorldPositions != null && sharedSettings.CloneWorldPositions.Length > 0
            ? sharedSettings.CloneWorldPositions
            : cloneWorldPositions;
    }

    private bool GetUseSpawnPoints()
    {
        return sharedSettings != null ? sharedSettings.UseSpawnPoints : useSpawnPoints;
    }

    private int GetCloneCount()
    {
        return sharedSettings != null ? sharedSettings.CloneCount : cloneCount;
    }

    private Vector3 GetClonePosition(int index)
    {
        if (GetUseSpawnPoints())
        {
            Transform[] points = GetCloneSpawnPoints();
            if (points != null && points.Length > 0)
            {
                int safeIndex = Mathf.Clamp(index, 0, points.Length - 1);
                if (points[safeIndex] != null)
                {
                    return points[safeIndex].position;
                }
            }
        }

        Vector3[] positions = GetCloneWorldPositions();
        if (positions != null && positions.Length > 0)
        {
            int safeIndex = Mathf.Clamp(index, 0, positions.Length - 1);
            return positions[safeIndex];
        }

        return transform.position;
    }

    private bool GetDisableAmbientLight()
    {
        return sharedSettings != null ? sharedSettings.DisableAmbientLight : disableAmbientLight;
    }

    private Color GetAmbientOffColor()
    {
        return sharedSettings != null ? sharedSettings.AmbientOffColor : ambientOffColor;
    }

    private float GetAmbientOffIntensity()
    {
        return sharedSettings != null ? sharedSettings.AmbientOffIntensity : ambientOffIntensity;
    }

    private Light[] GetEnvironmentLightsToDisable()
    {
        return sharedSettings != null ? sharedSettings.EnvironmentLightsToDisable : environmentLightsToDisable;
    }

    private CinemachineVirtualCamera GetVirtualCamera()
    {
        return sharedSettings != null && sharedSettings.VirtualCamera != null
            ? sharedSettings.VirtualCamera
            : virtualCamera;
    }

    private float GetCameraAttachDelay()
    {
        return sharedSettings != null ? sharedSettings.CameraAttachDelay : cameraAttachDelay;
    }

    private string GetHeadTag()
    {
        return sharedSettings != null && !string.IsNullOrWhiteSpace(sharedSettings.HeadTag)
            ? sharedSettings.HeadTag
            : headTag;
    }

    private Vector3 GetCameraLocalPosition()
    {
        return sharedSettings != null ? sharedSettings.CameraLocalPosition : cameraLocalPosition;
    }

    private Vector3 GetCameraLocalEuler()
    {
        return sharedSettings != null ? sharedSettings.CameraLocalEuler : cameraLocalEuler;
    }

    private RuntimeAnimatorController GetAnimatorController()
    {
        return sharedSettings != null && sharedSettings.AnimatorController != null
            ? sharedSettings.AnimatorController
            : animatorController;
    }

    private GameObject[] GetObjectsToActivateWithPrimaryCamera()
    {
        return sharedSettings != null ? sharedSettings.ObjectsToActivateWithPrimaryCamera : objectsToActivateWithPrimaryCamera;
    }

    private CinemachineVirtualCamera GetSecondaryVirtualCamera()
    {
        return sharedSettings != null && sharedSettings.SecondaryVirtualCamera != null
            ? sharedSettings.SecondaryVirtualCamera
            : secondaryVirtualCamera;
    }

    private float GetSecondaryCameraDelay()
    {
        return sharedSettings != null ? sharedSettings.SecondaryCameraDelay : secondaryCameraDelay;
    }

    private bool GetDisablePrimaryWhenSecondaryActive()
    {
        return sharedSettings != null ? sharedSettings.DisablePrimaryWhenSecondaryActive : disablePrimaryWhenSecondaryActive;
    }

    private GameObject[] GetObjectsToActivateWithSecondaryCamera()
    {
        return sharedSettings != null ? sharedSettings.ObjectsToActivateWithSecondaryCamera : objectsToActivateWithSecondaryCamera;
    }

    private TypewriterPanel GetTypingPanel()
    {
        return sharedSettings != null && sharedSettings.TypingPanel != null
            ? sharedSettings.TypingPanel
            : typingPanel;
    }

    private float GetTypingPanelDelay()
    {
        return sharedSettings != null ? sharedSettings.TypingPanelDelay : 0.5f;
    }

    private int GetPrimaryCameraPriority()
    {
        return sharedSettings != null ? sharedSettings.PrimaryCameraPriority : primaryCameraPriority;
    }

    private int GetSecondaryCameraPriority()
    {
        return sharedSettings != null ? sharedSettings.SecondaryCameraPriority : secondaryCameraPriority;
    }

    private int GetPrimaryCameraPriorityWhenSecondaryActive()
    {
        return sharedSettings != null ? sharedSettings.PrimaryCameraPriorityWhenSecondaryActive : primaryCameraPriorityWhenSecondaryActive;
    }

    private GameObject[] GetObjectsToDeactivate()
    {
        return sharedSettings != null ? sharedSettings.ObjectsToDeactivate : objectsToDeactivate;
    }

    private GameObject[] GetSelectableObjectsToDeactivate()
    {
        return sharedSettings != null ? sharedSettings.SelectableObjectsToDeactivate : selectableObjectsToDeactivate;
    }

    private FinalRoomMissionTrigger GetMissionTrigger()
    {
        return sharedSettings != null && sharedSettings.MissionTrigger != null
            ? sharedSettings.MissionTrigger
            : missionTrigger;
    }

    private void StartCameraAttach(GameObject clone)
    {
        if (clone == null)
        {
            return;
        }

        FinalRoomMissionTrigger trigger = GetMissionTrigger();
        if (trigger != null)
        {
            trigger.StartCoroutine(AttachCameraAfterDelay(clone));
            return;
        }

        EnsureCoroutineHost();
        if (coroutineHost != null)
        {
            coroutineHost.StartCoroutine(AttachCameraAfterDelay(clone));
        }
    }

    private IEnumerator AttachCameraAfterDelay(GameObject clone)
    {
        CinemachineVirtualCamera cam = GetVirtualCamera();
        if (cam == null)
        {
            yield break;
        }

        float delay = Mathf.Max(0f, GetCameraAttachDelay());
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (clone == null)
        {
            yield break;
        }

        Transform head = FindTaggedChild(clone.transform, GetHeadTag());
        if (head == null)
        {
            Debug.LogWarning($"{name}: Head tag not found on clone.");
            yield break;
        }

        cam.transform.SetParent(head, false);
        cam.transform.localPosition = GetCameraLocalPosition();
        cam.transform.localRotation = Quaternion.Euler(GetCameraLocalEuler());
        cam.gameObject.SetActive(true);
        cam.Priority = GetPrimaryCameraPriority();

        ApplyAnimatorToClones();

        GameObject[] toActivatePrimary = GetObjectsToActivateWithPrimaryCamera();
        if (toActivatePrimary != null)
        {
            for (int i = 0; i < toActivatePrimary.Length; i++)
            {
                if (toActivatePrimary[i] != null)
                {
                    toActivatePrimary[i].SetActive(true);
                }
            }
        }

        CinemachineVirtualCamera secondary = GetSecondaryVirtualCamera();
        if (secondary != null)
        {
            float secondaryDelay = Mathf.Max(0f, GetSecondaryCameraDelay());
            if (secondaryDelay > 0f)
            {
                yield return new WaitForSeconds(secondaryDelay);
            }

            ambientBeforeTransition = RenderSettings.ambientIntensity;
            RenderSettings.ambientIntensity = 0f;

            secondary.gameObject.SetActive(true);
            secondary.Priority = GetSecondaryCameraPriority();
            if (GetDisablePrimaryWhenSecondaryActive())
            {
                cam.Priority = GetPrimaryCameraPriorityWhenSecondaryActive();
            }

            GameObject[] toActivate = GetObjectsToActivateWithSecondaryCamera();
            if (toActivate != null)
            {
                for (int i = 0; i < toActivate.Length; i++)
                {
                    if (toActivate[i] != null)
                    {
                        toActivate[i].SetActive(true);
                    }
                }
            }

            RenderSettings.ambientIntensity = ambientBeforeTransition;

            float typingDelay = Mathf.Max(0f, GetTypingPanelDelay());
            if (typingDelay > 0f)
            {
                yield return new WaitForSeconds(typingDelay);
            }

            TypewriterPanel panel = GetTypingPanel();
            if (panel != null)
            {
                panel.Show(BuildSelectionText());
            }
        }
    }

    private static void EnsureCoroutineHost()
    {
        if (coroutineHost != null)
        {
            return;
        }

        GameObject host = new GameObject("_FinalRoomCoroutineHost");
        Object.DontDestroyOnLoad(host);
        coroutineHost = host.AddComponent<CoroutineHost>();
    }

    private static Transform FindTaggedChild(Transform root, string tagToFind)
    {
        if (root == null || string.IsNullOrWhiteSpace(tagToFind))
        {
            return null;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].CompareTag(tagToFind))
            {
                return children[i];
            }
        }

        return null;
    }

    private string BuildSelectionText()
    {
        if (string.IsNullOrWhiteSpace(selectionTextSecondary))
        {
            return selectionTextPrimary;
        }

        string colorA = ColorUtility.ToHtmlStringRGBA(primaryTextColor);
        string colorB = ColorUtility.ToHtmlStringRGBA(secondaryTextColor);
        string separator = selectionTextSeparator ?? string.Empty;
        return $"<color=#{colorA}>{selectionTextPrimary}</color>{separator}<color=#{colorB}>{selectionTextSecondary}</color>";
    }

    private void ApplyAnimatorToClones()
    {
        RuntimeAnimatorController controller = GetAnimatorController();
        if (controller == null || spawnedClones.Count == 0)
        {
            return;
        }

        for (int i = 0; i < spawnedClones.Count; i++)
        {
            GameObject clone = spawnedClones[i];
            if (clone == null)
            {
                continue;
            }

            Animator[] animators = clone.GetComponentsInChildren<Animator>(true);
            for (int a = 0; a < animators.Length; a++)
            {
                if (animators[a] != null)
                {
                    animators[a].runtimeAnimatorController = controller;
                }
            }
        }
    }

    private static void StripOutlineMaterials(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                continue;
            }

            int writeIndex = 0;
            for (int j = 0; j < materials.Length; j++)
            {
                Material mat = materials[j];
                if (mat == null)
                {
                    continue;
                }

                Shader shader = mat.shader;
                if (shader != null && shader.name == "Custom/OutlineBackface")
                {
                    continue;
                }

                materials[writeIndex++] = mat;
            }

            if (writeIndex != materials.Length)
            {
                System.Array.Resize(ref materials, writeIndex);
                renderer.sharedMaterials = materials;
            }
        }
    }

    private static void RestartCloneAnimations(List<GameObject> clones)
    {
        if (clones == null || clones.Count == 0)
        {
            return;
        }

        for (int i = 0; i < clones.Count; i++)
        {
            GameObject clone = clones[i];
            if (clone == null)
            {
                continue;
            }

            Animator[] animators = clone.GetComponentsInChildren<Animator>(true);
            for (int a = 0; a < animators.Length; a++)
            {
                Animator animator = animators[a];
                if (animator == null)
                {
                    continue;
                }

                animator.Rebind();
                animator.Update(0f);
            }
        }
    }
}
