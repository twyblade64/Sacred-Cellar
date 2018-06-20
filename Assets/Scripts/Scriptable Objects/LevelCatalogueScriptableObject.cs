using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for progression between level scenes.
/// Levels are divided using a minimum and maximum tier.
/// </summary>
[CreateAssetMenu(fileName = "Level Catalogue", menuName = "Level Catalogue")]
[System.Serializable]
public class LevelCatalogueScriptableObject : ScriptableObject {
    [System.Serializable]
    public struct LevelTier {
        public int minTier;
        public int maxTier;
        public string[] scenes;
    }
    public LevelTier[] entries;
}
