using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This enemy attacks the player from a middle distance with its lance.
/// </summary>
public class SwordmanController : SmartEnemy, ITriggerReciever {
    private PlayerController player;
    private Animator animator;

    // Enemy Structure
    [Header("Structure")]
    public GameObject bodyHitbox;
    public GameObject attackHitbox;

    // Enemy AudioClips
    [Header("Sounds")]
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
        base.Start();

        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

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
        return (player.transform.position - transform.position).magnitude <= 1.5;
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
                // Execute movement
                if (animator.GetInteger("Action") == 0) {
                    if (!CheckEnemyInAttackRange()) {
                        if (findNewTargetPos) {
                            FindNextPos();
                        }

                        moveVelocity = GetNextPosMove(Time.deltaTime);
                        if (moveVelocity.sqrMagnitude > 0)
                            animator.SetFloat("Move Speed", 1);
                    }

                    Vector3 dir;
                    if (moveVelocity.magnitude > 1f * Time.deltaTime) {
                        dir = moveVelocity;
                    } else {
                        dir = player.transform.position - transform.position;
                    }
                    if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.z)) {
                        if (dir.x > 0)
                            lookingDirection = 3;
                        else
                            lookingDirection = 1;
                    } else {
                        if (dir.z > 0)
                            lookingDirection = 0;
                        else
                            lookingDirection = 2;
                    }
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
            launchVelocity = Quaternion.AngleAxis(- 90 * direction, Vector3.up) * Vector3.forward * pushForceSum;
        }
        if (stunTime > 0) {
            isStunned = true;
            stunProgress = 0;
        }
        if (hitpoints < 0) {
            SoundManager.GetInstance().Play(deadClip);
            animator.SetBool("Death",true);
            alive = false;
            return true;
        }
        SoundManager.GetInstance().Play(hurtClip);
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
            fnc.transform.position = transform.position + Vector3.up * .5f + Vector3.right*UnityEngine.Random.Range(-.5f,.5f) + Vector3.forward * UnityEngine.Random.Range(-.5f,.5f);
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
    /// Play the attack AudioClip.
    /// </summary>
    public void SFXPlayAttack() {
        SoundManager.GetInstance().Play(attackClip);
    }
}
