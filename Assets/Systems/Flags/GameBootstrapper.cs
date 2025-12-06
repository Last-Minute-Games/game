using UnityEngine;

/// <summary>
/// Ensures critical game systems are initialized at startup.
/// Attach this to a GameObject in your first scene, or it will auto-create itself.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Force GameFlags to initialize before any scene loads
        var flags = GameFlags.Instance;
        Debug.Log("[GameBootstrapper] GameFlags initialized at startup");
    }
}
