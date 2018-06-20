using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Used as a way to store the reference of the BaseEnemy in a component in the object with the AI FSM animator.
/// </summary>
public class AIProxy : MonoBehaviour {
    public BaseEnemy owner { get; set; }
}
