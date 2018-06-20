using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reset 'Action' param on animator states om exit.
/// </summary>
public class StateActionReset : StateMachineBehaviour {

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        animator.SetInteger("Action", 0);
    }

    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
        base.OnStateMachineExit(animator, stateMachinePathHash);
        animator.SetInteger("Action", 0);
    }
}
