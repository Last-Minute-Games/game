#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class Light2DGlobalDeduper
{
    private struct Key
    {
        public string Layer;
        public int Blend;
        public Key(string layer, int blend)
        {
            Layer = layer;
            Blend = blend;
        }
        public override string ToString() => $"Layer='{Layer}', BlendStyle={Blend}";
    }

    // ───────────────────────────────────────────────
    // MENU 1: List duplicates (read-only)
    // ───────────────────────────────────────────────
    [MenuItem("Tools/URP 2D/List Duplicate Global Lights")]
    public static void ListDupes()
    {
        var groups = GroupGlobalLights();
        if (groups == null)
        {
            Debug.Log("[URP2D] No Global Light2D found.");
            return;
        }

        int dupCount = 0;
        foreach (var kv in groups)
        {
            var lights = kv.Value;
            if (kv.Key.Blend == 0 && lights.Count > 1)
            {
                dupCount++;
                Debug.LogWarning(
                    $"[URP2D] Duplicate Global Lights on {kv.Key}:\n" +
                    string.Join("\n", lights.Select(l => $" - {l.gameObject.scene.name}/{l.gameObject.name}")),
                    lights[0]
                );
            }
        }

        if (dupCount == 0)
            Debug.Log("[URP2D] No duplicates for BlendStyle=0 found. ✅");
        else
            Debug.LogWarning($"[URP2D] Found {dupCount} duplicate group(s). Fix by keeping one per layer for BlendStyle=0.");
    }

    // ───────────────────────────────────────────────
    // MENU 2: Auto-Fix duplicates (keep first, disable extras)
    // ───────────────────────────────────────────────
    [MenuItem("Tools/URP 2D/Auto-Fix Duplicates (Keep First)")]
    public static void AutoFixKeepFirst()
    {
        var groups = GroupGlobalLights();
        if (groups == null)
        {
            Debug.Log("[URP2D] No Global Light2D found.");
            return;
        }

        int changes = 0;
        Undo.IncrementCurrentGroup();
        foreach (var kv in groups)
        {
            if (kv.Key.Blend != 0)
                continue;

            var lights = kv.Value;
            if (lights.Count <= 1)
                continue;

            bool firstKept = false;
            foreach (var l in lights)
            {
                Undo.RecordObject(l, "Auto-Fix Global Light Duplicates");
                if (!firstKept && l.enabled)
                {
                    firstKept = true;
                    continue;
                }
                l.enabled = false;
                changes++;
            }
        }

        if (changes == 0)
            Debug.Log("[URP2D] No changes required. ✅");
        else
            Debug.LogWarning($"[URP2D] Disabled {changes} extra Global Light(s). Review and save your scene(s).");
    }

    // ───────────────────────────────────────────────
    // MENU 3: Auto-Retarget duplicates (move extras to BlendStyle=1)
    // ───────────────────────────────────────────────
    [MenuItem("Tools/URP 2D/Auto-Retarget Extras to Blend Style 1")]
    public static void AutoRetargetExtrasToBlend1()
    {
        var groups = GroupGlobalLights();
        if (groups == null)
        {
            Debug.Log("[URP2D] No Global Light2D found.");
            return;
        }

        int changes = 0;
        Undo.IncrementCurrentGroup();
        foreach (var kv in groups)
        {
            if (kv.Key.Blend != 0)
                continue;

            var lights = kv.Value;
            if (lights.Count <= 1)
                continue;

            bool firstKept = false;
            foreach (var l in lights)
            {
                if (!firstKept && l.enabled)
                {
                    firstKept = true;
                    continue;
                }

                Undo.RecordObject(l, "Auto-Retarget Global Light");
                l.blendStyleIndex = 1;
                changes++;
            }
        }

        if (changes == 0)
            Debug.Log("[URP2D] No changes required. ✅");
        else
            Debug.LogWarning($"[URP2D] Moved {changes} extra Global Light(s) to BlendStyle=1. Review and save your scene(s).");
    }

    // ───────────────────────────────────────────────
    // INTERNAL: Group Global Lights by (Layer, BlendStyle)
    // ───────────────────────────────────────────────
    private static Dictionary<Key, List<Light2D>> GroupGlobalLights()
    {
        var all = Object.FindObjectsOfType<Light2D>(true)
            .Where(l => l.lightType == Light2D.LightType.Global)
            .ToList();

        if (all.Count == 0)
            return null;

        var dict = new Dictionary<Key, List<Light2D>>();

        foreach (var l in all)
        {
            var targetLayers = (l.targetSortingLayers != null && l.targetSortingLayers.Length > 0)
                ? l.targetSortingLayers.Select(SortingLayer.IDToName)
                : new[] { "Default" };

            foreach (var layerName in targetLayers)
            {
                var name = string.IsNullOrEmpty(layerName) ? "Default" : layerName;
                var key = new Key(name, l.blendStyleIndex);

                if (!dict.TryGetValue(key, out var list))
                {
                    list = new List<Light2D>();
                    dict[key] = list;
                }

                list.Add(l);
            }
        }

        return dict;
    }
}
#endif
