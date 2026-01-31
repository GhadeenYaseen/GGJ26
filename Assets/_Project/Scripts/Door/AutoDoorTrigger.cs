using System.Collections;
using UnityEngine;

public class AutoDoorTrigger : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private float openAngleY = 90f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float closeDelaySeconds = 3f;
    [SerializeField] private string playerTag = "Player";

    private Coroutine closeRoutine;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine rotateRoutine;

    private void Reset()
    {
        if (doorTransform == null)
        {
            doorTransform = transform;
        }
    }

    private void Awake()
    {
        if (doorTransform == null)
        {
            doorTransform = transform;
        }

        closedRotation = doorTransform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngleY, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (doorTransform == null)
        {
            Debug.LogWarning($"{name}: Door Transform not assigned.");
            return;
        }

        StartRotate(openRotation);

        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
        }
        closeRoutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, closeDelaySeconds));
        StartRotate(closedRotation);
        closeRoutine = null;
    }

    private void StartRotate(Quaternion targetRotation)
    {
        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
        }
        rotateRoutine = StartCoroutine(RotateTo(targetRotation));
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (doorTransform != null && Quaternion.Angle(doorTransform.localRotation, targetRotation) > 0.1f)
        {
            doorTransform.localRotation = Quaternion.RotateTowards(
                doorTransform.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (doorTransform != null)
        {
            doorTransform.localRotation = targetRotation;
        }

        rotateRoutine = null;
    }
}
