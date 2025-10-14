using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/Card Library")]
public class CardLibrary : ScriptableObject
{
    private Dictionary<CardType, List<CardData>> cardLookup;

    public void Initialize()
    {
        CardData[] allCards = Resources.LoadAll<CardData>("Cards/Data");
        Debug.Log($"[CardLibrary] Loaded {allCards.Length} CardData assets from Resources.");

        cardLookup = new Dictionary<CardType, List<CardData>>();

        foreach (CardData card in allCards)
        {
            if (card == null) continue;

            if (!cardLookup.ContainsKey(card.cardType))
                cardLookup[card.cardType] = new List<CardData>();

            cardLookup[card.cardType].Add(card);
        }
    }

    public CardData GetRandomCard(CardType type, bool forPlayer = true)
    {
        if (cardLookup == null || cardLookup.Count == 0)
            Initialize();

        if (!cardLookup.ContainsKey(type) || cardLookup[type].Count == 0)
            return null;

        // Filter player usable cards if needed
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

        return pool[Random.Range(0, pool.Count)];
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

        // 🔁 fallback: if this type had no usable cards, try any usable card
        if (selected == null && forPlayer)
        {
            List<CardData> allUsable = new();
            foreach (var list in cardLookup.Values)
                allUsable.AddRange(list.FindAll(c => c.isPlayerUsable));

            if (allUsable.Count == 0)
            {
                Debug.LogError("❌ CardLibrary: No usable cards available for player!");
                return null;
            }

            selected = allUsable[Random.Range(0, allUsable.Count)];
        }

        return selected;
    }
}
