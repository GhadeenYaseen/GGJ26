using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Single Panel (Legacy)")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text dialogueText;
    [Header("Speaker Panels")]
    [SerializeField] private GameObject npcPanel;
    [SerializeField] private TMP_Text npcText;
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private Button nextButton;
    [SerializeField] private KeyCode nextKey = KeyCode.Space;
    [SerializeField] private TMP_Text nextInstructionText;
    [SerializeField] private string nextInstructionMessage = "Press {key} to continue";
    [SerializeField] private bool allowButtonClick = true;
    [SerializeField] private float characterDelay = 0.02f;
    [SerializeField] private bool hidePanelOnFinish = true;
    [SerializeField] private Speaker defaultSpeaker = Speaker.NPC;
    [SerializeField] private string[] npcPrefixes = new string[] { "NPC" };
    [SerializeField] private string[] playerPrefixes = new string[] { "Player", "Detective", "Ditective" };

    private readonly List<DialogueLine> sentences = new List<DialogueLine>();
    private int sentenceIndex;
    private Coroutine typingRoutine;
    private bool isTyping;
    private TMP_Text activeText;
    private GameObject activePanel;

    private enum Speaker
    {
        NPC,
        Player,
        Unknown
    }

    private struct DialogueLine
    {
        public Speaker Speaker;
        public string Text;
    }

    public bool IsOpen
    {
        get
        {
            if (npcPanel != null || playerPanel != null)
            {
                return (npcPanel != null && npcPanel.activeSelf) || (playerPanel != null && playerPanel.activeSelf);
            }

            return panel != null ? panel.activeSelf : gameObject.activeSelf;
        }
    }

    private void Awake()
    {
        if (nextButton != null)
        {
            if (allowButtonClick)
            {
                nextButton.onClick.AddListener(HandleNextClicked);
            }
            nextButton.interactable = allowButtonClick;
        }
        if (nextInstructionText == null && nextButton != null)
        {
            nextInstructionText = nextButton.GetComponentInChildren<TMP_Text>();
        }
        HideAllUI();
    }

    private void OnEnable()
    {
        HideAllUI();
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(nextKey))
        {
            HandleNextClicked();
        }
    }

    public void StartDialogue(string paragraph)
    {
        sentences.Clear();
        Speaker lastSpeaker = defaultSpeaker;
        foreach (string sentence in SplitIntoSentences(paragraph))
        {
            if (TryParseSpeaker(sentence, out Speaker speaker, out string cleaned))
            {
                lastSpeaker = speaker;
            }
            else
            {
                speaker = lastSpeaker;
                cleaned = sentence;
            }

            if (string.IsNullOrWhiteSpace(cleaned))
            {
                continue;
            }

            sentences.Add(new DialogueLine
            {
                Speaker = speaker,
                Text = cleaned
            });
        }

        if (sentences.Count == 0)
        {
            SetNextButtonActive(false);
            return;
        }

        sentenceIndex = 0;
        UpdateNextInstruction();
        SetNextButtonActive(true);
        SetNextInstructionActive(true);
        ShowSentence(sentenceIndex);
    }

    public void HandleNextClicked()
    {
        if (sentences.Count == 0)
        {
            return;
        }

        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        sentenceIndex++;
        if (sentenceIndex >= sentences.Count)
        {
            EndDialogue();
            return;
        }

        ShowSentence(sentenceIndex);
    }

    private void ShowSentence(int index)
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        DialogueLine line = sentences[index];
        if (!TrySelectSpeakerUI(line.Speaker))
        {
            Debug.LogWarning($"{name}: No dialogue text is assigned for speaker {line.Speaker}.");
            return;
        }

        typingRoutine = StartCoroutine(TypeSentence(line.Text));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        activeText.text = sentence;
        activeText.maxVisibleCharacters = 0;
        activeText.ForceMeshUpdate();

        int totalChars = activeText.textInfo.characterCount;
        float delay = Mathf.Max(0f, characterDelay);

        for (int i = 0; i <= totalChars; i++)
        {
            activeText.maxVisibleCharacters = i;
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
        }

        isTyping = false;
        typingRoutine = null;
    }

    private void CompleteTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (activeText == null)
        {
            return;
        }

        activeText.ForceMeshUpdate();
        activeText.maxVisibleCharacters = activeText.textInfo.characterCount;
        isTyping = false;
    }

    public void CloseDialogue()
    {
        EndDialogue(true);
    }

    private void EndDialogue(bool forceHide = false)
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        isTyping = false;
        sentences.Clear();
        sentenceIndex = 0;
        if (npcText != null)
        {
            npcText.text = string.Empty;
        }
        if (playerText != null)
        {
            playerText.text = string.Empty;
        }
        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        if (forceHide || hidePanelOnFinish)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
            if (npcPanel != null)
            {
                npcPanel.SetActive(false);
            }
            if (playerPanel != null)
            {
                playerPanel.SetActive(false);
            }
        }

        SetNextButtonActive(false);
        UpdateNextInstruction(false);
        SetNextInstructionActive(false);
    }

    private void SetNextButtonActive(bool isActive)
    {
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(isActive);
        }
    }

    private void SetNextInstructionActive(bool isActive)
    {
        if (nextInstructionText != null)
        {
            nextInstructionText.gameObject.SetActive(isActive);
        }
    }

    private void UpdateNextInstruction(bool show = true)
    {
        if (nextInstructionText == null)
        {
            return;
        }

        if (!show)
        {
            nextInstructionText.text = string.Empty;
            return;
        }

        string keyLabel = nextKey.ToString();
        nextInstructionText.text = string.IsNullOrWhiteSpace(nextInstructionMessage)
            ? keyLabel
            : nextInstructionMessage.Replace("{key}", keyLabel);
    }

    private void HideAllUI()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        if (npcPanel != null)
        {
            npcPanel.SetActive(false);
        }
        if (playerPanel != null)
        {
            playerPanel.SetActive(false);
        }
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
        }

        SetNextInstructionActive(false);
        UpdateNextInstruction(false);

        if (npcText != null)
        {
            npcText.text = string.Empty;
        }
        if (playerText != null)
        {
            playerText.text = string.Empty;
        }
        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }
    }

    private bool TrySelectSpeakerUI(Speaker speaker)
    {
        bool useSpeakerPanels = npcText != null || playerText != null || npcPanel != null || playerPanel != null;
        if (!useSpeakerPanels)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
            activePanel = panel;
            activeText = dialogueText;
            return activeText != null;
        }

        bool useNpc = speaker == Speaker.NPC || (speaker == Speaker.Unknown && defaultSpeaker == Speaker.NPC);
        TMP_Text preferredText = useNpc ? npcText : playerText;
        GameObject preferredPanel = useNpc ? npcPanel : playerPanel;
        TMP_Text fallbackText = useNpc ? playerText : npcText;
        GameObject fallbackPanel = useNpc ? playerPanel : npcPanel;

        if (preferredPanel != null)
        {
            preferredPanel.SetActive(true);
        }
        if (fallbackPanel != null)
        {
            fallbackPanel.SetActive(false);
        }

        activePanel = preferredPanel != null ? preferredPanel : fallbackPanel;
        activeText = preferredText != null ? preferredText : fallbackText;
        return activeText != null;
    }

    private bool TryParseSpeaker(string sentence, out Speaker speaker, out string cleaned)
    {
        cleaned = sentence.Trim();
        if (TryStripPrefix(cleaned, npcPrefixes, out string npcCleaned))
        {
            speaker = Speaker.NPC;
            cleaned = npcCleaned;
            return true;
        }
        if (TryStripPrefix(cleaned, playerPrefixes, out string playerCleaned))
        {
            speaker = Speaker.Player;
            cleaned = playerCleaned;
            return true;
        }

        speaker = Speaker.Unknown;
        return false;
    }

    private static bool TryStripPrefix(string sentence, string[] prefixes, out string cleaned)
    {
        cleaned = sentence;
        if (prefixes == null)
        {
            return false;
        }

        foreach (string rawPrefix in prefixes)
        {
            if (string.IsNullOrWhiteSpace(rawPrefix))
            {
                continue;
            }

            string prefix = rawPrefix.Trim();
            if (sentence.Length < prefix.Length)
            {
                continue;
            }

            if (!sentence.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int index = prefix.Length;
            while (index < sentence.Length && char.IsWhiteSpace(sentence[index]))
            {
                index++;
            }
            if (index < sentence.Length && (sentence[index] == ':' || sentence[index] == '-'))
            {
                index++;
                while (index < sentence.Length && char.IsWhiteSpace(sentence[index]))
                {
                    index++;
                }
            }

            cleaned = sentence.Substring(index).Trim();
            return true;
        }

        return false;
    }

    private static IEnumerable<string> SplitIntoSentences(string paragraph)
    {
        if (string.IsNullOrWhiteSpace(paragraph))
        {
            yield break;
        }

        string text = paragraph.Replace("\r", " ").Replace("\n", " ").Trim();
        int start = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c != '.' && c != '!' && c != '?')
            {
                continue;
            }

            int end = i + 1;
            if (c == '.')
            {
                while (end < text.Length && text[end] == '.')
                {
                    end++;
                }
            }

            while (end < text.Length && (text[end] == '"' || text[end] == '\'' || text[end] == ')'))
            {
                end++;
            }

            string sentence = text.Substring(start, end - start).Trim();
            if (!string.IsNullOrEmpty(sentence))
            {
                yield return sentence;
            }

            while (end < text.Length && char.IsWhiteSpace(text[end]))
            {
                end++;
            }

            start = end;
            i = end - 1;
        }

        if (start < text.Length)
        {
            string sentence = text.Substring(start).Trim();
            if (!string.IsNullOrEmpty(sentence))
            {
                yield return sentence;
            }
        }
    }
}
