using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistance data carried between game sessions
/// </summary>
[CreateAssetMenu(fileName = "Game Data", menuName = "Game Data")]
[System.Serializable]
public class GameDataScriptableObject : ScriptableObject {
    // Ammount of deaths of the player
    public int deathCount;

    // Max wave reached by the player
    public int maxWave;

    // Max score obtained by the player
    public int maxScore;
}

