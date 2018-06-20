using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom StateMachineBehaviour for FSM AI, which assumes it can control the target through an AIProxy component.
/// 
/// This state stalls through the isWaiting animator flag until the specified waitTime has passed.
/// </summary>
public class StateWait : StateMachineBehaviour {
    private AIProxy proxy;
    [SerializeField] private float waitTime;
    private float waitTimeProgress;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!proxy)
            proxy = animator.gameObject.GetComponent<AIProxy>();

        waitTimeProgress = 0;
        animator.SetBool("isWaiting", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        waitTimeProgress += Time.deltaTime;
        if (waitTimeProgress >= waitTime)
            animator.SetBool("isWaiting", false);

    }
}
