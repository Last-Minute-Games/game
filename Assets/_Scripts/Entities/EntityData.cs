using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardDrawEntry
{
    [Tooltip("Reference to a usable card for this entity.")]
    public CardData card;

    [Tooltip("Relative draw weight for this card.")]
    [Range(0f, 10f)]
    public float drawWeight = 1f;
}

public abstract class EntityData : ScriptableObject
{
    [Header("Base Stats")]
    public int baseHealth = 100;
    public int baseShield = 0;
    public float basePowerScale = 1f;

    [Header("Scaling Multipliers")]
    [Range(0f, 10f)]
    public float minScaleMultiplier = 1f;

    [Range(0f, 10f)]
    public float maxScaleMultiplier = 2f;

    [Header("Usable Cards & Draw Weights")]
    public List<CardDrawEntry> usableCards = new List<CardDrawEntry>();

    protected virtual void OnValidate()
    {
        if (minScaleMultiplier > maxScaleMultiplier)
            minScaleMultiplier = maxScaleMultiplier;
    }
}
