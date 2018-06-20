using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all enemy clases.
/// - Defines basic creation and death behaviour. Death creates floating numbers and spawns death particles.
/// - Has self faction and opposite faction in order to define allies and enemies.
/// - Has references to its stats and drop list scriptable objects.
/// - Contains method definition for use in the enemy AI system in conjuction with the AI Finite-State Machines.
/// </summary>
public abstract class BaseEnemy : MonoBehaviour, IDamagable {
    // Factions
    [SerializeField] protected FactionManager.Faction selfFaction;
    [EnumMask]
    [SerializeField] protected FactionManager.Faction oppositeFaction;

    // AI FSM
    protected Animator aiStateMachine;

    // Drop Lists
    [SerializeField] protected EnemyStatsScriptableObject stats;
    [SerializeField] protected DropListScriptableObject dropList;

    // Death effects
    public GameObject floatingNumberPrefab;
    public GameObject deathParticlesPrefab;

    // Stats
    public float hitpoints;
    protected bool alive;

    /// <summary>
    /// Basic creation for all enemies.
    /// Initializes hitponts and requires a child GameObject with an AIProxy component for the AI system to work.
    /// </summary>
    public virtual void Awake() {
        GameObject aiContainer = transform.Find("AI Container").gameObject;
        aiContainer.GetComponent<AIProxy>().owner = this;
        hitpoints = stats.hitpoints;
        alive = true;
    }

    // AI FSM methods
    public abstract void Attack();
    public abstract void Defend();
    public abstract void Dodge();
    public abstract void Wander();
    public abstract void MoveAway();
    public abstract void CloseIn();
    public abstract void PlanMoving();

    public abstract bool CheckEnemyInAttackRange();
    public abstract bool CheckEnemyInSight();
    public abstract bool CheckCanAttack();
    public abstract bool CheckAttackAvailable();

    public abstract bool isAttacking();
    public abstract bool isDefending();
    public abstract bool isDodging();
    public abstract bool isWandering();
    public abstract bool isMovingAway();
    public abstract bool isClosingIn();
    public abstract bool isPlanningMove();

    // IDamagable methods.
    public abstract bool Damage(float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0);
    public abstract bool RecieveAttack(GameObject source, Collider reciever, float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0);


    public FactionManager.Faction GetFaction() {
        return selfFaction;
    }

    /// <summary>
    /// Call the death of the enemy and give player rewards.
    /// </summary>
    public virtual void Kill() {
        // Adds points to the player score.
        PersistanceManager.GetInstance().AddPlayerScore(stats.scoreValue);

        // Generate items based on the drop list.
        float randomBonus = 1 + PersistanceManager.GetInstance().playerStatLuck*2f/5;
        while (randomBonus > 0) {
            if (randomBonus >= 1) {
                DropListScriptableObject.DropChoice dropChoice = dropList.GetRandomChoice();
                for (int i = 0; i < dropChoice.drops.Length; i++) {
                    GameObject obj = Instantiate(dropChoice.drops[i], transform.position, Quaternion.identity);
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb != null) {
                        rb.AddExplosionForce(100, transform.position + Random.onUnitSphere.normalized, 10);
                    }
                }
                randomBonus--;
            } else {
                if (Random.Range(0f,1f) <= randomBonus) {
                    DropListScriptableObject.DropChoice dropChoice = dropList.GetRandomChoice();
                    for (int i = 0; i < dropChoice.drops.Length; i++) {
                        GameObject obj = Instantiate(dropChoice.drops[i], transform.position, Quaternion.identity);
                        Rigidbody rb = obj.GetComponent<Rigidbody>();
                        if (rb != null) {
                            rb.AddForce(Random.onUnitSphere * Random.Range(0f, 1f));
                        }
                    }
                }
                randomBonus = 0;
            }
        }

        // Show death FX particles.
        if (deathParticlesPrefab) {
            GameObject deathParticles = PoolManager.poolManager.GetPoolInstance(deathParticlesPrefab);
            deathParticles.transform.position = transform.position;
            deathParticles.GetComponent<IPoolable>().Init();
        }

        Destroy(gameObject);
    }
}
