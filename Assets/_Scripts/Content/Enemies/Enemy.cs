private CardData GetWeightedRandomCard()
{
    if (data == null || data.availableCards.Count == 0)
        return null;

    float totalWeight = 0f;
    foreach (var entry in data.availableCards)
    {
        if (entry.card == null) continue;

        float weight = Mathf.Max(0f, entry.baseWeight);

        // 🧠 Behavior evolution: boost weight if HP ≤ ⅓ max and flag enabled
        if (entry.prioritizeWhenLowHP && currentHealth <= maxHealth / 3f)
            weight *= data.lowHPWeightMultiplier;

        totalWeight += weight;
    }

    if (totalWeight <= 0f)
        return data.availableCards[0].card; // fallback

    float roll = Random.value * totalWeight;
    float cumulative = 0f;

    foreach (var entry in data.availableCards)
    {
        if (entry.card == null) continue;

        float weight = Mathf.Max(0f, entry.baseWeight);
        if (entry.prioritizeWhenLowHP && currentHealth <= maxHealth / 3f)
            weight *= data.lowHPWeightMultiplier;

        cumulative += weight;
        if (roll <= cumulative)
            return entry.card;
    }

    return data.availableCards[0].card; // safety fallback
}
