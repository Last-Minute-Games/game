using System.Collections.Generic;
using UnityEngine;

namespace Entities.Players.Data
{
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

        [Header("Collections")]
        [Tooltip("Cards the player can use (initial deck and pool)")]
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

                // Populate cards from config if none assigned
                if ((usableCards == null || usableCards.Count == 0) && config.defaultCards != null)
                {
                    usableCards = new List<CardData>(config.defaultCards);
                }
            }

            // Basic sanity
            if (maxEnergy < baseEnergy) maxEnergy = baseEnergy;
            if (usableCards == null) usableCards = new List<CardData>();
            if (usableCards.Count == 0)
                Debug.LogWarning("PlayerConfig: No usable cards assigned.");
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
        
            // Copy card list reference
            data.usableCards = usableCards != null ? new List<CardData>(usableCards) : new List<CardData>();
        
            return data;
        }
    }
}