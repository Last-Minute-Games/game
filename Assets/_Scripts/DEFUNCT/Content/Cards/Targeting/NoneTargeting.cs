using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Targeting/None")]
public class NoneTargeting : TargetingRule
{
    public override CharacterBase ResolveTarget(CharacterBase user, Collider2D hit)
    {
        return null; // Always no target
    }
}
