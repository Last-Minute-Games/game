using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandView : MonoBehaviour
{
    [Header("Layout")]
    public float spacing = 1.5f;
    public float curveHeight = 1.2f;
    public float baseHeight = -5.0f;
    public float liftAmount = 0.8f;
    public float animationTime = 0.25f;
    public float cardScale = 0.8f;

    private readonly List<CardView> cards = new();
    private readonly Dictionary<CardView, Vector3> basePos = new();
    private CardView hoveredCard = null;

    // ID helpers (unique per card)
    private static string LayoutId(CardView c) => $"layout_{c.GetInstanceID()}";
    private static string HoverId(CardView c)  => $"hover_{c.GetInstanceID()}";

    public IEnumerator AddCard(CardView card)
    {
        cards.Add(card);
        yield return ReorganizeHand();
    }

    public void RemoveCard(CardView card)
    {
        if (!cards.Contains(card)) return;
        cards.Remove(card);
        // Kill tweens for this card so it doesn't keep animating after removal
        DOTween.Kill(LayoutId(card));
        DOTween.Kill(HoverId(card));
        StartCoroutine(ReorganizeHand());
    }

    private IEnumerator ReorganizeHand()
    {
        if (cards.Count == 0) yield break;

        float totalWidth = (cards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            var c = cards[i];

            float normalized = cards.Count > 1 ? (float)i / (cards.Count - 1) : 0.5f;
            float x = startX + i * spacing;
            float y = baseHeight + Mathf.Sin(normalized * Mathf.PI) * curveHeight;

            Vector3 targetPos = new Vector3(x, y, 0f);
            Quaternion targetRot = Quaternion.Euler(0, 0, (normalized - 0.5f) * -30f);

            basePos[c] = targetPos;

            // Stop any ongoing tweens for a clean layout pass
            DOTween.Kill(HoverId(c));
            DOTween.Kill(LayoutId(c));

            c.transform.DOLocalMove(targetPos, animationTime)
                       .SetEase(Ease.OutCubic)
                       .SetId(LayoutId(c));

            c.transform.DOLocalRotateQuaternion(targetRot, animationTime)
                       .SetEase(Ease.OutCubic)
                       .SetId(LayoutId(c));

            c.transform.DOScale(Vector3.one * cardScale, animationTime)
                       .SetEase(Ease.OutCubic)
                       .SetId(LayoutId(c));

            c.sortingGroup.sortingOrder = i;
        }

        yield return new WaitForSeconds(animationTime);
    }

    public void OnHover(CardView card)
    {
        if (hoveredCard == card) return;
        hoveredCard = card;

        foreach (var c in cards)
        {
            // Clear previous hover tweens; for the hovered card also stop layout so it doesn’t fight
            DOTween.Kill(HoverId(c));
            if (c == hoveredCard) DOTween.Kill(LayoutId(c));

            // Get this card’s base position
            var b = basePos.TryGetValue(c, out var p) ? p : c.transform.localPosition;

            if (c == hoveredCard)
            {
                // Move to base + lift (absolute), scale up, raise sorting
                c.transform.DOLocalMove(new Vector3(b.x, b.y + liftAmount, b.z), 0.15f)
                           .SetEase(Ease.OutCubic)
                           .SetId(HoverId(c));

                c.transform.DOScale(Vector3.one * cardScale * 1.1f, 0.15f)
                           .SetEase(Ease.OutCubic)
                           .SetId(HoverId(c));

                c.sortingGroup.sortingOrder = 100;
            }
            else
            {
                // Non-hovered cards return to base and base scale
                c.transform.DOLocalMove(b, 0.15f)
                           .SetEase(Ease.OutCubic)
                           .SetId(HoverId(c));

                c.transform.DOScale(Vector3.one * cardScale, 0.15f)
                           .SetEase(Ease.OutCubic)
                           .SetId(HoverId(c));

                c.sortingGroup.sortingOrder = cards.IndexOf(c);
            }
        }
    }

    public void OnHoverExit(CardView card)
    {
        if (hoveredCard != card) return;
        hoveredCard = null;

        // Only this card returns to base; do not reorganize entire hand here
        if (basePos.TryGetValue(card, out var b))
        {
            DOTween.Kill(HoverId(card));
            card.transform.DOLocalMove(b, 0.15f)
                .SetEase(Ease.OutCubic)
                .SetId(HoverId(card));
            card.transform.DOScale(Vector3.one * cardScale, 0.15f)
                .SetEase(Ease.OutCubic)
                .SetId(HoverId(card));
            card.sortingGroup.sortingOrder = cards.IndexOf(card);
        }
    }

    public IEnumerator ClearAllCards()
    {
        // make a snapshot so we can safely iterate
        var toRemove = new List<CardView>(cards);

        foreach (var c in toRemove)
        {
            if (c == null) continue;

            DOTween.Kill(c.transform); // stop any animations immediately

            // fade and shrink nicely
            var sr = c.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.DOFade(0f, 0.15f).SetEase(Ease.OutCubic);
            }

            c.transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (c != null) Destroy(c.gameObject);
                });
        }

        cards.Clear();
        basePos.Clear();
        hoveredCard = null;

        // wait a short bit for animations
        yield return new WaitForSeconds(0.2f);
    }
}
