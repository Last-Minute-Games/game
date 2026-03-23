using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to any minigame entrance that should appear on the map.
/// Visibility is controlled by revealFlagKey (for example "minigame.sokoban.show").
/// </summary>
public class MinigameMapTracker : MonoBehaviour
{
    private static readonly HashSet<MinigameMapTracker> _allTrackers = new HashSet<MinigameMapTracker>();

    public static IReadOnlyCollection<MinigameMapTracker> AllTrackers => _allTrackers;

    [Header("Display")]
    [SerializeField] private string displayName;
    [SerializeField] private Color markerColor = new Color(0.95f, 0.70f, 0.25f, 1f);
    [SerializeField] private bool includeInLegend = true;

    [Header("Reveal")]
    [Tooltip("If empty, this marker is always considered discovered.")]
    [SerializeField] private string revealFlagKey = "";

    [Header("Portrait")]
    [Tooltip("Optional: assign portrait sprite directly.")]
    [SerializeField] private Sprite portrait;
    [Tooltip("Optional: custom Resources path override for portrait loading.")]
    [SerializeField] private string portraitResourcePath;

    private Sprite _cachedPortrait;
    private bool _portraitResolved;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
    public Color MarkerColor => markerColor;
    public bool IncludeInLegend => includeInLegend;
    public string RevealFlagKey => revealFlagKey;
    public Vector3 WorldPosition => transform.position;

    public bool IsDiscovered => string.IsNullOrEmpty(revealFlagKey) || GameFlags.HasFlag(revealFlagKey);

    public Sprite Portrait
    {
        get
        {
            if (!_portraitResolved)
                ResolvePortrait();
            return _cachedPortrait;
        }
    }

    void Awake()
    {
        _allTrackers.Add(this);
    }

    void OnDestroy()
    {
        _allTrackers.Remove(this);
    }

    public void Configure(
        string configuredDisplayName,
        string configuredRevealFlag,
        Color configuredMarkerColor,
        Sprite configuredPortrait = null,
        string configuredPortraitResourcePath = null,
        bool configuredIncludeInLegend = true)
    {
        if (!string.IsNullOrWhiteSpace(configuredDisplayName))
            displayName = configuredDisplayName;

        revealFlagKey = configuredRevealFlag;
        markerColor = configuredMarkerColor;
        includeInLegend = configuredIncludeInLegend;

        if (configuredPortrait != null)
            portrait = configuredPortrait;

        if (!string.IsNullOrWhiteSpace(configuredPortraitResourcePath))
            portraitResourcePath = configuredPortraitResourcePath;

        _portraitResolved = false;
        _cachedPortrait = null;
    }

    private void ResolvePortrait()
    {
        _portraitResolved = true;

        // 1) Explicit override from inspector/config
        if (portrait != null)
        {
            _cachedPortrait = portrait;
            return;
        }

        // 2) Default to this minigame's in-world sprite
        if (TryResolveFromWorldSprite(out var worldSprite))
        {
            _cachedPortrait = worldSprite;
            return;
        }

        // 3) Optional custom Resources path
        if (!string.IsNullOrEmpty(portraitResourcePath))
        {
            var customTex = Resources.Load<Texture2D>(portraitResourcePath);
            if (customTex != null)
            {
                customTex.filterMode = FilterMode.Point;
                _cachedPortrait = TextureToSprite(customTex);
                return;
            }
        }

        // 4) Convention-based fallback in Resources/Minigames
        string minigamePath = $"Minigames/{DisplayName}/{DisplayName}Portrait";
        var minigameTex = Resources.Load<Texture2D>(minigamePath);
        if (minigameTex != null)
        {
            minigameTex.filterMode = FilterMode.Point;
            _cachedPortrait = TextureToSprite(minigameTex);
            return;
        }

        // 5) Legacy fallback path
        string dialoguePath = $"Dialogues/{DisplayName}/{DisplayName}Portrait";
        var dialogueTex = Resources.Load<Texture2D>(dialoguePath);
        if (dialogueTex != null)
        {
            dialogueTex.filterMode = FilterMode.Point;
            _cachedPortrait = TextureToSprite(dialogueTex);
            return;
        }

        _cachedPortrait = null;
    }

    private bool TryResolveFromWorldSprite(out Sprite resolvedSprite)
    {
        // Prefer the sprite on this object first.
        var ownRenderer = GetComponent<SpriteRenderer>();
        if (ownRenderer != null && ownRenderer.sprite != null)
        {
            resolvedSprite = ownRenderer.sprite;
            return true;
        }

        // Fall back to the first child sprite renderer with a sprite.
        var childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < childRenderers.Length; i++)
        {
            var sr = childRenderers[i];
            if (sr == null || sr == ownRenderer || sr.sprite == null)
                continue;

            resolvedSprite = sr.sprite;
            return true;
        }

        resolvedSprite = null;
        return false;
    }

    private static Sprite TextureToSprite(Texture2D tex)
    {
        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }
}
