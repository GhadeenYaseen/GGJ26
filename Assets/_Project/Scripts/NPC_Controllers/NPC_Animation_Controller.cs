using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Animation_Controller : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private string[] animationTriggers;
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
        if (animationTriggers == null || animationTriggers.Length == 0)
        {
            Debug.LogWarning($"{name}: No animation triggers configured on NPC_Animation_Controller.");
            return;
        }

        int index = Random.Range(0, animationTriggers.Length);
        string triggerName = animationTriggers[index];
        if (string.IsNullOrEmpty(triggerName))
        {
            Debug.LogWarning($"{name}: Animation trigger at index {index} is empty.");
            return;
        }

        animator.SetTrigger(triggerName);
    }

}
