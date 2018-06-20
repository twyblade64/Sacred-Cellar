using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tools {
    /// <summary>
    /// Creates an usable AudioSource component from an AudioClip and attaches it to the specified GameObject.
    /// </summary>
    /// <param name="gameObject">The specified GameObject to attach the AudioSource to.</param>
    /// <param name="audioClip">The AudioClip used to create the AudioSource.</param>
    /// <param name="volume">The volume of the generated AudioSource.</param>
    /// <param name="loop">Enable loop on the generated AudioSource.</param>
    /// <param name="playOnAwake">Enable PlayOnAwake on the AudioSource.</param>
    /// <returns>The generated AudioSource.</returns>
    public static AudioSource CreateAudioSource(GameObject gameObject, AudioClip audioClip, float volume = 1, bool loop = false, bool playOnAwake = false) {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = playOnAwake;
        return audioSource;
    }

    /// <summary>
    /// Snaps a Vector3 on x and z axis to a 1x1 grid.
    /// The snap function chooses the closes integer value to round to.
    /// </summary>
    /// <param name="v">The vector to snap.</param>
    /// <returns>The vector placed in the 1x1 grid.</returns>
    public static Vector3 VectorGridSnap(Vector3 v) {
        return new Vector3(Mathf.Floor(v.x + 0.5f), v.y, Mathf.Floor(v.z + 0.5f));
    }

    /// <summary>
    /// Obtain the integer direction from the biggest axis of a vector in the Y plane.
    /// The direction goes as follows.
    /// 0 - Forward
    /// 1 - Left
    /// 2 - Backwards
    /// 3 - Right
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static int Vector3ToDir4Int(Vector3 v) {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.z))
            return v.x > 0 ? 3 : 1;
        else
            return v.z > 0 ? 0 : 2;
    }

    /// <summary>
    /// Get the normalized vector facing the biggest axis of a vector in the Y planne.
    /// </summary>
    /// <param name="v">The source vector</param>
    /// <returns>The direction vector</returns>
    public static Vector3 Vector3ToDirVector(Vector3 v) {
        switch (Vector3ToDir4Int(v)) {
            case 0:
                return Vector3.forward;
            case 1:
                return Vector3.left;
            case 2:
                return Vector3.back;
            case 3:
                return Vector3.right;
        }
        return Vector3.forward;
    }

    /// <summary>
    /// Execute a runtime customizable comparison between two integers and return and integer.
    /// 
    /// The type of operation to perform can be changed through the Operation mode integer as follows:
    /// 0 - Equality
    /// 1 - Different
    /// 2 - Smaller than
    /// 3 - Smaller or equal than
    /// 4 - Bigger than
    /// 5 - Bigger or equal than
    /// 
    /// The return mode defines what value to return and goes as follows:
    /// 0 - Return 1 if the operation is true, otherwise 0.
    /// 1 - Return FirstInteger if operation is true, otherwise 0.
    /// 2 - Return FirstInteger if operation is true, otherwise SecondInteger.
    /// </summary>
    /// <param name="a">First integer</param>
    /// <param name="b">Second integer</param>
    /// <param name="op">Operation mode, goes from [0,5]</param>
    /// <param name="retMode">Return mode, goes from [0, 2]</param>
    /// <returns>The return value of the operation in the return mode chosen.</returns>
    public static int IntComparator(int a, int b = 0, int op = 4, int retMode = 0) {
        bool res = false;
        switch (op) {
            case 0: // ==
                res = a == b;
                break;
            case 1: // !=
                res = a != b;
                break;
            case 2: // <
                res = a < b;
                break;
            case 3: // <=
                res = a <= b;
                break;
            case 4: // >
                res = a > b;
                break;
            case 5: // >=
                res = a >= b;
                break;
        }
        switch (retMode) {
            case 0:
                return res ? 1 : 0;
            case 1:
                return res ? a : 0;
            case 2:
                return res ? a : b;
        }
        return 0;
    }

    /// <summary>
    /// Calculate the manhatan distance of a vector in the Y plane .
    /// </summary>
    /// <param name="v">Source vector</param>
    /// <returns>(|v.x| + |v.z|)</returns>
    public static float ManhatanDistance(Vector3 v) {
        return Mathf.Abs(v.x) + Mathf.Abs(v.z);
    }

    /// <summary>
    /// Get the length of the biggest axis of a vector in the Y plane.
    /// </summary>
    /// <param name="v">Source vector</param>
    /// <returns>Biggest axis length</returns>
    public static float MaxAxisDistance(Vector3 v) {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.z))
            return Mathf.Abs(v.x);
        return Mathf.Abs(v.z);
    }

    /// <summary>
    /// Get the length of the smallest axis of a vector in the Y plane.
    /// </summary>
    /// <param name="v">Source vector</param>
    /// <returns>Biggest axis length</returns>
    public static float MinAxisDistance(Vector3 v) {
        if (Mathf.Abs(v.x) <= Mathf.Abs(v.z))
            return Mathf.Abs(v.x);
        return Mathf.Abs(v.z);
    }
}
