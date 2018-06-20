using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This enemy doesn't attacks directly, instead, it has a trailing body that can damage the player on contact.
/// </summary>
public class SnakeController : BaseEnemy, ITriggerReciever {
    private CharacterController characterController;
    private Animator animator;

    // Variables needed for the body trail
    [Header("Custom Stats")]
    public float directionChangeTime;
    private float directionChangeProgress;
    public int trailResolution;
    public Vector3[] trailPositions;
    public float trailTime;
    private float trailDelta;
    private float trailDeltaProgress;
    public int trailSectionsQnt;
    public GameObject trailSectionPrefab;
    private int trailLastIndex;
    private SnakeSectionController[] trailSections;
    private Collider[] trailSectionsColliders;

    private int lookingDirection;
    private Vector3[] directionArray;

    // Enemy Structure
    [Header("Structure")]
    public GameObject bodyHitbox;
    public GameObject attackHitbox;

    private bool isStunned = false;
    private float stunTime;
    private float stunProgress;

    // Enemy Audio clips
    [Header("Sounds")]
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deadClip;

    Vector3 moveVelocity;

    public float attackRange;

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
    /// Create the trail sections and assign its positions.
    /// </summary>
    void Start() {
        directionArray = new Vector3[4];
        directionArray[0] = Vector3.forward;
        directionArray[1] = Vector3.left;
        directionArray[2] = Vector3.back;
        directionArray[3] = Vector3.right;
        lookingDirection = UnityEngine.Random.Range(0, 4);

        trailPositions = new Vector3[trailResolution];
        for (int i = 0; i < trailResolution; i++)
            trailPositions[i] = transform.position;

        trailSections = new SnakeSectionController[trailSectionsQnt];
        trailSectionsColliders = new Collider[trailSectionsQnt];
        for (int i = 0; i < trailSectionsQnt; i++) {
            trailSections[i] = Instantiate(trailSectionPrefab, gameObject.transform).GetComponent<SnakeSectionController>();
            trailSections[i].Setup(this, 1f * i / trailSectionsQnt);
            trailSectionsColliders[i] = trailSections[i].GetComponent<Collider>();
            trailSections[i].name += " " + i;
        }

        trailDelta = 1f * trailTime / trailResolution;
        Debug.Log(trailDelta);

        SoundManager.GetInstance().Load(hurtClip);
        SoundManager.GetInstance().Load(deadClip);
    }

    /// <summary>
    /// Move the enemy and update trail positions.
    /// </summary>
    void Update() {
        if (Time.timeScale > 0 && alive) {
            // Change direction
            directionChangeProgress += Time.deltaTime;

            Vector3 endVelocity = Vector3.zero;
            Vector3 gridSnap = Vector3.zero;

            if (isStunned) {
                stunProgress += Time.deltaTime;
                if (stunProgress >= stunTime) {
                    isStunned = false;
                }
            }

            endVelocity += moveVelocity;

            // Grid Snap
            if (Mathf.Abs(endVelocity.x) <= 0.0001f) {
                gridSnap.x = Mathf.Floor(transform.position.x + 0.5f) - transform.position.x;
            }
            if (Mathf.Abs(endVelocity.z) <= 0.0001f) {
                gridSnap.z = Mathf.Floor(transform.position.z + 0.5f) - transform.position.z;
            }

            gridSnap.x = Mathf.Sign(gridSnap.x) * Mathf.Pow(Mathf.Abs(gridSnap.x), 1f) * 2;
            gridSnap.z = Mathf.Sign(gridSnap.z) * Mathf.Pow(Mathf.Abs(gridSnap.z), 1f) * 2;

            endVelocity += gridSnap * Time.deltaTime;

            //  Apply Movement
            characterController.Move(endVelocity);
            moveVelocity = Vector3.zero;

            transform.rotation = Quaternion.AngleAxis(-lookingDirection * 90, Vector3.up);
            Debug.DrawRay(transform.position, transform.forward * 2);

            // Update trail
            trailDeltaProgress += Time.deltaTime;
            if (trailDeltaProgress >= trailDelta) {
                trailDeltaProgress -= trailDelta;
                trailPositions[trailLastIndex] = transform.position;
                trailLastIndex = (trailLastIndex + 1) % trailResolution;
            }

            foreach (SnakeSectionController trail in trailSections)
                trail.UpdatePosition();
        }
    }

    // Debug trail positions on inspector selection
    void OnDrawGizmosSelected() {
        foreach (Vector3 pos in trailPositions)
            Gizmos.DrawWireSphere(pos, .25f);
    }

    /// <summary>
    /// Obtain a position on the trail.
    /// </summary>
    /// <param name="offset">An offset value between 0 and 1, with 0 being the start of the trail and 1 the end.</param>
    /// <returns>The position in the trail.</returns>
    public Vector3 GetTrailOffsetPosition(float offset) {
        float relOffset = offset * (trailResolution-1) + trailLastIndex + trailDeltaProgress / trailDelta /** trailResolution / trailSectionsQnt*/;
        int offsetIndex = Mathf.FloorToInt(relOffset);
        float offsetP = relOffset - offsetIndex;

        return Vector3.Lerp(trailPositions[(offsetIndex)%trailResolution], trailPositions[(offsetIndex + 1) % trailResolution], offsetP);
    }

    // Not used.
    public override void Attack() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool CheckAttackAvailable() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool CheckCanAttack() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool CheckEnemyInAttackRange() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool CheckEnemyInSight() {
        throw new NotImplementedException();
    }

    // Not used.
    public override void CloseIn() {
        throw new NotImplementedException();
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
            fnc.transform.position = transform.position + Vector3.up * .5f + Vector3.right * UnityEngine.Random.Range(-.5f, .5f) + Vector3.forward * UnityEngine.Random.Range(-.5f, .5f);
            fnc.Init();

            Damage(damage, direction, pushForce, stunForce);
            return true;
        }
        return false;
    }

    // Not used.
    public override void Defend() {
        throw new NotImplementedException();
    }

    // Not used.
    public override void Dodge() {
        throw new NotImplementedException();
    }

    // Not used.
    public override void MoveAway() {
        throw new NotImplementedException();
    }

    // Not used.
    public override void PlanMoving() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Random movement within the world. 
    /// Constantly changes direction when a timer has completed or an obstacle is in front.
    /// Direction changes into a perpendicular one.
    /// </summary>
    public override void Wander() {
        if (directionChangeProgress >= directionChangeTime) {
            lookingDirection += (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) + 4;
            lookingDirection %= 4;
            directionChangeProgress = 0;
        }

        RaycastHit rayHit;
        if (Physics.Raycast(transform.position, directionArray[lookingDirection], out rayHit, 0.5f + stats.speed * Time.deltaTime, LayerMask.GetMask("Enviorment"))) {
            if (rayHit.distance > 0.5) {
                moveVelocity = directionArray[lookingDirection] * (rayHit.distance - 0.5f);
            } else {
                lookingDirection += (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) + 4;
                lookingDirection %= 4;
                directionChangeProgress = 0;
            }
        } else {
            moveVelocity = directionArray[lookingDirection] * stats.speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Apply damage to player if the attack hitbox of any of the body trails collides with it.
    /// </summary>
    /// <param name="origin">GameObject on which OnTriggerExit method was called.</param>
    /// <param name="other">Collider data of the OnTriggerExit method.</param>
    public void OnExtensionTriggerEnter(GameObject origin, Collider other) {
        if (alive) {
            bool attackCollision = false;
            if (origin == attackHitbox)
                attackCollision = true;
            for (int i = 0; i < trailSections.Length; i++) {
                if (origin == trailSections[i].gameObject) {
                    attackCollision = true;
                    break;
                }
            }

            if (attackCollision) {
                TriggerSender ts = other.GetComponent<TriggerSender>();
                if (ts != null) {
                    GameObject triggerReciever = ts.GetReciever().GetRecieverGameObject();
                    IDamagable damagable = triggerReciever.GetComponent<IDamagable>();
                    if (damagable != null) {
                        if ((damagable.GetFaction() & oppositeFaction) != 0) {
                            damagable.RecieveAttack(origin, other, stats.attackDamageMultiplier, Tools.Vector3ToDir4Int(other.transform.position - origin.transform.position), stats.attackForceMultiplier);
                        }
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

    // Not used.
    public override bool isAttacking() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool isDefending() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool isDodging() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool isWandering() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool isMovingAway() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool isClosingIn() {
        throw new NotImplementedException();
    }

    // Not used.
    public override bool isPlanningMove() {
        throw new NotImplementedException();
    }
}