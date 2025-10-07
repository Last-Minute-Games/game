using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandView : MonoBehaviour
{
    [Header("Layout")]
    public float spacing = 1.5f;         // horizontal spacing like old version
    public float curveHeight = 1.5f;     // how much curvature (vertical bend)
    public float verticalOffset = 2f;    // raise entire hand
    public float liftAmount = 1.2f;      // how high hovered card floats
    public float animationTime = 0.25f;

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
            // Position in a gentle arc
            float x = startX + i * spacing;
            float y = verticalOffset - Mathf.Pow(x / totalWidth * 2f, 2f) * curveHeight;

            Vector3 targetPos = transform.position + new Vector3(x, y, 0f);
            Quaternion targetRot = Quaternion.Euler(0, 0, -x * 2f); // small rotation proportional to x

            cards[i].transform.DOLocalMove(targetPos, animationTime).SetEase(Ease.OutCubic);
            cards[i].transform.DOLocalRotateQuaternion(targetRot, animationTime).SetEase(Ease.OutCubic);
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
                c.transform.DOScale(1.1f, 0.15f);
                c.sortingGroup.sortingOrder = 100;
            }
            else
            {
                c.transform.DOScale(1f, 0.15f);
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
