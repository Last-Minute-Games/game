using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/Targeting/All")]
public class AllTargeting : TargetingRule
{
    public override CharacterBase ResolveTarget(CharacterBase user, Collider2D hit)
    {
        // This targeting mode doesn't return one target — handled manually.
        return null;
    }

    // 🔹 Not overriding — just an extra helper
    public List<CharacterBase> ResolveAllTargets(CharacterBase user)
    {
        var results = new List<CharacterBase>();

        if (user is Player)
        {
            results.AddRange(Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None));
        }
        else if (user is Enemy)
        {
            var player = Object.FindFirstObjectByType<Player>();
            if (player != null)
                results.Add(player);
        }

        return results;
    }
}
