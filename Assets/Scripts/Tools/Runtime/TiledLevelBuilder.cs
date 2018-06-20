using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if (UNITY_EDITOR)
using UnityEditor;
#endif

using SimpleJSON;

/// <summary>
/// Custom Tiled json file parser.
/// Creates prefab instances from the Tiled files and their respective static Potential Field ScriptableObject data.
/// </summary>
public class TiledLevelBuilder : MonoBehaviour {
    // Tiled .json source file
    [Header("Import Settings")]
    public TextAsset mapFile;

    //-- Asset references
    [Header("World")]
    public GameObject playerPrefab;
    public GameObject endpointPrefab;
    public GameObject spawnerPrefab;
    public GameObject dialogZonePrefab;

    [Header("Mechanisms")]
    public GameObject doorPrefab;
    public GameObject keyPrefab;
    public GameObject enemyDetectorPrefab;

    [Header("Scenary")]
    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject pitPrefab;
    public GameObject treePrefab;
    public GameObject groundSpikesPrefab;

    public GameObject bridgePrefab;
    public GameObject campfirePrefab;
    public GameObject chairPrefab;
    public GameObject flowerPrefab;
    public GameObject gatePrefab;
    public GameObject gravePrefab;
    public GameObject homePrefab;
    public GameObject pot1Prefab;
    public GameObject pot2Prefab;
    public GameObject rockBig1Prefab;
    public GameObject rockMedium1Prefab;
    public GameObject rockSmall1Prefab;
    public GameObject rockSmall2Prefab;
    public GameObject shopPrefab;
    public GameObject shroomsPrefab;
    public GameObject signalAdvicePrefab;
    public GameObject signalDirectionalPrefab;
    public GameObject skullPrefab;
    public GameObject stoneColumnBrokenPrefab;
    public GameObject stoneColumnGroundPrefab;
    public GameObject stoneColumnNormalPrefab;
    public GameObject tablePrefab;
    public GameObject tree1Prefab;
    public GameObject tree2Prefab;
    public GameObject tree3Prefab;
    public GameObject tree4Prefab;
    public GameObject wellPrefab;
    public GameObject woodenDoorPrefab;

    [Header("Enemies")]
    public GameObject enemySwordmanPrefab;
    public GameObject enemyLancerPrefab;
    public GameObject enemySnakePrefab;
    public GameObject enemyCrossbowmanPrefab;
    public GameObject enemyHammermanPrefab;
    public GameObject enemySlimeBigPrefab;
    public GameObject enemySlimeNormalPrefab;
    public GameObject enemySlimeSmallPrefab;

    public void Awake() {
#if (UNITY_EDITOR)
        Destroy(this);
#endif
    }

    public void BuildLevelPrefab() {
#if (UNITY_EDITOR)
        //----- Data parse
        string mapName = mapFile.name;
        JSONNode n = JSON.Parse(mapFile.text);
        int height = n["height"].AsInt;
        int width = n["width"].AsInt;
        int tileWidth = n["tilewidth"].AsInt;
        int tileHeight = n["tilewidth"].AsInt;

        //---- Create static potential field map
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string pfPath = "Assets/Scenes/" + sceneName + "/Static PF-" + gameObject.name + ".asset";
        PotentialFieldScriptableObject pf = AssetDatabase.LoadAssetAtPath<PotentialFieldScriptableObject>(pfPath);
        if (pf == null) {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/" + sceneName))
                AssetDatabase.CreateFolder("Assets/Scenes", sceneName);
            pf = ScriptableObject.CreateInstance<PotentialFieldScriptableObject>();
            AssetDatabase.CreateAsset(pf, pfPath);
        }
        pf.Init(width, height);

        //----- Tile data load
        Dictionary<int, JSONNode> tilesetDataDictionary = new Dictionary<int, JSONNode>();
        Dictionary<int, JSONNode> tileObjectDataDictionary = new Dictionary<int, JSONNode>();

        Debug.Log("Loading data...");
        foreach (JSONNode tileset in n["tilesets"].AsArray) {
            int firstId = tileset["firstgid"].AsInt;
            string source = tileset["source"];
            string tilePath = "Assets/Other/Tiled/" + source;

            TextAsset tilesetJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(tilePath);
            JSONNode tilesetData = JSON.Parse(tilesetJSON.text);
            tilesetDataDictionary.Add(firstId, tilesetData);
            for (int i = 0; i < tilesetData["tilecount"].AsInt; i++) {
                JSONNode tileNode = tilesetData["tileproperties"]["" + i];
                if (tileNode.IsObject) {
                    JSONNode tileObjectData = tileNode;
                    tileObjectDataDictionary.Add(i + firstId, tileObjectData);
                }
            }
        }

        //----- Creating base root map object on scene
        Debug.Log("Loading map '" + mapName + "'...");
        GameObject prev = GameObject.Find(mapName);
        if (prev) {
            Debug.Log("Removing previous map object...");
            Undo.DestroyObjectImmediate(prev);
        }
        GameObject rootObject = new GameObject(mapName);

        //----- Popullating map
        foreach (JSONNode layer in n["layers"].AsArray) {
            float tileY = layer["properties"]["unityHeight"].AsFloat;
            bool levelMapCheck = layer["properties"]["pathLayer"].AsBool;

            //------- Tiled Ground layer
            for (int i = 0; i < layer["data"].Count; i++) {
                Vector3 tilePosition = Vector3.zero;
                tilePosition.x = i % width;
                tilePosition.z = -i / width;
                tilePosition.y = tileY;
                int id = layer["data"][i];
                if (id != 0) {
                    JSONNode tileObjectData;
                    if (tileObjectDataDictionary.TryGetValue(id, out tileObjectData)) {
                        string unityObject = tileObjectData["unityObject"];
                        if (unityObject != null) {
                            GameObject prefab = InstantiatePrefab(unityObject);

                            if (prefab) {
                                prefab.transform.position = tilePosition;
                                prefab.transform.parent = rootObject.transform;
                            }
                        }

                        // Layer modifies static potential field?
                        if (levelMapCheck) {
                            bool walkable = tileObjectData["walkable"];
                            int pX = i % pf.Width;
                            int pY = i / pf.Width;
                            if (!walkable) {
                                pf.AddLinearForce(pX, pY, -6, 3);
                                pf.SetCellValue(pX, pY, PotentialFieldScriptableObject.BLOCKED);
                            }
                        }
                    }
                }
            }

            //------ Tiled Object layer
            for (int i = 0; i < layer["objects"].Count; i++) {
                JSONNode tileObject = layer["objects"][i];
                Vector3 tilePosition = Vector3.zero;
                tilePosition.x = tileObject["x"].AsInt / tileWidth;
                tilePosition.z = 1 - tileObject["y"].AsInt / tileHeight;
                tilePosition.y = tileY;
                int id = tileObject["gid"]; 
                JSONNode properties = tileObject["properties"];

                // By unity object
                if (id != 0) { 
                    JSONNode tileObjectData;
                    if (tileObjectDataDictionary.TryGetValue(id, out tileObjectData)) {
                        string unityObject = tileObjectData["unityObject"];
                        
                        Debug.Log("Creating "+unityObject);
                        if (unityObject != null) {
                            GameObject prefab = InstantiatePrefab(unityObject);
                            if (prefab) {
                                prefab.transform.position = tilePosition;
                                prefab.transform.parent = rootObject.transform;

                                // Set special object properties from tiled data
                                switch (unityObject) {
                                    case "endpointPrefab":
                                        prefab.GetComponent<EndpointController>().nextLevel = GetTiledProperty("nextLevel", properties, id, tileObjectDataDictionary);
                                        break;
                                    case "keyPrefab":
                                        prefab.GetComponent<KeyController>().outSignalId = GetTiledProperty("outSignal", properties, id, tileObjectDataDictionary).AsInt;
                                        break;
                                    case "doorPrefab":
                                        prefab.GetComponent<DoorController>().inSignalId = GetTiledProperty("inSignal", properties, id, tileObjectDataDictionary);
                                        prefab.GetComponent<DoorController>().isOpen = GetTiledProperty("open", properties, id, tileObjectDataDictionary);
                                        break;
                                    case "gatePrefab":
                                        prefab.GetComponent<DoorController>().inSignalId = GetTiledProperty("inSignal", properties, id, tileObjectDataDictionary).AsInt;
                                        prefab.GetComponent<DoorController>().isOpen = GetTiledProperty("open", properties, id, tileObjectDataDictionary).AsBool;
                                        break;
                                    case "dialogZonePrefab":
                                        prefab.GetComponent<DialogZoneController>().dialogCanvas.dialogTitle = GetTiledProperty("textTitle", properties, id, tileObjectDataDictionary);
                                        prefab.GetComponent<DialogZoneController>().dialogCanvas.dialogBody = GetTiledProperty("textBody", properties, id, tileObjectDataDictionary);
                                        break;
                                }

                                // Set object facing direction
                                if (GetTiledProperty("direction", properties, id, tileObjectDataDictionary) != null) {
                                    int dir = GetTiledProperty("direction", properties, id, tileObjectDataDictionary);// properties["direction"].AsInt;
                                    prefab.transform.rotation = Quaternion.AngleAxis(-dir * 90, Vector3.up);
                                }

                                // Check for walkable flag
                                bool walkable = GetTiledProperty("walkable", properties, id, tileObjectDataDictionary);
                                if (!walkable) {
                                    int range = GetTiledProperty("blockSize", properties, id, tileObjectDataDictionary)-1;
                                    Debug.Log("Trying " + (tileObject["x"].AsInt / tileWidth) + " " + tileObject["y"].AsInt / tileHeight);
                                    pf.SetSquareAreaValue(tileObject["x"].AsInt / tileWidth, (tileObject["y"].AsInt / tileHeight)-1, PotentialFieldScriptableObject.BLOCKED, range);
                                }
                            }
                        }
                    }
                } else {
                    // By types 
                    string type = tileObject["type"]; 
                    switch (type) {
                        case "Spawner": {
                            GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(spawnerPrefab);
                            tilePosition.z -= 1;
                            prefab.transform.position = tilePosition;
                            prefab.transform.parent = rootObject.transform; 
                            string enemy = properties["spawnObject"];
                        } break;
                        case "Enemy Check": {
                            GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemyDetectorPrefab);
                            Debug.Log("Width: " + tileObject["width"].AsFloat);
                            Debug.Log(tilePosition.x);
                            tilePosition.z -= 1;
                            tilePosition.x += (1f * tileObject["width"].AsFloat / 64) / 2 - 0.5f;
                            Debug.Log(tilePosition.x);
                            tilePosition.z -= (1f * tileObject["height"].AsFloat / 64) / 2 - 0.5f;
                            prefab.transform.position = tilePosition;
                            prefab.transform.localScale = new Vector3(1f * tileObject["width"].AsInt / 64, 1, 1f * tileObject["height"].AsInt / 64);
                            prefab.transform.parent = rootObject.transform;
                            EnemyDetectorController edc = prefab.GetComponent<EnemyDetectorController>();
                            edc.outSignalId = properties["outSignal"].AsInt;
                        } break;
                    }
                }
            }
        }

        // Set the static potential field reference
        GetComponent<LevelManager>().SetStaticPotentialField(pf);
        EditorUtility.SetDirty(pf);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Undo.RegisterCreatedObjectUndo(rootObject, "Created map");
#endif
    }

    public JSONNode GetTiledProperty(string propertyName, JSONNode localPropertiesNode, int gid, Dictionary<int, JSONNode> tileObjectDataDictionary) {
        if (!localPropertiesNode.IsNull && !localPropertiesNode[propertyName].IsNull && localPropertiesNode[propertyName].Value != "") {
            return localPropertiesNode[propertyName];
        }
        JSONNode tiledObjectData;
        if (tileObjectDataDictionary.TryGetValue(gid, out tiledObjectData)) {
            return tiledObjectData[propertyName];
        }
        return null;

    }

    /// <summary>
    /// Instantiate objects by their string id
    /// </summary>
    /// <param name="name">The string id of the object</param>
    /// <returns>The instantiated object</returns>
    private GameObject InstantiatePrefab(string name) {
#if (UNITY_EDITOR)
        GameObject prefab;
        switch (name) {
                // Level design
            case "wallPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
                break;
            case "groundPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
                break;
            case "pitPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(pitPrefab);
                break;
            case "treePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab);
                break;
            case "groundSpikesPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(groundSpikesPrefab);
                break;

                // Props
            case "bridgePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab);
                break;
            case "campfirePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(campfirePrefab);
                break;
            case "chairPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(chairPrefab);
                break;
            case "flowerPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(flowerPrefab);
                break;
            case "gatePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(gatePrefab);
                break;
            case "gravePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(gravePrefab);
                break;
            case "homePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(homePrefab);
                break;
            case "pot1Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(pot1Prefab);
                break;
            case "pot2Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(pot2Prefab);
                break;
            case "rockBig1Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(rockBig1Prefab);
                break;
            case "rockMedium1Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(rockMedium1Prefab);
                break;
            case "rockSmall1Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(rockSmall1Prefab);
                break;
            case "rockSmall2Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(rockSmall2Prefab);
                break;
            case "shopPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(shopPrefab);
                break;
            case "shroomsPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(shroomsPrefab);
                break;
            case "signalAdvicePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(signalAdvicePrefab);
                break;
            case "signalDirectionalPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(signalDirectionalPrefab);
                break;
            case "skullPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(skullPrefab);
                break;
            case "stoneColumnBrokenPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(stoneColumnBrokenPrefab);
                break;
            case "stoneColumnGroundPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(stoneColumnGroundPrefab);
                break;
            case "stoneColumnNormalPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(stoneColumnNormalPrefab);
                break;
            case "tablePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
                break;
            case "tree1Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(tree1Prefab);
                break;
            case "tree2Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(tree2Prefab);
                break;
            case "tree3Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(tree3Prefab);
                break;
            case "tree4Prefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(tree4Prefab);
                break;
            case "wellPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(wellPrefab);
                break;
            case "woodenDoorPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(woodenDoorPrefab);
                break;

                // Level 
            case "playerPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                break;
            case "endpointPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(endpointPrefab);
                break;
            case "dialogZonePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(dialogZonePrefab);
                break;
            case "spawnerPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(spawnerPrefab);
                break;

                // Objects
            case "doorPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
                break;
            case "keyPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(keyPrefab);
                break;

                // Enemies
            case "swordmanPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemySwordmanPrefab);
                break;
            case "lancerPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemyLancerPrefab);
                break;
            case "snakePrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemySnakePrefab);
                break;
            case "crossbowmanPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemyCrossbowmanPrefab);
                break;
            case "enemyHammermanPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemyHammermanPrefab);
                break;
            case "enemySlimeBigPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemySlimeBigPrefab);
                break;
            case "enemySlimeNormalPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemySlimeNormalPrefab);
                break;
            case "enemySlimeSmallPrefab":
                prefab = (GameObject)PrefabUtility.InstantiatePrefab(enemySlimeSmallPrefab);
                break;

            default :
                prefab = null;
                break;

        }
        return prefab;
#else
        return null;
#endif
    }
}
