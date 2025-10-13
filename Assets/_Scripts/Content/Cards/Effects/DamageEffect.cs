using UnityEngine;
// Damage
[CreateAssetMenu(menuName = "Cards/Effects/Damage")]
public class DamageEffect : CardEffect {
    public int baseDamage = 10;
    public override void Apply(CharacterBase user, CharacterBase target) {
        if (target == null) return;
        int total = baseDamage + (user != null ? user.strength : 0);
        target.TakeDamage(total);
        target.ShowDamageFeedback(total);
    }
}
