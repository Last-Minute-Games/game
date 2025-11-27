using UnityEngine;
using System.Collections.Generic;
using Entities.Enemies.Render;
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

        // Reduce strength by 1 each turn
        if (playerData.strength > 0)
        {
            playerData.LoseStrength(1);
        }

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

    /// <summary>
    /// Attempts to play a card, checking energy cost and applying effects.
    /// </summary>
    /// <param name="cardData">The base card data</param>
    /// <param name="cardInstance">Optional card instance with rolled effects</param>
    /// <param name="targetEnemy">Target enemy for damage effects (null for self-target cards)</param>
    /// <returns>True if card was successfully played</returns>
    public bool PlayCard(CardData cardData, CardInstance cardInstance = null, EnemyRender targetEnemy = null)
    {
        if (playerData == null || cardData == null)
        {
            Debug.LogWarning("[PlayerManager] Cannot play card - playerData or cardData is null");
            return false;
        }

        // Check energy cost
        int energyCost = cardData.energyCost;
        if (!playerData.SpendEnergy(energyCost))
        {
            Debug.LogWarning($"[PlayerManager] Not enough energy to play {cardData.name}. Need {energyCost}, have {playerData.currentEnergy}");
            return false;
        }

        // Apply card effects
        ApplyCardEffects(cardData, cardInstance, targetEnemy);

        // Play card sound effect if assigned
        if (cardData.soundCue != null)
        {
            SFXManager.Instance?.Play(cardData.soundCue);
        }

        // Move card from hand to discard pile and remove its instance
        if (cardManager != null)
        {
            bool removed = cardManager.PlayCardFromHand(cardData, cardInstance);
            Debug.Log($"[PlayerManager] PlayCard moved card '{{cardData.name}}' from hand to discard. RemovedFromHand={removed}");
        }
        else
        {
            Debug.LogWarning("[PlayerManager] cardManager is null when trying to move played card to discard.");
        }
        
        Debug.Log($"[PlayerManager] Successfully played card: {cardData.name}. Energy remaining: {playerData.currentEnergy}/{playerData.maxEnergy}");
        return true;
    }

    private void ApplyCardEffects(CardData cardData, CardInstance cardInstance, EnemyRender targetEnemy)
    {
        // Use rolled effects from instance if available, otherwise use base effects
        List<Effect> effectsToApply = cardInstance != null && cardInstance.rolledEffects != null && cardInstance.rolledEffects.Count > 0
            ? cardInstance.rolledEffects
            : cardData.effects;

        if (effectsToApply == null || effectsToApply.Count == 0)
        {
            Debug.LogWarning($"[PlayerManager] Card '{cardData.itemName}' has no effects to apply");
            return;
        }

        foreach (var effect in effectsToApply)
        {

            // Get the actual value to apply (rolled value if from instance, base value otherwise)
            int value = (cardInstance != null && cardInstance.rolledEffects != null && cardInstance.rolledEffects.Contains(effect))
                ? effect.postCopyValue
                : effect.baseValue;

            var roundManager = FindFirstObjectByType<RoundManager>();
            
            switch (effect.operationType)
            {
                case OperationType.Damage:
                    if (targetEnemy != null && targetEnemy.data != null)
                    {
                        int totalDamage = value + (playerData != null ? playerData.strength : 0);
                        targetEnemy.data.TakeDamage(totalDamage);
                        Debug.Log($"[PlayerManager] Dealt {totalDamage} damage ({value} base + {playerData?.strength} strength) to {targetEnemy.data.enemyName}. HP: {targetEnemy.data.currentHealth}/{targetEnemy.data.maxHealth}");

                        // Update enemy health display
                        var enemyManager = FindFirstObjectByType<Entities.Enemies.Manager.EnemyManager>();
                        if (enemyManager != null)
                        {
                            enemyManager.UpdateEnemyHealth(targetEnemy.data);
                        }
                        else
                        {
                            // Fallback: update directly
                            targetEnemy.UpdateHealth();
                            if (targetEnemy.data.isAlive)
                                targetEnemy.PlayHurt();
                            else
                                targetEnemy.PlayDeath();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[PlayerManager] Damage effect requires an enemy target");
                    }
                    break;

                case OperationType.AddShield:
                    if (playerData != null)
                    {
                        playerData.GainBlock(value);
                        Debug.Log($"[PlayerManager] Player gained {value} block. Total block: {playerData.block}");
                    }
                    break;

                case OperationType.Heal:
                    if (playerData != null)
                    {
                        playerData.Heal(value);
                        Debug.Log($"[PlayerManager] Player healed {value} HP. Current HP: {playerData.currentHealth}/{playerData.maxHealth}");
                    }
                    break;

                case OperationType.AddEnergy:
                    if (playerData != null)
                    {
                        playerData.GainEnergy(value);
                        Debug.Log($"[PlayerManager] Player gained {value} energy. Current energy: {playerData.currentEnergy}/{playerData.maxEnergy}");
                    }
                    break;

                case OperationType.DrawCards:
                    if (cardManager != null)
                    {
                        for (int i = 0; i < value; i++)
                        {
                            cardManager.DrawCard();
                        }
                        Debug.Log($"[PlayerManager] Player drew {value} cards.");
                        
                        // Update hand viewer to show newly drawn cards
                        if (roundManager != null && roundManager.handViewer != null)
                        {
                            roundManager.handViewer.RebuildSmart();
                        }
                    }
                    break;

                case OperationType.AddStrength:
                    if (playerData != null)
                    {
                        playerData.AddStrength(value);
                        Debug.Log($"[PlayerManager] Player gained {value} strength. Total strength: {playerData.strength}");
                    }
                    break;

                case OperationType.EndTurn:
                    Debug.Log($"[PlayerManager] Card effect triggered: End Turn immediately");
                    // Find RoundManager and call EndPlayerTurn
                    if (roundManager != null)
                    {
                        roundManager.EndPlayerTurn();
                    }
                    else
                    {
                        Debug.LogWarning("[PlayerManager] Cannot end turn - RoundManager not found");
                    }
                    break;

                default:
                    Debug.LogWarning($"[PlayerManager] OperationType {effect.operationType} not yet implemented");
                    break;
            }
        }
    }

    public bool TryPlayCard(CardData card, ref EntityData target)
    {
        // Legacy method - kept for compatibility, delegates to new PlayCard method
        return PlayCard(card, null, null);
    }

    public void EndTurn()
    {
        cardManager.DiscardCardPile();
    }
}
