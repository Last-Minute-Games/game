using UnityEngine;

[System.Serializable]
public class EffectData
{
    public EffectType effectType;
    public int magnitude;
    public int duration; // optional (for poison, weak, etc.)
}

public enum EffectType
{
    Damage,
    Block,
    Heal,
    Draw,
    ApplyStatus
}