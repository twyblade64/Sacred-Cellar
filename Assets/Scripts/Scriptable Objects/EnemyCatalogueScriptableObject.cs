using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object containing a list of different enemies and information of their spawn conditions.
/// </summary>
[CreateAssetMenu(fileName = "Enemy Catalogue", menuName = "Enemy Catalogue")]
[System.Serializable]
public class EnemyCatalogueScriptableObject : ScriptableObject {
    [System.Serializable]
    public struct EnemyEntry {
        // Minimum level tier for the enemy to spawn
        public int minTier;

        // The prefab of the enemy
        public GameObject enemyPrefab;

        // The difficulty value of an enemy, used when trying to spawn it
        public int score;
    }
    public EnemyEntry[] catalogue;
}
