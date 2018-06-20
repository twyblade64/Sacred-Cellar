using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains different statistics of an enemy
/// </summary>
[CreateAssetMenu(fileName = "Enemy Stats", menuName = "Enemies/Stats")]
public class EnemyStatsScriptableObject : ScriptableObject {
    // Speed at which the enemy can move
    public float speed;

    // Ammount of damage an enemy can take
    public float hitpoints;

    // Ammount of damage an enemy can give
    public float attackDamageMultiplier;

    // Force applied when an enemy damages the player
    public float attackForceMultiplier;

    // Resistance of the enemy to stuns
    public float stunResistance;

    // Resistance of the enemy to pushes
    public float pushResistance;

    // Score value added to the player score when killed
    public int scoreValue;
}
