using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone where a dialog canvas is shown when player steps into
/// </summary>
public class DialogZoneController : MonoBehaviour {
    // The dialog canvas to show
    public DialogCanvasController dialogCanvas;

    // Player reference
    private PlayerController player;

    public void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    /// <summary>
    /// Shows canvas when player steps into the trigger
    /// </summary>
    public void OnTriggerEnter(Collider other) {
        TriggerSender hitbox = other.GetComponent<TriggerSender>();
        if (hitbox != null && hitbox.GetReciever().GetRecieverGameObject().Equals(player.gameObject) && player.bodyHitbox.Equals(other.gameObject)) {
            Debug.Log("Open");
            dialogCanvas.Open();
        }
    }

    /// <summary>
    /// Hides canvas when player steps out of the trigger
    /// </summary>
    public void OnTriggerExit(Collider other) {
        TriggerSender hitbox = other.GetComponent<TriggerSender>();
        if (hitbox != null && hitbox.GetReciever().GetRecieverGameObject().Equals(player.gameObject) && player.bodyHitbox.Equals(other.gameObject)) {
            Debug.Log("Close");
            dialogCanvas.Close();
        }
    }
}
