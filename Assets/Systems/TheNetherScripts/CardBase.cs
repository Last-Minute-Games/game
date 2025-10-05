using UnityEngine;

public abstract class CardBase : MonoBehaviour
{
    public string cardName;
    public int cost = 1;

    public abstract void Use(CharacterBase user, CharacterBase target);
}
