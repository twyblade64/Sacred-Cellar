using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Body sections of the snake enemy.
/// </summary>
public class SnakeSectionController : MonoBehaviour {
    public SnakeController master;
    public float trailOffset;
	
    /// <summary>
    /// Init the body section.
    /// </summary>
    /// <param name="master">Th snake owner of this body section.</param>
    /// <param name="trailOffset">The offset at which the section is. From 0 to 1.</param>
    public void Setup(SnakeController master, float trailOffset) {
        this.master = master;
        this.trailOffset = trailOffset;
    }

    /// <summary>
    /// Update the position of the body section.
    /// </summary>
    public void UpdatePosition() {
        transform.position = master.GetTrailOffsetPosition(trailOffset);
    }
}
