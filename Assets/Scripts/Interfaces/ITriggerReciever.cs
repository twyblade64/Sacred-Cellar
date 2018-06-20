using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for external trigger-event reception on GameObjects.
/// </summary>
public interface ITriggerReciever {
    /// <summary>
    /// Called on OnTriggerEnter call from origin GameObject.
    /// </summary>
    /// <param name="origin">GameObject on which OnTriggerEnter method was called.</param>
    /// <param name="other">Collider data of the OnTriggerEnter method.</param>
    void OnExtensionTriggerEnter(GameObject origin, Collider other);

    /// <summary>
    /// Called on OnTriggerEnter call from origin GameObject.
    /// </summary>
    /// <param name="origin">GameObject on which OnTriggerStay method was called.</param>
    /// <param name="other">Collider data of the OnTriggerStay method.</param>
    void OnExtensionTriggerStay(GameObject origin, Collider other);

    /// <summary>
    /// Called on OnTriggerEnter call from origin GameObject.
    /// </summary>
    /// <param name="origin">GameObject on which OnTriggerExit method was called.</param>
    /// <param name="other">Collider data of the OnTriggerExit method.</param>
    void OnExtensionTriggerExit(GameObject origin, Collider other);

    /// <summary>
    /// Access to the GameObject owner of the interface.
    /// </summary>
    /// <returns>The GameObject asociated to the interface.</returns>
    GameObject GetRecieverGameObject();
}
