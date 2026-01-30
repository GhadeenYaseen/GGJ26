using UnityEngine;

public class NPC_Dialogue_Controller : MonoBehaviour, IInteractable
{
    [TextArea(3, 10)]
    [SerializeField] private string paragraph;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private string prompt = "Press {key} to talk";

    public void Interact(PlayerCharacterController player)
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning($"{name}: DialogueUI is not assigned.");
            return;
        }

        dialogueUI.StartDialogue(paragraph);
    }

    public string GetPrompt(PlayerCharacterController player)
    {
        return prompt;
    }
}
