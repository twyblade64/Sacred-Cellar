using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for accessing to the SceneManager singleton instance.
/// Useful to use with the Unity UI events
/// </summary>
public class SceneManagerProxy : MonoBehaviour {
    public void ChangeScene(string scene) {
        SceneManager.GetInstance().ChangeScene(scene, true);
    }
}
