using UnityEngine;

public abstract class CardBase : MonoBehaviour
{
    public string cardName = "Attack Card";
    public int energy = 1; // energy cost to play this card

    // Must implement Use()
    public abstract void Use(CharacterBase user, CharacterBase target);
}
