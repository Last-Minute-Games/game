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

        Debug.Log($"{user.characterName} dealt {total} damage to {target.characterName} (scale {totalScale:F2})");
    }

    protected override float GetEffectiveValue(float totalScale) => baseDamage * totalScale;
}
