using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to any named NPC that should appear on the castle map.
/// The NPC will only show on the map after the player has interacted with them once
/// (tracked via the GameFlags system using "npc.met.{displayName}" flags).
///
/// Maintains a static registry so the map UI can enumerate all tracked NPCs cheaply.
///
/// Portraits are loaded automatically from Resources/Dialogues/{Name}/{Name}Portrait.
/// For NPCs whose portrait files don't follow that convention (e.g. king.png,
/// KnightNPC.png), set <see cref="portraitResourcePath"/> in the Inspector.
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

    [Header("Portrait (Wizard101-style map marker)")]
    [Tooltip("Optional: drag a portrait sprite here. If left empty the system " +
             "loads Resources/Dialogues/{displayName}/{displayName}Portrait automatically.")]
    [SerializeField] private Sprite portrait;

    [Tooltip("Override the Resources path when the portrait file doesn't follow the " +
             "standard naming convention (e.g. \"Dialogues/king\" for king.png). " +
             "Leave blank for the default pattern.")]
    [SerializeField] private string portraitResourcePath;

    // ─────────────── Cached portrait sprite ───────────────

    private Sprite _cachedPortrait;
    private bool _portraitResolved;

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

    /// <summary>
    /// The character portrait sprite used for the Wizard101-style map marker.
    /// Resolved lazily: Inspector sprite → Resources auto-load → null.
    /// </summary>
    public Sprite Portrait
    {
        get
        {
            if (!_portraitResolved)
                ResolvePortrait();
            return _cachedPortrait;
        }
    }

    // ─────────────── Portrait Resolution ───────────────

    private void ResolvePortrait()
    {
        _portraitResolved = true;

        // 1. Inspector-assigned sprite takes priority
        if (portrait != null)
        {
            _cachedPortrait = portrait;
            return;
        }

        // 2. Try explicit override path
        if (!string.IsNullOrEmpty(portraitResourcePath))
        {
            var tex = Resources.Load<Texture2D>(portraitResourcePath);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point; // Crisp rendering, no blur
                _cachedPortrait = TextureToSprite(tex);
                return;
            }
        }

        // 3. Default convention: Dialogues/{Name}/{Name}Portrait
        {
            string path = $"Dialogues/{displayName}/{displayName}Portrait";
            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point; // Crisp rendering, no blur
                _cachedPortrait = TextureToSprite(tex);
                return;
            }
        }

        // 4. Fallback: try Dialogues/{Name} (for assets like king.png at root)
        {
            string path = $"Dialogues/{displayName}";
            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point; // Crisp rendering, no blur
                _cachedPortrait = TextureToSprite(tex);
                return;
            }
        }

        Debug.LogWarning($"[NPCMapTracker] No portrait found for '{displayName}'.");
    }

    private static Sprite TextureToSprite(Texture2D tex)
    {
        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }

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
