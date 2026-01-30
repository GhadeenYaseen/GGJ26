using UnityEngine;

public class ChairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform sitPoint;

    public void Interact(PlayerCharacterController player)
    {
        if (player == null)
        {
            return;
        }

        Transform target = sitPoint != null ? sitPoint : transform;
        player.SitDown(target);
    }
}
