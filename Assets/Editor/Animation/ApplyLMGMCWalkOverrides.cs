using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ApplyLMGMCWalkOverrides
{
    private const string LmgmcAssetPath = "Assets/Sprites/AnimationClips/Characters/newMCAnimation/LMGMC.aseprite";
    private const string NikolausOverridePath = "Assets/Sprites/AnimationClips/Characters/Nikolaus/Nikolaus.overrideController";

    private const string BaseLeftClipName = "MainCharacter_WalkAnimation_Left";
    private const string BaseRightClipName = "MainCharacter_WalkAnimation_Right";

    private const string DesiredLeftWalkClipName = "SideWalk";
    private const string DesiredRightWalkClipName = "rightwalk";

    private const string AutoApplySessionKey = "ApplyLMGMCWalkOverrides.AutoApplied";

    [InitializeOnLoadMethod]
    private static void AutoApplyOnEditorLoad()
    {
        if (SessionState.GetBool(AutoApplySessionKey, false))
        {
            return;
        }

        SessionState.SetBool(AutoApplySessionKey, true);
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ApplyInternal(logPrefix: "[ApplyLMGMCWalkOverrides/AUTO]");
        };
    }

    [MenuItem("Tools/Animation/Apply LMGMC Side Walk Overrides")]
    public static void ApplyFromMenu()
    {
        ApplyInternal(logPrefix: "[ApplyLMGMCWalkOverrides/Menu]");
    }

    [MenuItem("Tools/Animation/Apply LMGMC Side Walk Overrides", true)]
    public static bool ValidateApplyFromMenu()
    {
        return AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(NikolausOverridePath) != null;
    }

    // Entry point for Unity -executeMethod in batch mode.
    public static void ApplyFromCommandLine()
    {
        ApplyInternal(logPrefix: "[ApplyLMGMCWalkOverrides/CLI]");
    }

    [MenuItem("Tools/Animation/Print LMGMC Clip Names")]
    public static void PrintClipNames()
    {
        var clips = LoadLmgmcClips();
        if (clips.Count == 0)
        {
            Debug.LogError("[ApplyLMGMCWalkOverrides] No AnimationClip sub-assets were found in LMGMC.aseprite.");
            return;
        }

        Debug.Log($"[ApplyLMGMCWalkOverrides] Found {clips.Count} clips in {LmgmcAssetPath}:");
        foreach (var clip in clips.OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase))
        {
            Debug.Log($"[ApplyLMGMCWalkOverrides] clip: {DescribeClip(clip)}");
        }
    }

    private static void ApplyInternal(string logPrefix)
    {
        var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(NikolausOverridePath);
        if (overrideController == null)
        {
            Debug.LogError($"{logPrefix} Could not load override controller at {NikolausOverridePath}.");
            return;
        }

        var lmgmcClips = LoadLmgmcClips();
        if (lmgmcClips.Count == 0)
        {
            Debug.LogError($"{logPrefix} No AnimationClip sub-assets found in {LmgmcAssetPath}.");
            return;
        }

        var leftWalkClip = ResolveClip(lmgmcClips, DesiredLeftWalkClipName);
        var rightWalkClip = ResolveClip(lmgmcClips, DesiredRightWalkClipName);

        if (leftWalkClip == null || rightWalkClip == null)
        {
            Debug.LogError($"{logPrefix} Failed to resolve target clips. Resolved left: {leftWalkClip?.name ?? "<null>"}, right: {rightWalkClip?.name ?? "<null>"}.");
            Debug.LogError($"{logPrefix} Use Tools/Animation/Print LMGMC Clip Names to inspect available names.");
            return;
        }

        Debug.Log($"{logPrefix} Resolved clips. Left={DescribeClip(leftWalkClip)} Right={DescribeClip(rightWalkClip)}");

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        var baseClipNames = overrides
            .Select(o => o.Key != null ? o.Key.name : "<null>")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var leftApplied = false;
        var rightApplied = false;

        for (var i = 0; i < overrides.Count; i++)
        {
            var baseClip = overrides[i].Key;
            if (baseClip == null)
            {
                continue;
            }

            if (string.Equals(baseClip.name, BaseLeftClipName, StringComparison.Ordinal))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, leftWalkClip);
                leftApplied = true;
                continue;
            }

            if (string.Equals(baseClip.name, BaseRightClipName, StringComparison.Ordinal))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, rightWalkClip);
                rightApplied = true;
            }
        }

        if (!leftApplied || !rightApplied)
        {
            var available = string.Join(", ", baseClipNames);
            Debug.LogError($"{logPrefix} Failed to apply overrides. leftApplied={leftApplied}, rightApplied={rightApplied}. Available base clips: {available}");
            return;
        }

        overrideController.ApplyOverrides(overrides);
        EditorUtility.SetDirty(overrideController);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"{logPrefix} Applied Nikolaus side-walk overrides successfully. Left={leftWalkClip.name}, Right={rightWalkClip.name}");
    }

    private static List<AnimationClip> LoadLmgmcClips()
    {
        return AssetDatabase
            .LoadAllAssetsAtPath(LmgmcAssetPath)
            .OfType<AnimationClip>()
            .ToList();
    }

    private static AnimationClip ResolveClip(IEnumerable<AnimationClip> clips, string targetName)
    {
        var exactMatch = clips.FirstOrDefault(c => string.Equals(c.name, targetName, StringComparison.Ordinal));
        if (exactMatch != null)
        {
            return exactMatch;
        }

        var caseInsensitiveMatch = clips.FirstOrDefault(c => string.Equals(c.name, targetName, StringComparison.OrdinalIgnoreCase));
        if (caseInsensitiveMatch != null)
        {
            return caseInsensitiveMatch;
        }

        var containsMatches = clips
            .Where(c => c.name.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (containsMatches.Count == 1)
        {
            return containsMatches[0];
        }

        if (containsMatches.Count > 1)
        {
            var options = string.Join(", ", containsMatches.Select(c => c.name));
            Debug.LogWarning($"[ApplyLMGMCWalkOverrides] Multiple clips matched '{targetName}': {options}");
        }

        return null;
    }

    private static string DescribeClip(AnimationClip clip)
    {
        if (clip == null)
        {
            return "<null>";
        }

        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out var guid, out long localId))
        {
            return $"{clip.name} (guid:{guid}, fileID:{localId})";
        }

        return clip.name;
    }
}