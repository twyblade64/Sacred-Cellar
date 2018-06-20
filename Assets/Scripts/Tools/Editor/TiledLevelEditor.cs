using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TiledLevelBuilder))]
public class TiledLevelEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        TiledLevelBuilder tlb = (TiledLevelBuilder)target;
        if (GUILayout.Button("Generate level")) {
            tlb.BuildLevelPrefab();
        }
    }
}
