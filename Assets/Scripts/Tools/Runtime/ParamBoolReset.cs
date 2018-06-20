using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reset a bool param to an specified value on different animator state conditions
/// </summary>
public class ParamBoolReset : StateMachineBehaviour {
    public string param;
    public bool value;
    public bool onEnter;
    public bool onUpdate;
    public bool onExit;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (onEnter)
            animator.SetBool(param, value);
    }

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (onUpdate)
            animator.SetBool(param, value);
    }

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (onExit)
            animator.SetBool(param, value);
    }

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
        base.OnStateMachineEnter(animator, stateMachinePathHash);
        if (onEnter)
            animator.SetBool(param, value);
    }

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
        base.OnStateMachineExit(animator, stateMachinePathHash);
        if (onExit)
            animator.SetBool(param, value);
    }
}
