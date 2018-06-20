using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains information about GameObjects that can be created when an enemy dies.
/// A DropChoice is a list of objects to be created asociated with a chance value.
/// When choosing a random DropChoice, the chances of one DropChoice is weighted on its value in comparison with the others.
/// </summary>
[CreateAssetMenu(fileName = "Drop List", menuName = "Enemies/Drop List")]
public class DropListScriptableObject : ScriptableObject {

    // Drop choice struct definition
    [System.Serializable]
    public struct DropChoice {
        public float chance;
        public GameObject[] drops;
    }

    // Drop choice list
    public DropChoice[] dropChoices;

    /// <summary>
    /// Get a random DropChoice based on the DropChoice list and their respective weight values
    /// </summary>
    /// <returns>The chosen DropChoice</returns>
    public DropChoice GetRandomChoice() {
        // Check for empty drop choice list
        if (dropChoices.Length == 0) {
            return Empty;
        }

        // Sum weight values
        float sum = 0;
        for (int i = 0; i < dropChoices.Length; i++)
            sum += dropChoices[i].chance;

        // Make the choice
        float r = Random.value * sum;

        // Find the choice according to the value
        sum = 0;
        for (int i = 0; i < dropChoices.Length; i++) {
            if (i + 1 >= dropChoices.Length || r < dropChoices[i].chance + sum) {
                return dropChoices[i];
            }
            sum += dropChoices[i].chance;
        }

        // Invalid weight configuration (Negative?)
        Debug.LogError("Invalid weight configuration! " + this + " " + dropChoices);
        return Empty;
    }

    // Define an Empty drop choice
    public DropChoice Empty {
        get {
            DropChoice d = new DropChoice();
            d.chance = 0;
            d.drops = new GameObject[0];
            return d;
        }
    }
}
