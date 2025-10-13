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
            if (!cardLookup.ContainsKey(card.cardType))
                cardLookup[card.cardType] = new List<CardData>();

            cardLookup[card.cardType].Add(card);
        }
    }

    public CardData GetRandomCard(CardType type)
    {
        if (cardLookup == null || cardLookup.Count == 0)
            Initialize();

        if (!cardLookup.ContainsKey(type) || cardLookup[type].Count == 0)
            return null;

        return cardLookup[type][Random.Range(0, cardLookup[type].Count)];
    }

    public CardData GetRandomCardWeighted(float attackChance, float defenseChance, float healChance)
    {
        if (cardLookup == null || cardLookup.Count == 0)
            Initialize();

        float total = attackChance + defenseChance + healChance;
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

        return GetRandomCard(type);
    }
}
