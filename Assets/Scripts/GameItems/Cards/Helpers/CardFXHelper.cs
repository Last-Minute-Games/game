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
        
        // Prevents SFX spam while hovering
        private CardRender currentlyHoveredCard = null;
        
        // Prevents SFX spam when selecting (OnPointerDown + OnBeginDrag both call select)
        private CardRender currentlySelectedCard = null;
        
        // Prevents draw sound spam when drawing multiple cards per round
        // Must be STATIC so it's shared across all card instances
        private static bool drawSoundPlayed = false;

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
            Debug.Log($"[CardFXHelper] OnCardDrawn called for card '{card?.Data?.name ?? "unknown"}'");
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardDrawn called with null card.");
                return;
            }

            Debug.Log($"[CardFXHelper] OnCardDrawn - animHelper: {animHelper != null}, sfxHelper: {sfxHelper != null}");
            
            // Always animate the card
            animHelper?.AnimateDraw(card);
            
            // Only play draw sound once per round (first card drawn)
            if (!drawSoundPlayed && sfxHelper != null)
            {
                Debug.Log("[CardFXHelper] Playing draw sound (first card of round)");
                sfxHelper.PlayDraw();
                drawSoundPlayed = true;
            }
        }

        // Reset draw sound flag at the start of a new round
        public void ResetDrawSoundFlag()
        {
            Debug.Log($"[CardFXHelper] Draw sound flag reset called (instance method). Before: {drawSoundPlayed}");
            ResetDrawSoundFlagStatic();
        }

        // Static method to reset the draw sound flag
        public static void ResetDrawSoundFlagStatic()
        {
            Debug.Log($"[CardFXHelper] Static reset called. Before: {drawSoundPlayed}");
            drawSoundPlayed = false;
            Debug.Log($"[CardFXHelper] Static reset complete. After: {drawSoundPlayed}");
        }

        // When hovering over a card
        public void OnCardHover(CardRender card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardHover called with null card.");
                return;
            }
            
            // Ignore hover if interactions are locked (wave transitions, etc.)
            if (CardInteraction.Locked)
            {
                Debug.Log($"[CardFXHelper] OnCardHover ignored - card interactions are locked");
                return;
            }
            
            // Ignore hover if card is currently being animated (prevents capturing mid-animation positions)
            if (DG.Tweening.DOTween.IsTweening(card.transform))
            {
                Debug.Log($"[CardFXHelper] OnCardHover ignored - card '{card.Data?.name ?? "unknown"}' is animating");
                return;
            }
            
            // Only play sound and visuals if this is a NEW hover (not the same card)
            if (currentlyHoveredCard != card)
            {
                currentlyHoveredCard = card;
                
                // Debug.Log("On Hover");

                animHelper?.HoverVisuals(card);
                sfxHelper?.PlayHover();
            }
        }

        // When hover exits (mouse leaves the card)
        public void OnCardHoverExit(CardRender card)
        {
            // Don't check lock - user interactions should always work
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardHoverExit called with null card.");
                return;
            }
            
            // Clear hover tracking when exiting
            if (currentlyHoveredCard == card)
            {
                currentlyHoveredCard = null;
            }

            animHelper?.HoverExit(card);
        }

        // When selecting (clicking / picking up) a card
        public void OnCardSelect(CardRender card, bool updatePosition = true)
        {
            // Don't check lock - user interactions should always work
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardSelect called with null card.");
                return;
            }

            // Clear hover state when selecting
            currentlyHoveredCard = null;

            // Only call SelectVisuals if this is a NEW selection (not already selected)
            // This prevents spamming the same card multiple times which stacks tweens
            if (currentlySelectedCard != card)
            {
                // Store the newly selected card
                currentlySelectedCard = card;
                animHelper?.SelectVisuals(card, updatePosition);
                
                // Only play sound if this is a NEW selection (not the same card already selected)
                sfxHelper?.PlaySelect();
            }
            else
            {
                Debug.Log($"[CardFXHelper] OnCardSelect ignored - card '{card.Data?.name ?? "unknown"}' is already selected");
            }

            // Reset drag SFX gate for the new drag session
            dragSoundPlayed = false;
        }

        // Called every frame while dragging the card
        public void OnCardDrag(CardRender card, Vector2 cursorPos)
        {
            // Don't check lock - user interactions should always work
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
            // Don't check lock - user interactions should always work
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardRelease called with null card.");
                return;
            }

            // Clear hover and selection state on release
            currentlyHoveredCard = null;
            currentlySelectedCard = null;
            
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
            // Keep lock check - discard is called during automated animations
            if (CardInteraction.Locked) return;
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardDiscard called with null card.");
                return;
            }

            animHelper?.AnimateDiscard(card);
            sfxHelper?.PlayDiscard();
        }

        // When card attacks an enemy
        public void OnCardAttack()
        {
            // Don't check lock - attack effects should always play
            sfxHelper?.PlayAttack();
        }

        // When card heals the player
        public void OnCardHeal()
        {
            // Don't check lock - heal effects should always play
            sfxHelper?.PlayHeal();
        }

        // When card gives block to the player
        public void OnCardBlock()
        {
            // Don't check lock - block effects should always play
            sfxHelper?.PlayBlock();
        }

        // On card exit, ensure Hover Visuals are reversed.
        public void OnCardExit(CardRender card)
        {
            // Don't check lock - this is cleanup and should always work
            if (card == null)
            {
                Debug.LogWarning("[CardFXHelper] OnCardExit called with null card.");
                return;
            }

            // Clear hover and selection tracking
            if (currentlyHoveredCard == card)
            {
                currentlyHoveredCard = null;
            }
            if (currentlySelectedCard == card)
            {
                currentlySelectedCard = null;
            }

            animHelper?.HoverExit(card);
            
            // Clear enemy hover sprites in case the card had arrow showing
            animHelper?.ClearEnemyHoverSprites();
        }
    }
}
