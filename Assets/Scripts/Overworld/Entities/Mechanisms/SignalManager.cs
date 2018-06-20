using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class for signal managing. Must be on a GameObject first to initialize.
/// </summary>
public class SignalManager : MonoBehaviour {
    // Singleton instance
    public static SignalManager instance;

    // Number of signals available
    [SerializeField] private int signalNumber;

    // Signal buffer
    private int[] signals;

    public static SignalManager GetInstance() {
        return instance;
    }

    void Awake () {
        signals = new int[signalNumber];
        instance = this;
    }

    /// <summary>
    /// Store a value into a signal slot by id.
    /// </summary>
    /// <param name="id">The id of the signal.</param>
    /// <param name="value">The value to store.</param>
	public void WriteSignal(int id, int value) {
        signals[id] = value;
    }

    /// <summary>
    /// Read the value of a signal by id
    /// </summary>
    /// <param name="id">The id of the signal</param>
    /// <returns>The value of the signal</returns>
    public int ReadSignal(int id) {
        return signals[id];
    }

    void OnDestroy() {
        if (instance == this) instance = null;
    }
}
