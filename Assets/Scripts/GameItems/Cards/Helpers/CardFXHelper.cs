using UnityEngine;

public class CardFXHelper : MonoBehaviour
{
    [Header("Sub-Helpers")]
    public CardSFXHelper sfxHelper;
    public CardAnimationHelper animHelper;

    // Prevents SFX spam while dragging
    private bool dragSoundPlayed = false;

    // ────────────────────────────────
    // Public API (state-based actions)
    // ────────────────────────────────

    // Draw card onto player hand
    public void OnCardDrawn(CardPrefab card)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardFXHelper] OnCardDrawn called with null card.");
            return;
        }

        animHelper?.AnimateDraw(card);
        sfxHelper?.PlayDraw();
    }

    // When hovering over a card
    public void OnCardHover(CardPrefab card)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardFXHelper] OnCardHover called with null card.");
            return;
        }

        animHelper?.HoverVisuals(card);
        sfxHelper?.PlayHover();
    }

    // When selecting (clicking / picking up) a card
    public void OnCardSelect(CardPrefab card)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardFXHelper] OnCardSelect called with null card.");
            return;
        }

        animHelper?.SelectVisuals(card);
        sfxHelper?.PlaySelect();

        // Reset drag SFX gate for the new drag session
        dragSoundPlayed = false;
    }

    // Called every frame while dragging the card
    public void OnCardDrag(CardPrefab card, Vector2 cursorPos)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardFXHelper] OnCardDrag called with null card.");
            return;
        }

        // Decide between dragging card vs showing arrow
        // if (card.targetRule == TargetRule.Enemy)
        // {
        //     animHelper?.DragFollowMouseWithArrow(card, cursorPos);
        // }
        // else
        // {
        //     animHelper?.DragFollowMouseWithCard(card, cursorPos);
        // }
        animHelper?.DragFollowMouseWithArrow(card, cursorPos);

        // Play drag sound only once per drag session
        if (!dragSoundPlayed)
        {
            sfxHelper?.PlayDrag();
            dragSoundPlayed = true;
        }
    }

    // When card is released (played or cancelled)
    public void OnCardRelease(CardPrefab card, bool validTarget)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardFXHelper] OnCardRelease called with null card.");
            return;
        }

        dragSoundPlayed = false; // reset for next drag

        if (validTarget)
        {
            animHelper?.PlayRelease(card);
            sfxHelper?.PlayConfirm();
        }
        else
        {
            animHelper?.ReturnToPosition(card);
            sfxHelper?.PlayCancel();
        }
    }

    // When card is discarded or removed from hand (visually, needs data to be handled via manager)
    public void OnCardDiscard(CardPrefab card)
    {
        if (card == null)
        {
            Debug.LogWarning("[CardFXHelper] OnCardDiscard called with null card.");
            return;
        }

        animHelper?.AnimateDiscard(card);
        sfxHelper?.PlayDiscard();
    }
}
