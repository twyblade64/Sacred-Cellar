using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for accessing to the SoundManager singleton instace
/// Useful for the Unity UI event system
/// </summary>
public class SoundManagerProxy : MonoBehaviour {
    // Audioclips to load on Start
    public AudioClip[] clips;

    /// <summary>
    /// Preload audioClips
    /// </summary>
    void Start () {
        foreach (AudioClip clip in clips)
            SoundManager.GetInstance().Load(clip);
	}
	
    public void PlayClip(int id) {
        if (id < clips.Length)
            SoundManager.GetInstance().Play(clips[id]);
    }
}
