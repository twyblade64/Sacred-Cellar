using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Doors use the signal system to be open or closed.
/// The opening is achieved through states in the animator and with the Open flag.
/// </summary>
public class DoorController : MonoBehaviour {
    // Activation signal
    public int inSignalId;
    public bool isOpen;

    // Animator
    private Animator animator;

    // Audio clips
    public AudioClip openClip;
    public AudioClip closeClip;

	/// <summary>
    /// Initiate instance
    /// </summary>
	void Start () {
        animator = GetComponent<Animator>();
        animator.SetBool("Open", isOpen);
        SoundManager.GetInstance().Load(openClip);
        SoundManager.GetInstance().Load(closeClip);
    }
	
	/// <summary>
    /// Manage door state.
    /// - Open if inSignal > 0.
    /// - Closed otherwise.
    /// </summary>
	void Update () {
        // Check if the game wasn't paused
        if (Time.timeScale > 0) {
            bool newState = CheckStatus();
            if (isOpen != newState) {
                isOpen = newState;
                animator.SetBool("Open", isOpen);
                if (isOpen == true) {
                    SoundManager.GetInstance().Play(openClip);
                } else {
                    SoundManager.GetInstance().Play(closeClip);
                }
            }
        }

	}

    /// <summary>
    /// Check if the inSignal is more than 0.
    /// </summary>
    /// <returns>If the inSignal is </returns>
    public bool CheckStatus () {
        return Tools.IntComparator(SignalManager.GetInstance().ReadSignal(inSignalId)) > 0;
    }
}
