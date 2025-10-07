using UnityEngine;

[CreateAssetMenu(fileName = "New Defense Card", menuName = "Cards/Defense Card")]
public class DefenseCard : CardBase
{
    [Header("Defense Settings")]
    public int blockAmount = 10;

    public override void Use(CharacterBase user, CharacterBase target)
    {
        if (user == null)
        {
            Debug.LogWarning($"{cardName}: No user found!");
            return;
        }

        user.AddBlock(blockAmount);
        Debug.Log($"{user.characterName} uses {cardName} and gains {blockAmount} block!");
    }
}
