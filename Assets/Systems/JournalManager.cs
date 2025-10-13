using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Journal Manager", fileName = "JournalManager")]
public class JournalManager : ScriptableObject
{
    [Serializable]
    public struct Mapping
    {
        [Tooltip("When this flag changes...")]
        public string flag;

        [Tooltip("...unlock this journal entry id.")]
        public string entryId;

        [Tooltip("Only when flag becomes TRUE? If false, unlock on any change.")]
        public bool onlyWhenTrue;
    }

    [Header("Journal Settings")]
    [SerializeField] private List<Mapping> mappings = new();

    private readonly HashSet<string> unlockedEntries = new();

    public event Action<string> OnEntryUnlocked;

    private GameFlags currentFlags;

    public void Hook(GameFlags flags)
    {
        if (flags == null)
        {
            Debug.LogWarning("[Journal] Tried to hook with null GameFlags.");
            return;
        }

        if (currentFlags != null)
            currentFlags.OnFlagChanged -= HandleFlagChanged;

        currentFlags = flags;
        currentFlags.OnFlagChanged += HandleFlagChanged;

        Debug.Log("[Journal] Hooked into GameFlags.");
    }

    private void HandleFlagChanged(string flag, bool value)
    {
        foreach (var m in mappings)
        {
            if (!string.Equals(m.flag, flag, StringComparison.OrdinalIgnoreCase))
                continue;

            if (m.onlyWhenTrue && !value)
                continue;

            AddEntry(m.entryId);
        }
    }

    public void AddEntry(string entryId)
    {
        if (string.IsNullOrWhiteSpace(entryId))
            return;

        if (unlockedEntries.Contains(entryId))
            return; // Already unlocked

        unlockedEntries.Add(entryId);
        Debug.Log($"[Journal] Unlocked entry: {entryId}");
        OnEntryUnlocked?.Invoke(entryId);

        // TODO: Hook Journal UI or saving system later
    }

#if UNITY_EDITOR
    [ContextMenu("Test Unlock 'found.blade'")]
    private void TestUnlock()
    {
        AddEntry("found.blade");
    }
#endif
}
