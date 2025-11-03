using UnityEngine;

public class CardAnimationHelper : MonoBehaviour
{
    [Header("References")]
    public CardArrowHelper arrowHelper;

    // ────────────────────────────────
    // Core Animation Actions
    // ────────────────────────────────
    public void AnimateDraw(CardPrefab card) { }
    public void HoverVisuals(CardPrefab card) { }
    public void SelectVisuals(CardPrefab card) { }
    public void DragFollowMouseWithCard(CardPrefab card, Vector2 cursorPos) { }
    public void DragFollowMouseWithArrow(CardPrefab card, Vector2 cursorPos) { }
    public void ReturnToPosition(CardPrefab card) { }
    public void AnimateDiscard(CardPrefab card) { }
    public void PlayRelease(CardPrefab card) { }
}
