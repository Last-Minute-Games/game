using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Players.Data
{
    [Serializable]
    public class PlayerData : EntityData
    {
        [Header("Energy Settings")]
        public int baseEnergy;
        public int maxEnergy;

        [Header("Combat Stats")]
        public int strength;

        [Header("Collections")]
        public List<CardData> usableCards = new List<CardData>();
        public event Action OnEnergyChanged;
        public event Action OnStatsChanged;

        // Current energy (runtime state)
        public int currentEnergy;

        public void Initialize(string playerName, int maxHp, int energy, int maxEnergyLimit)
        {
            // Call base entity initialization
            base.Initialize(playerName, maxHp);

            baseEnergy = energy;
            maxEnergy = maxEnergyLimit;
            currentEnergy = baseEnergy;
            strength = 0; // Initialize strength
        }

        public void AddStrength(int amount)
        {
            if (amount == 0) return;
            strength += amount;
            OnStatsChanged?.Invoke();
        }

        public void LoseStrength(int amount)
        {
            if (amount == 0) return;
            strength = Mathf.Max(0, strength - amount);
            OnStatsChanged?.Invoke();
        }

        public void ResetEnergy()
        {
            currentEnergy = baseEnergy;
            OnEnergyChanged?.Invoke();
        }

        public void GainEnergy(int amount)
        {
            if (amount <= 0) return;

            currentEnergy += amount;
            OnEnergyChanged?.Invoke();
        }

        public bool SpendEnergy(int amount)
        {
            if (currentEnergy < amount) return false;
            currentEnergy -= amount;
            OnEnergyChanged?.Invoke();
            return true;
        }
    }
}
