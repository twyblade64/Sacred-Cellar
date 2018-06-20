using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// End point for each level.
/// The player can go to the next level only when there are no more enemies.
/// </summary>
public class EndpointController : MonoBehaviour {
    public string nextLevel;
    public LevelCatalogueScriptableObject levelCatalogue;
    public string shopScene;
    public bool gotoShop;

    /// <summary>
    /// Change scene if conditions are met.
    /// </summary>
    /// <returns></returns>
    public bool GoNextLevel() {
        if (!GameObject.FindWithTag("Enemy")) {
            if (gotoShop) {
                SceneManager.GetInstance().ChangeScene(shopScene, true);
            } else {
                if (nextLevel.Equals("")) {
                    PersistanceManager.GetInstance().currentLevel++;
                    SceneManager.GetInstance().ChangeScene(GetNextLevel(), true);
                } else {
                    // Manual level change
                    SceneManager.GetInstance().ChangeScene(nextLevel, true);
                }
            }
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Select the next scene from a random scene list, taking into account the difficulty level
    /// </summary>
    /// <returns></returns>
    public string GetNextLevel() {
        int currentLevel = PersistanceManager.GetInstance().currentLevel;
        List<string> selectableLevels = new List<string>();
        for (int i = 0; i < levelCatalogue.entries.Length; i++) {
            if ((levelCatalogue.entries[i].minTier == -1 || levelCatalogue.entries[i].minTier <= currentLevel) && (levelCatalogue.entries[i].maxTier == -1 || levelCatalogue.entries[i].maxTier >= currentLevel))
                selectableLevels.AddRange(levelCatalogue.entries[i].scenes);
        }
        if (selectableLevels.Count == 0)
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return selectableLevels[Random.Range(0, selectableLevels.Count)];
    }
}
