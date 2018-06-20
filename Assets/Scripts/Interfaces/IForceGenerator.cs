using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface definition for entitites that generate a potential field force.
/// Used for the enemy pathfinding system.
/// </summary>
public interface IForceGenerator  {
    /// <summary>
    /// Affect a potential field with a defined force.
    /// </summary>
    /// <param name="pf">The potential field to affect.</param>
    void GenerateForce(ref PotentialFieldScriptableObject pf);
}
