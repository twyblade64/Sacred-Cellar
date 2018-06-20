using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Poolable particle system container
/// </summary>
public class IndependantParticlesController : MonoBehaviour, IPoolable {
    // Reference to all particle systems in the GameObject
    private ParticleSystem[] particles;

    // Creation sound
    public AudioClip burstClip;

    // Poolable atributes
    private bool alive;
    private bool inPool;

    public void Awake() {
        particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Start() {
        if (burstClip)
            SoundManager.GetInstance().Load(burstClip, 10);
        if (!inPool)
            Init();
    }

    public void Update() {
        if (!Alive && gameObject.activeInHierarchy) {
            Clear();
        }
    }

    public bool Alive {
        get {
            for (int i = 0; i < particles.Length; i++) {
                if (particles[i].IsAlive())
                    return true;
            }
            return false;
        }
    }

    public bool InPool {
        get {
            return inPool;
        }
        set {
            inPool = value;
        }
    }

    public GameObject PoolGameObject {
        get {
            return gameObject;
        }
    }

    public void Clear() {
        for(int i = 0; i < particles.Length; i++) {
            particles[i].Stop();
        }
        gameObject.SetActive(false);
    }

    public void Init() {
        if (burstClip)
            SoundManager.GetInstance().Play(burstClip, 10);
        gameObject.SetActive(true);
        for (int i = 0; i < particles.Length; i++) {
            particles[i].Play();
        }
    }
}
