using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Unnamed Enemy";
    public Sprite portrait;

    [Header("Core Stats")]
    public int maxHealth = 100;
    public int baseDamage = 10;
    public int defense = 2;

    [Header("Global Scaling (applies to all cards)")]
    [Range(0.5f, 3f)] public float globalCardMultiplier = 1f;

    [Header("Animations")]
    public EnemyAnimationSet animationSet;

    [Header("AI Deck (Weighted Cards)")]
    public List<WeightedCard> availableCards = new();

    [Header("Behavior")]
    [Range(0f, 1f)] public float idleChance = 0.1f;

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
            if (lastUsedCard.cardType == CardType.Defense)
            {
                entry.baseWeight *= 1.25f; // Attack bias after defense
            }
            else if (lastUsedCard.cardType == CardType.Healing)
            {
                entry.baseWeight *= 1.1f; // Guard after healing
            }
            else if (lastUsedCard.cardType == CardType.Attack)
            {
                entry.baseWeight *= 0.95f; // Slight cooldown for aggression
            }
        }
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
