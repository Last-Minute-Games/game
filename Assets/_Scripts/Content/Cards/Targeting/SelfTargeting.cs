using UnityEngine;
[CreateAssetMenu(menuName = "Cards/Targeting/Self")]
public class SelfTargeting : TargetingRule {
    public override CharacterBase ResolveTarget(CharacterBase user, Collider2D hit) {
        return user;
    }
}
