using UnityEngine;

public class InitialPositionn : MonoBehaviour
{
    private Vector3 savedPosition;
    private bool hasSavedPosition = false;

    // Call this when you enter the minigame
    public void SaveCurrentPosition()
    {
        savedPosition = transform.position;
        hasSavedPosition = true;
    }

    // Call this when the minigame ends
    public void RestorePosition()
    {
        if (!hasSavedPosition) return;
        transform.position = savedPosition;
    }
}
