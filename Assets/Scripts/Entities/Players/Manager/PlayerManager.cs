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
        int startingDeckSize = 30;
        cardManager.drawPile = cardManager.GenerateRandomCards(startingDeckSize);
        cardManager.ShuffleDrawPile();
    }

    /// <summary>
    /// Strength tick, energy and block reset — no draw. Drawing is driven by NetherBattleActionBridge / NetherDrawCardsGA for sequential animations.
    /// </summary>
    public void ApplyTurnStartResources()
    {
        if (playerData == null) return;

        playerData.TickPoisonAtTurnStart();

        if (playerData.strength > 0)
            playerData.LoseStrength(1);

        playerData.ResetEnergy();
        playerData.block = 0;
    }

    public int GetDefaultHandDrawCount()
    {
        return playerConfig != null && playerConfig.config != null
            ? Mathf.Max(1, playerConfig.config.defaultHandSize)
            : 5;
    }

    /// <summary>
    /// Applies turn-start rules and draws a full hand immediately (no sequential animation).
    /// </summary>
    public void StartTurn()
    {
        ApplyTurnStartResources();
        int handSize = GetDefaultHandDrawCount();
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
        // Notify RoundManager that a card is being played
        // This prevents the timer from ending the turn during card play
        var roundManager = FindFirstObjectByType<RoundManager>();
        if (roundManager != null)
        {
            roundManager.SetCardPlayingState(true);
        }

        if (playerData == null || cardData == null)
        {
            Debug.LogWarning("[PlayerManager] Cannot play card - playerData or cardData is null");
            if (roundManager != null)
                roundManager.SetCardPlayingState(false);
            return false;
        }

        // Check energy cost
        int energyCost = cardData.energyCost;
        if (!playerData.SpendEnergy(energyCost))
        {
            Debug.LogWarning($"[PlayerManager] Not enough energy to play {cardData.name}. Need {energyCost}, have {playerData.currentEnergy}");
            if (roundManager != null)
                roundManager.SetCardPlayingState(false);
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
        
        // Card play is complete - allow timer to end turn if it expired
        if (roundManager != null)
            roundManager.SetCardPlayingState(false);
        
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

        bool useRolledInstanceValues = cardInstance != null && cardInstance.rolledEffects != null &&
                                       cardInstance.rolledEffects.Count > 0 && ReferenceEquals(effectsToApply, cardInstance.rolledEffects);

        foreach (var effect in effectsToApply)
        {
            int value = useRolledInstanceValues ? effect.postCopyValue : effect.baseValue;
            int secondaryVal = useRolledInstanceValues ? effect.postCopySecondaryValue : effect.secondaryValue;
            int hits = Mathf.Max(1, effect.hitCount);

            var roundManager = FindFirstObjectByType<RoundManager>();
            
            switch (effect.operationType)
            {
                case OperationType.Damage:
                {
                    int strBonus = playerData != null ? playerData.strength : 0;
                    int damagePerHit = value + strBonus;

                    if (effect.targetRule == TargetRule.Self)
                    {
                        if (playerData == null)
                        {
                            Debug.LogWarning("[PlayerManager] Self-damage effect but playerData is null");
                            break;
                        }

                        var sfxSelf = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                        for (int h = 0; h < hits; h++)
                        {
                            sfxSelf?.OnCardAttack();
                            CameraShake.Shake();
                            playerData.TakeDamage(damagePerHit);
                        }

                        Debug.Log($"[PlayerManager] Player took {damagePerHit}x{hits} self-damage from '{cardData.itemName}'. HP: {playerData.currentHealth}/{playerData.maxHealth}");
                    }
                    else if (targetEnemy != null && targetEnemy.data != null)
                    {
                        var sfx = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                        for (int h = 0; h < hits; h++)
                        {
                            sfx?.OnCardAttack();
                            if (targetEnemy.data.block > 0)
                                CameraShake.Shake();

                            targetEnemy.data.TakeDamage(damagePerHit);

                            var enemyManager = FindFirstObjectByType<Entities.Enemies.Manager.EnemyManager>();
                            if (enemyManager != null)
                                enemyManager.UpdateEnemyHealth(targetEnemy.data);
                            else
                            {
                                targetEnemy.UpdateHealth();
                                if (targetEnemy.data.isAlive)
                                    targetEnemy.PlayHurt();
                                else
                                    targetEnemy.PlayDeath();
                            }
                        }

                        Debug.Log($"[PlayerManager] Dealt {damagePerHit} x {hits} hits to {targetEnemy.data.enemyName}. HP: {targetEnemy.data.currentHealth}/{targetEnemy.data.maxHealth}");
                    }
                    else
                        Debug.LogWarning("[PlayerManager] Damage effect requires a valid target (Self or Enemy)");

                    break;
                }

                case OperationType.ApplyPoison:
                {
                    if (effect.targetRule == TargetRule.Enemy && targetEnemy != null && targetEnemy.data != null)
                        targetEnemy.data.AddPoisonStacks(value);
                    else if (effect.targetRule == TargetRule.Self && playerData != null)
                        playerData.AddPoisonStacks(value);
                    else
                        Debug.LogWarning("[PlayerManager] ApplyPoison needs Enemy or Self target.");
                    break;
                }

                case OperationType.LifeSteal:
                {
                    if (targetEnemy == null || targetEnemy.data == null)
                    {
                        Debug.LogWarning("[PlayerManager] LifeSteal requires an enemy target.");
                        break;
                    }

                    int strB = playerData != null ? playerData.strength : 0;
                    int dmg = value + strB;
                    int healAmt = secondaryVal > 0 ? secondaryVal : Mathf.Max(1, value / 2);

                    var sfxLs = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                    sfxLs?.OnCardAttack();
                    if (targetEnemy.data.block > 0)
                        CameraShake.Shake();

                    targetEnemy.data.TakeDamage(dmg);

                    var em = FindFirstObjectByType<Entities.Enemies.Manager.EnemyManager>();
                    if (em != null)
                        em.UpdateEnemyHealth(targetEnemy.data);
                    else
                    {
                        targetEnemy.UpdateHealth();
                        if (targetEnemy.data.isAlive)
                            targetEnemy.PlayHurt();
                        else
                            targetEnemy.PlayDeath();
                    }

                    if (playerData != null)
                    {
                        var sfxHeal = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                        sfxHeal?.OnCardHeal();
                        playerData.Heal(healAmt);
                    }

                    break;
                }

                case OperationType.RecoilStrike:
                {
                    if (targetEnemy == null || targetEnemy.data == null || playerData == null)
                    {
                        Debug.LogWarning("[PlayerManager] RecoilStrike requires player + enemy target.");
                        break;
                    }

                    int strB = playerData.strength;
                    int toEnemy = value + strB;
                    int selfPain = secondaryVal > 0 ? secondaryVal : Mathf.Max(1, value / 4);

                    var sfxR = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                    sfxR?.OnCardAttack();
                    if (targetEnemy.data.block > 0)
                        CameraShake.Shake();

                    targetEnemy.data.TakeDamage(toEnemy);

                    var em2 = FindFirstObjectByType<Entities.Enemies.Manager.EnemyManager>();
                    if (em2 != null)
                        em2.UpdateEnemyHealth(targetEnemy.data);
                    else
                    {
                        targetEnemy.UpdateHealth();
                        if (targetEnemy.data.isAlive)
                            targetEnemy.PlayHurt();
                        else
                            targetEnemy.PlayDeath();
                    }

                    sfxR?.OnCardAttack();
                    CameraShake.Shake();
                    playerData.TakeDamage(selfPain);
                    break;
                }
                case OperationType.AddShield:
                    if (playerData != null)
                    {
                        // Play block sound effect
                        var cardFXHelperBlock = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                        if (cardFXHelperBlock != null)
                        {
                            cardFXHelperBlock.OnCardBlock();
                        }
                        
                        // Camera shake when gaining block
                        CameraShake.Shake();
                        
                        playerData.GainBlock(value);
                        Debug.Log($"[PlayerManager] Player gained {value} block. Total block: {playerData.block}");
                    }
                    break;

                case OperationType.Heal:
                    if (effect.targetRule == TargetRule.Enemy && targetEnemy != null && targetEnemy.data != null)
                    {
                        var cardFXHelperHealE = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                        cardFXHelperHealE?.OnCardHeal();
                        targetEnemy.data.Heal(value);
                        FindFirstObjectByType<Entities.Enemies.Manager.EnemyManager>()?.UpdateEnemyHealth(targetEnemy.data);
                    }
                    else if (playerData != null)
                    {
                        var cardFXHelperHeal = FindFirstObjectByType<GameItems.Cards.Helpers.CardFXHelper>();
                        cardFXHelperHeal?.OnCardHeal();
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
