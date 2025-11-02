using UnityEngine;
using System.Collections;

public class FlipScript : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [Tooltip("Index 0 = Heads, Index 1 = Tails")]
    public Sprite[] sides; // keep your existing array

    public int LastResult { get; private set; } = 0; // 0=heads, 1=tails

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (sides != null && sides.Length > 0)
        {
            spriteRenderer.sprite = sides[0]; // default look
        }
    }

    // Keep mouse tap support (optional), but the GameManager will call Flip().
    private void OnMouseDown()
    {
        if (!isFlipping) StartCoroutine(Flip(0.01f, 0.07f, null));
    }

    bool isFlipping = false;

    /// <summary>
    /// Flips the coin with animation and randomly chooses heads/tails.
    /// onComplete(int) gets 0 for heads, 1 for tails.
    /// </summary>
    public IEnumerator Flip(float durationStep = 0.01f, float scaleStep = 0.07f, System.Action<int> onComplete = null)
    {
        if (isFlipping) yield break;
        isFlipping = true;

        float size = transform.localScale.y;

        // Shrink
        while (size > 0.1f)
        {
            size -= scaleStep;
            transform.localScale = new Vector3(1f, size, 1f);
            yield return new WaitForSeconds(durationStep);
        }

        // Decide random result and swap sprite when "edge-on"
        LastResult = (Random.value < 0.5f) ? 0 : 1; // 0=heads, 1=tails
        if (sides != null && sides.Length >= 2)
            spriteRenderer.sprite = sides[LastResult];

        // Grow (a little z squash to give some pop)
        while (size < 0.99f)
        {
            size += scaleStep;
            transform.localScale = new Vector3(1f, size, size);
            yield return new WaitForSeconds(durationStep);
        }

        isFlipping = false;
        onComplete?.Invoke(LastResult);
    }
}
