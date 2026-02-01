using System.Collections;
using TMPro;
using UnityEngine;

public class FinalRoomSlidingDoor : MonoBehaviour
{
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 slideDirection = Vector3.right;
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float slideSpeed = 2f;
    [SerializeField] private bool closeOnExit = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool slideAwayFromPlayer = true;
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorClip;

    [Header("Conversation Gate")]
    [SerializeField] private ConversationCounter conversationCounter;
    [SerializeField] private TMP_Text lockedMessageText;
    [SerializeField] private string lockedMessage = "Finish all conversations before entering.";

    private Vector3 closedLocalPos;
    private Coroutine slideRoutine;

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

        closedLocalPos = doorTransform.localPosition;
        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
        }
        if (doorAudioSource == null)
        {
            doorAudioSource = gameObject.AddComponent<AudioSource>();
        }

        HideLockedMessage();
    }

    public void NotifyPlayerEnter(Transform player)
    {
        if (conversationCounter != null && !conversationCounter.IsCompleted)
        {
            ShowLockedMessage();
            return;
        }

        Vector3 openLocalPos = GetOpenPositionForPlayer(player);
        StartSlide(openLocalPos);
        HideLockedMessage();
        PlayDoorSound();
    }

    public void NotifyPlayerExit()
    {
        HideLockedMessage();
        if (closeOnExit)
        {
            StartSlide(closedLocalPos);
            PlayDoorSound();
        }
    }

    private void StartSlide(Vector3 target)
    {
        if (slideRoutine != null)
        {
            StopCoroutine(slideRoutine);
        }
        slideRoutine = StartCoroutine(SlideTo(target));
    }

    private IEnumerator SlideTo(Vector3 target)
    {
        while (doorTransform != null && (doorTransform.localPosition - target).sqrMagnitude > 0.0001f)
        {
            doorTransform.localPosition = Vector3.MoveTowards(
                doorTransform.localPosition,
                target,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (doorTransform != null)
        {
            doorTransform.localPosition = target;
        }

        slideRoutine = null;
    }

    private void ShowLockedMessage()
    {
        if (lockedMessageText == null)
        {
            return;
        }

        lockedMessageText.text = lockedMessage;
        lockedMessageText.enabled = true;
    }

    private void HideLockedMessage()
    {
        if (lockedMessageText == null)
        {
            return;
        }

        lockedMessageText.text = string.Empty;
        lockedMessageText.enabled = false;
    }

    private void PlayDoorSound()
    {
        if (doorAudioSource == null || doorClip == null)
        {
            return;
        }

        doorAudioSource.PlayOneShot(doorClip);
    }

    private Vector3 GetOpenPositionForPlayer(Transform player)
    {
        Vector3 dir = slideDirection.sqrMagnitude > 0.0001f ? slideDirection.normalized : Vector3.right;
        if (player == null || doorTransform == null)
        {
            return closedLocalPos + dir * slideDistance;
        }

        Vector3 localPlayer = doorTransform.InverseTransformPoint(player.position);
        float side = Mathf.Sign(localPlayer.x);
        if (Mathf.Approximately(side, 0f))
        {
            side = 1f;
        }

        if (slideAwayFromPlayer)
        {
            side = -side;
        }

        return closedLocalPos + dir * slideDistance * side;
    }
}
