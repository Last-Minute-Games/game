using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardManager : MonoBehaviour
{
    [Header("Global Pool")]
    [Tooltip("All cards that exist in the game. Used for global random draws.")]
    public List<CardData> globalPoolPile = new List<CardData>();

    // ─────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────

    public CardData PullCardByID(int id, bool multiplierApplied = true, float powerScale = 1f)
    {
        var match = globalPoolPile.FirstOrDefault(c => c.uniqueID == id);
        if (match == null)
        {
            Debug.LogWarning($"[CardManager] No CardData with ID {id}");
            return null;
        }

        return CloneCardData(match, multiplierApplied, powerScale);
    }

    public List<CardData> PullMultipleRandomCards(
        int amount,
        List<CardDrawEntry> entityDataCards = null,
        bool multiplierApplied = false,
        float powerScale = 1f)
    {
        var result = new List<CardData>();

        if (entityDataCards != null && entityDataCards.Count > 0)
        {
            // Weighted random draw
            for (int i = 0; i < amount; i++)
            {
                var drawn = DrawWeightedRandomCard(entityDataCards);
                if (drawn != null)
                    result.Add(CloneCardData(drawn, multiplierApplied, powerScale));
            }
            return result;
        }

        // Global uniform draw
        if (globalPoolPile.Count == 0)
        {
            Debug.LogWarning("[CardManager] Global pool is empty!");
            return result;
        }

        for (int i = 0; i < amount; i++)
        {
            var card = globalPoolPile[Random.Range(0, globalPoolPile.Count)];
            result.Add(CloneCardData(card, multiplierApplied, powerScale));
        }

        return result;
    }

    // ─────────────────────────────────────────────
    // Internal Helpers
    // ─────────────────────────────────────────────

    private CardData CloneCardData(CardData original, bool multiplierApplied, float powerScale = 1f)
    {
        if (original == null) return null;

        var clone = Instantiate(original);
        clone.effectData = new List<EffectData>();

        foreach (var effect in original.effectData)
        {
            if (effect == null) continue;
            bool shouldApply = multiplierApplied && original.isVariableCard && original.IsCardVariabilityValid();
            clone.effectData.Add(effect.Clone(shouldApply, powerScale));
        }

        return clone;
    }

    private CardData DrawWeightedRandomCard(List<CardDrawEntry> pool)
    {
        if (pool == null || pool.Count == 0)
            return null;

        float totalWeight = pool.Sum(e => Mathf.Max(0f, e.drawWeight));
        if (totalWeight <= 0f)
            return pool[Random.Range(0, pool.Count)].card;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in pool)
        {
            cumulative += Mathf.Max(0f, entry.drawWeight);
            if (roll <= cumulative)
                return entry.card;
        }

        return pool[0].card;
    }
}
