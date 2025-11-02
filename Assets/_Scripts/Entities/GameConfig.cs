using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    /// <summary>
    /// This file holds all STARTING configuration options for The Nether.
    /// If you want to tweak some global variable bullshit please add it here.
    /// </summary>
    [Header("Player Defaults")]
    public int defaultHealth = 100;
    public int defaultShield = 0;
    public int defaultBaseEnergy = 3;
    public int defaultMaxEnergy = 3;

    [Tooltip("List of starting cards.")]
    public List<CardData> defaultCards = new List<CardData>();

    [Tooltip("List of starting relics.")]
    public List<RelicData> defaultRelics = new List<RelicData>();

    [Header("Card Settings")]
    public int defaultHandSize = 5;

    [Header("Battle Settings")]
    public float turnDuration = 15f;

}
