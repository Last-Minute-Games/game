using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Unnamed Enemy";
    public Sprite portrait;

    [Header("Core Stats (Health Range)")]
    [Tooltip("The minimum and maximum possible health this enemy can spawn with.")]
    public int minHealth = 80;
    public int maxHealthRange = 150;

    [Header("Global Scaling (applies to all cards)")]
    [Range(0.5f, 3f)]
    public float globalCardMultiplier = 1f;

    [Header("Animations")]
    public EnemyAnimationSet animationSet;

    [Header("AI Deck (Weighted Cards)")]
    public List<WeightedCard> availableCards = new();

    [Header("Behavior Evolution")]
    [Tooltip("Multiplier applied to weighted cards marked 'prioritize when low HP' when enemy health ≤ ⅓ max.")]
    public float lowHPWeightMultiplier = 1.5f;

    public void ModifyBehavior(CardData lastUsedCard)
    {
        if (lastUsedCard == null) return;

        foreach (var entry in availableCards)
        {
            if (entry.card == null) continue;

            // Simple adaptive logic
            switch (lastUsedCard.cardType)
            {
                case CardType.Defense:
                    entry.baseWeight *= 1.25f; // Attack bias after defense
                    break;

                case CardType.Healing:
                    entry.baseWeight *= 1.1f; // Guard after healing
                    break;

                case CardType.Attack:
                    entry.baseWeight *= 0.95f; // Slight cooldown for aggression
                    break;
            }
        }
    }

    /// <summary>
    /// Returns a randomized health value between minHealth and maxHealthRange.
    /// </summary>
    public int GetRandomizedHealth()
    {
        return Random.Range(minHealth, maxHealthRange + 1);
    }
}

[System.Serializable]
public class WeightedCard
{
    public CardData card;
    [Range(0f, 1f)] public float baseWeight = 1f;
    [Tooltip("Increases this card’s chance when HP ≤ ⅓ max.")]
    public bool prioritizeWhenLowHP = false;
}
