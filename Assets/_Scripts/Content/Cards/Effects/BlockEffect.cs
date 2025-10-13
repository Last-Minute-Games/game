using UnityEngine;
// Block
[CreateAssetMenu(menuName = "Cards/Effects/Block")]
public class BlockEffect : CardEffect {
    public int block = 8;
    public override void Apply(CharacterBase user, CharacterBase target) {
        user?.AddBlock(block);
    }
}
