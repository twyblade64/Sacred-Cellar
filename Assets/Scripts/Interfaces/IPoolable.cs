using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for basic pool instance design. 
/// </summary>
public interface IPoolable {
    /// <summary>
    /// Get if the instance is not being used.
    /// </summary>
    bool Alive { get; }

    /// <summary>
    /// Get if the instance belongs to a pool.
    /// </summary>
    bool InPool { get; set; }

    /// <summary>
    /// Get the GameObject asociated with the interface instance.
    /// </summary>
    GameObject PoolGameObject { get; }

    /// <summary>
    /// Initialize any logic for the use of the instance.
    /// </summary>
    void Init();

    /// <summary>
    /// Clear any logic for the reuse of the instance.
    /// </summary>
    void Clear();
}
