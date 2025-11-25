using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Wave Configuration", fileName = "NewWaveConfig")]
public class WaveConfig : ScriptableObject
{
    [Header("Day 1 Waves")]
    public List<WaveData> day1Waves = new();

    [Header("Day 2 Waves")]
    public List<WaveData> day2Waves = new();

    [Header("Day 3 Waves")]
    public List<WaveData> day3Waves = new();

    [Header("Day 4 Waves")]
    public List<WaveData> day4Waves = new();

    [Header("Day 5 Waves")]
    public List<WaveData> day5Waves = new();

    /// <summary>
    /// Returns the waves for whichever day flag is active.
    /// </summary>
    public List<WaveData> GetWavesForCurrentDay()
    {
        if (GameFlags.HasFlag("day.one"))   return day1Waves;
        if (GameFlags.HasFlag("day.two"))   return day2Waves;
        if (GameFlags.HasFlag("day.three")) return day3Waves;
        if (GameFlags.HasFlag("day.four"))  return day4Waves;
        if (GameFlags.HasFlag("day.five"))  return day5Waves;

        // fallback (should never happen)
        return day1Waves;
    }
}
