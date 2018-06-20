using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main class for player control
/// </summary>
public class PlayerController : MonoBehaviour, ITriggerReciever, IDamagable, IForceGenerator {
    protected CharacterController characterController;
    protected Animator animator;

    protected bool isTouching;
    protected Vector2 prevTouch;
    protected Vector2 currentTouch;

    // Player statistics
    [Header("Stats")]
    public int coins;

    public int baseHitpoints;
    public float baseSpeed;
    public float baseDodgeTime;
    public float baseSlideTime;
    public float baseAttackDamage;
    public float basePushForce;
    public float baseLaunchDrag;

    public float hitpoints;
    public float hitpointsMax;

    protected float attackDamage;
    protected float pushForce;
    public float stunForce;
    public float invencibilityTime;
    protected float invencibilityProgress;
    public bool isActive;
    public bool isAlive;
    public bool isDespawning;
    protected bool attackCheck;
    protected List<GameObject> attackWhitelist;

    // Player faction
    public FactionManager.Faction selfFaction;
    [EnumMask]
    public FactionManager.Faction oppositeFaction;

    // Movement data
    [Header("Movement")]
    // -- Walk
    protected float speed;
    public float walkDrag;
    public float snapSpeedThereshold;
    public float snapDistanceThereshold;
    protected Vector3 walkVelocity;

    // -- Dodge
    protected float dodgeTime;
    public float dodgeDistance;
    protected float dodgeTimeProgress;
    protected Vector3 dodgeDistanceVector;
    public float dodgeSmoothing;

    // -- Slide
    protected float slideTime;
    public float slideDistance;
    protected float slideTimeProgress;
    protected Vector3 slideDistanceVector;
    public float slideSmoothing;

    protected Vector3 launchVelocity;
    protected float launchDrag;

    protected enum MoveMode { Walk, Slide, Dodge, Launched };
    protected MoveMode moveMode;

    protected Vector3 despawnPosition;

    // Audio files
    [Header("Audio files")]
    public AudioClip hurtClip;
    public AudioClip dodgeClip;
    public AudioClip walkClip;
    public AudioClip slideClip;
    public AudioClip attackClip;
    public AudioClip coinClip;

    // GameObject structure
    [Header("Structure")]
    public GameObject bodyHitbox;
    public GameObject attackHitbox;

    [Header("Other")]
    public GameObject floatingNumberPrefab;

    protected int facingDirection;

    public bool isAttacking;

    /// <summary>
    /// Initiate local component references and variables
    /// </summary>
    public virtual void Awake() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Physics.IgnoreLayerCollision(9, 10, true);

        attackWhitelist = new List<GameObject>(10);
    }

    /// <summary>
    /// Load AudioClips to SoundManager and apply persistance to the player
    /// </summary>
    public virtual void Start() {
        // Load AudioClips
        SoundManager.GetInstance().Load(hurtClip);
        SoundManager.GetInstance().Load(slideClip);
        SoundManager.GetInstance().Load(attackClip);
        SoundManager.GetInstance().Load(coinClip);

        // Retrive persistent player data and load it
        PersistanceManager persistanceManager = PersistanceManager.GetInstance();
        UpdateStats();

        isAlive = true;
        if (!persistanceManager.playerAlive) {
            persistanceManager.playerAlive = true;
            persistanceManager.playerHealth = hitpointsMax;
            hitpoints = hitpointsMax;
            coins = 0;
        } else {
            hitpoints = persistanceManager.playerHealth;
            coins = persistanceManager.playerCoins;
        }

        // Add player to AI pathfiding system
        LevelManager.GetInstance().AddDynamicGenerator(this);
    }

    /// <summary>
    /// Apply movement to the player
    /// </summary>
    public virtual void Update() {
        // Check if game not paused
        if (Time.timeScale > 0) {
            isAttacking = animator.GetBool("Attack");

            // Update facing direction and snap position to y=1
            transform.rotation = Quaternion.AngleAxis(-facingDirection * 90, Vector3.up);
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);

            // Check if player can move
            if (isActive && isAlive && !isDespawning) {
                // Initiate velocity vectors
                Vector3 gridSnap = Vector3.zero;
                Vector3 dodgeVelocity = Vector3.zero;
                Vector3 slideVelocity = Vector3.zero;
                Vector3 endVelocity = Vector3.zero;

                // Clear animator flags
                animator.SetBool("Walking", false);
                animator.SetBool("Dodging", false);
                animator.SetBool("Sliding", false);
                animator.SetBool("Hurt", false);

                // Check current move mode
                switch (moveMode) {
                    // Walk - Constant movement while button is pressed.
                    case MoveMode.Walk:
                        endVelocity += walkVelocity * Time.deltaTime;
                        if (walkVelocity.magnitude > .01f)
                            animator.SetBool("Walking", true);
                        walkVelocity *= walkDrag;
                        break;
                    // Dodge - Fast movement on a short time. Cannot walk while in it.
                    case MoveMode.Dodge:
                        dodgeVelocity = dodgeDistanceVector * (Mathf.Pow(Mathf.Min(dodgeTimeProgress + Time.deltaTime, slideTime), dodgeSmoothing) - Mathf.Pow(dodgeTimeProgress, dodgeSmoothing)) / Mathf.Pow(dodgeTime, dodgeSmoothing);
                        dodgeTimeProgress += Time.deltaTime;
                        animator.SetFloat("Dodge Progress", (Mathf.Pow(Mathf.Min(dodgeTimeProgress, dodgeTime) / dodgeTime, dodgeSmoothing)));
                        if (dodgeTimeProgress >= dodgeTime)
                            moveMode = MoveMode.Walk;
                        else
                            animator.SetBool("Dodging", true);
                        endVelocity += dodgeVelocity;
                        break;
                    // Slide - Slightly fast movement over a large period of time.
                    case MoveMode.Slide:
                        slideVelocity = slideDistanceVector * (Mathf.Pow(Mathf.Min(slideTimeProgress + Time.deltaTime, slideTime), slideSmoothing) - Mathf.Pow(slideTimeProgress, slideSmoothing)) / Mathf.Pow(slideTime, slideSmoothing);
                        slideTimeProgress += Time.deltaTime;
                        if (slideTimeProgress >= slideTime)
                            moveMode = MoveMode.Walk;
                        else
                            animator.SetBool("Sliding", true);
                        endVelocity += slideVelocity;
                        break;
                    // Launched - Beign pushed by the attacks of enemies.
                    case MoveMode.Launched:
                        endVelocity += launchVelocity * Time.deltaTime;
                        launchVelocity *= launchDrag;
                        if (launchVelocity.magnitude <= .5f)
                            moveMode = MoveMode.Walk;
                        else
                            animator.SetBool("Hurt", true);
                        break;
                }

                // Try to snap player position to x,Z grid
                // Check speed theresholds
                if (Mathf.Abs(endVelocity.x) < snapSpeedThereshold * Time.deltaTime) {
                    gridSnap.x = Mathf.Floor(transform.position.x + 0.5f) - transform.position.x;
                    walkVelocity.x = 0;
                }
                if (Mathf.Abs(endVelocity.z) < snapSpeedThereshold * Time.deltaTime) {
                    gridSnap.z = Mathf.Floor(transform.position.z + 0.5f) - transform.position.z;
                    walkVelocity.z = 0;
                }

                gridSnap.x = Mathf.Sign(gridSnap.x) * Mathf.Pow(Mathf.Abs(gridSnap.x), .5f) * snapSpeedThereshold;
                gridSnap.z = Mathf.Sign(gridSnap.z) * Mathf.Pow(Mathf.Abs(gridSnap.z), .5f) * snapSpeedThereshold;

                endVelocity += gridSnap * Time.deltaTime;

                // Apply movement
                endVelocity.y = 0;
                characterController.Move(endVelocity);
            } else {
                // Activate after spawning animation
                if (!isActive) {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Spawn"))
                        isActive = true;
                }

                // Make despawning animation
                if (isDespawning) {
                    transform.position = Vector3.Lerp(transform.position, despawnPosition, .2f);
                }
            }
        }
    }

    /// <summary>
    /// Apply walk move-mode to the player in the specified direction.
    /// Walking applies a constant velocity in the specified direction and a smooth slowdown on release.
    /// </summary>
    /// <param name="dir">The movement direction in the 4 integer notation [0-3]</param>
    public void MoveWalk(int dir) {
        if (isActive && isAlive && !isDespawning && !isAttacking) {
            if (moveMode != MoveMode.Dodge && moveMode != MoveMode.Launched) {
                SoundManager.GetInstance().SoloPlay(walkClip);
                walkVelocity = Quaternion.AngleAxis(-dir * 90, Vector3.up) * Vector3.forward * speed;

                moveMode = MoveMode.Walk;
                facingDirection = dir;
            }
        }
    }

    /// <summary>
    /// Apply slide move-mode to the player in the specified direction.
    /// Sliding makes a slightly-fast movement over a long period of time.
    /// </summary>
    /// <param name="dir">The movement direction in the 4 integer notation [0-3]</param>
    public void MoveSlide(int dir) {
        if (isActive && isAlive && !isDespawning) {
            if (moveMode != MoveMode.Dodge && moveMode != MoveMode.Launched) {
                SoundManager.GetInstance().Play(slideClip);
                moveMode = MoveMode.Slide;

                slideDistanceVector = Quaternion.AngleAxis(-dir * 90, Vector3.up) * Vector3.forward * slideDistance;
                slideTimeProgress = 0;
                facingDirection = dir;
            }
        }
    }

    /// <summary>
    /// Stop the slide move-mode
    /// </summary>
    public void StopSlide() {
        if (isActive && isAlive && !isDespawning) {
            if (moveMode == MoveMode.Slide)
                moveMode = MoveMode.Walk;
        }
    }

    /// <summary>
    /// Apply dodge move-mode to the player in the specified direction.
    /// Dodging makes a fast movement to a defined distance over a short period of time where the player cannot make any other movement.
    /// </summary>
    /// <param name="dir">The movement direction in the 4 integer notation [0-3]</param>
    public void MoveDodge(int dir) {
        if (isActive && isAlive && !isDespawning) {
            if (moveMode != MoveMode.Dodge && moveMode != MoveMode.Launched) {
                SoundManager.GetInstance().Play(dodgeClip);
                moveMode = MoveMode.Dodge;

                dodgeDistanceVector = Quaternion.AngleAxis(-dir * 90, Vector3.up) * Vector3.forward * dodgeDistance;
                dodgeTimeProgress = 0;
                facingDirection = dir;
            }
        }
    }

    /// <summary>
    /// Make an attack in the specified direction.
    /// Attack vary depending on the current movemode used when called, and can be seen in the animator tree.
    /// </summary>
    /// <param name="dir">The attack direction in the 4 integer notation [0-3]</param>
    public virtual void Attack(int dir) {
        if (isActive && isAlive && !isDespawning) {
            if (moveMode != MoveMode.Launched && !isAttacking) {
                int relDir = (dir - facingDirection + 4)%4;
                if (moveMode != MoveMode.Dodge && moveMode != MoveMode.Launched && moveMode != MoveMode.Slide) {
                    animator.SetBool("Attack", true);
                    animator.SetInteger("Attack Direction", relDir);
                    SoundManager.GetInstance().Play(attackClip);
                    facingDirection = dir;
                } else {
                    if (relDir != 2) {
                        animator.SetBool("Attack", true);
                        animator.SetInteger("Attack Direction", relDir);
                        SoundManager.GetInstance().Play(attackClip);
                    }
                    if (relDir == 0 && moveMode == MoveMode.Slide)
                        StopSlide();
                }
            }
        }
    }

    // Not used
    public void Push(int dir, float force) {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Check the colisions of the bodyHitbox and attackHitobx, and respond acordingly.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="other"></param>
    public void OnExtensionTriggerEnter(GameObject origin, Collider other) {
        if (isActive && isAlive && !isDespawning) {

            // Check attack
            if (attackCheck && origin == attackHitbox ) {
                TriggerSender ts = other.GetComponent<TriggerSender>();
                if (ts != null) {
                    // Attacking enemy entities
                    GameObject triggerReciever = ts.GetReciever().GetRecieverGameObject();
                    IDamagable damagable = triggerReciever.GetComponent<IDamagable>();
                    if (damagable != null) {
                        // Apply damage
                        if (!attackWhitelist.Contains(triggerReciever)) {
                            if ((damagable.GetFaction() & oppositeFaction) != 0) {
                                bool recieved = damagable.RecieveAttack(origin, other, attackDamage, facingDirection, pushForce, stunForce);
                                if (recieved)
                                    attackWhitelist.Add(triggerReciever);
                            }
                        }
                    }
                } else {
                    // Attacking destructible enviorment.
                    IDamagable damagable = other.GetComponent<IDamagable>();
                    if (damagable != null) {
                        if (!attackWhitelist.Contains(other.gameObject)) {
                            if ((damagable.GetFaction() & oppositeFaction) != 0) {
                                bool recieved = damagable.RecieveAttack(origin, other, attackDamage, facingDirection, pushForce, stunForce);
                                if (recieved)
                                    attackWhitelist.Add(other.gameObject);
                            }
                        }
                    }
                }
            }

            // Check other collisions
            if (origin == bodyHitbox) {
                // Pickups
                if (other.CompareTag("Collectible")) {
                    other.GetComponent<ICollectible>().Collect();
                }

                // Level end
                if (other.CompareTag("Endpoint")) {
                    if (other.GetComponent<EndpointController>().GoNextLevel()) {
                        despawnPosition = other.transform.position;
                        animator.SetTrigger("Despawn");
                        isDespawning = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Not used.
    /// </summary>
    public void OnExtensionTriggerStay(GameObject origin, Collider other) {}

    /// <summary>
    /// Not used.
    /// </summary>
    public void OnExtensionTriggerExit(GameObject origin, Collider other) {}

    public GameObject GetRecieverGameObject() {
        return gameObject;
    }

    public FactionManager.Faction GetFaction() {
        return selfFaction;
    }

    /// <summary>
    /// The entity takes damage from an unknown source.
    /// </summary>
    /// <param name="damage">Ammount of damage to be taken.</param>
    /// <param name="direction">Incoming damage direction. Axis represented in clockwise order: 0-Forward, 1-Left, 2-Back, 3-Right.</param>
    /// <param name="pushForce">Force applied to the entity in the specified direction.</param>
    /// <param name="stunForce">Stun duration applied to the entity.</param>
    /// <returns>True if enemy was damaged, otherwise false.</returns>
    public bool Damage(float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        if (isActive && isAlive && !isDespawning) {
            // Check invencibility
            if (moveMode != MoveMode.Launched) {
                // Play sound
                SoundManager.GetInstance().Play(hurtClip);

                // Apply damage
                AddHealth(-damage);

                // Create floating number effect
                FloatingNumberController fnc = PoolManager.poolManager.GetPoolInstance(floatingNumberPrefab).GetComponent<FloatingNumberController>();
                fnc.number = -(int)damage;
                fnc.textColor = Color.red;
                fnc.transform.position = transform.position + Vector3.up * .5f + Vector3.right * UnityEngine.Random.Range(-.1f, .1f) + Vector3.forward * UnityEngine.Random.Range(-.1f, .1f);
                fnc.Init();

                // Apply push
                if (pushForce > 0) {
                    moveMode = MoveMode.Launched;
                    launchVelocity = Quaternion.AngleAxis(-90 * direction, Vector3.up) * Vector3.forward * pushForce;
                }

                // Check for death
                if (hitpoints <= 0) {
                    PersistanceManager.GetInstance().playerAlive = false;
                    isAlive = false;
                    animator.SetTrigger("Death");
                    return true;
                }
            }
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
    public bool RecieveAttack(GameObject source, Collider reciever, float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        // Reciever must not be the attackHitbox
        if (reciever.gameObject == bodyHitbox) {
            Damage(damage, direction, pushForce, stunForce);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Death logic
    /// </summary>
    public void OnDestroy() {
        // Save player score
        PersistanceManager.GetInstance().playerCoins = coins;

        LevelManager lm = LevelManager.GetInstance();
        if (lm != null && this != null)
            lm.RemoveDynamicGenerator(this);
    }

    /// <summary>
    /// Add hit points to the player health
    /// </summary>
    /// <param name="value">Ammount of hitpoints to add</param>
    public void AddHealth(float value) {
        hitpoints = Mathf.Min(hitpoints + value, hitpointsMax);
        PersistanceManager.GetInstance().playerHealth = hitpoints;
    }

    /// <summary>
    /// Add coins to the player statistics
    /// </summary>
    /// <param name="value">The ammount of coins to add</param>
    public void AddCoins(int value) {
        coins += value;
        PersistanceManager.GetInstance().playerCoins = coins;
    }

    /// <summary>
    /// Generate a force so enemies can find the player
    /// </summary>
    /// <param name="pf">The where the force will be applied</param>
    public void GenerateForce(ref PotentialFieldScriptableObject pf) {
        int pX = pf.LocalPosX(transform.position);
        int pY = pf.LocalPosY(transform.position);
        pf.AddLinearForce(pX, pY, 180, 60);
    }

    /// <summary>
    /// Activate attack logic from external sources (Animator)
    /// </summary>
    public void ActivateAttack() {
        attackCheck = true;
    }

    /// <summary>
    /// Deactivate attack logic from external sources (Animator)
    /// </summary>
    public void DeactivateAttack() {
        attackCheck = false;
        attackWhitelist.Clear();
    }

    /// <summary>
    /// Load statistics from persistance
    /// </summary>
    public void UpdateStats() {
        PersistanceManager persistanceManager = PersistanceManager.GetInstance();
        hitpointsMax = baseHitpoints + persistanceManager.playerStatHealth;
        speed = baseSpeed + persistanceManager.playerStatSpeed * 4f / 5;
        dodgeTime = baseDodgeTime - persistanceManager.playerStatSpeed * .3f / 5;
        slideTime = baseSlideTime - persistanceManager.playerStatSpeed * 2f / 5;
        attackDamage = baseAttackDamage + persistanceManager.playerStatStrength * 2.5f / 5;
        pushForce = basePushForce + persistanceManager.playerStatStrength * 2;
        launchDrag = baseLaunchDrag - persistanceManager.playerStatTenacity * .6f / 5;
    }
}