using UnityEngine;
using UnityEngine.UI;
using Blackjack;

public class CardViews : MonoBehaviour
{
    public Image image;
    public CardSpriteLibrary library;

    public void Show(Card card, bool faceUp = true)
    {
        if (!faceUp)
        {
            image.sprite = library.cardBack;
        }
        else
        {
            image.sprite = library.GetCardSprite(card.Suit, card.Face);
        }
        image.SetNativeSize(); // optional, or control size via layout
    }
}
