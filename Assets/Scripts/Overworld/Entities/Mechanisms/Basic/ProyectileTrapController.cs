using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that fires proyectiles at a constant rate when its signal is activated.
/// Also can generate a signal each time it fires.
/// </summary>
public class ProyectileTrapController : MonoBehaviour {
    // Activation signal
    public bool activateEnabled;
    public int activateId;

    // Fire signal
    public bool generateEnabled;
    public int generateId;

    public bool isActivated;

    [Header("Fire settings")]
    public float fireTime;
    private float fireProgress;
    public AudioClip fireClip;

    [Header("Proyectile statistics")]
    public GameObject proyectilePrefab;
    public float proyectileSpeed;
    public float proyectileDamage;
    public float proyectileForce;

    // Faction of the proyectile
    public FactionManager.Faction selfFaction;
    public FactionManager.Faction oppositeFaction;

    /// <summary>
    /// Logic initialization
    /// </summary>
    void Start () {
        SoundManager.GetInstance().Load(fireClip);
        PoolManager.poolManager.CreatePool(proyectilePrefab, 10);
    }
	
	/// <summary>
    /// Update logic
    /// </summary>
	void Update () {
        // Check if not pause
        if (Time.timeScale > 0) {
            if (activateEnabled) {
                // Update activation signal
                isActivated = CheckStatus();
            }

            // Update fire timing if activated
            if (isActivated) {
                fireProgress -= Time.deltaTime;
                if (fireProgress <= 0) {
                    fireProgress += fireTime;
                    Fire();
                }
            }
        }
    }

    /// <summary>
    /// Fire proyectile
    /// </summary>
    public void Fire() {
        ProyectileController pc = PoolManager.poolManager.GetPoolInstance(proyectilePrefab).GetComponent<ProyectileController>();
        pc.transform.position = transform.position;
        pc.Initialize(selfFaction, oppositeFaction, proyectileSpeed, Tools.Vector3ToDir4Int(transform.forward), proyectileDamage, proyectileForce);
        SoundManager.GetInstance().Play(fireClip);

        // Generate fire signal
        if (generateEnabled) {
            SignalManager.GetInstance().WriteSignal(generateId, isActivated ? 1 : 0);
        }
    }

    public bool CheckStatus() {
        return Tools.IntComparator(SignalManager.GetInstance().ReadSignal(activateId)) > 0;
    }
}

