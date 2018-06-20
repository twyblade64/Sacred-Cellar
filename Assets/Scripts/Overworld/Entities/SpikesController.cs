using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spikes check for an instance  of a target faction and rise to damage it when stepped on.
/// </summary>
public class SpikesController : MonoBehaviour {
    enum SpikeState {hidden, anticipated, rised };
    
    [SerializeField] private FactionManager.Faction targetFaction;
    [SerializeField] private float damage;
    [SerializeField] private float pushForce;
    [SerializeField] private float stunForce;

    [SerializeField] private float anticipationTime;
    [SerializeField] public float anticipationProgress;
    [SerializeField] private float riseTime;
    [SerializeField] public float riseProgress;
    [SerializeField] private float ignoreTime;
    [SerializeField] public float ignoreProgress;

    [SerializeField] private AudioClip anticipationClip;
    [SerializeField] private AudioClip riseClip;
    [SerializeField] private AudioClip descendClip;

    private SpikeState state;
    private Animator animator;

    /// <summary>
    /// Initialize variables, state and sounds
    /// </summary>
    void Start () { 
        anticipationProgress = 0;
        riseProgress = 0;
        ignoreProgress = 0;
        state = SpikeState.hidden;
        animator = GetComponent<Animator>();

        SoundManager.GetInstance().Load(anticipationClip);
        SoundManager.GetInstance().Load(riseClip);
        SoundManager.GetInstance().Load(descendClip);

    }
	
	/// <summary>
    /// Update spike state times
    /// </summary>
	void Update () {
        if (Time.timeScale > 0) {
            // Update spike state
            switch (state) {
                case SpikeState.hidden:
                    break;
                case SpikeState.anticipated:
                    anticipationProgress += Time.deltaTime;
                    if (anticipationProgress >= anticipationTime) {
                        state = SpikeState.rised;
                        anticipationProgress = 0;
                        SoundManager.GetInstance().Play(riseClip);
                    }
                    break;
                case SpikeState.rised:
                    riseProgress += Time.deltaTime;
                    if (riseProgress >= riseTime) {
                        state = SpikeState.hidden;
                        riseProgress = 0;
                        SoundManager.GetInstance().Play(descendClip);
                    }
                    break;
            }

            // Update reactivation delay
            if (ignoreProgress != 0)
                ignoreProgress = Mathf.Max(ignoreProgress - Time.deltaTime, 0);

            animator.SetInteger("State", (int)state);
        }
	}

    /// <summary>
    /// Evaluate trigger and damage other instance.
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerStay(Collider col) {
        IDamagable other = null;
        TriggerSender ts = col.gameObject.GetComponent<TriggerSender>();

        // Get IDamagable component on child or parent.
        if (ts != null)
            other = ts.GetReciever().GetRecieverGameObject().GetComponent<IDamagable>();
        if (other == null)
            other = col.gameObject.GetComponent<IDamagable>();
        
        // Check not null and other faction.
        if (other != null && (other.GetFaction() & targetFaction) != 0) {
            // Check state
            switch (state) {
                case SpikeState.hidden: {
                    // Prepare spikes
                    state = SpikeState.anticipated;
                    anticipationProgress = 0;
                    SoundManager.GetInstance().Play(anticipationClip);
                } break;
                case SpikeState.rised: {
                    // Effect damage
                    if (ignoreProgress == 0) {
                        Vector3 proy = Vector3.ProjectOnPlane(col.gameObject.transform.position - transform.position, Vector3.up);
                        int dir;
                        if (Mathf.Abs(proy.z) > Mathf.Abs(proy.x))
                            dir = proy.z > 0 ? 0 : 2;
                        else
                            dir = proy.x > 0 ? 3 : 1;
                        other.Damage(damage, dir, pushForce, stunForce);
                        ignoreProgress = ignoreTime;
                    }
                } break;
            }
        }
    }
}
