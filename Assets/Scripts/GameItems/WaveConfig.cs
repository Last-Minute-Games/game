using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Wave Configuration", fileName = "NewWaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Header("Waves")]
    [Tooltip("Each WaveData defines its own spawn rules. Waves follow this order.")]
    public List<WaveData> waves = new();

    public List<WaveData> GetWaves() => waves;
}
