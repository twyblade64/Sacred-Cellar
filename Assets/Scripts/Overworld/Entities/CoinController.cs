using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickable coin.
/// </summary>
public class CoinController : MonoBehaviour, ICollectible {
    public int value;
    public AudioClip grabClip;

    void Start() {
        SoundManager.GetInstance().Load(grabClip);
    }

    /// <summary>
    /// Add coins to player statistics and destroy.
    /// </summary>
    public void Collect() {
        SoundManager.GetInstance().Play(grabClip);
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().AddCoins(value);
        Destroy(gameObject);
    }
}
