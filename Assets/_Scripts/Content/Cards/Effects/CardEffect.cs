using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    /// <summary>
    /// Core application logic per effect (implemented by subclasses)
    /// </summary>
    protected abstract void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale);

    /// <summary>
    /// Entry point used by CardRunner.
    /// Handles scaling for both cards and enemies automatically.
    /// </summary>
    public void Apply(CharacterBase user, CharacterBase target)
    {
        if (user == null) return;

        float randomScale = 1f;
        float globalScale = 1f;

        // 🔹 Step 1: Pull random card-based multiplier
        CardRunner runner = user.GetComponent<CardRunner>();
        if (runner != null && runner.data != null)
        {
            var data = runner.data;
            randomScale = Random.Range(data.minMultiplier, data.maxMultiplier);

            // 🔹 Step 2: Apply global scaling for enemy (boss modifier)
            if (user is Enemy e && e.data != null)
                globalScale = e.data.globalCardMultiplier;
        }

        float totalScale = randomScale * globalScale;
        ApplyEffect(user, target, totalScale);


        Debug.Log($"[CardEffect] {user.characterName} plays card with total scale {totalScale:F2}");
    }
}
