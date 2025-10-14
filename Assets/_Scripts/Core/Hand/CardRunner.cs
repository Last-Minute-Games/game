using UnityEngine;

// CardRunner.cs (additions/changes)

[RequireComponent(typeof(SpriteRenderer))]
public class CardRunner : MonoBehaviour
{
    [Header("Card Reference")]
    public CardData data;
    [HideInInspector] public CharacterBase owner;

    [HideInInspector] public float cachedPotency = 1f;

    // --- NEW: single source of truth for rolls ---
    public bool hasRoll { get; private set; }
    public float randomScale { get; private set; } = 1f;

    public void RollIfNeeded(CardData d = null)
    {
        var src = d ?? data;
        if (!hasRoll && src != null)
        {
            randomScale = Random.Range(src.minMultiplier, src.maxMultiplier);
            hasRoll = true;
        }
    }

    public float GetTotalScale(CharacterBase user)
    {
        RollIfNeeded(data);
        float g = 1f;
        if (user is Enemy e) g = e.globalPowerScale;
        else if (user is Player p) g = p.globalPowerScale;
        cachedPotency = randomScale * g;
        return cachedPotency;
    }

    // Preview the <X> value for UI using the first effect
    public int GetPreviewX(CharacterBase user)
    {
        if (data == null || data.effects == null || data.effects.Length == 0) return 0;

        float totalScale = GetTotalScale(user);
        var eff = data.effects[0];
        return eff.PreviewAmount(this, user, totalScale);
    }

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (data && data.artwork && sr) sr.sprite = data.artwork;
        // OPTIONAL: lock the roll on draw (for player cards)
        // RollIfNeeded(data);
    }

    public void Execute(CharacterBase user, CharacterBase target)
    {
        if (data == null) { Debug.LogWarning($"⚠️ {name} has no CardData assigned!"); return; }

        // Ensure we use the SAME roll we previewed
        RollIfNeeded(data);

        foreach (var effect in data.effects)
            effect?.Apply(this, user, target);
    }

    public int EnergyCost => data ? data.energy : 0;

    public bool TryPlay(CharacterBase user, Collider2D hit, out CharacterBase finalTarget)
    {
        finalTarget = null;
        if (data == null) return false;

        var rule = data.targetingRule;
        finalTarget = rule ? rule.ResolveTarget(user, hit) : user;
        if (finalTarget == null) return false;

        // Ensure we use the SAME roll we previewed
        RollIfNeeded(data);

        foreach (var eff in data.effects)
            eff?.Apply(this, user, finalTarget);

        return true;
    }
}
