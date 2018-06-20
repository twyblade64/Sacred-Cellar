using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Root class for enemies that move using the potential field system.
/// Enemies leave an small trail to avoid going back the same way and also getting into another entity's path.
/// </summary>
public abstract class SmartEnemy : BaseEnemy, IForceGenerator {
    // Small int vector helper.
    [System.Serializable]
    public struct Vector2Int {
        public int x;
        public int y;
        public Vector2Int(int a, int b) {
            x = a;
            y = b;
        }
    }

    // Potential field usage variables.
    [Header("Smart enemy")]
    public int prevPosSize;
    [SerializeField]
    public Vector2Int[] prevPosHistory;
    private int prevPosHistoryLastIndex;
    private Vector2Int currentPos;
    private Vector2Int targetPos;
    protected bool findNewTargetPos;
    private Vector3 remainingDistance;

    protected CharacterController characterController;

    [Header("Custom Stats")]
    public float launchDrag;
    protected Vector3 launchVelocity;

    protected bool isStunned = false;
    protected float stunTime;
    protected float stunProgress;
    protected int lookingDirection;
    protected Vector3 moveVelocity;

    /// <summary>
    /// Call BaseEnemy.Awake() and init trail history.
    /// </summary>
    public override void Awake() {
        base.Awake();

        prevPosHistory = new Vector2Int[prevPosSize];
        for (int i = 0; i < prevPosHistory.Length; i++) {
            prevPosHistory[i] = new Vector2Int(-1, -1);
        }

        characterController = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Init variables and add enemy to the potential field generator list.
    /// </summary>
    public virtual void Start() {
        LevelManager.GetInstance().AddDynamicGenerator(this);
        Vector3 snap = Tools.VectorGridSnap(transform.position);
        currentPos.x = (int)snap.x;
        currentPos.y = -(int)snap.z;
        findNewTargetPos = true;
    }

    /// <summary>
    /// Update logic and move through the game grid.
    /// </summary>
    public virtual void Update() {
        if (Time.timeScale > 0 && alive) {
            //-- Update variables
            Vector3 endVelocity = Vector3.zero;
            Vector3 gridSnap = Vector3.zero;

            if (isStunned) {
                stunProgress += Time.deltaTime;
                if (stunProgress >= stunTime) {
                    isStunned = false;
                }
            } else {
                endVelocity += moveVelocity;
            }

            endVelocity += launchVelocity * Time.deltaTime;
            launchVelocity *= launchDrag;

            //-- Grid Snap
            if (Mathf.Abs(endVelocity.x) <= 0.0001f) {
                gridSnap.x = Mathf.Floor(transform.position.x + 0.5f) - transform.position.x;
            }
            if (Mathf.Abs(endVelocity.z) <= 0.0001f) {
                gridSnap.z = Mathf.Floor(transform.position.z + 0.5f) - transform.position.z;
            }

            gridSnap.x = Mathf.Sign(gridSnap.x) * Mathf.Pow(Mathf.Abs(gridSnap.x), 1f) * 2;
            gridSnap.z = Mathf.Sign(gridSnap.z) * Mathf.Pow(Mathf.Abs(gridSnap.z), 1f) * 2;

            endVelocity += gridSnap * Time.deltaTime;

            //-- Apply Movement
            characterController.Move(endVelocity);
            moveVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Choose the next tile to go to.
    /// </summary>
    protected void FindNextPos() {
        int bestDir = -1;
        int bestVal = 0;
        PotentialFieldScriptableObject pf = LevelManager.GetInstance().GetResultPF();
        int pX = pf.LocalPosX(transform.position);
        int pY = pf.LocalPosY(transform.position);
        Vector2Int newTarget = new Vector2Int(0, 0);

        if (pf.GetCellValue(pX, pY - 1) > bestVal) {
            bestVal = pf.GetCellValue(pX, pY - 1);
            bestDir = 0;
            newTarget.x = 0;
            newTarget.y = -1;
        }
        if (pf.GetCellValue(pX - 1, pY) > bestVal) {
            bestVal = pf.GetCellValue(pX - 1, pY);
            bestDir = 1;
            newTarget.x = -1;
            newTarget.y = 0;
        }
        if (pf.GetCellValue(pX, pY + 1) > bestVal) {
            bestVal = pf.GetCellValue(pX, pY + 1);
            bestDir = 2;
            newTarget.x = 0;
            newTarget.y = 1;
        }
        if (pf.GetCellValue(pX + 1, pY) > bestVal) {
            bestVal = pf.GetCellValue(pX + 1, pY);
            bestDir = 3;
            newTarget.x = 1;
            newTarget.y = 0;
        }

        if (bestDir != -1) {
            findNewTargetPos = false;
            targetPos = newTarget;
            remainingDistance = new Vector3(targetPos.x, 0, -targetPos.y);
            //Debug.Log("Target pos: " + targetPos.x + " " + targetPos.y);
        }

    }

    /// <summary>
    /// Generate a velocity to move towards the next tile.
    /// </summary>
    /// <param name="deltaTime">The current delta time.</param>
    /// <returns>The generated velocity towards the next tile.</returns>
    protected Vector3 GetNextPosMove(float deltaTime) {
        Vector3 vel = Vector3.zero;
        if (!findNewTargetPos) {
            float dist = remainingDistance.magnitude;
            if (dist > stats.speed * deltaTime) {
                vel = new Vector3(targetPos.x, 0, -targetPos.y) * stats.speed * deltaTime;
                remainingDistance -= vel;
            } else {
                vel = remainingDistance;
                remainingDistance -= vel;
                findNewTargetPos = true;
                prevPosHistory[prevPosHistoryLastIndex] = currentPos;
                prevPosHistoryLastIndex = (prevPosHistoryLastIndex + 1) % prevPosHistory.Length;
                Vector3 snap = Tools.VectorGridSnap(transform.position);
                currentPos.x = (int)snap.x;
                currentPos.y = -(int)snap.z;
                targetPos.x = 0;
                targetPos.y = 0;
            }
        }
        return vel;
    }

    /// <summary>
    /// Affect the referenced potential field with a force.
    /// Adds a weight to the current position to avoid enemies colliding into eachother paths.
    /// Also generates an small trail to avoid making the enemy go back the same way it came.
    /// </summary>
    /// <param name="pf">The potential field to affect.</param>
    public void GenerateForce(ref PotentialFieldScriptableObject pf) {
        for (int i = 0; i < prevPosHistory.Length; i++) {
            pf.AddCellValue(prevPosHistory[i].x, prevPosHistory[i].y, -10);
        }
        pf.AddCellValue(currentPos.x, currentPos.y, -1024);
        pf.AddCellValue(currentPos.x + targetPos.x, currentPos.y + targetPos.y, -1024);
    }

    /// <summary>
    /// Remove the instance from the potential field generator list.
    /// </summary>
    public void OnDestroy() {
        LevelManager lm = LevelManager.GetInstance();
        if (lm != null && this != null)
            lm.RemoveDynamicGenerator(this);
    }
}
