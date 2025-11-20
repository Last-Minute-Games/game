namespace GameItems.Cards.Helpers
{
    using DG.Tweening;
    using UnityEngine;

    public class CardAnimationHelper : MonoBehaviour
    {
        [Header("Arrow Helper")] public CardArrowHelper arrowHelper;

        [Header("Visual Settings")] 
        public float hoverScale = 1.1f;
        public float hoverYOffset = 0.5f; // How much the card moves up on hover
        public float selectScale = 1.15f;
        public float dragScale = 1.12f;
        public float returnDuration = 0.25f;
        public float drawDuration = 0.35f;
        public float discardDuration = 0.35f;

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

            // Return to base scale and original position
            cardTransform.DOScale(_baseScale, 0.15f).SetEase(Ease.OutQuad);
            cardTransform.DOLocalMove(_originalPosition, 0.15f).SetEase(Ease.OutQuad);
        }

        // Called by FXHelper.OnCardSelect()
        public void SelectVisuals(CardRender card)
        {
            var cardTransform = card.transform;
            
            // Initialize base scale if not done yet
            if (!_isInitialized)
            {
                _baseScale = cardTransform.localScale;
                _originalPosition = cardTransform.localPosition;
                _isInitialized = true;
            }
            
            // If hovering, exit hover first
            if (_isHovering)
            {
                _isHovering = false;
            }
            
            // Store current position (might be offset from hover)
            _originalPosition = cardTransform.localPosition;

            // Scale to select size (always relative to base scale)
            cardTransform.DOScale(_baseScale * selectScale, 0.15f);
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

        // Drag following cursor WITH ARROW (Enemy targeting)
        public void DragFollowMouseWithArrow(CardRender card, Vector2 cursorPos)
        {
            DragFollowMouseWithCard(card, cursorPos);

            if (arrowHelper == null)
            {
                return;
            }

            var cam = ResolveCamera();
            Vector3 screenPos = cam != null
                ? cam.WorldToScreenPoint(card.transform.position)
                : card.transform.position;
            Vector2 startScreen = new(screenPos.x, screenPos.y);

            arrowHelper.UpdateArrow(startScreen, cursorPos);
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
    }
}
