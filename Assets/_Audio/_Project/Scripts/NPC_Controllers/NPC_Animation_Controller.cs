using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Animation_Controller : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private string[] animationTriggers;
    [SerializeField] private bool useStateNames = false;
    [SerializeField] private string[] animationStates;
    [SerializeField] private float crossFadeDuration = 0.05f;
    [SerializeField] private int animationLayer = 0;
    [SerializeField] private bool resetTriggersBeforePlay = true;
    [SerializeField] private bool avoidRepeatingLast = true;
    private int lastTriggerIndex = -1;
    private int lastStateIndex = -1;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    [ContextMenu("Do Random Animation")]
    public void TriggerRandomAnimation()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (useStateNames && animationStates != null && animationStates.Length > 0)
        {
            int stateIndex = GetRandomIndex(animationStates.Length, lastStateIndex, avoidRepeatingLast);
            string stateName = animationStates[stateIndex];
            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogWarning($"{name}: Animation state at index {stateIndex} is empty.");
                return;
            }

            if (crossFadeDuration <= 0f)
            {
                animator.Play(stateName, animationLayer, 0f);
            }
            else
            {
                animator.CrossFadeInFixedTime(stateName, crossFadeDuration, animationLayer, 0f);
            }
            lastStateIndex = stateIndex;
            return;
        }

        if (animationTriggers == null || animationTriggers.Length == 0)
        {
            Debug.LogWarning($"{name}: No animation triggers configured on NPC_Animation_Controller.");
            return;
        }

        int index = GetRandomIndex(animationTriggers.Length, lastTriggerIndex, avoidRepeatingLast);
        string triggerName = animationTriggers[index];
        if (string.IsNullOrEmpty(triggerName))
        {
            Debug.LogWarning($"{name}: Animation trigger at index {index} is empty.");
            return;
        }

        if (resetTriggersBeforePlay)
        {
            for (int i = 0; i < animationTriggers.Length; i++)
            {
                string trigger = animationTriggers[i];
                if (!string.IsNullOrEmpty(trigger))
                {
                    animator.ResetTrigger(trigger);
                }
            }
        }

        animator.SetTrigger(triggerName);
        lastTriggerIndex = index;
    }

    private static int GetRandomIndex(int length, int lastIndex, bool avoidRepeat)
    {
        if (length <= 1 || !avoidRepeat)
        {
            return Random.Range(0, length);
        }

        int index = Random.Range(0, length);
        if (index == lastIndex)
        {
            index = (index + 1) % length;
        }

        return index;
    }

}
