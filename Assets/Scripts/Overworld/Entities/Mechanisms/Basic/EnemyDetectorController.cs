using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class detects if enemies are within its space, defined by the transform component, and sends a 1 on its output signal if true, otherwise a 0.
/// It also detects spawners that are actively spawning enemies.
/// </summary>
public class EnemyDetectorController : MonoBehaviour {
    public int outSignalId;
    private int layerMask;

    /// <summary>
    /// Initialize logic.
    /// </summary>
	void Start () {
        // Initialize the LayerMask to search in the Characters and Spawners layers.
        layerMask = LayerMask.GetMask(new string[] { "Characters", "Spawners" });
    }

    /// <summary>
    /// Look for enemies and send signal.
    /// </summary>
    void Update() {
        // Check if not paused.
        if (Time.timeScale > 0) {
            bool activated = true;
            Collider[] res = Physics.OverlapBox(transform.position, transform.localScale / 2, Quaternion.identity, layerMask);
            // Check for entitites with tags Enemy and Spawner
            for (int i = 0; i < res.Length; i++) {
                if (res[i].CompareTag("Enemy")) {
                    activated = false;
                    break;
                }
                if (res[i].CompareTag("Spawner")) {
                    // Check if the spawner is still spawnining enemies
                    if (!res[i].GetComponent<SpawnerController>().Completed) {
                        activated = false;
                        break;
                    }
                }
            }
            SignalManager.GetInstance().WriteSignal(outSignalId, activated ? 1 : 0);
        }
    }
}
