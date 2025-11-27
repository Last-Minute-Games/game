using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// The BezierArrows class.
/// </summary>
/// @Unity Script | 0 references
public class BezierArrow : MonoBehaviour
{
    #region Public Fields
    
    [Tooltip("The prefab of arrow head")]
    public GameObject arrowHeadPrefab;
    
    [Tooltip("The prefab of arrow node")]
    public GameObject arrowNodePrefab;
    
    [Tooltip("The number of arrow nodes")]
    public int arrowNodeNum;
    
    [Tooltip("The scale multiplier for arrow nodes")]
    public float scaleFactor = 1f;
    
    [Tooltip("Offset distance so arrow head tip points at cursor (in screen space pixels)")]
    public float arrowHeadTipOffset = 50f;
    
    [Header("Color Settings")]
    [Tooltip("Color when not hovering over an enemy")]
    public Color invalidTargetColor = new Color(0.878f, 0.075f, 0.020f, 1f); // #E01305
    
    [Tooltip("Color when hovering over a valid enemy")]
    public Color validTargetColor = new Color(0.298f, 0.710f, 0.447f, 1f); // #4CB572
    
    [Tooltip("Duration of color tween")]
    public float colorTweenDuration = 0.2f;
    
    #endregion
    
    #region Private Fields
    
    /// <summary>
    /// The position of P0 (The arrows emitter point).
    /// </summary>
    private RectTransform _origin;
    
    /// <summary>
    /// The list of arrow nodes' transform.
    /// </summary>
    private readonly List<RectTransform> _arrowNodes = new List<RectTransform>();
    
    /// <summary>
    /// The list of control points.
    /// </summary>
    private readonly List<Vector2> _controlPoints = new List<Vector2>();
    
    /// <summary>
    /// The factors to determine the position of control point P1, P2.
    /// </summary>
    private readonly List<Vector2> _controlPointFactors = new List<Vector2> { new Vector2(-0.3f, 0.8f), new Vector2(0.1f, 1.4f) };
    
    /// <summary>
    /// Tracks whether we're currently hovering over a valid target.
    /// </summary>
    private bool _isHoveringValidTarget;
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Executes when the gameObject instantiates.
    /// </summary>
    /// @Unity Message | 0 references
    private void Awake()
    {
        // Gets position of the arrow emitter point.
        _origin = GetComponent<RectTransform>();
        
        // Instantiates the arrow nodes and arrow head.
        for (int i = 0; i < arrowNodeNum; ++i)
        {
            _arrowNodes.Add(Instantiate(arrowNodePrefab, transform).GetComponent<RectTransform>());
        }
        
        _arrowNodes.Add(Instantiate(arrowHeadPrefab, transform).GetComponent<RectTransform>());
        
        // Initialize arrow nodes to red (invalid target color)
        foreach (var node in _arrowNodes)
        {
            var image = node.GetComponent<Image>();
            if (image != null)
            {
                image.color = invalidTargetColor;
            }
        }
        
        // Hides the arrow nodes.
        _arrowNodes.ForEach(n => n.GetComponent<RectTransform>().position = new Vector2(-1000, -1000));
        
        // Initializes the control points list.
        for (int i = 0; i < 4; ++i)
        {
            _controlPoints.Add(Vector2.zero);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Shows the arrow and updates it to point from start position to end position (screen space).
    /// </summary>
    /// <param name="startScreenPos">Start position in screen coordinates</param>
    /// <param name="endScreenPos">End position in screen coordinates (usually mouse/cursor)</param>
    /// <param name="isHoveringValidTarget">Whether the arrow is currently hovering over a valid target</param>
    public void ShowArrow(Vector2 startScreenPos, Vector2 endScreenPos, bool isHoveringValidTarget = false)
    {
        // P0 is at the arrow start point.
        _controlPoints[0] = startScreenPos;
        
        // Calculate direction from start to end
        Vector2 directionToEnd = (endScreenPos - startScreenPos).normalized;
        
        // P3 is offset backward from the cursor position so the arrow head tip reaches the cursor
        _controlPoints[3] = endScreenPos - (directionToEnd * arrowHeadTipOffset);
        
        // P1, P2 determines by P0 and P3.
        // P1 = P0 + (P3 - P0) * Vector2(-0.3f, 0.8f)
        // P2 = P0 + (P3 - P0) * Vector2(0.1f, 1.4f)
        _controlPoints[1] = _controlPoints[0] + (_controlPoints[3] - _controlPoints[0]) * _controlPointFactors[0];
        _controlPoints[2] = _controlPoints[0] + (_controlPoints[3] - _controlPoints[0]) * _controlPointFactors[1];
        
        // First pass: Calculate all node positions
        Vector2[] positions = new Vector2[_arrowNodes.Count];
        
        for (int i = 0; i < _arrowNodes.Count; ++i)
        {
            // Calculates t.
            var t = Mathf.Log((i * 1f / (_arrowNodes.Count - 1)) + 1f, 2f);
            
            // Cubic Bezier curve
            // B(t) = (1-t)^3 * P0 + 3 * (1-t)^2 * t * P1 + 3 * (1-t) * t^2 * P2 + t^3 * P3
            positions[i] =
                Mathf.Pow(1 - t, 3) * _controlPoints[0] +
                3 * (1 - t) * (1 - t) * t * _controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * _controlPoints[2] +
                Mathf.Pow(t, 3) * _controlPoints[3];
        }
        
        // Second pass: Apply positions, rotations, and scales
        for (int i = 0; i < _arrowNodes.Count; ++i)
        {
            _arrowNodes[i].position = positions[i];
            
            // Calculates rotations for each arrow node.
            if (i > 0)
            {
                var euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, positions[i] - positions[i - 1]));
                _arrowNodes[i].rotation = Quaternion.Euler(euler);
            }
            
            // Calculates scales for each arrow node.
            var scale = scaleFactor * (0.5f + (i / (float)_arrowNodes.Count) * 0.5f);
            _arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }
        
        // The first arrow node's rotation.
        _arrowNodes[0].transform.rotation = _arrowNodes[1].transform.rotation;
        
        // Update color based on hover state if changed
        if (_isHoveringValidTarget != isHoveringValidTarget)
        {
            _isHoveringValidTarget = isHoveringValidTarget;
            Color targetColor = _isHoveringValidTarget ? validTargetColor : invalidTargetColor;
            
            // Tween all arrow nodes' colors
            foreach (var node in _arrowNodes)
            {
                var image = node.GetComponent<Image>();
                if (image != null)
                {
                    image.DOColor(targetColor, colorTweenDuration);
                }
            }
        }
    }
    
    /// <summary>
    /// Hides the arrow by moving all nodes offscreen.
    /// </summary>
    public void HideArrow()
    {
        _arrowNodes.ForEach(n => n.position = new Vector2(-1000, -1000));
    }
    
    #endregion
}
