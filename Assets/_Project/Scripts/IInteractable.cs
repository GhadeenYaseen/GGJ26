using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerCharacterController player);
    string GetPrompt(PlayerCharacterController player);
}
