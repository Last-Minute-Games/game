using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardDrawEntry
{
    [Tooltip("Reference to a usable card for this entity.")]
    public CardData card;

    [Tooltip("Relative draw weight for this card.")]
    [Range(0f, 1f)]
    public float drawWeight = 1f;
}

public abstract class EntityData : ScriptableObject
{
    [Header("Base Stats")]
    public int baseHealth = 100;
    public int baseShield = 0;
    public float basePowerScale = 1f;

    [Header("Usable Cards & Draw Weights")]
    public List<CardDrawEntry> usableCards = new List<CardDrawEntry>();

    // Ensure OnValidate() is defined
    protected virtual void OnValidate() {}
}
