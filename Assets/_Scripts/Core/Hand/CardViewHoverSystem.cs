using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

    public void Show(CardBase card, Vector3 position)
    {
        if (card == null || cardViewHover == null) return;

        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position;
    }

    public void Hide()
    {
        if (cardViewHover != null)
            cardViewHover.gameObject.SetActive(false);
    }
}
