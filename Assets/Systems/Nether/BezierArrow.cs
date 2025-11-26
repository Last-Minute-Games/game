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
    
    /// <summary>
    /// Executes every frame.
    /// </summary>
    /// @Unity Message | 0 references
    private void Update()
    {
        // P0 is at the arrow emitter point.
        this._controlPoints[0] = new Vector2(this._origin.position.x, this._origin.position.y);
        
        // P3 is at the mouse position.
        this._controlPoints[3] = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        
        // P1, P2 determines by P0 and P3.
        // P1 = P0 + (P3 - P0) * Vector2(-0.3f, 0.8f)
        // P2 = P0 + (P3 - P0) * Vector2(0.1f, 1.4f)
        this._controlPoints[1] = this._controlPoints[0] + (this._controlPoints[3] - this._controlPoints[0]) * this._controlPointFactors[0];
        this._controlPoints[2] = this._controlPoints[0] + (this._controlPoints[3] - this._controlPoints[0]) * this._controlPointFactors[1];
        
        for (int i = 0; i < this._arrowNodes.Count; ++i)
        {
            // Calculates t.
            var t = Mathf.Log((i * 1f / (this._arrowNodes.Count - 1)) + 1f, 2f);
            
            // Cubic Bezier curve
            // B(t) = (1-t)^3 * P0 + 3 * (1-t)^2 * t * P1 + 3 * (1-t) * t^2 * P2 + t^3 * P3
            this._arrowNodes[i].position =
                Mathf.Pow(1 - t, 3) * this._controlPoints[0] +
                3 * (1 - t) * (1 - t) * t * this._controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * this._controlPoints[2] +
                Mathf.Pow(t, 3) * this._controlPoints[3];
            
            // Calculates rotations for each arrow node.
            if (i > 0)
            {
                var euler = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, this._arrowNodes[i].position - this._arrowNodes[i - 1].position));
                this._arrowNodes[i].rotation = Quaternion.Euler(euler);
            }
            
            // Calculates scales for each arrow node.
            var scale = this.scaleFactor * (0.5f + (i / (float)this._arrowNodes.Count) * 0.5f);
            this._arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }
        
        // The first arrow node's rotation.
        this._arrowNodes[0].transform.rotation = this._arrowNodes[1].transform.rotation;
    }
    
    #endregion
}
