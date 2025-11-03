using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Effect Data", fileName = "NewEffectData")]
public class EffectData : ScriptableObject
{
    [Header("Effect Rules")]
    [Tooltip("Determines which entity or group this effect targets.")]
    public TargetRule targetRule = TargetRule.None;

    [Tooltip("Minimum multiplier applied to this effect.")]
    [Range(0f, 10f)] public float minMultiplier = 1f;

    [Tooltip("Maximum multiplier applied to this effect.")]
    [Range(0f, 10f)] public float maxMultiplier = 1f;

    [Header("Effect Data")]
    [Tooltip("Defines what operation this effect performs.")]
    public OperationType operationType = OperationType.None;

    [Tooltip("The base value of this effect before modifiers or scaling.")]
    public int baseValue = 0;

    [Header("Timing")]
    [Tooltip("How long the effect lasts once active (0 = instant).")]
    [Range(0, 20)] public int duration = 0;

    [Tooltip("Whether duration counts in Turns or Rounds.")]
    public TimeUnit durationUnit = TimeUnit.Turns;

    [Tooltip("How long to delay before this effect activates (0 = immediate).")]
    [Range(0, 20)] public int delay = 0;

    [Tooltip("Whether delay counts in Turns or Rounds.")]
    public TimeUnit delayUnit = TimeUnit.Turns;

    [Header("UI & Display")]
    [Tooltip("Color used to highlight this effect's value in card descriptions.")]
    public Color variableColor = Color.white;

    // true value that will be used
    [HideInInspector] public int postCopyValue;

    // --------------------------------------------------
    // Helper Methods
    // --------------------------------------------------

    // helper method for cloning EffectData
    public EffectData Clone(bool applyMultiplier)
    {
        var clone = Instantiate(this);

        float rolledMultiplier = applyMultiplier ? Random.Range(minMultiplier, maxMultiplier) : 1f;
        clone.postCopyValue = Mathf.RoundToInt(baseValue * rolledMultiplier);
        return clone;
    }

    /// <summary>
    /// Returns the HTML hex string representation of variableColor for rich text tags.
    /// Example: "<color=#FF0000>+10</color>"
    /// </summary>
    public string GetColorTag()
    {
        return ColorUtility.ToHtmlStringRGB(variableColor);
    }

    // --------------------------------------------------
    // Validation
    // --------------------------------------------------
    protected void OnValidate()
    {
        // Keep multipliers in valid order
        if (minMultiplier > maxMultiplier)
            minMultiplier = maxMultiplier;

        // Clamp values
        minMultiplier = Mathf.Max(0f, minMultiplier);
        maxMultiplier = Mathf.Max(minMultiplier, maxMultiplier);

        duration = Mathf.Max(0, duration);
        delay = Mathf.Max(0, delay);
    }
}
