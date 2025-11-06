using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Header("Data")]
    public PlayerData playerData; // Assign in Inspector

    [Header("Runtime State")]
    public int energy;
    public int maxEnergy;
    public CardManager cardManager = new CardManager();

    private bool _initialized;

    private void Awake()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerManager: No PlayerData assigned.");
            return;
        }

        InitializeFromData();
    }

    // New: runtime API to support code paths that pass PlayerData programmatically
    public void Initialize(PlayerData data)
    {
        playerData = data;
        if (playerData == null)
        {
            Debug.LogError("PlayerManager.Initialize: Provided PlayerData is null.");
            return;
        }
        InitializeFromData();
    }

    private void InitializeFromData()
    {
        if (_initialized) return;
        _initialized = true;

        // Initialize runtime entity from PlayerData
        playerData.InitializeRuntime();

        // Energy setup
        maxEnergy = Mathf.Max(1, playerData.maxEnergy > 0 ? playerData.maxEnergy : (playerData.config != null ? playerData.config.defaultMaxEnergy : 3));
        energy = Mathf.Clamp(playerData.baseEnergy > 0 ? playerData.baseEnergy : (playerData.config != null ? playerData.config.defaultBaseEnergy : 3), 0, maxEnergy);

        // Card pools
        cardManager.allCardPool = playerData.usableCards != null && playerData.usableCards.Count > 0
            ? new List<CardData>(playerData.usableCards)
            : new List<CardData>(Resources.LoadAll<CardData>("Cards"));

        foreach (var card in cardManager.allCardPool)
        {
            Debug.Log($"CardManager: Loaded card {card}");
        }
        
        // Create a starting draw pile (fallback to 10 random cards)
        int startingDeckSize = 10;
        cardManager.drawPile = cardManager.GenerateRandomCards(startingDeckSize);
        cardManager.ShuffleDrawPile();
    }

    public void StartTurn()
    {
        if (playerData == null) return;

        energy = maxEnergy;
        playerData.entity.ResetBlock();

        int handSize = playerData.config != null ? Mathf.Max(1, playerData.config.defaultHandSize) : 5;
        for (int i = 0; i < handSize; i++)
            cardManager.DrawCard();
    }

    public bool TryPlayCard(CardData card, ref EntityData target)
    {
        if (playerData == null || card == null) return false;

        // Simplified energy check (1 per card until card has explicit cost)
        if (energy < 1)
            return false;

        energy -= 1; // TODO: replace with card.energyCost when available
        cardManager.ApplyCard(card, playerData.entity, ref target);
        return true;
    }

    public void EndTurn()
    {
        cardManager.DiscardCardPile();
    }
}