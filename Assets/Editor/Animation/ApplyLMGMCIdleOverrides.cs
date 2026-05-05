using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ApplyLMGMCIdleOverrides
{
    private const string LmgmcAssetPath = "Assets/Sprites/AnimationClips/Characters/newMCAnimation/LMGMC.aseprite";
    private const string NikolausOverridePath = "Assets/Sprites/AnimationClips/Characters/Nikolaus/Nikolaus.overrideController";
    private const string IdleOverridePath = "Assets/Resources/Animation/NikolausIdle.overrideController";

    private const string BaseDownClipName = "MainCharacter_WalkAnimation_Down";
    private const string BaseLeftClipName = "MainCharacter_WalkAnimation_Left";
    private const string BaseRightClipName = "MainCharacter_WalkAnimation_Right";
    private const string BaseUpClipName = "MainCharacter_WalkAnimation_Up";

    private static readonly string[] FrontIdleCandidates = { "Front_Idle", "FrontIdle", "FrontRegularIdle" };
    private static readonly string[] SideIdleCandidates = { "SideIdle", "Side_Idle", "SideRedIdle" };
    private static readonly string[] BackIdleCandidates = { "BackIdle", "Back_Idle" };

    private const string AutoApplySessionKey = "ApplyLMGMCIdleOverrides.AutoApplied";

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

            ApplyInternal(logPrefix: "[ApplyLMGMCIdleOverrides/AUTO]");
        };
    }

    [MenuItem("Tools/Animation/Apply LMGMC Idle Overrides")]
    public static void ApplyFromMenu()
    {
        ApplyInternal(logPrefix: "[ApplyLMGMCIdleOverrides/Menu]");
    }

    [MenuItem("Tools/Animation/Apply LMGMC Idle Overrides", true)]
    public static bool ValidateApplyFromMenu()
    {
        return AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(NikolausOverridePath) != null;
    }

    [MenuItem("Tools/Animation/Print LMGMC Idle Clip Names")]
    public static void PrintIdleClipNames()
    {
        var clips = LoadLmgmcClips();
        if (clips.Count == 0)
        {
            Debug.LogError($"[ApplyLMGMCIdleOverrides] No AnimationClip sub-assets were found in {LmgmcAssetPath}.");
            return;
        }

        Debug.Log($"[ApplyLMGMCIdleOverrides] Found {clips.Count} clips in {LmgmcAssetPath}:");
        foreach (var clip in clips.OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase))
        {
            Debug.Log($"[ApplyLMGMCIdleOverrides] clip: {DescribeClip(clip)}");
        }

        var frontIdle = ResolveClipByCandidates(clips, FrontIdleCandidates);
        var sideIdle = ResolveClipByCandidates(clips, SideIdleCandidates);
        var backIdle = ResolveClipByCandidates(clips, BackIdleCandidates);

        Debug.Log($"[ApplyLMGMCIdleOverrides] Resolved candidates Front={DescribeClip(frontIdle)} Side={DescribeClip(sideIdle)} Back={DescribeClip(backIdle)}");
    }

    private static void ApplyInternal(string logPrefix)
    {
        var nikolausOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(NikolausOverridePath);
        if (nikolausOverride == null)
        {
            Debug.LogError($"{logPrefix} Could not load override controller at {NikolausOverridePath}.");
            return;
        }

        var baseRuntimeController = nikolausOverride.runtimeAnimatorController;
        if (baseRuntimeController == null)
        {
            Debug.LogError($"{logPrefix} Nikolaus override runtime controller is null at {NikolausOverridePath}.");
            return;
        }

        var lmgmcClips = LoadLmgmcClips();
        if (lmgmcClips.Count == 0)
        {
            Debug.LogError($"{logPrefix} No AnimationClip sub-assets found in {LmgmcAssetPath}.");
            return;
        }

        var frontIdle = ResolveClipByCandidates(lmgmcClips, FrontIdleCandidates);
        var sideIdle = ResolveClipByCandidates(lmgmcClips, SideIdleCandidates);
        var backIdle = ResolveClipByCandidates(lmgmcClips, BackIdleCandidates);

        if (frontIdle == null || sideIdle == null || backIdle == null)
        {
            Debug.LogError($"{logPrefix} Failed to resolve one or more idle clips. Front={frontIdle?.name ?? "<null>"} Side={sideIdle?.name ?? "<null>"} Back={backIdle?.name ?? "<null>"}");
            Debug.LogError($"{logPrefix} Use Tools/Animation/Print LMGMC Idle Clip Names to inspect available names.");
            return;
        }

        Debug.Log($"{logPrefix} Resolved idle clips Front={DescribeClip(frontIdle)} Side={DescribeClip(sideIdle)} Back={DescribeClip(backIdle)}");

        var idleOverride = LoadOrCreateIdleOverride(baseRuntimeController, logPrefix);
        if (idleOverride == null)
        {
            return;
        }

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        idleOverride.GetOverrides(overrides);

        var downApplied = false;
        var leftApplied = false;
        var rightApplied = false;
        var upApplied = false;

        for (var i = 0; i < overrides.Count; i++)
        {
            var baseClip = overrides[i].Key;
            if (baseClip == null)
            {
                continue;
            }

            if (string.Equals(baseClip.name, BaseDownClipName, StringComparison.Ordinal))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, frontIdle);
                downApplied = true;
                continue;
            }

            if (string.Equals(baseClip.name, BaseLeftClipName, StringComparison.Ordinal))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, sideIdle);
                leftApplied = true;
                continue;
            }

            if (string.Equals(baseClip.name, BaseRightClipName, StringComparison.Ordinal))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, sideIdle);
                rightApplied = true;
                continue;
            }

            if (string.Equals(baseClip.name, BaseUpClipName, StringComparison.Ordinal))
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(baseClip, backIdle);
                upApplied = true;
            }
        }

        if (!downApplied || !leftApplied || !rightApplied || !upApplied)
        {
            var availableBaseClips = string.Join(
                ", ",
                overrides
                    .Where(o => o.Key != null)
                    .Select(o => o.Key.name)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

            Debug.LogError($"{logPrefix} Failed to apply all required idle mappings. down={downApplied}, left={leftApplied}, right={rightApplied}, up={upApplied}. Available base clips: {availableBaseClips}");
            return;
        }

        idleOverride.ApplyOverrides(overrides);
        EditorUtility.SetDirty(idleOverride);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"{logPrefix} Applied idle overrides successfully. Front={DescribeClip(frontIdle)} Side={DescribeClip(sideIdle)} Back={DescribeClip(backIdle)}");
    }

    private static AnimatorOverrideController LoadOrCreateIdleOverride(RuntimeAnimatorController baseRuntimeController, string logPrefix)
    {
        EnsureFoldersForAssetPath(IdleOverridePath);

        var idleOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(IdleOverridePath);
        if (idleOverride == null)
        {
            idleOverride = new AnimatorOverrideController
            {
                runtimeAnimatorController = baseRuntimeController
            };

            AssetDatabase.CreateAsset(idleOverride, IdleOverridePath);
            Debug.Log($"{logPrefix} Created idle override controller at {IdleOverridePath}.");
            return idleOverride;
        }

        if (idleOverride.runtimeAnimatorController != baseRuntimeController)
        {
            idleOverride.runtimeAnimatorController = baseRuntimeController;
            EditorUtility.SetDirty(idleOverride);
        }

        return idleOverride;
    }

    private static void EnsureFoldersForAssetPath(string assetPath)
    {
        var folder = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        folder = folder.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        var parts = folder.Split('/');
        var current = parts[0];

        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static List<AnimationClip> LoadLmgmcClips()
    {
        return AssetDatabase
            .LoadAllAssetsAtPath(LmgmcAssetPath)
            .OfType<AnimationClip>()
            .ToList();
    }

    private static AnimationClip ResolveClipByCandidates(List<AnimationClip> clips, string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var exactMatch = clips.FirstOrDefault(c => string.Equals(c.name, candidate, StringComparison.Ordinal));
            if (exactMatch != null)
            {
                return exactMatch;
            }
        }

        foreach (var candidate in candidates)
        {
            var caseInsensitiveMatch = clips.FirstOrDefault(c => string.Equals(c.name, candidate, StringComparison.OrdinalIgnoreCase));
            if (caseInsensitiveMatch != null)
            {
                return caseInsensitiveMatch;
            }
        }

        foreach (var candidate in candidates)
        {
            var containsMatches = clips
                .Where(c => c.name.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (containsMatches.Count == 1)
            {
                return containsMatches[0];
            }

            if (containsMatches.Count > 1)
            {
                var options = string.Join(", ", containsMatches.Select(c => c.name));
                Debug.LogWarning($"[ApplyLMGMCIdleOverrides] Multiple clips matched '{candidate}': {options}");
            }
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