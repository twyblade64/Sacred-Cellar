using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class allows enviorment props to be destroyed when recieving damage, modifying AI paths in the process.
/// </summary>
public class DestroyableEnviormentController : MonoBehaviour, IDamagable, IForceGenerator, ITriggerReciever {
    // Ammount of damage the prop can take
    public float hitpoints;

    // FX prefab instantiated when prop is destroyed
    public GameObject[] derbisPrefabs;

    // Force which will be applied to the derbis when created
    public float derbisLaunchForce;

    // Ammount of derbis instances to be created when destroyed
    public float derbisAmount;


    [Header("Force generator")]
    // Activate AI path modification
    public bool isForceGenerator;

    // Value of the force to be applied
    public int force;

    // Circular radius of the applied force
    public int rad;

    // Faction of the prop used by the damage system
    public FactionManager.Faction selfFaction;


    /// <summary>
    /// Add prop to the AI path system if isForceGenerator enabled
    /// </summary>
    public void Start() {
        if (isForceGenerator) {
            LevelManager.GetInstance().AddSemistaticGenerator(this);
            LevelManager.GetInstance().IssueSemistaticUpdate();
        }
    }

    /// <summary>
    /// Damage the prop
    /// </summary>
    public bool Damage(float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        if (hitpoints > 0) {
            hitpoints -= damage;
            if (hitpoints <= 0) {
                InitiateDestruction();
                Destroy(gameObject);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Damage the prop
    /// </summary>
    public bool RecieveAttack(GameObject source, Collider reciever, float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0) {
        Damage(damage, direction, pushForce, stunForce);
        return true;
    }

    /// <summary>
    /// Generate a force in the current position of the prop
    /// </summary>
    /// <param name="pf"></param>
    public void GenerateForce(ref PotentialFieldScriptableObject pf) {
        int pX = pf.LocalPosX(transform.position);
        int pY = pf.LocalPosY(transform.position);
        pf.AddLinearForce(pX, pY, force, rad);
    }

    public FactionManager.Faction GetFaction() {
        return selfFaction;
    }

    /// <summary>
    /// Create prop destruction effects
    /// </summary>
    public void InitiateDestruction() {
        for (int i = 0; i < derbisAmount; i++) {
            GameObject derbisPrefab = derbisPrefabs[UnityEngine.Random.Range(0, derbisPrefabs.Length)];
            Rigidbody rb = Instantiate(derbisPrefab, transform.position+ UnityEngine.Random.insideUnitSphere.normalized, derbisPrefab.transform.rotation).GetComponent<Rigidbody>();
            if (rb != null) {
                rb.AddForce(UnityEngine.Random.insideUnitSphere.normalized * derbisLaunchForce);
            }
        }
    }

    /// <summary>
    /// Remove the prop from the AI path system
    /// </summary>
    public void OnDestroy() {
        if (isForceGenerator) {
            LevelManager lm = LevelManager.GetInstance();
            if (lm != null && this != null) {
                lm.RemoveSemistaticGenerator(this);
                LevelManager.GetInstance().IssueSemistaticUpdate();
            }
        }
    }

    public void OnExtensionTriggerEnter(GameObject origin, Collider other) {}

    public void OnExtensionTriggerStay(GameObject origin, Collider other) {}

    public void OnExtensionTriggerExit(GameObject origin, Collider other) {}

    public GameObject GetRecieverGameObject() {
        return gameObject;
    }
}
