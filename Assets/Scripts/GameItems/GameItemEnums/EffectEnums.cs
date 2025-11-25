using UnityEngine;
using System;

public enum TargetRule // do not change target rules specifically
{
    None,
    Self,
    Enemy,
    AllEnemies
}

public enum OperationType // player and enemy manager must define handling all op types
{
    None,
    Damage, 
    AddShield,
    Heal,
    EndTurn,
    ShuffleDeck,
    MultiplyPowerScale,
    AddEnergy,
    DrawCards,
    AddStrength
}

public enum TimeUnit
{
    Turns,
    Rounds
}

[Serializable]
public struct Effect
{
    [Header("Effect Rules")]
    [Tooltip("Determines which entity or group this effect targets.")]
    public TargetRule targetRule;

    [Tooltip("Minimum multiplier applied to this effect.")]
    [Range(0f, 10f)] public float minMultiplier;

    [Tooltip("Maximum multiplier applied to this effect.")]
    [Range(0f, 10f)] public float maxMultiplier;

    [Header("Effect Data")]
    [Tooltip("Defines what operation this effect performs.")]
    public OperationType operationType;

    [Tooltip("The base value of this effect before modifiers or scaling.")]
    public int baseValue;

    [Header("Timing")]
    [Tooltip("How long the effect lasts once active (0 = instant).")]
    [Range(0, 20)] public int duration;

    [Tooltip("Whether duration counts in Turns or Rounds.")]
    public TimeUnit durationUnit;

    [Tooltip("How long to delay before this effect activates (0 = immediate).")]
    [Range(0, 20)] public int delay;

    [Tooltip("Whether delay counts in Turns or Rounds.")]
    public TimeUnit delayUnit;

    [Header("UI & Display")]
    [Tooltip("Color used to highlight this effect's value in card descriptions.")]
    public Color variableColor;

    // Runtime value that will be used after multiplier is applied
    [HideInInspector] public int postCopyValue;

    // --------------------------------------------------
    // Helper Methods
    // --------------------------------------------------

    /// <summary>
    /// Creates a copy of this effect with an optional multiplier applied.
    /// </summary>
    public Effect Clone(bool applyMultiplier)
    {
        Effect clone = this;
        
        float rolledMultiplier = applyMultiplier ? UnityEngine.Random.Range(minMultiplier, maxMultiplier) : 1f;
        clone.postCopyValue = Mathf.RoundToInt(baseValue * rolledMultiplier);
        
        return clone;
    }

    /// <summary>
    /// Returns the HTML hex string representation of variableColor for rich text tags.
    /// Example: "&lt;color=#FF0000&gt;+10&lt;/color&gt;"
    /// </summary>
    public string GetColorTag()
    {
        return ColorUtility.ToHtmlStringRGB(variableColor);
    }

    /// <summary>
    /// Creates a default Effect with sensible values.
    /// </summary>
    public static Effect CreateDefault()
    {
        return new Effect
        {
            targetRule = TargetRule.None,
            minMultiplier = 1f,
            maxMultiplier = 1f,
            operationType = OperationType.None,
            baseValue = 0,
            duration = 0,
            durationUnit = TimeUnit.Turns,
            delay = 0,
            delayUnit = TimeUnit.Turns,
            variableColor = Color.white,
            postCopyValue = 0
        };
    }
}

