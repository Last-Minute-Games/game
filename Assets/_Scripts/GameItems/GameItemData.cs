using UnityEngine;
using System.Collections.Generic;

public abstract class GameItemData : ScriptableObject
{
    [Header("Item Info")]
    [Tooltip("Display name of this item.")]
    public string itemName;

    [Tooltip("Description or lore text for this item.")]
    [TextArea(2, 5)]
    public string description;

    [Tooltip("Main artwork displayed for this item.")]
    public Sprite artwork;

    [Tooltip("Small icon used for UI or quick reference.")]
    public Sprite icon;

    [Header("Item Effects")]
    [Tooltip("List of effects this item applies (e.g., stat changes, damage, healing).")]
    public List<EffectData> effectData = new List<EffectData>();

    [Header("Metadata")]
    [Tooltip("Unique identifier for this item.")]
    public int uniqueID;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(itemName))
            Debug.LogWarning($"[GameItemData] {name} is missing an item name!", this);

        if (uniqueID <= 0)
            Debug.LogWarning($"[GameItemData] {name} has no valid Unique ID assigned!", this);

        if (effectData == null || effectData.Count == 0)
            Debug.LogWarning($"[GameItemData] {name} has no EffectData assigned!", this);
    }
}
