using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the level state, mostly updating the potential fields for the AI
/// </summary>
public class LevelManager : MonoBehaviour {
    public static LevelManager instance;

    // Semi-static potential field generators, like destructible enviorment
    private List<IForceGenerator> semistaticGenerators;

    // Dynamic potential field generators, like the player and enemies
    private List<IForceGenerator> dynamicGenerators;

    // Static potential field
    [SerializeField]
    private PotentialFieldScriptableObject staticPF;

    // Semi-static potential field
    private PotentialFieldScriptableObject semistaticPF;
    [SerializeField]

    // Dynamic potential field
    private PotentialFieldScriptableObject dynamicPF;

    // Final potential field
    [SerializeField]
    private PotentialFieldScriptableObject resultPF;

    // Player reference
    private PlayerController player;

    // Semi-static potential field update flag
    private bool semistaticPFUpdateIssue;

    public static LevelManager GetInstance() {
        return instance;
    }

    /// <summary>
    /// Initiate references. Static potential field is created in editor using TiledLevelBuilder
    /// </summary>
    public void Awake() {
        instance = this;
        semistaticPF = ScriptableObject.CreateInstance<PotentialFieldScriptableObject>();
        dynamicPF = ScriptableObject.CreateInstance<PotentialFieldScriptableObject>();
        resultPF = ScriptableObject.CreateInstance<PotentialFieldScriptableObject>();
        semistaticPF.Init(staticPF.Width, staticPF.Height);
        dynamicPF.Init(staticPF.Width, staticPF.Height);
        resultPF.Init(staticPF.Width, staticPF.Height);
        semistaticGenerators = new List<IForceGenerator>(128);
        dynamicGenerators = new List<IForceGenerator>(64);
    }

    /// <summary>
    /// Update dynamic potential field
    /// </summary>
    public void Update() {
        if (Time.timeScale > 0) {
            dynamicPF.Clear(0);
            foreach (IForceGenerator gen in dynamicGenerators)
                gen.GenerateForce(ref dynamicPF);
            UpdateResultPF();
        }
    }

    /// <summary>
    /// Update semi-static potential field
    /// </summary>
    public void LateUpdate() {
        if (semistaticPFUpdateIssue) {
            UpdateSemistaticPF();
            semistaticPFUpdateIssue = false;
        }
    }

    /// <summary>
    /// Set the static potential field reference
    /// </summary>
    /// <param name="pf">The static potential field</param>
    public void SetStaticPotentialField(PotentialFieldScriptableObject pf) {
        staticPF = pf;
    }

    /// <summary>
    /// Remove static reference
    /// </summary>
    public void OnDestroy() {
        instance = null;
    }

    /// <summary>
    /// Draw debug graphics of the potential field when selected
    /// </summary>
    public void OnDrawGizmosSelected() {
        if (Application.isPlaying) {
            if (resultPF) {
                for (int i = 0; i < resultPF.Width * resultPF.Height; i++) {
                    if (resultPF.Data[i] <= PotentialFieldScriptableObject.BLOCKED)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.Lerp(Color.blue, Color.cyan, 1f * resultPF.Data[i] / 50);

                    Gizmos.DrawCube(new Vector3(i % resultPF.Width, .51f + Mathf.Max(0, (1f * resultPF.Data[i] / 100) + 0.5f) * 0.5f, -i / resultPF.Width), new Vector3(.9f, .1f + Mathf.Max(0, (1f * resultPF.Data[i] / 100) + 0.5f) * 1f, .9f));
                }
            }
        } else {
            if (staticPF) {
                for (int i = 0; i < staticPF.Width * staticPF.Height; i++) {
                    if (staticPF.Data[i] <= PotentialFieldScriptableObject.BLOCKED)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.Lerp(Color.blue, Color.cyan, (1f * staticPF.Data[i] - short.MinValue) / (short.MaxValue - short.MinValue));

                    Gizmos.DrawCube(new Vector3(i % staticPF.Width, .51f + Mathf.Max(0, (1f * staticPF.Data[i] / 100) + 0.5f) * 0.5f, -i / staticPF.Width), new Vector3(.9f, .1f + Mathf.Max(0, (1f * staticPF.Data[i] / 100) + 0.5f) * 1f, .9f));
                }
            }
        }
    }

    public PotentialFieldScriptableObject GetResultPF() {
        return resultPF;
    }

    public PotentialFieldScriptableObject GetSemistaticPF() {
        return semistaticPF;
    }

    public PotentialFieldScriptableObject GetDynamicPF() {
        return dynamicPF;
    }

    /// <summary>
    /// Activate semi-static potential field update flag
    /// </summary>
    public void IssueSemistaticUpdate() {
        semistaticPFUpdateIssue = true;
    }

    /// <summary>
    /// Update semi-static potential field
    /// </summary>
    public void UpdateSemistaticPF() {
        semistaticPF.Clear(0);
        for (int i = semistaticGenerators.Count-1; i >= 0; i--) {
            IForceGenerator gen = semistaticGenerators[i];
            if (gen != null)
                gen.GenerateForce(ref semistaticPF);
            else
                semistaticGenerators.RemoveAt(i);
        }
    }

    /// <summary>
    /// Update result potential field with the sum of all potential fields
    /// </summary>
    /// <returns>The updated result potential field</returns>
    public PotentialFieldScriptableObject UpdateResultPF() {
        for (int i = 0; i < staticPF.Width * staticPF.Height; i++) {
            resultPF.Data[i] = staticPF.Data[i] + semistaticPF.Data[i] + dynamicPF.Data[i];
        }
        return resultPF;
    }

    /// <summary>
    /// Add a semi-static force generator
    /// </summary>
    /// <param name="generator">A force generator</param>
    public void AddSemistaticGenerator(IForceGenerator generator) {
        semistaticGenerators.Add(generator);
        UpdateResultPF();
    }

    /// <summary>
    /// Remove a semi-static force generator
    /// </summary>
    /// <param name="generator">The force generator</param>
    public void RemoveSemistaticGenerator(IForceGenerator generator) {
        semistaticGenerators.Remove(generator);
        UpdateResultPF();
    }

    /// <summary>
    /// Add a dynamic force generator
    /// </summary>
    /// <param name="generator">A force generator</param>
    public void AddDynamicGenerator(IForceGenerator generator) {
        dynamicGenerators.Add(generator);
    }

    /// <summary>
    /// Add a dynamic force generator
    /// </summary>
    /// <param name="generator">The force generator</param>
    public void RemoveDynamicGenerator(IForceGenerator generator) {
        dynamicGenerators.Remove(generator);
    }
    
}
