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

        [Header("Collections")]
        public List<CardData> usableCards = new List<CardData>();

        public event Action OnEnergyChanged;

        // Current energy (runtime state)
        public int currentEnergy;

        public void Initialize(string playerName, int maxHealth, int energy, int maxEnergyLimit)
        {
            // Call base entity initialization
            base.Initialize(playerName, maxHealth);

            baseEnergy = energy;
            maxEnergy = maxEnergyLimit;
            currentEnergy = baseEnergy;
        }

        public void ResetEnergy()
        {
            currentEnergy = baseEnergy;
            OnEnergyChanged?.Invoke();
        }

        public void GainEnergy(int amount)
        {
            currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
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
