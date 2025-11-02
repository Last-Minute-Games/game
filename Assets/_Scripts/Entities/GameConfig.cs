using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    /// <summary>
    /// This file holds all configuration options for The Nether.
    /// If you want to tweak some global variable bullshit please add it here.
    /// </summary>
    [Header("Player Defaults")]
    public int defaultBaseEnergy = 3;
    public int defaultMaxEnergy = 3;

    [Header("Card Settings")]
    public int defaultHandSize = 5;

    [Header("Battle Settings")]
    public float turnDuration = 15f;
}
