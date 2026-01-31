using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FinalRoomDoorPlayerTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool forwardTriggerEvents = true;

    private void Awake()
    {
        if (!CompareTag(playerTag))
        {
            Debug.LogWarning($"{name}: Player tag is not '{playerTag}'. Door triggers may not fire.");
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null)
        {
            return;
        }

        if (!CompareTag(playerTag))
        {
            return;
        }

        FinalRoomSlidingDoor door = hit.collider.GetComponentInParent<FinalRoomSlidingDoor>();
        if (door != null)
        {
            door.NotifyPlayerEnter(transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!forwardTriggerEvents)
        {
            return;
        }

        if (!CompareTag(playerTag))
        {
            return;
        }

        FinalRoomSlidingDoor door = other.GetComponentInParent<FinalRoomSlidingDoor>();
        if (door != null)
        {
            door.NotifyPlayerEnter(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!forwardTriggerEvents)
        {
            return;
        }

        if (!CompareTag(playerTag))
        {
            return;
        }

        FinalRoomSlidingDoor door = other.GetComponentInParent<FinalRoomSlidingDoor>();
        if (door != null)
        {
            door.NotifyPlayerExit();
        }
    }
}
