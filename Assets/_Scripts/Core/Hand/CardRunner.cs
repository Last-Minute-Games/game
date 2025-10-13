using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CardRunner : MonoBehaviour {
    [Header("Card Reference")]
    public CardData data;
    [HideInInspector] public CharacterBase owner;

    private void Awake() {
        var sr = GetComponent<SpriteRenderer>();
        if (data && data.artwork && sr) sr.sprite = data.artwork;
    }

    public void Execute(CharacterBase user, CharacterBase target)
    {
        if (data == null)
        {
            Debug.LogWarning($"⚠️ {name} has no CardData assigned!");
            return;
        }

        foreach (var effect in data.effects)
            effect?.Apply(user, target);
    }

    public int EnergyCost => data ? data.energyCost : 0;

    public bool TryPlay(CharacterBase user, Collider2D hit, out CharacterBase finalTarget) {
        finalTarget = null;
        if (data == null) return false;

        var rule = data.targetingRule;
        finalTarget = rule ? rule.ResolveTarget(user, hit) : user;

        if (finalTarget == null) return false;

        foreach (var eff in data.effects)
            eff?.Apply(user, finalTarget);

        return true;
    }
}


