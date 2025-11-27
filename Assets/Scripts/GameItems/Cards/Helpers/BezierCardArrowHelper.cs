using UnityEngine;

namespace GameItems.Cards.Helpers
{
    /// <summary>
    /// Helper for displaying bezier arrows when dragging cards onto targets.
    /// </summary>
    public class BezierCardArrowHelper : MonoBehaviour
    {
        [Header("Bezier Arrow Settings")]
        [Tooltip("Reference to the BezierArrow component (will auto-create if not assigned)")]
        public BezierArrow bezierArrow;

        [Header("Arrow Prefabs (Optional - assign if creating new arrow)")]
        [Tooltip("Prefab for arrow body segments")]
        public GameObject arrowNodePrefab;
        [Tooltip("Prefab for arrow head")]
        public GameObject arrowHeadPrefab;

        [Header("Arrow Anchor Settings")]
        [Tooltip("Offset from card position where arrow originates (in world units)")]
        public Vector3 arrowAnchorOffset = new Vector3(0f, 0.5f, 0f);

        private Camera _mainCamera;
        private bool _isDrawing;

        private void Awake()
        {
            _mainCamera = Camera.main;

            // Auto-setup BezierArrow if not assigned
            if (bezierArrow == null)
            {
                // Try to find existing BezierArrow in scene
                bezierArrow = FindFirstObjectByType<BezierArrow>();

                // If none exists and we have prefabs, create one
                if (bezierArrow == null && arrowNodePrefab != null && arrowHeadPrefab != null)
                {
                    GameObject arrowObj = new GameObject("BezierArrow_CardTargeting");
                    arrowObj.transform.SetParent(transform);
                    bezierArrow = arrowObj.AddComponent<BezierArrow>();

                    bezierArrow.arrowNodePrefab = arrowNodePrefab;
                    bezierArrow.arrowHeadPrefab = arrowHeadPrefab;
                    bezierArrow.arrowNodeNum = 10;
                    bezierArrow.scaleFactor = 0.15f;
                }
                else if (bezierArrow == null)
                {
                    Debug.LogWarning("[BezierCardArrowHelper] No BezierArrow found and no prefabs assigned. Please assign a BezierArrow or arrow prefabs in the inspector.");
                }
            }
        }

        private void Start()
        {
            // Ensure arrow is hidden on start
            StopDrawing();
        }

        /// <summary>
        /// Starts drawing the arrow from the card.
        /// </summary>
        public void StartDrawing()
        {
            _isDrawing = true;
        }

        /// <summary>
        /// Stops drawing and hides the arrow.
        /// </summary>
        public void StopDrawing()
        {
            _isDrawing = false;
            if (bezierArrow != null)
            {
                bezierArrow.HideArrow();
            }
        }

        /// <summary>
        /// Updates the arrow to point from the card to the cursor position.
        /// </summary>
        /// <param name="cardWorldPosition">World position of the card</param>
        /// <param name="cursorScreenPosition">Screen position of the cursor</param>
        public void UpdateArrow(Vector3 cardWorldPosition, Vector2 cursorScreenPosition)
        {
            if (bezierArrow == null || !_isDrawing || _mainCamera == null)
                return;

            // Calculate arrow start position (top of card with offset)
            Vector3 arrowStartWorld = cardWorldPosition + arrowAnchorOffset;
            Vector2 arrowStartScreen = _mainCamera.WorldToScreenPoint(arrowStartWorld);

            // Update bezier arrow
            bezierArrow.ShowArrow(arrowStartScreen, cursorScreenPosition);
        }

        /// <summary>
        /// Checks if the arrow is currently being drawn.
        /// </summary>
        public bool IsDrawing => _isDrawing;
    }
}

