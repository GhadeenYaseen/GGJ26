using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float characterDelay = 0.03f;
    [SerializeField] private bool hideWhenEmpty = true;

    private Coroutine typingRoutine;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (hideWhenEmpty)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Show(string message)
    {
        if (messageText == null)
        {
            return;
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        panelRoot.SetActive(true);
        typingRoutine = StartCoroutine(TypeMessage(message));
    }

    public void Hide()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        if (hideWhenEmpty && panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private IEnumerator TypeMessage(string message)
    {
        messageText.text = message;
        messageText.maxVisibleCharacters = 0;
        messageText.ForceMeshUpdate();

        int totalChars = messageText.textInfo.characterCount;
        float delay = Mathf.Max(0f, characterDelay);

        for (int i = 0; i <= totalChars; i++)
        {
            messageText.maxVisibleCharacters = i;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
        }

        typingRoutine = null;
    }
}
