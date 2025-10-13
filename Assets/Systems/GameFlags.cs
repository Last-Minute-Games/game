using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Global Flags", fileName = "GameFlags")]
public class GameFlags : ScriptableObject
{
    [Serializable]
    public struct Entry { public string key; public bool value; }

    [Tooltip("Seed flags you want to start the game with.")]
    [SerializeField] private List<Entry> initialFlags = new();

    private Dictionary<string, bool> map;
    public event Action<string, bool> OnFlagChanged;

    void OnEnable()
    {
        if (map == null) Build();
    }

    private void Build()
    {
        map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in initialFlags)
        {
            if (!string.IsNullOrWhiteSpace(e.key))
                map[e.key] = e.value;
        }
    }

    public bool Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        if (map == null) Build();
        return map.TryGetValue(key, out var v) && v;
    }

    public void Set(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        if (map == null) Build();

        bool changed = !map.TryGetValue(key, out var old) || old != value;
        map[key] = value;
        if (changed) OnFlagChanged?.Invoke(key, value);
    }

    public void Toggle(string key) => Set(key, !Get(key));
}
