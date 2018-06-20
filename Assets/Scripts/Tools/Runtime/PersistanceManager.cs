using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Singleton used to save and load data between play sessions
/// </summary>
public class PersistanceManager : MonoBehaviour {
    // Default data used when no data found
    public GameDataScriptableObject defaultGameData;

    // Current data after beign loaded
    public GameDataScriptableObject currentGameData;

    // Player progress
    public int playerCoins;
    public float playerHealth;
    public bool playerAlive = false;
    public int playerScore;
    public int controlScheme = 0;
    public string lastLevelScene;
    public int currentLevel;

    // Player stats
    public int playerStatHealth;
    public int playerStatLuck;
    public int playerStatStrength;
    public int playerStatTenacity;
    public int playerStatSpeed;

    private static PersistanceManager instance;

    /// <summary>
    /// Singleton creation logic
    /// </summary>
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else if (instance != this) {
            Destroy(this.gameObject);
        }
        LoadGameData();
    }

    /// <summary>
    /// Singleton access
    /// </summary>
    /// <returns>The singleton instance</returns>
    public static PersistanceManager GetInstance() {
        return instance;
    }

    /// <summary>
    /// Obtain the max score achieved by the player
    /// </summary>
    /// <returns>The max score achieved by the player</returns>
    public int GetMaxScore() {
        return currentGameData.maxScore;
    }

    /// <summary>
    /// Try to set-up a new max score
    /// </summary>
    /// <param name="newValue">A new max-score</param>
    public void UpdateMaxScore(int newValue) {
        if (newValue > currentGameData.maxScore) {
            currentGameData.maxScore = newValue;
            SaveGameData();
        }
    }

    /// <summary>
    /// Add a value to the current player score
    /// </summary>
    /// <param name="val">The value to add</param>
    public void AddPlayerScore(int val) {
        playerScore += val;
    }

    /// <summary>
    /// Get the current player score
    /// </summary>
    /// <returns>The player score</returns>
    public int GetPlayerScore() {
        return playerScore;
    }

    /// <summary>
    /// Update the max-score and set the player score to 0
    /// </summary>
    public void ResetPlayerScore() {
        UpdateMaxScore(playerScore);
        playerScore = 0;
    }

    /// <summary>
    /// Save current persistance data to disk
    /// </summary>
    public void SaveGameData() {
        Debug.Log("Saving game data...");
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(Application.persistentDataPath + "/gamedata.dat");
        bf.Serialize(fs, new GameDataSerializableWrapper(currentGameData));
        fs.Close();
        Debug.Log("Game data saved!");
    }

    /// <summary>
    /// Load save data from disk or create one if it doesn't exists
    /// </summary>
    public void LoadGameData() {
        Debug.Log("Loading game data...");
        if (File.Exists(Application.persistentDataPath + "/gamedata.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.OpenRead(Application.persistentDataPath + "/gamedata.dat");

            GameDataSerializableWrapper gdsw = (GameDataSerializableWrapper)bf.Deserialize(fs);
            currentGameData = gdsw.BuildGameData();
            fs.Close();
            Debug.Log("Game data loaded!");
            return;
        }
        Debug.Log("No save file found!");
        CreateGameData();
        SaveGameData();

    }

    /// <summary>
    /// Create default game data
    /// </summary>
    /// <returns>Default game data</returns>
    public GameDataScriptableObject CreateGameData() {
        currentGameData = UnityEngine.Object.Instantiate<GameDataScriptableObject>(defaultGameData);
        return currentGameData;
    }

    /// <summary>
    /// Reset game data to default
    /// </summary>
    public void ResetGameData() {
        CreateGameData();
        SaveGameData();
    }

    /// <summary>
    /// Serializable representation of the game data
    /// </summary>
    [System.Serializable]
    internal class GameDataSerializableWrapper {
        public int deathCount;
        public int maxWave;
        public int maxScore;

        public GameDataSerializableWrapper(GameDataScriptableObject gameData) {
            deathCount = gameData.deathCount;
            maxWave = gameData.maxWave;
            maxScore = gameData.maxScore;
        }

        public GameDataScriptableObject BuildGameData() { 
            GameDataScriptableObject gd = ScriptableObject.CreateInstance<GameDataScriptableObject>();
            gd.deathCount = deathCount;
            gd.maxScore = maxScore;
            gd.maxWave = maxWave;
            return gd;
        }
    }

    /// <summary>
    /// Set the player stats to 0
    /// </summary>
    public void ResetPlayerStats() {
        playerStatHealth = 0;
        playerCoins = 0;
        playerStatLuck = 0;
        playerStatStrength = 0;
        playerStatTenacity = 0;
        playerStatSpeed = 0;
    }
}
