using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public int baseDamage = 10;

    protected override void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale)
    {
        if (target == null) return;
        int total = Mathf.RoundToInt(baseDamage * totalScale + (user != null ? user.strength : 0));
        target.TakeDamage(total);
        target.ShowDamageFeedback(total);
    }

    protected override int GetBaseValue() => baseDamage;

    public override int PreviewAmount(CardRunner runner, CharacterBase user, float totalScale)
    {
        int total = Mathf.RoundToInt(baseDamage * totalScale + (user != null ? user.strength : 0));
        return total;
    }
}
