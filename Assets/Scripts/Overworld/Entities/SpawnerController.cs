using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates GameObjects from a list in sequence with a small delay between them.
/// </summary>
public class SpawnerController : MonoBehaviour {
    public BoxCollider spawnAreaCheck;
    public float spawnDelay = 1;

    private List<GameObject> currentSpawnSequence;
    private int currentPrefab;
    private float currentDelay;

    public bool Completed {
        get {
            if (currentSpawnSequence == null)
                return true;
            return currentPrefab >= currentSpawnSequence.Count;
        }
    }

    /// <summary>
    /// Update delay and instantiate objects.
    /// </summary>
    public void Update () {
        if (Time.timeScale > 0) {
            if (!Completed) {
                // Check if delay has finished
                if (currentDelay == 0) {
                    // Check if spawn space is empty
                    if (Physics.OverlapBox(spawnAreaCheck.center, spawnAreaCheck.size / 2, Quaternion.identity, LayerMask.GetMask("Characters")).Length == 0) {
                        Instantiate(currentSpawnSequence[currentPrefab], transform.position, Quaternion.identity);
                        currentDelay = spawnDelay;
                    }
                } else { 
                    // Progress delay
                    currentDelay -= Time.deltaTime;
                    if (currentDelay <= 0) {
                        currentDelay = 0;
                        currentPrefab++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set a spawn sequence list and start spawning GameObjects.
    /// </summary>
    /// <param name="spawnSequence">A list containing the objects to instantiate.</param>
    public void SetSequence(List<GameObject> spawnSequence) {
        currentSpawnSequence = spawnSequence;
        currentPrefab = 0;
    }
}
