using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom StateMachineBehaviour for FSM AI, which assumes it can control the target through an AIProxy component.
/// 
/// This state stalls until the AIProxy.BaseEnemy founds an enemy through its CheckEnemyInSight method.
/// </summary>
public class StateWander : StateMachineBehaviour {
    private AIProxy proxy;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!proxy)
            proxy = animator.gameObject.GetComponent<AIProxy>();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        animator.SetBool("isEnemyInSight", proxy.owner.CheckEnemyInSight());
    }
}
