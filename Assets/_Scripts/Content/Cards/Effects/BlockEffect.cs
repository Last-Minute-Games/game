using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Effects/Block")]
public class BlockEffect : CardEffect
{
    public int baseBlock = 8;

    protected override void ApplyEffect(CharacterBase user, CharacterBase target, float totalScale)
    {
        int total = Mathf.RoundToInt(baseBlock * totalScale);
        user?.AddBlock(total);

        Debug.Log($"{user.characterName} gained {total} Block (scale {totalScale:F2})");
    }
}
