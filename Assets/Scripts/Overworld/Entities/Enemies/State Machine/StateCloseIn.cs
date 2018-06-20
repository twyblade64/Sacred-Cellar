using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom StateMachineBehaviour for FSM AI, which assumes it can control the target through an AIProxy component.
/// 
/// This state tries to call an close-in beahviour on the BaseEnemy instance.
/// </summary>
public class StateCloseIn : StateMachineBehaviour {
    private AIProxy proxy;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!proxy)
            proxy = animator.gameObject.GetComponent<AIProxy>();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        animator.SetBool("isEnemyInRange", false);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        animator.SetBool("isEnemyInRange", proxy.owner.CheckEnemyInAttackRange());
        proxy.owner.CloseIn();
    }
}
