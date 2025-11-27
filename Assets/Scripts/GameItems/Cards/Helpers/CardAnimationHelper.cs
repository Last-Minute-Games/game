namespace GameItems.Cards.Helpers
{
    using DG.Tweening;
    using UnityEngine;

    public class CardAnimationHelper : MonoBehaviour
    {
        [Header("Arrow Helper")] public BezierCardArrowHelper arrowHelper;

        [Header("Visual Settings")] 
        public float hoverScale = 1.1f;
        public float hoverYOffset = 0.5f; // How much the card moves up on hover
        public float selectScale = 1.15f;
        public float dragScale = 1.12f;
        public float returnDuration = 0.25f;
        public float drawDuration = 0.35f;
        public float discardDuration = 0.35f;

        [Header("Return to Hand Settings")]
        [Tooltip("Maximum distance (in world units) from original position to automatically return card to hand")]
        public float returnToHandThreshold = 2.0f;

        [Header("Input Settings")]
        [SerializeField] private Camera dragCamera;

        // internal tracking
        private Vector3 _baseScale = Vector3.one; // True original scale (never changes)
        private Vector3 _originalPosition;
        private bool _isHovering;
        private bool _isInitialized;

        private void Start()
        {
            // Capture the true base scale at start
            if (!_isInitialized)
            {
                _baseScale = transform.localScale;
                _originalPosition = transform.localPosition;
                _isInitialized = true;
            }
        }

        // Called by FXHelper.OnCardDrawn()
        public void AnimateDraw(CardRender card)
        {
            var cardTransform = card.transform;
            
            // Initialize base scale if not done yet
            if (!_isInitialized)
            {
                _baseScale = cardTransform.localScale;
                _originalPosition = cardTransform.localPosition;
                _isInitialized = true;
            }

            cardTransform.localScale = Vector3.zero;
            cardTransform.DOScale(_baseScale, drawDuration).SetEase(Ease.OutBack);
        }

        public void AnimateDiscard(CardRender card)
        {
            var cardTransform = card.transform;
            cardTransform.DOScale(Vector3.zero, discardDuration).SetEase(Ease.OutBack);
        }

        // Called by FXHelper.OnCardHover()
        public void HoverVisuals(CardRender card)
        {
            var cardTransform = card.transform;
            
            // Store original position if not already hovering
            if (!_isHovering)
            {
                _originalPosition = cardTransform.localPosition;
                
                // Initialize base scale if not done yet
                if (!_isInitialized)
                {
                    _baseScale = cardTransform.localScale;
                    _isInitialized = true;
                }
                
                _isHovering = true;
            }

            // Ensure arrow is hidden during hover (only show during drag)
            arrowHelper?.StopDrawing();

            // Scale up and move up (always relative to base scale)
            cardTransform.DOScale(_baseScale * hoverScale, 0.15f).SetEase(Ease.OutQuad);
            cardTransform.DOLocalMove(_originalPosition + new Vector3(0, hoverYOffset, 0), 0.15f).SetEase(Ease.OutQuad);
        }

        // Called when hover exits
        public void HoverExit(CardRender card)
        {
            if (!_isHovering) return;

            var cardTransform = card.transform;
            _isHovering = false;

            // Hide arrow if it's showing
            arrowHelper?.StopDrawing();

            // Return to base scale and original position
            cardTransform.DOScale(_baseScale, 0.15f).SetEase(Ease.OutQuad);
            cardTransform.DOLocalMove(_originalPosition, 0.15f).SetEase(Ease.OutQuad);
        }

        // Called by FXHelper.OnCardSelect()
        public void SelectVisuals(CardRender card, bool updatePosition = true)
        {
            var cardTransform = card.transform;
            
            // Initialize base scale if not done yet
            if (!_isInitialized)
            {
                _baseScale = cardTransform.localScale;
                _originalPosition = cardTransform.localPosition;
                _isInitialized = true;
                Debug.Log($"[CardAnimationHelper] Initialized - base scale: {_baseScale}, original position: {_originalPosition}");
            }
            
            // Only update original position if requested
            if (updatePosition)
            {
                // If hovering, we already have the original position from HoverVisuals
                if (_isHovering)
                {
                    Debug.Log($"[CardAnimationHelper] Select while hovering - keeping original position: {_originalPosition}");
                    _isHovering = false;
                }
                else
                {
                    // Not hovering, capture current position
                    // _originalPosition = cardTransform.localPosition;
                    Debug.Log($"[CardAnimationHelper] Select without hover - storing original position: {_originalPosition}");
                }
            }
            else
            {
                // Don't update position (used when drag starts)
                Debug.Log($"[CardAnimationHelper] Select (drag start) - keeping original position: {_originalPosition}");
                if (_isHovering)
                {
                    _isHovering = false;
                }
            }

            // Kill any existing tweens
            cardTransform.DOKill();

            // Scale to select size (always relative to base scale)
            cardTransform.DOScale(_baseScale * selectScale, 0.15f);
            
            // Check if this is an enemy-targeting card
            bool isEnemyTargeting = false;
            if (card.Data != null)
            {
                TargetRule targetRule = card.Data.GetDominatingTargetRule();
                isEnemyTargeting = targetRule == TargetRule.Enemy;
            }
            
            // For enemy-targeting cards, keep them in the elevated hover position
            if (isEnemyTargeting)
            {
                // Move to hover position (slightly elevated)
                cardTransform.DOLocalMove(_originalPosition + new Vector3(0, hoverYOffset, 0), 0.15f).SetEase(Ease.OutQuad);
                
                // Start arrow drawing
                if (arrowHelper != null)
                {
                    arrowHelper.StartDrawing();
                }
            }
            else
            {
                // For non-enemy cards, ensure arrow is hidden
                if (arrowHelper != null)
                {
                    arrowHelper.StopDrawing();
                }
            }
        }

        // Basic drag following cursor (NO ARROW)
        public void DragFollowMouseWithCard(CardRender card, Vector2 cursorPos)
        {
            var cardTransform = card.transform;
            var cam = ResolveCamera();

            if (cam == null)
            {
                Debug.LogWarning("[CardAnimationHelper] DragFollowMouseWithCard requires a camera reference.");
                return;
            }

            float depth = cam.WorldToScreenPoint(cardTransform.position).z;
            if (depth < 0.01f)
            {
                depth = Mathf.Abs(cardTransform.position.z - cam.transform.position.z);
                if (depth < 0.01f)
                {
                    depth = 1f;
                }
            }

            Vector3 screenPoint = new(cursorPos.x, cursorPos.y, depth);
            Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);

            // if (cardTransform.parent != null)
            // {
            //     Vector3 localTarget = cardTransform.parent.InverseTransformPoint(worldPoint);
            //     cardTransform.DOLocalMove(localTarget, 0.1f).SetEase(Ease.OutCubic);
            // }
            // else
            // {
            //     cardTransform.DOMove(worldPoint, 0.1f).SetEase(Ease.OutCubic);
            // }

            Vector3 localTarget = cardTransform.parent.InverseTransformPoint(worldPoint);
            cardTransform.localPosition = localTarget;
            
            // Use base scale for drag scale (prevents compounding)
            cardTransform.localScale = Vector3.Lerp(cardTransform.localScale, _baseScale * dragScale, 0.25f);
        }

        // Drag with arrow (Enemy targeting) - card stays in place, only arrow moves
        public void DragFollowMouseWithArrow(CardRender card, Vector2 cursorPos)
        {
            // Card stays in place - don't move it
            // Only update the bezier arrow
            if (arrowHelper == null)
            {
                return;
            }

            // Get the card's world position (use transform.position for world space)
            Vector3 cardWorldPos = card.transform.position;
            
            // Check if cursor is hovering over an enemy
            bool isHoveringEnemy = IsHoveringOverEnemy(cursorPos);
            
            // Update the arrow with card's world position and cursor screen position
            arrowHelper.UpdateArrow(cardWorldPos, cursorPos, isHoveringEnemy);
        }
        
        /// <summary>
        /// Checks if the cursor is hovering over a valid enemy target.
        /// </summary>
        private bool IsHoveringOverEnemy(Vector2 screenPosition)
        {
            Camera cam = ResolveCamera();
            if (cam == null)
                return false;

            // Convert screen position to world position
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(cam.transform.position.z)));
            
            // Use OverlapPoint to check what's at the cursor position
            Collider2D[] colliders = Physics2D.OverlapPointAll(new Vector2(worldPos.x, worldPos.y));
            
            foreach (var collider in colliders)
            {
                if (collider == null) continue;

                // Check if the hit object has an EnemyRender component
                var enemyRender = collider.GetComponent<EnemyRender>();
                if (enemyRender != null && enemyRender.data != null && enemyRender.data.isAlive)
                {
                    return true;
                }
            }

            return false;
        }

        // Called when player lets go but target is invalid
        public void ReturnToPosition(CardRender card)
        {
            var cardTransform = card.transform;

            // clear arrow
            arrowHelper?.StopDrawing();

            // Return to base scale and original position
            cardTransform.DOScale(_baseScale, 0.15f);
            cardTransform.DOLocalMove(_originalPosition, returnDuration).SetEase(Ease.OutCubic);
        }

        // Called when card successfully hits a target
        public void PlayRelease(CardRender card)
        {
            var cardTransform = card.transform;

            arrowHelper?.StopDrawing();

            cardTransform
                .DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(card.gameObject));
        }

        private Camera ResolveCamera()
        {
            return dragCamera != null ? dragCamera : Camera.main;
        }

        /// <summary>
        /// Checks if the card is close enough to its original position to be returned to hand.
        /// </summary>
        public bool IsNearOriginalPosition(CardRender card)
        {
            if (card == null) return false;

            Vector3 currentPos = card.transform.localPosition;
            float distance = Vector3.Distance(currentPos, _originalPosition);

            return distance <= returnToHandThreshold;
        }

        /// <summary>
        /// Gets the original position where the card was picked up from.
        /// </summary>
        public Vector3 GetOriginalPosition()
        {
            return _originalPosition;
        }
    }
}
