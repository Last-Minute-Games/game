using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Targeting/Enemy Only")]
public class EnemyOnlyTargeting : TargetingRule
{
    public override CharacterBase ResolveTarget(CharacterBase user, Collider2D hit)
    {
        // If the USER is the player, target enemies.
        if (user is Player)
        {
            return hit ? hit.GetComponent<Enemy>() : null;
        }

        // If the USER is an enemy, target the player.
        if (user is Enemy)
        {
            return Object.FindFirstObjectByType<Player>();
        }

        return null;
    }
}
