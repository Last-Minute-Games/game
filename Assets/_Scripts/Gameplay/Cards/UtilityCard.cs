using UnityEngine;

[CreateAssetMenu(fileName = "New Utility Card", menuName = "Cards/Utility Card")]
public class UtilityCard : CardBase
{
    [Header("Utility Settings")]
    public string specialEffectName = "Energy Boost";
    public int energyGain = 1;

    public override void Use(CharacterBase user, CharacterBase target)
    {
        Debug.Log($"{user.characterName} uses {cardName} ({specialEffectName})!");

        // Example: Regain energy
        EnergySystem.Instance.RefillEnergy();
    }
}
