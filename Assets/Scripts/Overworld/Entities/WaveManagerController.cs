using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class feeds enemy spawners with enemy waves.
/// Enemies are selected randomly from the enemy catalogue scriptable object depending on their asigned difficulty points and the difficulty points left for each wave.
/// Difficulty points increase as you go through levels and with the passing of the waves for each level.
/// Formula for difficulty points: dp =  diffBase + currentLevel * diffDelta + waveProgress * (diffIncreaseBase + diffIncreaseDelta * currentLevel)
/// </summary>
public class WaveManagerController : MonoBehaviour {

    // Base ammount of enemy waves
    private int waveBase    = 5;

    // Increase of enemy waves per difficulty level
    private int waveDelta   = 1;

    // Base difficulty points (Waves enemies are selected randomly consuming difficulty points)
    private int diffBase    = 100;

    // Increase of base difficulty points per difficutly level
    private int diffDelta   = 50;

    // Increase of current difficulty per wave increase
    private int diffIncreaseBase    = 10;

    // Increase of current difficulty per wave increase that increases for each completed level
    private int diffIncreaseDelta   = 5;

    // Catalogue for each of the different enemies with their respective difficulty value
    public EnemyCatalogueScriptableObject enemyCatalogue;

    // The total number of waves 
    [SerializeField] private int waveNumber;

    // The current wave in progress
    [SerializeField] private int waveProgress;

    // If the wave manager has started sending enemy waves
    private bool started;

    // References to spawners in scene
    private SpawnerController[] spawners;

    // The manager has finished sending all enemy waves
    public bool Finished {
        get {
            return waveProgress >= waveNumber;
        }
    }

    /// <summary>
    /// Find spawners in scene
    /// </summary>
    public void Awake() {
        spawners = GameObject.FindObjectsOfType<SpawnerController>();
    }

    /// <summary>
    /// Initiate values and start sending enemy waves.
    ///
    /// </summary>
    public void Start() {
        waveNumber = waveBase + waveDelta * PersistanceManager.GetInstance().currentLevel;
        SendWaves();
    }

    /// <summary>
    /// Check for enemies in scene and continue with the next wave if there are none.
    /// </summary>
    public void Update() {
        // Check manager status
        if (started && !Finished) {
            // Check if there are no more enemies in scene
            if (!GameObject.FindGameObjectWithTag("Enemy")) {
                // Check spawners status
                bool completed = true;
                for (int i = 0; i < spawners.Length; i++)
                    completed = completed && spawners[i].Completed;

                // Continue with the next wave
                if (completed) {
                    waveProgress++;
                    SendWaves();
                }
            }
        }
    }

    /// <summary>
    /// Create an enemy sequence and send it to the spawners.
    /// </summary>
    public void SendWaves() {
        List<GameObject>[] waveSequence = ChooseSequence();
        for (int i = 0;  i < waveSequence.Length; i++)
            spawners[i].SetSequence(waveSequence[i]);
        started = true;
    }

    /// <summary>
    /// Creates an object list for each spawner and fills them based on the calculated difficulty points and the difficulty value of each enemy in the catalogue.
    /// Stops filling the lists when the available difficulty points are less than the minimum value of all enemies.
    /// </summary>
    /// <returns></returns>
    private List<GameObject>[] ChooseSequence() {
        // Get the available difficulty points
        float targetNumber = diffBase + PersistanceManager.GetInstance().currentLevel * diffDelta + waveProgress * (diffIncreaseBase + diffIncreaseDelta * PersistanceManager.GetInstance().currentLevel);

        // Get the minimum value of all enemies
        float minBudget = float.MaxValue;
        for (int i = 0; i < enemyCatalogue.catalogue.Length; i++)
            if (enemyCatalogue.catalogue[i].minTier <= PersistanceManager.GetInstance().currentLevel)
                if (enemyCatalogue.catalogue[i].score < minBudget)
                    minBudget = enemyCatalogue.catalogue[i].score;

        // Create the lists for each spawner
        List<GameObject>[] sequence = new List<GameObject>[spawners.Length];
        for (int i = 0; i < spawners.Length; i++) {
            sequence[i] = new List<GameObject>();
        }

        // Infinite loop exit
        int margin = 1000;

        // Fill object lists
        int spawnerIndex = 0;
        float budget = targetNumber;
        while (budget >= minBudget) {
            // Check for random enemy
            int enemy = Random.Range(0, enemyCatalogue.catalogue.Length);
            if (enemyCatalogue.catalogue[enemy].minTier <= PersistanceManager.GetInstance().currentLevel && enemyCatalogue.catalogue[enemy].score <= budget) {
                budget -= enemyCatalogue.catalogue[enemy].score;
                sequence[spawnerIndex].Add(enemyCatalogue.catalogue[enemy].enemyPrefab);
                spawnerIndex = (spawnerIndex + 1) % spawners.Length;
                margin = 1000;
            } else {
                margin--;
            }
            if (margin <= 0)
                break;
        }

        return sequence;
    }
}
