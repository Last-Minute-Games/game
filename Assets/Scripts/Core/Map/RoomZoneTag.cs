using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to every room's RoomAudioZone GameObject.
/// When the player enters the trigger, it tells RoomTracker which room they're in.
///
/// On Awake the component automatically resolves its roomId from the
/// GameObject name so the scene doesn't need manual string wiring.
/// Also exposes world-space centre + radius so the map can read them at runtime.
/// </summary>
public class RoomZoneTag : MonoBehaviour
{
    [Tooltip("Auto-resolved from GameObject name.  Override only if the name doesn't match.")]
    public string roomId;

    // ── Static registry so other systems can find all zones cheaply ──
    private static readonly List<RoomZoneTag> _allZones = new List<RoomZoneTag>();
    public static IReadOnlyList<RoomZoneTag> AllZones => _allZones;

    // ── Cached world bounds (set once in Awake) ──
    /// <summary>Effective world-space centre of the trigger collider.</summary>
    public Vector2 WorldCenter { get; private set; }
    /// <summary>Effective world-space radius of the trigger zone.</summary>
    public float WorldRadius   { get; private set; }

    // Maps common GameObject names → RoomMapData roomId strings.
    // Case-insensitive. Add new rooms here if the castle grows.
    private static readonly Dictionary<string, string> _nameToId =
        new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
    {
        { "Ballroom",    "ballroom"  },
        { "Throne Room", "throne"    },
        { "Armory",      "armory"    },
        { "Kitchen",     "kitchen"   },
        { "Classroom",   "classroom" },
        { "Dining Room", "dining"    },
        { "Study Room",  "study"     },
        { "Library",     "library"   },
        { "Patio",       "patio"     },
        { "Bedroom",     "bedroom"   },
    };

    void Awake()
    {
        // Auto-resolve roomId from the GameObject name
        if (_nameToId.TryGetValue(gameObject.name, out string resolved))
        {
            roomId = resolved;
        }
        else if (string.IsNullOrEmpty(roomId))
        {
            // Last-resort fallback: lowercase the GO name
            roomId = gameObject.name.ToLowerInvariant().Replace(" ", "");
        }

        // Cache world bounds from the attached collider
        var circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            WorldCenter = (Vector2)transform.position + circle.offset;
            WorldRadius = circle.radius * Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.y));
        }
        else
        {
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                WorldCenter = (Vector2)transform.position + box.offset;
                Vector2 halfSize = box.size * 0.5f;
                WorldRadius = halfSize.magnitude * Mathf.Max(
                    Mathf.Abs(transform.lossyScale.x),
                    Mathf.Abs(transform.lossyScale.y));
            }
            else
            {
                WorldCenter = transform.position;
                WorldRadius = 20f;
            }
        }
    }

    void OnEnable()  { _allZones.Add(this); }
    void OnDisable() { _allZones.Remove(this); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RoomTracker.SetCurrentRoom(roomId);
    }
}
