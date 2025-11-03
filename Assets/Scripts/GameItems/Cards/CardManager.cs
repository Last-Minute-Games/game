using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    [Header("References")]
    public PlayerData playerData;
    public CardFXHelper fxHelper;
    public CardPrefab cardPrefabTemplate;
    public Transform handContainer;

    [Header("Runtime State")]
    public List<CardPrefab> handCards = new();

    // ────────────────────────────────
    // Public Methods (declarations only)
    // ────────────────────────────────
    public CardPrefab PullCardByID(int id) { return null; }
    public void PullCards(int amount, bool playerCardsOnly = true) { }
    public void DiscardCards() { }
    public void DiscardCardByID(int id) { }

    // ────────────────────────────────
    // Helper Methods (declarations only)
    // ────────────────────────────────
    private CardPrefab SpawnCardPrefab(CardData data) { return null; }
    private CardPrefab FindCardInHand(int id) { return null; }
    private CardData DrawRandomCard(List<CardDrawEntry> pool) { return null; }
    private void ClearHand() { }
}
