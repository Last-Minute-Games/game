using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardArrowHelper : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite bodySprite;
    public Sprite headSprite;

    [Header("Settings")]
    public int segmentCount = 20;
    public float curvatureHeight = 2.5f;
    public float segmentSpacing = 0.25f;
    public float spriteScale = 0.01f;

    private Camera mainCam;
    private List<Image> bodySegments = new();
    private Image arrowHead;
    private bool isDrawing;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartDrawing();
        if (Input.GetMouseButtonUp(0))
            StopDrawing();

        if (isDrawing)
            UpdateArrow();
    }

    public void StartDrawing()
    {
        isDrawing = true;
        ClearArrow();
        CreateSegments();
    }

    public void StopDrawing()
    {
        isDrawing = false;
        ClearArrow();
    }

    private void CreateSegments()
    {
        // create body segments
        for (int i = 0; i < segmentCount; i++)
        {
            var go = new GameObject($"Body_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = bodySprite;
            img.SetNativeSize();
            img.rectTransform.localScale = Vector3.one * spriteScale;
            bodySegments.Add(img);
        }

        // create arrowhead
        var headGO = new GameObject("Head", typeof(RectTransform), typeof(Image));
        headGO.transform.SetParent(transform, false);
        arrowHead = headGO.GetComponent<Image>();
        arrowHead.sprite = headSprite;
        arrowHead.SetNativeSize();
        arrowHead.rectTransform.localScale = Vector3.one * spriteScale;
    }
    
    public void UpdateArrow(Vector2 start = default, Vector2 end = default)
    {
        if (start == default) start = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (end == default) end = Input.mousePosition;

        // World-to-screen conversion
        Vector3 worldStart = mainCam.ScreenToWorldPoint(new Vector3(start.x, start.y, 10f));
        Vector3 worldEnd = mainCam.ScreenToWorldPoint(new Vector3(end.x, end.y, 10f));

        // control point for curvature
        Vector3 mid = (worldStart + worldEnd) * 0.5f + Vector3.up * curvatureHeight;

        // draw Bezier points
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 point = Mathf.Pow(1 - t, 2) * worldStart +
                            2 * (1 - t) * t * mid +
                            Mathf.Pow(t, 2) * worldEnd;

            // move body segment
            var img = bodySegments[i];
            img.transform.position = point;

            // compute direction to next segment for rotation
            if (i < segmentCount - 1)
            {
                Vector3 nextPoint = Mathf.Pow(1 - (t + 1f / segmentCount), 2) * worldStart +
                                    2 * (1 - (t + 1f / segmentCount)) * (t + 1f / segmentCount) * mid +
                                    Mathf.Pow(t + 1f / segmentCount, 2) * worldEnd;
                Vector3 dir = nextPoint - point;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                img.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // place arrowhead
        Vector3 finalDir = (worldEnd - worldStart).normalized;
        arrowHead.transform.position = worldEnd;
        float finalAngle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
        arrowHead.rectTransform.rotation = Quaternion.Euler(0, 0, finalAngle);
    }

    private void ClearArrow()
    {
        foreach (var img in bodySegments)
            if (img) Destroy(img.gameObject);
        bodySegments.Clear();

        if (arrowHead) Destroy(arrowHead.gameObject);
        arrowHead = null;
    }
}
