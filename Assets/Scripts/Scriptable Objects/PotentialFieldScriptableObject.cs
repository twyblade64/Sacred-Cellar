using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to store the potential field data of levels
/// </summary>
public class PotentialFieldScriptableObject : ScriptableObject {
    // Blocked path flag
    public const int BLOCKED = short.MinValue;

    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private int[] data;

    public int Width {
        get {
            return width;
        }
    }

    public int Height {
        get {
            return height;
        }
    }

    public int[] Data {
        get {
            return data;
        }
        set {
            data = value;
        }
    }

    /// <summary>
    /// Initiate the potential field and its values.
    /// </summary>
    /// <param name="width">Width of the potential field</param>
    /// <param name="height">Height of the potential field</param>
    public void Init(int width, int height) {
        this.width = width;
        this.height = height;
        data = new int[width * height];
        Clear(0);
    }

    /// <summary>
    /// Clear all the spaces of the potential field to a single value
    /// </summary>
    /// <param name="value">The value to clear to</param>
    public void Clear(int value) {
        for (int i = 0; i < width * height; i++)
            data[i] = value;
    }

    /// <summary>
    /// Map a world position to its X coordinate in the potential field
    /// </summary>
    /// <param name="position">The world position</param>
    /// <returns>The x coordinate in the potential field</returns>
    public int LocalPosX(Vector3 position) {
        return Mathf.FloorToInt(position.x + .5f);
    }

    /// <summary>
    /// Map a world position to its y coordinate in the potential field
    /// </summary>
    /// <param name="position">The world position</param>
    /// <returns>The y coordinate in the potential field</returns>
    public int LocalPosY(Vector3 position) {
        return -Mathf.FloorToInt(position.z + .5f);
    }

    /// <summary>
    /// Get the cell value of the potential field's cell.
    /// Accessing to a cell outside of the potential field returns a BLOCKED flag value.
    /// </summary>
    /// <param name="x">The cell's x coordinate</param>
    /// <param name="y">The cell's y coordinate</param>
    /// <returns>The value of the cell</returns>
    public int GetCellValue(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y > height)
            return BLOCKED;
        return data[x + y * width];
    }

    /// <summary>
    /// Set the value of a potential field's cell
    /// Setting the value of a cell outside of the potential field does nothing.
    /// </summary>
    /// <param name="x">The cell's x coordinate</param>
    /// <param name="y">The cell's y coordinate</param>
    /// <param name="v">The value of the cell</param>
    public void SetCellValue(int x, int y, int v) {
        if (x < 0 || x >= width || y < 0 || y > height)
            return;
        data[x + y * width] = v;
    }

    /// <summary>
    /// Add to the value of a potential field's cell
    /// Adding to the value of a cell outside of the potential field does nothing.
    /// </summary>
    /// <param name="x">The cell's x coordinate</param>
    /// <param name="y">The cell's y coordinate</param>
    /// <param name="v">The value to add to the cell</param>
    public void AddCellValue(int x, int y, int v) {
        if (x < 0 || x >= width || y < 0 || y > height)
            return;
        data[x + y * width] += v;
    }

    /// <summary>
    /// Generates a force from a cell that adds to all affected cells in range and decays linearly using the manhattan distance.
    /// </summary>
    /// <param name="pX">The x coordinate of the center of the force</param>
    /// <param name="pY">The y coordinate of the center of the force</param>
    /// <param name="force">The force to be applied</param>
    /// <param name="range"> The range of the force</param>
    public void AddLinearForce(int pX, int pY, int force, float range) {
        for (int i = 0; i < Width * Height; i++) {
            int x = i % Width;
            int y = i / Width;
            float dist = Mathf.Abs(x - pX) + Mathf.Abs(y - pY);
            if (dist <= range) {
                int distForce = (int)(force * (1 - dist / range));
                data[i] += distForce;
            }
        }
    }

    /// <summary>
    /// Sets the value of all cells in a square region.
    /// </summary>
    /// <param name="pX">The x coordinate of the center of the region</param>
    /// <param name="pY">The y coordinate of the center of the region</param>
    /// <param name="force">The value to be set</param>
    /// <param name="range"> The range half-extension of the square</param>
    public void SetSquareAreaValue(int pX, int pY, int force, float range) {
        for (int i = 0; i < Width * Height; i++) {
            int x = i % Width;
            int y = i / Width;
            float dist = Mathf.Max(Mathf.Abs(x - pX), Mathf.Abs(y - pY));
            if (dist <= range) {
                data[i] = force;
            }
        }
    }
}
