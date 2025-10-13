using UnityEngine;

public abstract class CardEffect : ScriptableObject {
    public abstract void Apply(CharacterBase user, CharacterBase target);
}
