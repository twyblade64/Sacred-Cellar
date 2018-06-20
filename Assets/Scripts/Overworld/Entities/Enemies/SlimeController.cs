using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This enemy attacks the player from close range and subdivides when killed.
/// </summary>
public class SlimeController : SmartEnemy, ITriggerReciever, IForceGenerator {
    private PlayerController player;
    private Animator animator;


    [Header("Custom Stats")]
    public GameObject subdivPrefab;
    public int subdivQnt;
    public float attackRange;

    // Enemy Structure
    [Header("Structure")]
    public GameObject bodyHitbox;
    public GameObject attackHitbox;
    
    // Enemy Audio Clips
    [Header("Sounds")]
    public AudioClip prepareClip;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deadClip;

    /// <summary>
    /// Call BaseEnemy.Awake() and initiate component references.
    /// </summary>
    public override void Awake() {
        base.Awake();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Initialize logic and load AudioClips.
    /// </summary>
    public override void Start() {
        LevelManager.GetInstance().AddDynamicGenerator(this);
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        SoundManager.GetInstance().Load(prepareClip);
        SoundManager.GetInstance().Load(attackClip);
        SoundManager.GetInstance().Load(hurtClip);
        SoundManager.GetInstance().Load(deadClip);
    }

    /// <summary>
    /// Call SmartEnemy.Update() and update Hurt animator flag.
    /// </summary>
    public override void Update() {
        base.Update();
        animator.SetBool("Hurt", isStunned);
    }

    /// <summary>
    /// Call the attack animations and play the attack clip.
    /// </summary>
    public override void Attack() {
        if (!isStunned && !animator.GetCurrentAnimatorStateInfo(0).IsName("Action 1")) {
            animator.SetInteger("Action", 1);
        }
    }

    // Not used
    public override bool CheckAttackAvailable() {
        throw new NotImplementedException();
    }

    // Not used
    public override bool CheckCanAttack() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if the player is within attack distance.
    /// </summary>
    /// <returns>True if enemy in range, otherwise false.</returns>
    public override bool CheckEnemyInAttackRange() {
        Vector3 dist = player.transform.position - transform.position;
        return Tools.MaxAxisDistance(dist) <= attackRange && Tools.MinAxisDistance(dist) < .5f + transform.localScale.x*.4f;
    }

    /// <summary>
    /// Get if the player is withing the sight range.
    /// </summary>
    /// <returns>Always true. Can be modified for more complex behaviour.</returns>
    public override bool CheckEnemyInSight() {
        return true;
    }

    /// <summary>
    /// Try to get closer to the player.
    /// </summary>
    public override void CloseIn() {
        if (!isStunned) {
            if (player.isAlive && player.isActive) {
                if (animator.GetInteger("Action") == 0) {
                    if (!CheckEnemyInAttackRange()) {
                        moveVelocity = Tools.Vector3ToDirVector(player.transform.position - transform.position) * stats.speed * Time.deltaTime;
                        animator.SetFloat("Move Speed", 1);
                    } else {
                        animator.SetFloat("Move Speed", 0);
                    }

                    Vector3 dir;
                    if (moveVelocity.magnitude > 0) {
                        dir = moveVelocity;
                    } else {
                        dir = player.transform.position - transform.position;
                    }
                    lookingDirection = Tools.Vector3ToDir4Int(dir);
                    transform.rotation = Quaternion.AngleAxis(-lookingDirection * 90, Vector3.up);
                }
            }
        }
    }

    /// <summary>
    /// The entity takes damage from an unknown source.
    /// </summary>
    /// <param name="damage">Ammount of damage to be taken.</param>
    /// <param name="direction">Incoming damage direction. Axis represented in clockwise order: 0-Forward, 1-Left, 2-Back, 3-Right.</param>
    /// <param name="pushForce">Force applied to the entity in the specified direction.</param>
    /// <param name="stunForce">Stun duration applied to the entity.</param>
    /// <returns>True if enemy was damaged, otherwise false.</returns>
    public override bool Damage(float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        hitpoints -= damage;
        stunTime = Mathf.Max(0, stunForce - stats.stunResistance);
        float pushForceSum = pushForce - stats.pushResistance;
        if (pushForceSum > 0) {
            launchVelocity = Quaternion.AngleAxis(-90 * direction, Vector3.up) * Vector3.forward * pushForceSum;
        }

        if (stunTime > 0) {
            isStunned = true;
            stunProgress = 0;
        }
        if (hitpoints < 0) {
            SoundManager.GetInstance().Play(deadClip);
            animator.SetBool("Death", true);
            alive = false;
            return true;
        } else {
            SoundManager.GetInstance().Play(hurtClip);
        }
        return false;
    }

    /// <summary>
    /// The entity takes damage from a known source.
    /// </summary>
    /// <param name="source">The source GameObject damaging the entity.</param>
    /// <param name="reciever">The entity collider involved in the collision.</param>
    /// <param name="damage">Ammount of damage to be taken.</param>
    /// <param name="direction">Incoming damage direction. Axis represented in clockwise order: 0-Forward, 1-Left, 2-Back, 3-Right.</param>
    /// <param name="pushForce">Force applied to the entity in the specified direction.</param>
    /// <param name="stunForce">Stun duration applied to the entity.</param>
    /// <returns>True if enemy was damaged, otherwise false.</returns>
    public override bool RecieveAttack(GameObject source, Collider reciever, float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        if (reciever.gameObject == bodyHitbox) {
            FloatingNumberController fnc = PoolManager.poolManager.GetPoolInstance(floatingNumberPrefab).GetComponent<FloatingNumberController>();
            fnc.number = -(int)damage;
            fnc.textColor = Color.red;
            fnc.transform.position = transform.position + Vector3.up * .5f + Vector3.right * UnityEngine.Random.Range(-.1f, .1f) + Vector3.forward * UnityEngine.Random.Range(-.1f, .1f);
            fnc.Init();

            Damage(damage, direction, pushForce, stunForce);
            return true;
        }
        return false;
    }

    // Not used
    public override void Defend() {
        throw new NotImplementedException();
    }

    // Not used
    public override void Dodge() {
        throw new NotImplementedException();
    }

    // Not used
    public override void MoveAway() {
        throw new NotImplementedException();
    }

    // Not used
    public override void PlanMoving() {
        throw new NotImplementedException();
    }

    // Not used
    public override void Wander() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Apply damage to player if self attackHitbox collides with it.
    /// </summary>
    /// <param name="origin">GameObject on which OnTriggerExit method was called.</param>
    /// <param name="other">Collider data of the OnTriggerExit method.</param>
    public void OnExtensionTriggerEnter(GameObject origin, Collider other) {
        if (origin == attackHitbox) {
            TriggerSender ts = other.GetComponent<TriggerSender>();
            if (ts != null) {
                GameObject triggerReciever = ts.GetReciever().GetRecieverGameObject();
                IDamagable damagable = triggerReciever.GetComponent<IDamagable>();
                if (damagable != null) {
                    if ((damagable.GetFaction() & oppositeFaction) != 0) {
                        damagable.RecieveAttack(origin, other, stats.attackDamageMultiplier, lookingDirection, stats.attackForceMultiplier);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public void OnExtensionTriggerStay(GameObject origin, Collider other) { }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public void OnExtensionTriggerExit(GameObject origin, Collider other) { }

    public GameObject GetRecieverGameObject() {
        return gameObject;
    }

    public override bool isAttacking() {
        return animator.GetInteger("Action") != 0;
        //throw new NotImplementedException();
    }

    // Not used
    public override bool isDefending() {
        throw new NotImplementedException();
    }

    // Not used
    public override bool isDodging() {
        throw new NotImplementedException();
    }

    // Not used
    public override bool isWandering() {
        throw new NotImplementedException();
    }

    // Not used
    public override bool isMovingAway() {
        throw new NotImplementedException();
    }

    // Not used
    public override bool isClosingIn() {
        throw new NotImplementedException();
    }

    // Not used
    public override bool isPlanningMove() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Affect the referenced potential field with a force.
    /// Adds a small weight to the current position to avoid enemies colliding into eachother paths.
    /// </summary>
    /// <param name="pf">The potential field to affect.</param>
    public new void GenerateForce(ref PotentialFieldScriptableObject pf) {
        pf.AddCellValue(pf.LocalPosX(transform.position), pf.LocalPosY(transform.position), 2);
    }

    /// <summary>
    /// Remove enemy from the potential field generation list.
    /// </summary>
    public new void OnDestroy() {
        LevelManager lm = LevelManager.GetInstance();
        if (lm != null && this != null)
            lm.RemoveDynamicGenerator(this);
    }

    /// <summary>
    /// Kill enemy instance and spawn other enemies if defined.
    /// </summary>
    public override void Kill() {
        if (subdivPrefab != null && subdivQnt > 0) {
            Vector3 spawnDir = Quaternion.AngleAxis(-90 * (lookingDirection + 2) - 45, Vector3.up) * Vector3.forward;
            Quaternion deltaRot = Quaternion.AngleAxis(180 / Mathf.Max(1, (subdivQnt - 1)), Vector3.up);

            for (int i = 0; i < subdivQnt; i++) {
                SlimeController sc = Instantiate(subdivPrefab, transform.position + spawnDir * 0.25f, Quaternion.identity).GetComponent<SlimeController>();
                sc.Damage(0, Tools.Vector3ToDir4Int(spawnDir), 4, 2);
                spawnDir = deltaRot * spawnDir;
            }
        }
        base.Kill();
    }

    /// <summary>
    /// Play the attack AudioClip.
    /// </summary>
    public void SFXPlayAttack() {
        SoundManager.GetInstance().Play(attackClip);
    }

    /// <summary>
    /// Play the wind-up AudioClip.
    /// </summary>
    public void SFXPlayPrepare() {
        SoundManager.GetInstance().Play(prepareClip);
    }
}
