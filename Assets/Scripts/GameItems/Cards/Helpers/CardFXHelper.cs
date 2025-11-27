using UnityEngine;

namespace GameItems.Cards.Helpers
{
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

        // interaction lock for on draw/card pull events
        public static class CardInteraction
        {
            public static bool Locked = false;
        }

        // Draw card onto player hand
        public void OnCardDrawn(CardRender card)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardDrawn called with null card.");
                return;
            }

            animHelper?.AnimateDraw(card);
            sfxHelper?.PlayDraw();
        }

        // When hovering over a card
        public void OnCardHover(CardRender card)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardHover called with null card.");
                return;
            }
            
            // Debug.Log("On Hover");

            animHelper?.HoverVisuals(card);
            sfxHelper?.PlayHover();
        }

        // When hover exits (mouse leaves the card)
        public void OnCardHoverExit(CardRender card)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardHoverExit called with null card.");
                return;
            }

            animHelper?.HoverExit(card);
        }

        // When selecting (clicking / picking up) a card
        public void OnCardSelect(CardRender card, bool updatePosition = true)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardSelect called with null card.");
                return;
            }

            animHelper?.SelectVisuals(card, updatePosition);
            sfxHelper?.PlaySelect();

            // Reset drag SFX gate for the new drag session
            dragSoundPlayed = false;
        }

        // Called every frame while dragging the card
        public void OnCardDrag(CardRender card, Vector2 cursorPos)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardDrag called with null card.");
                return;
            }

            // Decide between dragging card vs showing arrow based on target rule
            if (card.Data != null)
            {
                TargetRule targetRule = card.Data.GetDominatingTargetRule();
                if (targetRule == TargetRule.Enemy)
                {
                    animHelper?.DragFollowMouseWithArrow(card, cursorPos);
                }
                else
                {
                    animHelper?.DragFollowMouseWithCard(card, cursorPos);
                }
            }
            else
            {
                // Default to no arrow if no data
                animHelper?.DragFollowMouseWithCard(card, cursorPos);
            }

            // Play drag sound only once per drag session
            if (!dragSoundPlayed)
            {
                sfxHelper?.PlayDrag();
                dragSoundPlayed = true;
            }
        }

        // When card is released (played or cancelled)
        public void OnCardRelease(CardRender card, bool validTarget)
        {
            if (CardInteraction.Locked) return;
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
        public void OnCardDiscard(CardRender card)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardDiscard called with null card.");
                return;
            }

            animHelper?.AnimateDiscard(card);
            sfxHelper?.PlayDiscard();
        }

        // On card exit, ensure Hover Visuals are reversed.
        public void OnCardExit(CardRender card)
        {
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardExit called with null card.");
                return;
            }

            animHelper?.HoverExit(card);
            
            // Clear enemy hover sprites in case the card had arrow showing
            animHelper?.ClearEnemyHoverSprites();
        }
    }
}
