using UnityEngine;
using DG.Tweening;

public class CardAnimationHelper : MonoBehaviour
{
    [Header("Arrow Helper")]
    public CardArrowHelper arrowHelper;

    [Header("Visual Settings")]
    public float hoverScale = 1.1f;
    public float selectScale = 1.15f;
    public float dragScale = 1.12f;
    public float returnDuration = 0.25f;
    public float drawDuration = 0.35f;
    public float discardDuration = 0.35f;

    // internal tracking
    private Vector3 originalScale = Vector3.one;
    private Vector3 originalPosition;
    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    // Called by FXHelper.OnCardDrawn()
    public void AnimateDraw(CardPrefab card)
    {
        var rect = card.GetComponent<RectTransform>();
        originalScale = rect.localScale;

        rect.localScale = Vector3.zero;
        rect.DOScale(originalScale, drawDuration).SetEase(Ease.OutBack);
    }

    // Called by FXHelper.OnCardHover()
    public void HoverVisuals(CardPrefab card)
    {
        var rect = card.GetComponent<RectTransform>();
        rect.DOScale(hoverScale, 0.15f);
    }

    // Called by FXHelper.OnCardSelect()
    public void SelectVisuals(CardPrefab card)
    {
        var rect = card.GetComponent<RectTransform>();
        originalPosition = rect.localPosition;
        originalScale = rect.localScale;

        rect.DOScale(selectScale, 0.15f);
    }

    // Basic drag following cursor (NO ARROW)
    public void DragFollowMouseWithCard(CardPrefab card, Vector2 cursorPos)
    {
        var rect = card.GetComponent<RectTransform>();
        rect.position = cursorPos;
        rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one * dragScale, 0.25f);
    }

    // Drag following cursor WITH ARROW (Enemy targeting)
    public void DragFollowMouseWithArrow(CardPrefab card, Vector2 cursorPos)
    {
        DragFollowMouseWithCard(card, cursorPos);

        if (arrowHelper != null)
        {
            arrowHelper.UpdateArrowFrom(card.transform.position, cursorPos);
        }
    }

    // Called when player lets go but target is invalid
    public void ReturnToPosition(CardPrefab card)
    {
        var rect = card.GetComponent<RectTransform>();

        // clear arrow
        arrowHelper?.StopDrawingArrow();

        rect.DOScale(originalScale, 0.15f);
        rect.DOLocalMove(originalPosition, returnDuration).SetEase(Ease.OutCubic);
    }

    // Called when card successfully hits a target
    public void PlayRelease(CardPrefab card)
    {
        var rect = card.GetComponent<RectTransform>();

        arrowHelper?.StopDrawingArrow();

        rect
            .DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnCompl
