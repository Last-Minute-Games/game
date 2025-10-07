using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandView : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("How far apart cards are horizontally")]
    public float spacing = 1.5f;
    [Tooltip("How deep the arc bends vertically")]
    public float curveHeight = 1.2f;
    [Tooltip("Move entire hand up/down")]
    public float baseHeight = -5.0f;
    [Tooltip("Hover lift height")]
    public float liftAmount = 0.8f;
    [Tooltip("Animation speed")]
    public float animationTime = 0.25f;
    [Tooltip("Scale of all cards in the hand")]
    public float cardScale = 0.8f; // 1 = normal size, <1 smaller, >1 larger

    private readonly List<CardView> cards = new();
    private CardView hoveredCard = null;

    public IEnumerator AddCard(CardView card)
    {
        cards.Add(card);
        yield return ReorganizeHand();
    }

    public void RemoveCard(CardView card)
    {
        if (cards.Contains(card))
        {
            cards.Remove(card);
            StartCoroutine(ReorganizeHand());
        }
    }

    public IEnumerator ReorganizeHand()
    {
        if (cards.Count == 0) yield break;

        float totalWidth = (cards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            float x = startX + i * spacing;
            float normalized = cards.Count > 1 ? (float)i / (cards.Count - 1) : 0.5f;

            // smoother arc (using sine instead of cosine gives nicer spread)
            float y = baseHeight + Mathf.Sin(normalized * Mathf.PI) * curveHeight;

            Vector3 targetPos = new Vector3(x, y, 0f);
            Quaternion targetRot = Quaternion.Euler(0, 0, (normalized - 0.5f) * -30f);

            cards[i].transform.DOLocalMove(targetPos, animationTime).SetEase(Ease.OutCubic);
            cards[i].transform.DOLocalRotateQuaternion(targetRot, animationTime).SetEase(Ease.OutCubic);
            cards[i].transform.DOScale(Vector3.one * cardScale, animationTime).SetEase(Ease.OutCubic);
            cards[i].sortingGroup.sortingOrder = i;
        }

        yield return new WaitForSeconds(animationTime);
    }

    public void OnHover(CardView card)
    {
        if (hoveredCard == card) return;
        hoveredCard = card;

        foreach (CardView c in cards)
        {
            if (c == hoveredCard)
            {
                c.transform.DOLocalMoveY(c.transform.localPosition.y + liftAmount, 0.15f);
                c.transform.DOScale(Vector3.one * cardScale * 1.1f, 0.15f);
                c.sortingGroup.sortingOrder = 100;
            }
            else
            {
                c.transform.DOScale(Vector3.one * cardScale, 0.15f);
                c.sortingGroup.sortingOrder = cards.IndexOf(c);
            }
        }
    }

    public void OnHoverExit(CardView card)
    {
        if (hoveredCard != card) return;
        hoveredCard = null;
        StartCoroutine(ReorganizeHand());
    }
}
