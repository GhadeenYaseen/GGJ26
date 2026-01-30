using UnityEngine;

public class ChairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform sitPoint;
    [SerializeField] private string prompt = "Press {key} to sit";

    public void Interact(PlayerCharacterController player)
    {
        if (player == null)
        {
            return;
        }

        Transform target = sitPoint != null ? sitPoint : transform;
        player.SitDown(target);
    }

    public string GetPrompt(PlayerCharacterController player)
    {
        return prompt;
    }
}
