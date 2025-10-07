using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandView : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("How far apart cards are horizontally")]
    public float spacing = 2.5f;         // increase this for wider spacing
    [Tooltip("How deep the arc bends vertically")]
    public float curveHeight = 1.2f;     // smaller = flatter hand
    [Tooltip("Move entire hand up/down")]
    public float baseHeight = -5.0f;       // pushes all cards lower on screen
    [Tooltip("Hover lift height")]
    public float liftAmount = 0.8f;
    [Tooltip("Animation speed")]
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

        float totalWidth = Mathf.Max((cards.Count - 1) * spacing, 0.01f);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            float x = startX + i * spacing;
            float normalized = (totalWidth == 0) ? 0f : (x / totalWidth);

            // Arc curve — smooth parabola
            float y = baseHeight + Mathf.Cos(normalized * Mathf.PI) * curveHeight;

            Vector3 targetPos = new Vector3(x, y, 0f);
            Quaternion targetRot = Quaternion.Euler(0, 0, -normalized * 25f);

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
