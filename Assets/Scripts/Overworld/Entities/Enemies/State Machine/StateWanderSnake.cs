﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom StateMachineBehaviour for FSM AI, which assumes it can control the target through an AIProxy component.
/// 
/// This state makes the BaseEnemy just wander. Only used for the snake enemy.
/// </summary>
public class StateWanderSnake : StateMachineBehaviour {
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
        proxy.owner.Wander();
    }
}
