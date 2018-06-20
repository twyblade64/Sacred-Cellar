using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that sends a one time signal when it's Collect method is called.
/// Also plays a sound when collected.
/// </summary>
public class KeyController : MonoBehaviour, ICollectible {
    public int outSignalId;
    public AudioClip grabClip;

	void Start () {
        SoundManager.GetInstance().Load(grabClip);
	}

    /// <summary>
    /// Send a 1 signal to the outSignalId and play a sound.
    /// Also destroys its GameObject.
    /// </summary>
    public void Collect() {
        SoundManager.GetInstance().Play(grabClip);
        SignalManager.GetInstance().WriteSignal(outSignalId, 1);
        Destroy(gameObject);
    }
}
