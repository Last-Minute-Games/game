using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Layout")]
    public float spacing = 160f;          // horizontal spacing between card centers
    public float baselineY = 0f;          // y offset inside hand panel
    public float lerpSpeed = 12f;

    [Header("Hover")]
    public float hoverScale = 1.4f;       // scale of hovered card
    public float normalScale = 1f;
    public float hoverLift = 80f;         // vertical lift for hovered card
    public float adjacentPush = 70f;      // how much neighbors shift aside
    public float otherScale = 0.95f;      // slight shrink for non-hovered cards

    private List<CardHover> cards = new List<CardHover>();
    private int hoveredIndex = -1;

    void Start()
    {
        RefreshCards();
    }

    public void RefreshCards()
    {
        cards = GetComponentsInChildren<CardHover>(true).ToList();
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].hand = this;
            // make sure initial scale/positions are predictable
            cards[i].rectTransform.anchoredPosition = Vector2.zero;
            cards[i].rectTransform.localScale = Vector3.one * normalScale;
        }
    }

    public void OnCardPointerEnter(CardHover card)
    {
        hoveredIndex = cards.IndexOf(card);
        if (hoveredIndex >= 0)
        {
            // bring hovered card to front visually
            card.rectTransform.SetAsLastSibling();
        }
    }

    public void OnCardPointerExit(CardHover card)
    {
        int idx = cards.IndexOf(card);
        if (idx == hoveredIndex) hoveredIndex = -1;
    }

    void Update()
    {
        if (cards == null || cards.Count == 0) return;

        int count = cards.Count;
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform rt = cards[i].rectTransform;
            // base target
            Vector2 targetPos = new Vector2(startX + i * spacing, baselineY);
            float targetScale = normalScale;
            float targetRotation = 0f;

            if (hoveredIndex != -1)
            {
                if (i == hoveredIndex)
                {
                    targetScale = hoverScale;
                    targetPos.y += hoverLift;
                    // hovered card should be drawn on top (already SetAsLastSibling in enter)
                }
                else
                {
                    // push others away from hovered card
                    if (i < hoveredIndex) targetPos.x -= adjacentPush;
                    else if (i > hoveredIndex) targetPos.x += adjacentPush;
                    targetScale = otherScale;
                }
            }

            // smooth transform
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, Time.deltaTime * lerpSpeed);
            Vector3 scl = Vector3.one * targetScale;
            rt.localScale = Vector3.Lerp(rt.localScale, scl, Time.deltaTime * lerpSpeed);
            rt.localEulerAngles = Vector3.Lerp(rt.localEulerAngles, new Vector3(0, 0, targetRotation), Time.deltaTime * lerpSpeed);
        }
    }
}
