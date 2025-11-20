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
    public bool isAlive;

    public Vector3 worldPosition;  // Assigned by PlayerPrefab / EnemyRender
    public bool isPlayer;          // True for player, false for enemy

    public virtual void Initialize(string name, int maxHealth)
    {
        this.name = name;
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        block = 0;
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
    }

    public virtual void GainBlock(int amount)
    {
        block += amount;

        TooltipManager.SpawnTooltip(
            worldPosition,
            "+" + amount + " BLOCK",
            Color.cyan,
            isPlayer ? TooltipDirection.Down : TooltipDirection.Up
        );
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
}
