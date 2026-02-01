using System.Collections;
using UnityEngine;

public class AutoDoorTrigger : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private float openAngleY = 90f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float closeDelaySeconds = 3f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool alternateOpenDirection = true;
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private float doorVolume = 1f;

    private Coroutine closeRoutine;
    private Quaternion closedRotation;
    private Coroutine rotateRoutine;
    private bool openPositive = true;
    private bool isOpen;

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

        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
        }
        if (doorAudioSource == null)
        {
            doorAudioSource = gameObject.AddComponent<AudioSource>();
        }
        doorAudioSource.playOnAwake = false;
        doorAudioSource.loop = false;
        doorAudioSource.volume = doorVolume;
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

        Quaternion targetOpen = GetAlternatingOpenRotation();
        StartRotate(targetOpen);
        isOpen = true;
        PlayOpenSound();

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
        isOpen = false;
        closeRoutine = null;
    }

    private void StartRotate(Quaternion targetRotation)
    {
        if (isOpen && Quaternion.Angle(targetRotation, closedRotation) > 0.1f)
        {
            return;
        }

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

    private Quaternion GetAlternatingOpenRotation()
    {
        if (isOpen)
        {
            return closedRotation * Quaternion.Euler(0f, openAngleY, 0f);
        }

        float direction = (alternateOpenDirection && !openPositive) ? -1f : 1f;
        openPositive = !openPositive;
        float angle = openAngleY * direction;
        return closedRotation * Quaternion.Euler(0f, angle, 0f);
    }

    private void PlayOpenSound()
    {
        if (doorAudioSource == null || doorOpenClip == null)
        {
            return;
        }

        doorAudioSource.volume = doorVolume;
        doorAudioSource.PlayOneShot(doorOpenClip);
    }
}
