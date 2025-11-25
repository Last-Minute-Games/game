using System.Collections.Generic;
using UnityEngine;

namespace Entities.Players.Data
{
    /// <summary>
    /// Weighted card entry for controlling spawn rates.
    /// Higher weight = more common in deck.
    /// </summary>
    [System.Serializable]
    public class WeightedCard
    {
        [Tooltip("The card to spawn")]
        public CardData card;
        
        [Tooltip("Spawn weight (higher = more common). Example: 200 = very common, 20 = rare")]
        [Range(1, 1000)]
        public int weight = 100;
    }

    [CreateAssetMenu(menuName = "Player/Player Config", fileName = "NewPlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("References")]
        [Tooltip("Reference to global configuration settings.")]
        public GameConfig config;

        [Header("Core Stats")]
        public string playerName = "Player";
        public int baseHealth;
        public int baseShield;

        [Header("Energy Settings")]
        [Tooltip("Starting energy")]
        public int baseEnergy;

        [Tooltip("Maximum energy cap")]
        public int maxEnergy;

        [Header("Deck Configuration")]
        [Tooltip("Weighted cards - higher weight = more common in deck")]
        public List<WeightedCard> weightedCards = new List<WeightedCard>();
        
        [Header("Legacy Support")]
        [Tooltip("Old card list (deprecated - use weightedCards instead)")]
        public List<CardData> usableCards = new List<CardData>();

        private void OnValidate()
        {
            if (config != null)
            {
                // Pull defaults from GameConfig
                if (baseHealth <= 0) baseHealth = config.defaultHealth;
                if (baseShield < 0) baseShield = config.defaultShield;
                if (baseEnergy <= 0) baseEnergy = config.defaultBaseEnergy;
                if (maxEnergy <= 0) maxEnergy = config.defaultMaxEnergy;

                // Populate cards from config if none assigned (legacy)
                if ((usableCards == null || usableCards.Count == 0) && 
                    (weightedCards == null || weightedCards.Count == 0) && 
                    config.defaultCards != null)
                {
                    usableCards = new List<CardData>(config.defaultCards);
                }
            }

            // Basic sanity
            if (maxEnergy < baseEnergy) maxEnergy = baseEnergy;
            
            // Migration: Convert old usableCards to weightedCards if needed
            if ((weightedCards == null || weightedCards.Count == 0) && usableCards != null && usableCards.Count > 0)
            {
                Debug.Log("[PlayerConfig] Migrating usableCards to weightedCards with default weight 100");
                if (weightedCards == null) weightedCards = new List<WeightedCard>();
                
                foreach (var card in usableCards)
                {
                    if (card != null)
                    {
                        weightedCards.Add(new WeightedCard { card = card, weight = 100 });
                    }
                }
                
                // Clear usableCards after migration
                usableCards.Clear();
            }
            
            if (weightedCards == null) weightedCards = new List<WeightedCard>();
            if (weightedCards.Count == 0)
                Debug.LogWarning("PlayerConfig: No weighted cards assigned. Add cards to 'Deck Configuration' section.");
        }

        public PlayerData CreateRuntimeInstance()
        {
            var data = new PlayerData();
            var effectiveName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
            var effectiveMaxHealth = Mathf.Max(1, baseHealth);
        
            data.Initialize(effectiveName, effectiveMaxHealth);
        
            if (baseShield > 0) 
                data.GainBlock(baseShield);
        
            // Copy energy settings
            data.baseEnergy = baseEnergy;
            data.maxEnergy = maxEnergy;
        
            // Build usableCards from weighted cards
            data.usableCards = BuildWeightedCardList();
        
            return data;
        }
        
        /// <summary>
        /// Builds the final card list based on weights.
        /// Cards with higher weights appear more frequently.
        /// </summary>
        private List<CardData> BuildWeightedCardList()
        {
            List<CardData> result = new List<CardData>();
            
            // Use weighted cards if available
            if (weightedCards != null && weightedCards.Count > 0)
            {
                foreach (var weightedCard in weightedCards)
                {
                    if (weightedCard != null && weightedCard.card != null)
                    {
                        // Normalize weight to reasonable deck size (divide by 10 for base copies)
                        // Weight 100 = 10 copies, Weight 200 = 20 copies, Weight 20 = 2 copies
                        int copies = Mathf.Max(1, weightedCard.weight / 10);
                        
                        for (int i = 0; i < copies; i++)
                        {
                            result.Add(weightedCard.card);
                        }
                    }
                }
                
                Debug.Log($"[PlayerConfig] Built deck with {result.Count} cards from {weightedCards.Count} weighted entries");
            }
            else if (usableCards != null && usableCards.Count > 0)
            {
                // Fallback to legacy usableCards
                result = new List<CardData>(usableCards);
                Debug.Log($"[PlayerConfig] Using legacy usableCards ({result.Count} cards)");
            }
            
            return result;
        }
    }
}