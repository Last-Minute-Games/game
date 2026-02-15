using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to any named NPC that should appear on the castle map.
/// The NPC will only show on the map after the player has interacted with them once
/// (tracked via the GameFlags system using "npc.met.{displayName}" flags).
///
/// Maintains a static registry so the map UI can enumerate all tracked NPCs cheaply.
/// </summary>
public class NPCMapTracker : MonoBehaviour
{
    // ─────────────── Static Registry ───────────────

    private static readonly HashSet<NPCMapTracker> _allTrackers = new HashSet<NPCMapTracker>();

    /// <summary>All currently active NPCMapTracker instances.</summary>
    public static IReadOnlyCollection<NPCMapTracker> AllTrackers => _allTrackers;

    // ─────────────── Inspector ───────────────

    [Tooltip("Display name shown on the map (also used for the npc.met.{name} flag).")]
    [SerializeField] private string displayName;

    [Tooltip("Colour of this NPC's dot on the map.")]
    [SerializeField] private Color markerColor = Color.cyan;

    // ─────────────── Public API ───────────────

    /// <summary>The name shown beside the NPC dot on the map.</summary>
    public string DisplayName => displayName;

    /// <summary>Colour used for this NPC's map marker.</summary>
    public Color MarkerColor => markerColor;

    /// <summary>The flag key used to track whether the player has met this NPC.</summary>
    public string MetFlagKey => $"npc.met.{displayName}";

    /// <summary>
    /// True if the player has interacted with this NPC at least once
    /// (i.e. the "npc.met.{displayName}" flag exists).
    /// </summary>
    public bool IsDiscovered => GameFlags.HasFlag(MetFlagKey);

    /// <summary>Shortcut to the NPC's current world position.</summary>
    public Vector3 WorldPosition => transform.position;

    // ─────────────── Lifecycle ───────────────

    void OnEnable()
    {
        _allTrackers.Add(this);
    }

    void OnDisable()
    {
        _allTrackers.Remove(this);
    }
}
