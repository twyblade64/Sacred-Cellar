using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface definition for damagable entities
/// 
/// The damage system uses a bitmask enum to asign which faction an entity belongs to and which factions they can damage with their attacks.
/// </summary>
public interface IDamagable {
    /// <summary>
    /// Get the entity faction.
    /// </summary>
    /// <returns>The faction of the entity</returns>
    FactionManager.Faction GetFaction();

    /// <summary>
    /// The entity takes damage from an unknown source.
    /// </summary>
    /// <param name="damage">Ammount of damage to be taken.</param>
    /// <param name="direction">Incoming damage direction. Axis represented in clockwise order: 0-Forward, 1-Left, 2-Back, 3-Right.</param>
    /// <param name="pushForce">Force applied to the entity in the specified direction.</param>
    /// <param name="stunForce">Stun duration applied to the entity.</param>
    /// <returns>True if enemy was damaged, otherwise false.</returns>
    bool Damage(float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0);

    /// <summary>
    /// The entity takes damage from a known source.
    /// </summary>
    /// <param name="source">The source GameObject damaging the entity.</param>
    /// <param name="reciever">The entity collider involved in the collision.</param>
    /// <param name="damage">Ammount of damage to be taken.</param>
    /// <param name="direction">Incoming damage direction. Axis represented in clockwise order: 0-Forward, 1-Left, 2-Back, 3-Right.</param>
    /// <param name="pushForce">Force applied to the entity in the specified direction.</param>
    /// <param name="stunForce">Stun duration applied to the entity.</param>
    /// <returns>True if enemy was damaged, otherwise false.</returns>
    bool RecieveAttack(GameObject source, Collider reciever, float damage = 0, int direction = 0, float pushForce = 0, float stunForce = 0);
}
