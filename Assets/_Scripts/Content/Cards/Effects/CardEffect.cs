using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    protected abstract void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale);

    public void Apply(CharacterBase user, CharacterBase target)
    {
        if (user == null) return;

        float randomScale = 1f;
        float globalScale = 1f;

        CardRunner runner = user.GetComponent<CardRunner>();
        if (runner != null && runner.data != null)
        {
            var data = runner.data;
            float min = data.minMultiplier;
            float max = data.maxMultiplier;

            randomScale = Random.Range(min, max);

            // Determine global scale
            if (user is Enemy e)
                globalScale = e.globalPowerScale;
            else if (user is Player p)
                globalScale = p.globalPowerScale;

            // 🧮 Total scale
            float totalScale = randomScale * globalScale;

            // 💬 Rename card dynamically
            string prefix = "";
            float range = max - min;

            if (range > 0.5f) // only meaningful spread
            {
                float normalized = (randomScale - min) / range;
                if (normalized < 0.33f) prefix = "Poor ";
                else if (normalized > 0.66f) prefix = "Potent ";
            }

            string originalName = data.cardName;
            runner.data.cardName = prefix + originalName;

            // Cache potency info for tooltip display
            runner.cachedPotency = totalScale;

            // Apply effect
            ApplyEffect(user, target, totalScale);

            Debug.Log($"[CardEffect] {runner.data.cardName} scale={totalScale:F2} (random={randomScale:F2}, global={globalScale:F2})");
        }
    }
}
