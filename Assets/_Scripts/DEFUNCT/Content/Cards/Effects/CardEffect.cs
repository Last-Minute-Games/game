using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    protected abstract void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale);

    // FINAL amounts for UI preview (default: base * scale)
    public virtual int PreviewAmount(CardRunner runner, CharacterBase user, float totalScale)
    {
        return Mathf.RoundToInt(GetBaseValue() * totalScale);
    }

    // Each effect reports its own base value
    protected abstract int GetBaseValue();

    public void Apply(CardRunner runner, CharacterBase user, CharacterBase target)
    {
        if (runner == null || runner.data == null || user == null) return;

        // Use the runner’s single roll + user scaling
        float totalScale = runner.GetTotalScale(user);

        // Apply gameplay
        ApplyEffect(user, target, totalScale);
    }
}
