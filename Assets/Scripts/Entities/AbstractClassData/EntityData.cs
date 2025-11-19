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
        var remaining = amount - block;
        block = Mathf.Max(0, block - amount);
        if (remaining > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - remaining);
            if (currentHealth == 0) isAlive = false;
        }
    }

    public virtual void GainBlock(int amount)
    {
        block += amount;
    }

    public virtual void Heal(int amount)
    {
        if (!isAlive) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public virtual void ApplyStatus(string statusId, int value)
    {
        // Implement status logic here
    }
}