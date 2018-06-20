using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constant moving object that can damage entities.
/// </summary>
public class ProyectileController : MonoBehaviour, IPoolable, IDamagable {
    // Faction variables
    [SerializeField] protected FactionManager.Faction selfFaction;
    [EnumMask]
    [SerializeField]
    protected FactionManager.Faction oppositeFaction;

    // Proyectile information
    public float speed;
    public int dir;
    private Vector3 moveDirection;
    public float attackDamage;
    public float attackForce;
    public float duration;
    private float durationProgress;
    public AudioClip destructionClip;

    private Rigidbody rb;

    // Pool variables
    private bool alive;
    private bool inPool;

    /// <summary>
    /// Initialize the object for pool reusability.
    /// </summary>
    /// <param name="selfFaction">The faction the proyectile belongs to</param>
    /// <param name="oppositeFaction">The faction the proyectile damages.</param>
    /// <param name="speed">The speed of the proyectile</param>
    /// <param name="dir">Direction of movement of the proyectile, using the 4 integer notation.</param>
    /// <param name="attackDamage">Ammount of damage caused by the proyectile.</param>
    /// <param name="attackForce">Ammount of push force caused by the proyectile.</param>
    public void Initialize(FactionManager.Faction selfFaction, FactionManager.Faction oppositeFaction, float speed, int dir, float attackDamage, float attackForce) {
        this.selfFaction = selfFaction;
        this.oppositeFaction = oppositeFaction;
        this.speed = speed;
        this.dir = dir;
        this.attackDamage = attackDamage;
        this.attackForce = attackForce;
        Init();
    }

    /// <summary>
    /// Load sounds
    /// </summary>
    public void Start() {
        SoundManager.GetInstance().Load(destructionClip, 10);
    }

    /// <summary>
    /// Setup self component references
    /// </summary>
    public void Awake() {
        rb = GetComponent<Rigidbody>();
    }

	/// <summary>
    /// Apply movement and destroy when time limit has been reached
    /// </summary>
	void Update () {
        // Check if the game is not paused
        if (Time.timeScale > 0) {
            if (alive) {
                rb.velocity = moveDirection * speed;
                durationProgress += Time.deltaTime;
                if (durationProgress > duration)
                    Clear();
            }
        }
	}

    /// <summary>
    /// Check collision and apply damage if other object is enemy
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerEnter(Collider other) {
        if (alive) {
            TriggerSender ts = other.GetComponent<TriggerSender>();
            if (ts != null) {
                GameObject triggerReciever = ts.GetReciever().GetRecieverGameObject();
                IDamagable damagable = triggerReciever.GetComponent<IDamagable>();
                if (damagable != null) {
                    if ((damagable.GetFaction() & oppositeFaction) != 0) {
                        if (damagable.RecieveAttack(gameObject, other, attackDamage, dir, attackForce)) {
                            SoundManager.GetInstance().Play(destructionClip);
                            Clear();
                        }
                    }
                }
            } else {
                if (other.CompareTag("Enviorment")) {
                    SoundManager.GetInstance().Play(destructionClip);
                    Clear();
                }
            }
        }
    }

    public bool Alive
    {
        get
        {
            return alive;
        }
    }

    public bool InPool
    {
        get
        {
            return inPool;
        }
        set
        {
            inPool = value;
        }
    }

    public GameObject PoolGameObject
    {
        get
        {
            return gameObject;
        }
    }

    /// <summary>
    /// Let the object become available for another use in the pool or destroy it if created by other means.
    /// </summary>
    public void Clear() {
        if (InPool) {
            alive = false;
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize movement variables
    /// </summary>
    public void Init() {
        this.moveDirection = Quaternion.AngleAxis(-dir * 90, Vector3.up) * Vector3.forward;
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        durationProgress = 0;

        gameObject.SetActive(true);
        alive = true;
    }

    public FactionManager.Faction GetFaction() {
        return selfFaction;
    }

    public bool Damage(float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        if (damage > 0) {
            SoundManager.GetInstance().Play(destructionClip);
            Clear();
            return true;
        }
        return false;
    }

    public bool RecieveAttack(GameObject source, Collider reciever, float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        return Damage(damage, direction, pushForce, stunForce);
    }
}
