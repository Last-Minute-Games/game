using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-9998)]
public class KickCameraRenderer : MonoBehaviour
{
    void Awake()
    {
        var cam = Camera.main; if (!cam) return;
        var data = cam.GetUniversalAdditionalCameraData(); if (data == null) return;

        int fallbackIndex = 0;              // your 2D renderer is almost always index 0
        data.SetRenderer(fallbackIndex);    // force a concrete renderer
        data.SetRenderer(-1);               // -1 == Default (back to normal)
        Debug.Log("[KickCameraRenderer] Flipped renderer to force rebind");
    }
}
