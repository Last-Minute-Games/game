
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Player Defaults")]
    public int defaultBaseEnergy = 3;
    public int defaultMaxEnergy = 3;

    [Header("Card Settings")]
    public int defaultHandSize = 5;

    [Header("Battle Settings")]
    public float turnDuration = 15f;
}
