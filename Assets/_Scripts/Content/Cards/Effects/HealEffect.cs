using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Heal")]
public class HealEffect : CardEffect
{
    public int baseHeal = 12;

    protected override void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale)
    {
        int total = Mathf.RoundToInt(baseHeal * totalScale);
        user?.Heal(total);
    }

    protected override int GetBaseValue() => baseHeal;
}
