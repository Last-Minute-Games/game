using UnityEngine;
using System.Collections.Generic;

public class PlayerPrefab : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Static data defining base stats and configuration.")]
    public PlayerData playerData;

    [Header("Runtime Stats")]
    [ReadOnly] public int currentHealth;
    [ReadOnly] public int currentShield;
    [ReadOnly] public int currentEnergy;

    [Header("Runtime Effects")]
    [Tooltip("Active effects applied to the player during battle.")]
    public List<EffectData> activeEffects = new List<EffectData>();

    // ------------------------------------------------------------------
    // INITIALIZATION
    // ------------------------------------------------------------------

    public void Initialize(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("[PlayerPrefab] Initialization failed: No PlayerData assigned.");
            return;
        }

        playerData = data;

        // Initialize runtime stats
        currentHealth = data.baseHealth;
        currentShield = data.baseShield;
        currentEnergy = data.baseEnergy;

        // Clear previous effects
        activeEffects.Clear();

        Debug.Log($"[PlayerPrefab] Initialized player with {currentHealth} HP, {currentEnergy} Energy.");
    }

    // ------------------------------------------------------------------
    // RUNTIME BEHAVIOR
    // ------------------------------------------------------------------

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        // Shield absorbs first
        int remaining = amount - currentShield;
        currentShield = Mathf.Max(0, currentShield - amount);

        if (remaining > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remaining);
        }

        Debug.Log($"[PlayerPrefab] Took {amount} damage. HP: {currentHealth}, Shield: {currentShield}");
    }

    public void AddShield(int amount)
    {
        if (amount <= 0) return;
        currentShield += amount;
        Debug.Log($"[PlayerPrefab] Gained {amount} Shield. New total: {currentShield}");
    }

    public void RestoreHealth(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, playerData.baseHealth);
        Debug.Log($"[PlayerPrefab] Restored {amount} HP. New total: {currentHealth}");
    }

    public void RestoreEnergy()
    {
        currentEnergy = playerData.maxEnergy;
        Debug.Log($"[PlayerPrefab] Energy reset to {currentEnergy}");
    }

    public void ApplyEffect(EffectData effect)
    {
        if (effect == null) return;
        activeEffects.Add(effect);
        Debug.Log($"[PlayerPrefab] Applied effect: {effect.name}");
    }

    public void ClearEffects()
    {
        activeEffects.Clear();
        Debug.Log("[PlayerPrefab] Cleared all active effects.");
    }

    // ------------------------------------------------------------------
    // DEBUG / UTILITIES
    // ------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerData == null)
            return;

        name = $"PlayerPrefab ({playerData.itemName})";
    }
#endif
}
