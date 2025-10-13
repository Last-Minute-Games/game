using UnityEngine;
// Heal
[CreateAssetMenu(menuName = "Cards/Effects/Heal")]
public class HealEffect : CardEffect {
    public int amount = 12;
    public override void Apply(CharacterBase user, CharacterBase target) {
        user?.Heal(amount);
    }
}
