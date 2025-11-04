using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class FlipScript : MonoBehaviour
{
    // Supports either UI Image (Canvas) or SpriteRenderer (world)
    Image uiImage;
    SpriteRenderer spriteRenderer;

    [Tooltip("Index 0 = Heads, Index 1 = Tails")]
    public Sprite[] sides;           // 0 = Heads, 1 = Tails
    public int LastResult { get; private set; } = 0;

    [Header("Flip Timing")]
    public float totalFlipTime = 0.45f;
    public int flips = 6;

    bool isFlipping;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set a safe default sprite at start
        if (sides != null && sides.Length > 0)
            SetSprite(sides[0]);
    }

    void SetSprite(Sprite s)
    {
        if (uiImage) uiImage.sprite = s;
        if (spriteRenderer) spriteRenderer.sprite = s;
    }

    public void SetVisible(bool v)
    {
        if (uiImage) uiImage.enabled = v;
        if (spriteRenderer) spriteRenderer.enabled = v;
    }

    public void SetResult(int result)   // 0 heads, 1 tails
    {
        LastResult = Mathf.Clamp(result, 0, 1);
        SetSprite(sides[LastResult]);
    }

    public void Flip(bool forceHeads, Action<int> onComplete)
    {
        if (isFlipping) return;
        StartCoroutine(FlipRoutine(forceHeads, onComplete));
    }

    IEnumerator FlipRoutine(bool forceHeads, Action<int> onComplete)
    {
        isFlipping = true;
        SetVisible(true);

        float step = totalFlipTime / Mathf.Max(flips, 1);

        for (int i = 0; i < flips; i++)
        {
            // Toggle sprite quickly to pretend spinning
            int temp = (i % 2 == 0) ? 0 : 1;
            SetSprite(sides[temp]);
            yield return new WaitForSeconds(step);
        }

        LastResult = forceHeads ? 0 : 1;   // your GameManager chooses outcome
        SetSprite(sides[LastResult]);

        isFlipping = false;
        onComplete?.Invoke(LastResult);
    }
}
