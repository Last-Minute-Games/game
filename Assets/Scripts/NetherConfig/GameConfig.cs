using UnityEngine;
using System.Collections.Generic;

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

    [Tooltip("List of starting relics.")]
    public List<RelicData> defaultRelics = new List<RelicData>();

    [Header("Card Settings")]
    [Tooltip("Default number of cards drawn at the start of a turn (excluding guaranteed cards).")]
    public int defaultHandSize = 5;

    [Tooltip("List of usable cards with associated draw weights.")]
    public List<CardDrawEntry> defaultCards = new List<CardDrawEntry>();

    [Tooltip("List of guaranteed pulled cards every turn (always drawn before random cards).")]
    public List<CardDrawEntry> guaranteedCards = new List<CardDrawEntry>();

    [Header("Battle Settings")]
    [Tooltip("Duration (in seconds) for each turn before auto-advance.")]
    public float turnDuration = 15f;
}
