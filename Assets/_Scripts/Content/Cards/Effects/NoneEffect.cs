using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/None")]
public class NoneEffect : CardEffect
{
    protected override void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale)
    {
        // Intentionally does nothing.
    }

    protected override int GetBaseValue() => 0;

    public override int PreviewAmount(CardRunner runner, CharacterBase user, float totalScale)
    {
        return 0; // For tooltip display
    }
}
