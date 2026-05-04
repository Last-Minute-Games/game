using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StatusEffect
{
    public string name;
    public int stacks;
}


[Serializable]
public class EntityData
{
    public string name;
    public int maxHealth;
    public int currentHealth;
    public int block;
    public int blockAge; // Tracks how many enemy turns this block has persisted
    public bool isAlive;

    public Vector3 worldPosition;  // Assigned by PlayerPrefab / EnemyRender
    public bool isPlayer;          // True for player, false for enemy

    public virtual void Initialize(string name, int maxHealth)
    {
        this.name = name;
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        block = 0;
        blockAge = 0;
        isAlive = true;
    }

    public virtual void TakeDamage(int amount)
    {
        int originalAttack = amount;
        int blocked = Mathf.Min(block, amount);
        int remaining = amount - block;

        block = Mathf.Max(0, block - amount);

        if (remaining > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remaining);
            if (currentHealth == 0) isAlive = false;

            // Case: damage passes through block → "-(amount-block)"
            TooltipManager.SpawnTooltip(
                worldPosition,
                "-" + remaining,
                Color.red,
                isPlayer ? TooltipDirection.Down : TooltipDirection.Up
            );
        }
        else
        {
            // Case: full block → "-X BLOCKED"
            TooltipManager.SpawnTooltip(
                worldPosition,
                "-" + originalAttack + " BLOCKED",
                new Color(0.7f, 0.3f, 1f), // purple
                isPlayer ? TooltipDirection.Down : TooltipDirection.Up
            );
        }
        if (isPlayer)
        {
            // Shake when taking damage - subtle but noticeable
            float shakeDuration = 0.15f + (amount * 0.01f); // Short shake, slightly longer for more damage
            float shakeMagnitude = 0.1f + (amount * 0.02f); // Subtle shake, scales with damage
            CameraShake.Shake(shakeDuration, shakeMagnitude);
        }
    }

    public virtual void GainBlock(int amount)
    {
        block += amount;
        blockAge = 0; // Reset age when gaining new block

        TooltipManager.SpawnTooltip(
            worldPosition,
            "+" + amount + " BLOCK",
            Color.cyan,
            isPlayer ? TooltipDirection.Down : TooltipDirection.Up
        );
        
        if (isPlayer) CameraShake.Shake(0.15f, 0.1f);
    }

    public virtual void Heal(int amount)
    {
        if (!isAlive) return;

        int healed = Mathf.Min(maxHealth - currentHealth, amount);
        if (healed <= 0) return;

        currentHealth += healed;

        TooltipManager.SpawnTooltip(
            worldPosition,
            "+" + healed,
            Color.green,
            isPlayer ? TooltipDirection.Down : TooltipDirection.Up
        );
    }

    public virtual void ApplyStatus(string statusId, int value)
    {
        // Implement status logic here
    }

    // Poison (Slay-the-Spire style: at start of your turn, lose HP equal to stacks, then stacks decrease by 1)

    [Tooltip("Poison stacks; tick at start of this entity's turn via TickPoisonAtTurnStart.")]
    public int poisonStacks;

    public void AddPoisonStacks(int amount)
    {
        if (amount <= 0) return;
        poisonStacks += amount;
        TooltipManager.SpawnTooltip(
            worldPosition,
            "+" + amount + " POISON",
            new Color(0.35f, 0.9f, 0.35f),
            isPlayer ? TooltipDirection.Down : TooltipDirection.Up
        );
    }

    /// <summary>Call at the beginning of this entity's turn (player or individual enemy).</summary>
    public virtual void TickPoisonAtTurnStart()
    {
        if (poisonStacks <= 0) return;
        TakeDamage(poisonStacks);
        poisonStacks--;
    }
}
