using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-9999)]
public class ForceRebuild2DShadows : MonoBehaviour
{
    void Awake() { StartCoroutine(RebuildNextFrame()); }

    IEnumerator RebuildNextFrame()
    {
        yield return null; // wait 1 frame so URP is alive

        int c = 0, l = 0, g = 0;

        foreach (var sc in FindObjectsOfType<ShadowCaster2D>(true))
        { bool e = sc.enabled; sc.enabled = false; sc.enabled = e; c++; }

        foreach (var grp in FindObjectsOfType<ShadowCasterGroup2D>(true))
        { bool e = grp.enabled; grp.enabled = false; grp.enabled = e; g++; }

        foreach (var li in FindObjectsOfType<Light2D>(true))
        { bool e = li.enabled; li.enabled = false; li.enabled = e; l++; }

        Debug.Log($"[ForceRebuild2DShadows] casters={c}, groups={g}, lights={l} (kicked)");
    }
}
