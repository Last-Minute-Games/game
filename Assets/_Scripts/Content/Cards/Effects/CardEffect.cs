using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    protected abstract void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale);

    public void Apply(CardRunner runner, CharacterBase user, CharacterBase target)
    {
        if (runner == null || runner.data == null || user == null) return;

        CardData source = runner.data; // treat as read-only reference

        // 1️⃣ Compute potency
        float randomScale = Random.Range(source.minMultiplier, source.maxMultiplier);
        float globalScale = 1f;

        if (user is Enemy e)
            globalScale = e.globalPowerScale;
        else if (user is Player p)
            globalScale = p.globalPowerScale;

        float totalScale = randomScale * globalScale;
        runner.cachedPotency = totalScale;

        // 2️⃣ Dynamic prefix (for naming only)
        float range = source.maxMultiplier - source.minMultiplier;
        string prefix = "";
        if (range > 0.5f)
        {
            float normalized = (randomScale - source.minMultiplier) / range;
            if (normalized < 0.33f) prefix = "Poor ";
            else if (normalized > 0.66f) prefix = "Potent ";
        }

        string cardDisplayName = prefix + source.cardName.Split(' ')[^1];

        // 3️⃣ Substitute potency-adjusted value into display-only copies
        float effectiveValue = GetEffectiveValue(totalScale);
        string coloredValue = $"<color=#FFCF40>{effectiveValue:F0}</color>";

        string displayDescription = source.description;
        string displayIntention = source.intentionText;

        if (!string.IsNullOrEmpty(displayDescription) && displayDescription.Contains("<X>"))
            displayDescription = displayDescription.Replace("<X>", coloredValue);

        if (!string.IsNullOrEmpty(displayIntention) && displayIntention.Contains("<X>"))
            displayIntention = displayIntention.Replace("<X>", coloredValue);

        // 4️⃣ Apply the gameplay effect (unchanged)
        ApplyEffect(user, target, totalScale);

        // 5️⃣ Debug log for testing
        Debug.Log(
            $"[CardEffect] {cardDisplayName} → {displayDescription}\n" +
            $"   total={effectiveValue:F0} (scale={totalScale:F2})"
        );

        // (Optional: If you want UI or Tooltip to see these dynamic versions,
        // pass them through TooltipManager or CardRunner, not by editing data.)
    }

    protected virtual float GetEffectiveValue(float totalScale) => 0f;
}
