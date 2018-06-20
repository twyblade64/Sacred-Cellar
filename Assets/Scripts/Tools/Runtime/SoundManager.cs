using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Audio managing class.
/// - Allows music clips to be played through multiple scenes.
/// - Sound FX pooling
/// - (Not Implemented) Manage the global volume of sounds and music
/// </summary>
public class SoundManager : MonoBehaviour {
    // Default sound FX pool size
    private int defaultFxQnt = 3;

    // Sound FX pool dictionary
    private Dictionary<AudioClip, IndexedAudioSourceList> fxSourceDictionary;

    // The music clip to play
    public AudioClip musicClip;

    // The currently playing music AudioSource
    private AudioSource musicSource;

    // Singleton instance reference
    private static SoundManager soundManager;

    /// <summary>
    /// Initiate singleton logic and variables
    /// </summary>
    public void Awake() {
        if (soundManager == null) {
            soundManager = this;
            DontDestroyOnLoad(this.gameObject);

            if (musicClip != null)
                PlayMusic(musicClip);
            fxSourceDictionary = new Dictionary<AudioClip, IndexedAudioSourceList>();
        } else {
            // If a new AudioManager is found and it has a different musicClip attached, play the new musicClip.
            if (soundManager != this) {
                if (musicClip != soundManager.musicClip)
                    soundManager.PlayMusic(musicClip);
                Destroy(this.gameObject);
            }
        }
    }

    /// <summary>
    /// Access the singleton instance
    /// </summary>
    /// <returns>The SoundManager instance</returns>
    public static SoundManager GetInstance() {
        return soundManager;
    }

    /// <summary>
    /// Play a music clip
    /// </summary>
    /// <param name="clip"></param>
    public void PlayMusic(AudioClip clip) {
        if (musicSource != null) {
            musicSource.Stop();
            Destroy(musicSource);
        }
        musicClip = clip;
        musicSource = Tools.CreateAudioSource(this.gameObject, clip, 1, true);
        musicSource.Play();
    }

    /// <summary>
    /// Pauses the current playing music if one exists.
    /// </summary>
    public void PauseMusic() {
        if (musicSource != null) {
            musicSource.Pause();
        }
    }

    /// <summary>
    /// Unpauses the current playing music if one exists.
    /// </summary>
    public void UnPauseMusic() {
        if (musicSource != null) {
            musicSource.UnPause();
        }
    }

    /// <summary>
    /// Creates a pool of audio clips for multiple simultaneous playbacks
    /// </summary>
    /// <param name="clip"> The audio clip to play </param>
    /// <param name="fxQnt"> The maximum ammount of AudioSources played from this clip at any given time </param>
    public void Load(AudioClip clip, int fxQnt = 0) {
        if (fxQnt <= 0)
            fxQnt = defaultFxQnt;

        IndexedAudioSourceList audioList;
        if (!fxSourceDictionary.TryGetValue(clip, out audioList)) {
            audioList = new IndexedAudioSourceList(this.gameObject, clip, fxQnt);
            fxSourceDictionary.Add(clip, audioList);
        }
    }


    /// <summary>
    /// Plays an audio clip from an existing pool. 
    /// If no pool exists, one is created.
    /// Audio sources in the pool are played in sequence and if the current audio to play is already playing, the audio is reset.
    /// </summary>
    /// <param name="clip"> The audio </param>
    /// <param name="fxQnt"> The size of the pool if no pool exists</param>
    public void Play(AudioClip clip, int fxQnt = 0) {
        if (fxQnt <= 0)
            fxQnt = defaultFxQnt;

        IndexedAudioSourceList audioList;
        if (!fxSourceDictionary.TryGetValue(clip, out audioList)) {
            audioList = new IndexedAudioSourceList(gameObject, clip, fxQnt);
            fxSourceDictionary.Add(clip, audioList);
        }
        AudioSource source = audioList.Next();
        source.Play();
    }

    /// <summary>
    /// Plays an audio clip from an existing pool, only one sound can be playing at the same time.
    /// If no pool exists, one is created.
    /// Audio sources in the pool are played in sequence and if the current audio to play is already playing, the audio is reset.
    /// </summary>
    /// <param name="clip"> The audio </param>
    /// <param name="fxQnt"> The size of the pool if no pool exists</param>
    public void SoloPlay(AudioClip clip, int fxQnt = 1) {
        if (fxQnt <= 0)
            fxQnt = defaultFxQnt;

        IndexedAudioSourceList audioList;
        if (!fxSourceDictionary.TryGetValue(clip, out audioList)) {
            audioList = new IndexedAudioSourceList(gameObject, clip, fxQnt);
            fxSourceDictionary.Add(clip, audioList);
        }

        if(!audioList.Current().isPlaying)
            audioList.Current().Play();
    }

    /// <summary>
    /// Pool container class.
    /// </summary>
    class IndexedAudioSourceList {
        public List<AudioSource> audioSourceList;
        public int index;

        /// <summary>
        /// Create an AudioSource pool from an AudioClip.
        /// AudioSource components are attached to the specified GameObject.
        /// </summary>
        /// <param name="hostObject">The GameObject which AudioSources will be attached.</param>
        /// <param name="audioClip">The AudioClip from which the AudioSources will be played.</param>
        /// <param name="size">The size of the pool.</param>
        public IndexedAudioSourceList(GameObject hostObject, AudioClip audioClip, int size) {
            audioSourceList = new List<AudioSource>(size);
            for (int i = 0; i < size; ++i) {
                audioSourceList.Add(Tools.CreateAudioSource(hostObject, audioClip));
            }
        }

        /// <summary>
        /// Get the next available AudioSource.
        /// </summary>
        /// <returns>The next avialable AudioSource</returns>
        public AudioSource Next() {
            AudioSource audioSource = audioSourceList[index];
            index = (index + 1) % audioSourceList.Count;
            return audioSource;
        }

        /// <summary>
        /// Get the AudioSource at the current index.
        /// </summary>
        /// <returns>The AudioSource af the current index.</returns>
        public AudioSource Current() {
            return audioSourceList[index];
        }

        /// <summary>
        /// Destroy all created AudioSource components.
        /// </summary>
        public void Destroy() {
            foreach (AudioSource audioSource in audioSourceList)
                GameObject.Destroy(audioSource);
        }
    }

}