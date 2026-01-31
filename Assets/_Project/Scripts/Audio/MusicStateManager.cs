using System.Collections;
using UnityEngine;

public class MusicStateManager : MonoBehaviour
{
    public enum MusicState
    {
        Idle,
        Conversation,
        FinalMission,
        AfterSelect
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip conversationClip;
    [SerializeField] private AudioClip finalMissionClip;
    [SerializeField] private AudioClip[] afterSelectClips;
    [SerializeField] private bool loopLastAfterSelect = true;
    [SerializeField] private float idleVolume = 1f;
    [SerializeField] private float conversationVolume = 1f;
    [SerializeField] private float finalMissionVolume = 1f;
    [SerializeField] private float afterSelectVolume = 1f;
    [SerializeField] private float fadeDuration = 0.5f;

    private static MusicStateManager instance;
    private MusicState currentState = MusicState.Idle;
    private float fadeTimer;
    private float startVolume;
    private AudioClip targetClip;
    private bool isFading;
    private Coroutine sequenceRoutine;

    public static MusicStateManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        PlayState(MusicState.Idle, true);
    }

    private void Update()
    {
        if (!isFading)
        {
            return;
        }

        fadeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fadeTimer / Mathf.Max(0.01f, fadeDuration));
        audioSource.volume = Mathf.Lerp(startVolume, GetVolumeForState(currentState), t);

        if (t >= 1f)
        {
            isFading = false;
        }
    }

    public void SetIdle()
    {
        PlayState(MusicState.Idle);
    }

    public void SetConversation(bool active)
    {
        PlayState(active ? MusicState.Conversation : MusicState.Idle);
    }

    public void SetFinalMission()
    {
        PlayState(MusicState.FinalMission);
    }

    public void SetAfterSelect()
    {
        PlayState(MusicState.AfterSelect);
    }

    public void PlayState(MusicState state, bool instant = false)
    {
        if (state == currentState && (state != MusicState.AfterSelect || sequenceRoutine != null))
        {
            return;
        }

        StopSequence();
        currentState = state;

        if (state == MusicState.AfterSelect)
        {
            if (afterSelectClips == null || afterSelectClips.Length == 0)
            {
                return;
            }

            audioSource.loop = false;
            sequenceRoutine = StartCoroutine(PlayAfterSelectSequence());

            if (instant || fadeDuration <= 0f)
            {
                audioSource.volume = GetVolumeForState(state);
                isFading = false;
            }
            else
            {
                startVolume = 0f;
                audioSource.volume = startVolume;
                fadeTimer = 0f;
                isFading = true;
            }

            return;
        }

        targetClip = GetClipForState(state);
        if (targetClip == null)
        {
            return;
        }

        audioSource.loop = true;
        audioSource.clip = targetClip;
        audioSource.Play();

        if (instant || fadeDuration <= 0f)
        {
            audioSource.volume = GetVolumeForState(state);
            isFading = false;
            return;
        }

        startVolume = 0f;
        audioSource.volume = startVolume;
        fadeTimer = 0f;
        isFading = true;
    }

    private AudioClip GetClipForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.Conversation:
                return conversationClip;
            case MusicState.FinalMission:
                return finalMissionClip;
            default:
                return idleClip;
        }
    }

    private float GetVolumeForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.Conversation:
                return conversationVolume;
            case MusicState.FinalMission:
                return finalMissionVolume;
            case MusicState.AfterSelect:
                return afterSelectVolume;
            default:
                return idleVolume;
        }
    }

    private IEnumerator PlayAfterSelectSequence()
    {
        int lastIndex = GetLastValidAfterSelectIndex();
        if (lastIndex < 0)
        {
            yield break;
        }

        for (int i = 0; i <= lastIndex; i++)
        {
            AudioClip clip = afterSelectClips[i];
            if (clip == null)
            {
                continue;
            }

            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();

            float length = Mathf.Max(0.01f, clip.length);
            float elapsed = 0f;
            while (elapsed < length)
            {
                if (currentState != MusicState.AfterSelect)
                {
                    yield break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (loopLastAfterSelect && currentState == MusicState.AfterSelect)
        {
            AudioClip lastClip = afterSelectClips[lastIndex];
            if (lastClip != null)
            {
                audioSource.clip = lastClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    private int GetLastValidAfterSelectIndex()
    {
        if (afterSelectClips == null || afterSelectClips.Length == 0)
        {
            return -1;
        }

        for (int i = afterSelectClips.Length - 1; i >= 0; i--)
        {
            if (afterSelectClips[i] != null)
            {
                return i;
            }
        }

        return -1;
    }

    private void StopSequence()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }
    }
}
