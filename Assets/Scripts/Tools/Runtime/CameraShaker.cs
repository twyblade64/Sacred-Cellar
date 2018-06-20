using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small object shaking script
/// </summary>
public class CameraShaker : MonoBehaviour {
    // Static reference for easier use
    private static CameraShaker instance;

    // Current shake force
    private float shakeForce;

    // Force reduction over time
    public float shakeForceDampen;


    public void Awake() {
        instance = this;
    }

    public static CameraShaker GetInstance() {
        return instance;
    }

	/// <summary>
    /// Apply shake effect
    /// </summary>
	void Update () {
        if (Time.timeScale > 0) {
            if (shakeForce > 0) {
                // Reduce shake
                shakeForce *= shakeForceDampen;

                // Add a random vector to the origin position with 'shake' magnitude
                Vector3 pos = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward) * Vector3.up * shakeForce;
                transform.localPosition = pos;

                // Remove shake when small
                if (shakeForce <= 0.001f) {
                    shakeForce = 0;
                    transform.localPosition = Vector3.zero;
                }
            }
        }
	}

    /// <summary>
    /// Add shake effect to the camera
    /// </summary>
    /// <param name="force">The ammount of shake</param>
    public void AddForce(float force) {
        shakeForce += force;
    }
}
