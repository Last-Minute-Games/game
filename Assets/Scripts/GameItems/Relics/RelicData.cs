using UnityEngine;

[CreateAssetMenu(menuName = "Relic/Relic Data", fileName = "NewRelicData")]
public class RelicData : GameItemData
{
    [Header("Relic Settings")]
    [Tooltip("If false, the relic starts disabled and must be activated by an event.")]
    public bool isEnabled = true;
}
