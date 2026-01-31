using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ConversationCounter : MonoBehaviour
{
    [SerializeField] private int requiredConversations = 4;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private string inProgressMessage = "Conversations: {current}/{total}";
    [SerializeField] private string completedMessage = "All conversations finished. You can enter the final room.";
    [SerializeField] private UnityEvent onAllConversationsCompleted;

    private readonly HashSet<string> completedIds = new HashSet<string>();
    private int totalCount;
    private bool isCompleted;

    private void Start()
    {
        UpdateLabel();
    }

    public void RegisterConversation()
    {
        if (isCompleted)
        {
            return;
        }

        totalCount++;
        CheckCompletion();
    }

    public void RegisterConversation(string conversationId)
    {
        if (isCompleted)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(conversationId))
        {
            RegisterConversation();
            return;
        }

        if (completedIds.Add(conversationId))
        {
            totalCount++;
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (totalCount >= requiredConversations)
        {
            isCompleted = true;
            UpdateLabel();
            if (onAllConversationsCompleted != null)
            {
                onAllConversationsCompleted.Invoke();
            }
            return;
        }

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (statusLabel == null)
        {
            return;
        }

        if (isCompleted)
        {
            statusLabel.text = completedMessage;
            return;
        }

        string message = string.IsNullOrWhiteSpace(inProgressMessage)
            ? "{current}/{total}"
            : inProgressMessage;
        statusLabel.text = message
            .Replace("{current}", totalCount.ToString())
            .Replace("{total}", requiredConversations.ToString());
    }
}
