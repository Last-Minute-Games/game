using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Cards/Card Library")]
public class CardLibrary : ScriptableObject
{
    private Dictionary<CardType, List<CardData>> cardLookup;
    private Dictionary<int, CardData> idLookup;
    private Dictionary<string, CardData> nameLookup;

    public void Initialize()
    {
        CardData[] allCards = Resources.LoadAll<CardData>("Cards/Data");
        Debug.Log($"[CardLibrary] Loaded {allCards.Length} CardData assets from Resources.");

        cardLookup = new Dictionary<CardType, List<CardData>>();
        idLookup = new Dictionary<int, CardData>();
        nameLookup = new Dictionary<string, CardData>();

        foreach (CardData card in allCards)
        {
            if (card == null) continue;

            // Type grouping
            if (!cardLookup.ContainsKey(card.cardType))
                cardLookup[card.cardType] = new List<CardData>();
            cardLookup[card.cardType].Add(card);

            // ID lookup
            if (!idLookup.ContainsKey(card.uniqueId))
                idLookup[card.uniqueId] = card;

            // Name lookup (case-insensitive)
            string key = card.cardName.ToLower();
            if (!nameLookup.ContainsKey(key))
                nameLookup[key] = card;
        }
    }

    // =============================
    //  DIRECT LOOKUPS
    // =============================
    public CardData GetById(int id)
    {
        if (idLookup == null || idLookup.Count == 0)
            Initialize();

        return idLookup.TryGetValue(id, out var data) ? data : null;
    }

    public CardData GetByName(string name)
    {
        if (nameLookup == null || nameLookup.Count == 0)
            Initialize();

        return nameLookup.TryGetValue(name.ToLower(), out var data) ? data : null;
    }

    // =============================
    //  RANDOM DRAW LOGIC
    // =============================
    public CardData GetRandomCard(CardType type, bool forPlayer = true)
    {
        if (cardLookup == null || cardLookup.Count == 0)
            Initialize();

        if (!cardLookup.ContainsKey(type) || cardLookup[type].Count == 0)
            return null;

        // Filter player usable cards
        List<CardData> pool = new List<CardData>();
        foreach (var card in cardLookup[type])
        {
            if (!forPlayer || card.isPlayerUsable)
                pool.Add(card);
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning($"⚠️ CardLibrary: No {(forPlayer ? "player-usable " : "")}{type} cards found!");
            return null;
        }

        // Weighted by 0–1 playerDrawWeight
        var weighted = pool.Where(c => c.playerDrawWeight > 0f).ToList();
        float totalWeight = weighted.Sum(c => c.playerDrawWeight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var card in weighted)
        {
            cumulative += card.playerDrawWeight;
            if (roll <= cumulative)
                return card;
        }

        return weighted[0]; // fallback
    }

    public CardData GetRandomCardWeighted(float attackChance, float defenseChance, float healChance, bool forPlayer = true)
    {
        if (cardLookup == null || cardLookup.Count == 0)
            Initialize();

        float total = attackChance + defenseChance + healChance;
        if (total <= 0)
        {
            Debug.LogError("❌ CardLibrary: Invalid weights — all zero!");
            return null;
        }

        attackChance /= total;
        defenseChance /= total;
        healChance /= total;

        float roll = Random.value;
        CardType type;

        if (roll < attackChance)
            type = CardType.Attack;
        else if (roll < attackChance + defenseChance)
            type = CardType.Defense;
        else
            type = CardType.Healing;

        CardData selected = GetRandomCard(type, forPlayer);

        // 🔁 fallback: if no cards of this type exist, pick any usable card
        if (selected == null && forPlayer)
        {
            List<CardData> allUsable = idLookup.Values.Where(c => c.isPlayerUsable && c.playerDrawWeight > 0f).ToList();
            if (allUsable.Count == 0)
            {
                Debug.LogError("❌ CardLibrary: No usable cards available for player!");
                return null;
            }

            float totalWeight = allUsable.Sum(c => c.playerDrawWeight);
            float r = Random.Range(0f, totalWeight);
            float cum = 0f;
            foreach (var c in allUsable)
            {
                cum += c.playerDrawWeight;
                if (r <= cum)
                    return c;
            }

            return allUsable[0];
        }

        return selected;
    }
}
