using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    protected abstract void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale);

    // Add a new overload that accepts CardRunner context
    public void Apply(CardRunner runner, CharacterBase user, CharacterBase target)
    {
        if (runner == null || runner.data == null || user == null) return;

        var data = runner.data;

        float randomScale = Random.Range(data.minMultiplier, data.maxMultiplier);
        float globalScale = 1f;

        if (user is Enemy e)
            globalScale = e.globalPowerScale;
        else if (user is Player p)
            globalScale = p.globalPowerScale;

        float totalScale = randomScale * globalScale;

        // Optional dynamic naming
        float range = data.maxMultiplier - data.minMultiplier;
        string prefix = "";
        if (range > 0.5f)
        {
            float normalized = (randomScale - data.minMultiplier) / range;
            if (normalized < 0.33f) prefix = "Poor ";
            else if (normalized > 0.66f) prefix = "Potent ";
        }

        string originalName = data.cardName.Replace("Poor ", "").Replace("Potent ", "");
        data.cardName = prefix + originalName;

        runner.cachedPotency = totalScale;

        // Apply final effect
        ApplyEffect(user, target, totalScale);

        Debug.Log($"[CardEffect] {data.cardName} scale={totalScale:F2} (random={randomScale:F2}, global={globalScale:F2})");
    }
}
