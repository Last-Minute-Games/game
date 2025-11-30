using GameItems.Cards;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[CreateAssetMenu(menuName = "Cards/Card Data", fileName = "NewCardData")]
public class CardData : GameItemData
{
    // --------------------------------------------------
    // CARD INFO
    // --------------------------------------------------

    [Header("Card Info")]
    [Tooltip("Displayed text for the card's intention (e.g., 'Deal damage', 'Block', etc.).")]
    [TextArea(1, 3)]
    public string intentionText;

    [Tooltip("Sound cue played when this card is used.")]
    public SFXCueData soundCue;

    [Tooltip("Energy cost to play this card.")]
    public int energyCost;

    // --------------------------------------------------
    // ANIMATION SETTINGS
    // --------------------------------------------------

    [Header("Animation Settings")]
    [Tooltip("Animation played by the source entity when this card is used (e.g., Attack).")]
    public EnemyAnim sourceAnim = EnemyAnim.Attack;

    [Tooltip("Animation played by the target entity when this card is used (e.g., Hurt).")]
    public EnemyAnim targetAnim = EnemyAnim.Hurt;

    // --------------------------------------------------
    // VARIABILITY SETTINGS
    // --------------------------------------------------

    [Header("Variability Settings")]
    [Tooltip("If true, this card can roll variable potency (Poor, Normal, Potent) based on its multiplier range.")]
    public bool isVariableCard = false;

    [Tooltip("Artwork shown when the card rolls a Poor outcome (optional).")]
    public Sprite poorArtwork;

    [Tooltip("Artwork shown when the card rolls a Potent outcome (optional).")]
    public Sprite potentArtwork;

    [Header("Variability Threshold")]
    [Tooltip("The upper bound range of the variability threshold.")]
    [Range(0f, 1f)] public float maxMultiplierThreshold = 0.66f;

    [Tooltip("The lower bound range of the variability threshold.")]
    [Range(0f, 1f)] public float minMultiplierThreshold = 0.33f;

    // naming prefixes
    [Header("Name Prefixes")]
    public string weakPrefix = "Poor";
    public string strongPrefix = "Potent";

    [Tooltip("Color for weak prefix text.")]
    public Color weakPrefixColor = Color.gray;

    [Tooltip("Color for strong prefix text.")]
    public Color strongPrefixColor = new Color(1f, 0.4f, 0.4f);

    // Card flag unlocker
    [Header("Unlocking")]
    [Tooltip("Flag name required to unlock this card. Automatically assigned.")]
    public string unlockFlag;

    [Tooltip("Unlock the card by default")] // fallback, incase GameFlags doesn't work
    public bool unlockedByDefault;

    // --------------------------------------------------
    // Description substitution
    // --------------------------------------------------

    static readonly Regex PlaceholderRegex = new Regex(@"<([A-Za-z]+)>", RegexOptions.Compiled);

    /// <summary>
    /// Base-value substitution. Used when there is no CardInstance.
    /// </summary>
    public string BuildDescriptionWithSubstitutions(IEnumerable<Effect> sourceEffects)
    {
        if (string.IsNullOrEmpty(description)) return string.Empty;

        var map = new Dictionary<string, Effect>(System.StringComparer.OrdinalIgnoreCase);
        if (sourceEffects != null)
        {
            foreach (var e in sourceEffects)
            {
                if (!string.IsNullOrWhiteSpace(e.textKey) && !map.ContainsKey(e.textKey))
                    map[e.textKey] = e;
            }
        }

        return PlaceholderRegex.Replace(description, m =>
        {
            string key = m.Groups[1].Value;
            if (map.TryGetValue(key, out var eff))
                return $"<color=#{eff.GetColorTag()}>{eff.baseValue}</color>";
            return "?";
        });
    }

    /// <summary>
    /// Substitutes placeholders using final rolled + Strength-buffed values from the CardInstance.
    /// Strength scales enemy damage, self-damage, and heal. Distributed proportionally across all such effects.
    /// </summary>
    public string BuildDescriptionWithSubstitutionsFromInstance(CardInstance instance, PlayerManager playerManager)
    {
        if (string.IsNullOrEmpty(description) || instance == null || instance.rolledEffects == null || instance.rolledEffects.Count == 0)
            return BuildDescriptionWithSubstitutions(effects);

        // Map rolled effects by key
        var rolledByKey = new Dictionary<string, Effect>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var e in instance.rolledEffects)
        {
            if (!string.IsNullOrWhiteSpace(e.textKey) && !rolledByKey.ContainsKey(e.textKey))
                rolledByKey[e.textKey] = e;
        }

        // Start with rolled values
        var finalValues = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var kv in rolledByKey)
            finalValues[kv.Key] = kv.Value.postCopyValue;

        // Strength from player
        int strength = 0;
        if (playerManager != null && playerManager.playerData != null)
            strength = playerManager.playerData.strength;

        if (strength != 0)
        {
            // Eligible: Damage (self/enemy) and Heal
            var eligibleKeys = new List<string>();
            int baseSum = 0;

            foreach (var kv in rolledByKey)
            {
                var e = kv.Value;
                if (e.operationType == OperationType.Damage || e.operationType == OperationType.Heal)
                {
                    eligibleKeys.Add(kv.Key);
                    baseSum += Mathf.Max(0, e.postCopyValue);
                }
            }

            if (eligibleKeys.Count > 0)
            {
                if (baseSum <= 0)
                {
                    // Equal split if all are zero; distribute remainder round-robin
                    int per = strength / eligibleKeys.Count;
                    int rem = strength - per * eligibleKeys.Count;
                    for (int i = 0; i < eligibleKeys.Count; i++)
                    {
                        string k = eligibleKeys[i];
                        int add = per + (i < Mathf.Abs(rem) ? (rem > 0 ? 1 : -1) : 0);
                        finalValues[k] = Mathf.Max(0, finalValues[k] + add);
                    }
                }
                else
                {
                    // Proportional split
                    int remaining = strength;
                    var adds = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
                    foreach (var k in eligibleKeys)
                    {
                        int v = rolledByKey[k].postCopyValue;
                        float share = (float)Mathf.Max(0, v) / baseSum;
                        int add = Mathf.RoundToInt(share * strength);
                        adds[k] = add;
                        remaining -= add;
                    }
                    // Fix rounding drift by largest base first
                    eligibleKeys.Sort((a, b) => rolledByKey[b].postCopyValue.CompareTo(rolledByKey[a].postCopyValue));
                    int steps = Mathf.Abs(remaining);
                    int step = remaining > 0 ? 1 : -1;
                    for (int i = 0; i < steps; i++)
                    {
                        string k = eligibleKeys[i % eligibleKeys.Count];
                        adds[k] += step;
                    }
                    foreach (var k in eligibleKeys)
                        finalValues[k] = Mathf.Max(0, finalValues[k] + adds[k]);
                }
            }
        }

        // Replace
        return PlaceholderRegex.Replace(description, m =>
        {
            string key = m.Groups[1].Value;
            if (rolledByKey.TryGetValue(key, out var eff) && finalValues.TryGetValue(key, out int v))
                return $"<color=#{eff.GetColorTag()}>{v}</color>";
            return "?";
        });
    }

    // --------------------------------------------------
    // Existing helpers (unchanged)
    // --------------------------------------------------

    public bool IsCardVariabilityValid()
    {
        return isVariableCard && effects != null && effects.Count == 1;
    }

    public CardVariationTier GetVariationTier(Effect rolledEffect)
    {
        float min = rolledEffect.minMultiplier;
        float max = rolledEffect.maxMultiplier;
        float baseVal = rolledEffect.baseValue;
        float post = rolledEffect.postCopyValue;
        float lowBound = baseVal * min;
        float highBound = baseVal * max;

        if (post == baseVal) // for cases where there is no gap between minT, maxT and multiplier isn't on
            return CardVariationTier.NormalModifier;

        float t = Mathf.InverseLerp(lowBound, highBound, post);

        if (t <= minMultiplierThreshold)
            return CardVariationTier.WeakModifier;
        if (t >= maxMultiplierThreshold)
            return CardVariationTier.StrongModifier;
        return CardVariationTier.NormalModifier; // ultimate fallback
    }
    public string GetWeakPrefixColorTag() =>
        ColorUtility.ToHtmlStringRGB(weakPrefixColor);

    public string GetStrongPrefixColorTag() =>
        ColorUtility.ToHtmlStringRGB(strongPrefixColor);

    public string GetColoredPrefix(CardVariationTier tier)
    {
        return tier switch
        {
            CardVariationTier.WeakModifier =>
                $"<color=#{GetWeakPrefixColorTag()}>{weakPrefix}</color>",
            CardVariationTier.StrongModifier =>
                $"<color=#{GetStrongPrefixColorTag()}>{strongPrefix}</color>",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the appropriate artwork sprite based on the variation tier.
    /// Returns poorArtwork for weak tier, potentArtwork for strong tier, or default artwork otherwise.
    /// </summary>
    public Sprite GetArtworkForTier(CardVariationTier tier)
    {
        return tier switch
        {
            CardVariationTier.WeakModifier => poorArtwork != null ? poorArtwork : artwork,
            CardVariationTier.StrongModifier => potentArtwork != null ? potentArtwork : artwork,
            _ => artwork
        };
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        // Ensure max ≥ min
        if (maxMultiplierThreshold < minMultiplierThreshold)
            maxMultiplierThreshold = minMultiplierThreshold;

        // Ensure min ≤ max
        if (minMultiplierThreshold > maxMultiplierThreshold)
            minMultiplierThreshold = maxMultiplierThreshold;

        minMultiplierThreshold = Mathf.Clamp01(minMultiplierThreshold);
        maxMultiplierThreshold = Mathf.Clamp01(maxMultiplierThreshold);

        // automatically add custom unlockFlag name using name
        if (string.IsNullOrWhiteSpace(unlockFlag))
        {
            unlockFlag = $"card.{name.ToLower().Replace(" ", "_")}";
        }

        EnsureUniqueTextKeys(); // enforce uniqueness for substitution
    }

    void EnsureUniqueTextKeys()
    {
        if (effects == null || effects.Count == 0) return;

        var used = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var e in effects)
        {
            if (!string.IsNullOrWhiteSpace(e.textKey) && !used.Contains(e.textKey))
                used.Add(e.textKey);
        }

        for (int i = 0; i < effects.Count; i++)
        {
            var e = effects[i];
            if (string.IsNullOrWhiteSpace(e.textKey) || used.Contains(e.textKey))
            {
                string next = NextKey(used.Count);
                while (used.Contains(next))
                    next = NextKey(used.Count + 1);

                e.textKey = next;
                effects[i] = e;
                used.Add(next);
            }
        }
    }

    static string NextKey(int index)
    {
        // Order: X, Y, Z, A, B, ..., Z, AA, AB, ...
        if (index == 0) return "X";
        if (index == 1) return "Y";
        if (index == 2) return "Z";
        int n = index - 3;
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string s = "";
        do
        {
            s = alphabet[n % 26] + s;
            n = n / 26 - 1;
        } while (n >= 0);
        return s;
    }
}
