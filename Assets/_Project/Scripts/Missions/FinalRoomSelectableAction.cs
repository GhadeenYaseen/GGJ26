using System.Collections;
using UnityEngine;

public class FinalRoomSelectableAction : MonoBehaviour
{
    [Header("Shared Settings")]
    [SerializeField] private FinalRoomActionSettings sharedSettings;

    [Header("Duplicate Character")]
    [SerializeField] private GameObject sourceCharacter;
    [SerializeField] private Transform duplicateSpawnPoint;
    [SerializeField] private Vector3 duplicateWorldPosition;
    [SerializeField] private float duplicateYawDegrees = 180f;
    [SerializeField] private bool useSpawnPoint = true;

    [Header("Mirror Move")]
    [SerializeField] private Transform mirrorTransform;
    [SerializeField] private float mirrorMoveDownDistance = 1f;
    [SerializeField] private float mirrorMoveDuration = 1f;
    [SerializeField] private bool mirrorUseLocalSpace = true;

    [Header("Camera Rotate")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraRotateDuration = 1.5f;
    [SerializeField] private float cameraRotateZDegrees = 180f;

    [Header("Light")]
    [SerializeField] private Light lightToEnable;

    [Header("Behavior")]
    [SerializeField] private bool playOnce = true;

    private bool hasPlayed;
    private Coroutine mirrorRoutine;
    private Coroutine cameraRoutine;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

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

        DuplicateCharacter();

        if (mirrorRoutine != null)
        {
            StopCoroutine(mirrorRoutine);
        }
        if (cameraRoutine != null)
        {
            StopCoroutine(cameraRoutine);
        }

        mirrorRoutine = StartCoroutine(PlaySequence());
    }

    private void DuplicateCharacter()
    {
        if (sourceCharacter == null)
        {
            Debug.LogWarning($"{name}: Source character not assigned.");
            return;
        }

        bool usePoint = GetUseSpawnPoint();
        Transform spawnPoint = GetDuplicateSpawnPoint();
        Vector3 spawnPos = usePoint && spawnPoint != null
            ? spawnPoint.position
            : GetDuplicateWorldPosition();
        Quaternion baseRot = usePoint && spawnPoint != null
            ? spawnPoint.rotation
            : sourceCharacter.transform.rotation;

        GameObject clone = Instantiate(sourceCharacter, spawnPos, baseRot);
        if (clone != null)
        {
            clone.transform.rotation = baseRot * Quaternion.Euler(0f, GetDuplicateYawDegrees(), 0f);
            OutlineHighlight[] highlights = clone.GetComponentsInChildren<OutlineHighlight>(true);
            for (int i = 0; i < highlights.Length; i++)
            {
                if (highlights[i] != null)
                {
                    highlights[i].SetHighlighted(false);
                    highlights[i].enabled = false;
                }
            }
            StripOutlineMaterials(clone);
        }
    }

    private IEnumerator PlaySequence()
    {
        Transform mirror = GetMirrorTransform();
        if (mirror != null)
        {
            yield return StartCoroutine(MoveMirrorDown());
        }

        Transform cam = GetCameraTransform();
        if (cam != null)
        {
            cameraRoutine = StartCoroutine(RotateCameraZ());
            yield return cameraRoutine;
        }

        EnableLight();
    }

    private IEnumerator MoveMirrorDown()
    {
        Transform mirror = GetMirrorTransform();
        bool useLocal = GetMirrorUseLocalSpace();
        Vector3 start = useLocal ? mirror.localPosition : mirror.position;
        Vector3 end = start + Vector3.down * GetMirrorMoveDownDistance();
        float duration = Mathf.Max(0.01f, GetMirrorMoveDuration());
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            if (useLocal)
            {
                mirror.localPosition = pos;
            }
            else
            {
                mirror.position = pos;
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (useLocal)
        {
            mirror.localPosition = end;
        }
        else
        {
            mirror.position = end;
        }
    }

    private IEnumerator RotateCameraZ()
    {
        Transform cam = GetCameraTransform();
        Quaternion start = cam.rotation;
        Quaternion end = start * Quaternion.Euler(0f, 0f, GetCameraRotateZDegrees());
        float duration = Mathf.Max(0.01f, GetCameraRotateDuration());
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            cam.rotation = Quaternion.Slerp(start, end, t);
            time += Time.deltaTime;
            yield return null;
        }

        cam.rotation = end;
    }

    private Transform GetDuplicateSpawnPoint()
    {
        return sharedSettings != null && sharedSettings.DuplicateSpawnPoint != null
            ? sharedSettings.DuplicateSpawnPoint
            : duplicateSpawnPoint;
    }

    private Vector3 GetDuplicateWorldPosition()
    {
        return sharedSettings != null ? sharedSettings.DuplicateWorldPosition : duplicateWorldPosition;
    }

    private float GetDuplicateYawDegrees()
    {
        return sharedSettings != null ? sharedSettings.DuplicateYawDegrees : duplicateYawDegrees;
    }

    private bool GetUseSpawnPoint()
    {
        return sharedSettings != null ? sharedSettings.UseSpawnPoint : useSpawnPoint;
    }

    private Transform GetMirrorTransform()
    {
        return sharedSettings != null && sharedSettings.MirrorTransform != null
            ? sharedSettings.MirrorTransform
            : mirrorTransform;
    }

    private float GetMirrorMoveDownDistance()
    {
        return sharedSettings != null ? sharedSettings.MirrorMoveDownDistance : mirrorMoveDownDistance;
    }

    private float GetMirrorMoveDuration()
    {
        return sharedSettings != null ? sharedSettings.MirrorMoveDuration : mirrorMoveDuration;
    }

    private bool GetMirrorUseLocalSpace()
    {
        return sharedSettings != null ? sharedSettings.MirrorUseLocalSpace : mirrorUseLocalSpace;
    }

    private Transform GetCameraTransform()
    {
        return sharedSettings != null && sharedSettings.CameraTransform != null
            ? sharedSettings.CameraTransform
            : cameraTransform;
    }

    private float GetCameraRotateDuration()
    {
        return sharedSettings != null ? sharedSettings.CameraRotateDuration : cameraRotateDuration;
    }

    private float GetCameraRotateZDegrees()
    {
        return sharedSettings != null ? sharedSettings.CameraRotateZDegrees : cameraRotateZDegrees;
    }

    private void EnableLight()
    {
        if (lightToEnable != null)
        {
            lightToEnable.enabled = true;
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
}
