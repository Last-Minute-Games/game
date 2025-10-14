using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Block")]
public class BlockEffect : CardEffect
{
    public int baseBlock = 8;

    protected override void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale)
    {
        int total = Mathf.RoundToInt(baseBlock * totalScale);
        user?.AddBlock(total);
    }

    protected override int GetBaseValue() => baseBlock;
}
