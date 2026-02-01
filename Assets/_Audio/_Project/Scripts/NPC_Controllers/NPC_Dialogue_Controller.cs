using UnityEngine;

public class NPC_Dialogue_Controller : MonoBehaviour, IInteractable
{
    [TextArea(3, 10)]
    [SerializeField] private string paragraph;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private string prompt = "Press {key} to talk";
    [SerializeField] private NPC_Animation_Controller npcAnimation;
    [SerializeField] private ConversationCounter conversationCounter;
    [SerializeField] private string conversationId;

    private void Awake()
    {
        if (npcAnimation == null)
        {
            npcAnimation = GetComponentInChildren<NPC_Animation_Controller>(true);
        }
        if (npcAnimation == null)
        {
            npcAnimation = GetComponentInParent<NPC_Animation_Controller>(true);
        }
        if (conversationCounter == null)
        {
            conversationCounter = FindObjectOfType<ConversationCounter>();
        }
    }

    public void Interact(PlayerCharacterController player)
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning($"{name}: DialogueUI is not assigned.");
            return;
        }

        if (conversationCounter != null)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                conversationCounter.RegisterConversation();
            }
            else
            {
                conversationCounter.RegisterConversation(conversationId);
            }
        }

        dialogueUI.StartDialogue(paragraph, npcAnimation);
    }

    public string GetPrompt(PlayerCharacterController player)
    {
        return prompt;
    }
}
