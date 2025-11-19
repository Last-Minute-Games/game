using UnityEngine;
using System.Collections.Generic;
using Entities.Players.Data;
using GameItems.Cards;

public class PlayerManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Player configuration asset (ScriptableObject).")]
    public PlayerConfig playerConfig; // Assign in Inspector

    [Header("Runtime State")]
    public PlayerData playerData; // Runtime instance created from config
    public CardManager cardManager = new CardManager();

    private bool _initialized;

    private void Awake()
    {
        if (playerConfig == null)
        {
            Debug.LogError("PlayerManager: No PlayerConfig assigned.");
            return;
        }

        InitializeFromConfig();
    }

    // Runtime API to support code paths that pass PlayerConfig programmatically
    public void Initialize(PlayerConfig config)
    {
        playerConfig = config;
        if (playerConfig == null)
        {
            Debug.LogError("PlayerManager.Initialize: Provided PlayerConfig is null.");
            return;
        }
        _initialized = false;
        InitializeFromConfig();
    }

    private void InitializeFromConfig()
    {
        if (_initialized) return;
        _initialized = true;

        // Create runtime instance from config
        playerData = playerConfig != null ? playerConfig.CreateRuntimeInstance() : null;
        if (playerData == null)
        {
            Debug.LogError("PlayerManager: Failed to create runtime PlayerData from PlayerConfig.");
            return;
        }

        // Energy is now managed by PlayerData itself
        playerData.currentEnergy = playerData.baseEnergy;

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

        // Reset energy and block
        playerData.ResetEnergy();
        playerData.block = 0;

        // Draw hand
        int handSize = playerConfig != null && playerConfig.config != null 
            ? Mathf.Max(1, playerConfig.config.defaultHandSize) 
            : 5;
        for (int i = 0; i < handSize; i++)
            cardManager.DrawCard();
    }

    public bool TryPlayCard(CardData card, ref EntityData target)
    {
        if (playerData == null || card == null) return false;

        // Simplified energy check (1 per card until card has explicit cost)
        if (!playerData.SpendEnergy(1))
            return false;

        // cardManager.ApplyCard(card, playerData, ref target); // TODO: re-fix definition of ApplyCard with actual combat system
        return true;
    }

    public void EndTurn()
    {
        cardManager.DiscardCardPile();
    }
}
