using UnityEngine;

/// <summary>
/// Simple script attached to objects that need to be returned to their starting point
/// when the level is reset (e.g., Boxes and optionally the Player).
/// </summary>
public class InitialPosition : MonoBehaviour
{
    private Vector3 initialPosition;

    void Awake()
    {
        // Save the current position when the object is initialized (on scene load)
        initialPosition = transform.position;
    }

    /// <summary>
    /// Moves the object back to its saved starting position.
    /// Called by the MinigameController's ResetPuzzle() function.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = initialPosition;
    }
}
