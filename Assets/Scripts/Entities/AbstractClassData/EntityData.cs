using System;
using System.Collections.Generic;

[Serializable]
public struct StatusEffect
{
    public string name;
    public int stacks;
}

[Serializable]
public struct EntityData
{
    public string name;
    public int health;
    public int maxHealth;
    public int block;
    public bool isAlive;

    // Optional: status effects (Weak, Vulnerable, Poison, etc.)
    public List<StatusEffect> statuses;

    public void Initialize(string entityName, int maxHp)
    {
        name = entityName;
        maxHealth = maxHp;
        health = maxHp;
        block = 0;
        isAlive = true;
        statuses = new List<StatusEffect>();
    }

    public void TakeDamage(int amount)
    {
        int remaining = amount;

        if (block > 0)
        {
            int absorbed = Math.Min(block, amount);
            block -= absorbed;
            remaining -= absorbed;
        }

        if (remaining > 0)
        {
            health -= remaining;
            if (health <= 0)
            {
                health = 0;
                isAlive = false;
            }
        }
    }

    public void GainBlock(int amount)
    {
        block += amount;
    }

    public void Heal(int amount)
    {
        health = Math.Min(health + amount, maxHealth);
    }

    public void ResetBlock()
    {
        block = 0;
    }
    
    public void ApplyStatus(string statusName, int stacks)
    {
        int index = statuses.FindIndex(s => s.name == statusName);
        if (index >= 0)
            statuses[index] = new StatusEffect { name = statusName, stacks = statuses[index].stacks + stacks };
        else
            statuses.Add(new StatusEffect { name = statusName, stacks = stacks });
    }

    public void RemoveStatus(string statusName)
    {
        statuses.RemoveAll(s => s.name == statusName);
    }


    public override string ToString()
    {
        return $"{name}: {health}/{maxHealth} HP, {block} Block";
    }
}