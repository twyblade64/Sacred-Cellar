using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bitmask enum container for identification of different objects
/// Used in the damage system
/// </summary>
public class FactionManager : MonoBehaviour{
    [System.Flags]
    public enum Faction {
        Player  = 1 << 0,
        Enemy   = 1 << 1,
        Enviorment = 1 << 2
    };
}
