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

    [Header("Conversation Gate")]
    [SerializeField] private ConversationCounter conversationCounter;
    [SerializeField] private TMP_Text lockedMessageText;
    [SerializeField] private string lockedMessage = "Finish all conversations before entering.";

    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
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
        Vector3 dir = slideDirection.sqrMagnitude > 0.0001f ? slideDirection.normalized : Vector3.right;
        openLocalPos = closedLocalPos + dir * slideDistance;

        HideLockedMessage();
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerEnter(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        HandlePlayerExit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        HandlePlayerExit(other);
    }

    private void HandlePlayerEnter(Collider other)
    {
        if (other == null || !other.CompareTag(playerTag))
        {
            return;
        }

        if (conversationCounter != null && !conversationCounter.IsCompleted)
        {
            ShowLockedMessage();
            return;
        }

        StartSlide(openLocalPos);
        HideLockedMessage();
    }

    private void HandlePlayerExit(Collider other)
    {
        if (other == null || !other.CompareTag(playerTag))
        {
            return;
        }

        HideLockedMessage();
        if (closeOnExit)
        {
            StartSlide(closedLocalPos);
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
}
