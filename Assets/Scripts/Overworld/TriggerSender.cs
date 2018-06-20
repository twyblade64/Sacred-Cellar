using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class recieves trigger-events callbacks and sends the information to the parent with the ITriggerReciever.
/// Useful when placing triggers on child objects but keeping the logic in parent
/// </summary>
public class TriggerSender : MonoBehaviour {
    private ITriggerReciever reciever;

    void Awake() {
        reciever = GetComponentInParent<ITriggerReciever>();
    }

    public void OnTriggerEnter(Collider other) {
        reciever.OnExtensionTriggerEnter(gameObject, other);
    }

    public void OnTriggerStay(Collider other) {
        reciever.OnExtensionTriggerStay(gameObject, other);
    }

    public void OnTriggerExit(Collider other) {
        reciever.OnExtensionTriggerExit(gameObject, other);
    }

    public ITriggerReciever GetReciever() {
        return reciever;
    }
}
