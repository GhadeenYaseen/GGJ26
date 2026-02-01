using UnityEngine;

public class ChairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform sitPoint;
    [SerializeField] private string prompt = "Press {key} to sit";
    [SerializeField] private NPC_Dialogue_Controller npcDialogue;
    [SerializeField] private bool triggerDialogueOnSit = true;

    public void Interact(PlayerCharacterController player)
    {
        if (player == null)
        {
            return;
        }

        Transform target = sitPoint != null ? sitPoint : transform;
        player.SitDown(target);

        if (triggerDialogueOnSit && npcDialogue != null)
        {
            npcDialogue.Interact(player);
        }
    }

    public string GetPrompt(PlayerCharacterController player)
    {
        return prompt;
    }
}
