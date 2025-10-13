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

    [Header("Global Scaling (applied to all cards)")]
    [Tooltip("Multiplier applied to all card effects this enemy uses (e.g. bosses = 2.0).")]
    [Range(0.5f, 3f)] public float globalCardMultiplier = 1f;

    [Header("Animations")]
    public EnemyAnimationSet animationSet;

    [Header("AI Deck (Cards they can use)")]
    public List<CardData> availableCards;

    [Header("Behavior")]
    [Range(0f, 1f)] public float idleChance = 0.2f;
}
