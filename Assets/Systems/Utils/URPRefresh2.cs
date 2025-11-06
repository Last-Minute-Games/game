using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-9999)]
public class ForceRebuild2DShadows : MonoBehaviour
{
    void Awake()
    {
        // 1) Re-register all ShadowCaster2D components
        var casters = FindObjectsOfType<ShadowCaster2D>(true);
        foreach (var c in casters)
        {
            // toggle to force OnDisable/OnEnable registration cycle
            bool wasEnabled = c.enabled;
            c.enabled = false;
            c.enabled = wasEnabled; // restores original state
        }

        // 2) Re-register all Light2D components (esp. Global lights)
        var lights = FindObjectsOfType<Light2D>(true);
        foreach (var l in lights)
        {
            bool wasEnabled = l.enabled;
            l.enabled = false;
            l.enabled = wasEnabled;
        }
    }
}
