using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FinalRoomMissionPlayerTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool forwardTriggerEvents = true;
    [SerializeField] private bool useOverlapFallback = true;
    [SerializeField] private float overlapRadius = 0.6f;
    [SerializeField] private LayerMask missionMask = ~0;

    private void Awake()
    {
        if (!CompareTag(playerTag))
        {
            Debug.LogWarning($"{name}: Player tag is not '{playerTag}'. Final room mission may not start.");
        }
    }

    private void Update()
    {
        if (!useOverlapFallback || !CompareTag(playerTag))
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, overlapRadius, missionMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            FinalRoomMissionTrigger mission = hits[i].GetComponentInParent<FinalRoomMissionTrigger>();
            if (mission != null)
            {
                Debug.Log("FinalRoomMissionPlayerTrigger: Activating mission (overlap fallback).", this);
                mission.Activate();
                return;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider == null || !CompareTag(playerTag))
        {
            return;
        }

        Debug.Log($"FinalRoomMissionPlayerTrigger: Controller hit {hit.collider.name}", this);
        FinalRoomMissionTrigger mission = hit.collider.GetComponentInParent<FinalRoomMissionTrigger>();
        if (mission != null)
        {
            Debug.Log("FinalRoomMissionPlayerTrigger: Activating mission (controller hit).", this);
            mission.Activate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!forwardTriggerEvents || other == null || !CompareTag(playerTag))
        {
            return;
        }

        Debug.Log($"FinalRoomMissionPlayerTrigger: Trigger enter {other.name}", this);
        FinalRoomMissionTrigger mission = other.GetComponentInParent<FinalRoomMissionTrigger>();
        if (mission != null)
        {
            Debug.Log("FinalRoomMissionPlayerTrigger: Activating mission (trigger).", this);
            mission.Activate();
        }
    }
}
