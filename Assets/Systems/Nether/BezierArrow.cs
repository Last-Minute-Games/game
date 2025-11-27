using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    [Tooltip("Offset for arrow head tip (distance from center to tip in world units)")]
    public float arrowHeadTipOffset = 0.5f;
    
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
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Executes when the gameObject instantiates.
    /// </summary>
    /// @Unity Message | 0 references
    private void Awake()
    {
        // Gets position of the arrow emitter point.
        this._origin = this.GetComponent<RectTransform>();
        
        // Instantiates the arrow nodes and arrow head.
        for (int i = 0; i < this.arrowNodeNum; ++i)
        {
            this._arrowNodes.Add(Instantiate(this.arrowNodePrefab, this.transform).GetComponent<RectTransform>());
        }
        
        this._arrowNodes.Add(Instantiate(this.arrowHeadPrefab, this.transform).GetComponent<RectTransform>());
        
        // Hides the arrow nodes.
        this._arrowNodes.ForEach(n => n.GetComponent<RectTransform>().position = new Vector2(-1000, -1000));
        
        // Initializes the control points list.
        for (int i = 0; i < 4; ++i)
        {
            this._controlPoints.Add(Vector2.zero);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Shows the arrow and updates it to point from start position to end position (screen space).
    /// </summary>
    /// <param name="startScreenPos">Start position in screen coordinates</param>
    /// <param name="endScreenPos">End position in screen coordinates (usually mouse/cursor)</param>
    public void ShowArrow(Vector2 startScreenPos, Vector2 endScreenPos)
    {
        // P0 is at the arrow start point.
        this._controlPoints[0] = startScreenPos;
        
        // P3 is at the end point (cursor).
        this._controlPoints[3] = endScreenPos;
        
        // P1, P2 determines by P0 and P3.
        // P1 = P0 + (P3 - P0) * Vector2(-0.3f, 0.8f)
        // P2 = P0 + (P3 - P0) * Vector2(0.1f, 1.4f)
        this._controlPoints[1] = this._controlPoints[0] + (this._controlPoints[3] - this._controlPoints[0]) * this._controlPointFactors[0];
        this._controlPoints[2] = this._controlPoints[0] + (this._controlPoints[3] - this._controlPoints[0]) * this._controlPointFactors[1];
        
        // First pass: Calculate all node positions
        Vector2[] positions = new Vector2[this._arrowNodes.Count];
        
        for (int i = 0; i < this._arrowNodes.Count; ++i)
        {
            // Calculates t.
            var t = Mathf.Log((i * 1f / (this._arrowNodes.Count - 1)) + 1f, 2f);
            
            // Cubic Bezier curve
            // B(t) = (1-t)^3 * P0 + 3 * (1-t)^2 * t * P1 + 3 * (1-t) * t^2 * P2 + t^3 * P3
            positions[i] =
                Mathf.Pow(1 - t, 3) * this._controlPoints[0] +
                3 * (1 - t) * (1 - t) * t * this._controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * this._controlPoints[2] +
                Mathf.Pow(t, 3) * this._controlPoints[3];
        }
        
        // For the arrow head (last node), offset it so the tip points at the cursor
        if (this._arrowNodes.Count > 1)
        {
            int lastIndex = this._arrowNodes.Count - 1;
            // Calculate direction from second-to-last node to last node
            Vector2 direction = (positions[lastIndex] - positions[lastIndex - 1]).normalized;
            
            // Offset the arrow head backward by the tip offset distance
            positions[lastIndex] -= direction * arrowHeadTipOffset * scaleFactor;
        }
        
        // Second pass: Apply positions, rotations, and scales
        for (int i = 0; i < this._arrowNodes.Count; ++i)
        {
            this._arrowNodes[i].position = positions[i];
            
            // Calculates rotations for each arrow node.
            if (i > 0)
            {
                var euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, positions[i] - positions[i - 1]));
                this._arrowNodes[i].rotation = Quaternion.Euler(euler);
            }
            
            // Calculates scales for each arrow node.
            var scale = this.scaleFactor * (0.5f + (i / (float)this._arrowNodes.Count) * 0.5f);
            this._arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }
        
        // The first arrow node's rotation.
        this._arrowNodes[0].transform.rotation = this._arrowNodes[1].transform.rotation;
    }
    
    /// <summary>
    /// Hides the arrow by moving all nodes offscreen.
    /// </summary>
    public void HideArrow()
    {
        this._arrowNodes.ForEach(n => n.position = new Vector2(-1000, -1000));
    }
    
    #endregion
}
