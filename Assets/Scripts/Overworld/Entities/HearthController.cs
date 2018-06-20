using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickable hearth.
/// </summary>
public class HearthController : MonoBehaviour, ICollectible {
    public int value;
    public AudioClip grabClip;

    void Start() {
        SoundManager.GetInstance().Load(grabClip);
    }

    /// <summary>
    /// Add health to player statistics and destroy.
    /// </summary>
    public void Collect() {
        SoundManager.GetInstance().Play(grabClip);
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().AddHealth(value);
        Destroy(gameObject);
    }
}
