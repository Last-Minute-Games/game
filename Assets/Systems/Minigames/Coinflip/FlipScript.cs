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

    [Header("Spin Animation (sprite sheet)")]
    [Tooltip("Frames from your coin spin sprite sheet, left to right.")]
    public Sprite[] spinFrames;      // e.g. the 7 frames you sliced

    [Header("Flip Timing")]
    public float totalFlipTime = 1f;
    public int flips = 3;            // how many times to loop through spinFrames

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Optional: short sound that plays when the flip starts (whoosh/spin).")]
    [SerializeField] private AudioClip spinClip;
    [Tooltip("Sound that plays when the coin lands on the final side.")]
    [SerializeField] private AudioClip landClip;

    bool isFlipping;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Set a safe default sprite at start
        if (sides != null && sides.Length > 0)
            SetSprite(sides[0]);
    }

    void SetSprite(Sprite s)
    {
        if (!s) return;
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
        if (sides != null && sides.Length > LastResult)
            SetSprite(sides[LastResult]);
    }

    private void PlaySpinSound()
    {
        if (audioSource != null && spinClip != null)
        {
            audioSource.PlayOneShot(spinClip);
        }
    }

    private void PlayLandSound()
    {
        if (audioSource != null && landClip != null)
        {
            audioSource.PlayOneShot(landClip);
        }
    }

    // Called by GameManager
    public void Flip(bool forceHeads, Action<int> onComplete)
    {
        if (isFlipping) return;
        StartCoroutine(FlipRoutine(forceHeads, onComplete));
    }

    IEnumerator FlipRoutine(bool forceHeads, Action<int> onComplete)
    {
        isFlipping = true;
        SetVisible(true);

        PlaySpinSound();

        // --- NEW: use the sprite-sheet frames if we have them ---
        if (spinFrames != null && spinFrames.Length > 0)
        {
            int loops = Mathf.Max(flips, 1);
            int totalFrames = Mathf.Max(spinFrames.Length * loops, 1);
            float step = totalFlipTime / totalFrames;

            for (int i = 0; i < totalFrames; i++)
            {
                int frameIndex = i % spinFrames.Length;
                SetSprite(spinFrames[frameIndex]);
                yield return new WaitForSeconds(step);
            }
        }
        else
        {
            // Fallback: old simple flicker between heads/tails
            float step = totalFlipTime / Mathf.Max(flips, 1);

            for (int i = 0; i < flips; i++)
            {
                int temp = (i % 2 == 0) ? 0 : 1;
                if (sides != null && sides.Length > temp)
                    SetSprite(sides[temp]);
                yield return new WaitForSeconds(step);
            }
        }

        // Decide final result (GameManager already randomizes forceHeads)
        LastResult = forceHeads ? 0 : 1;

        if (sides != null && sides.Length > LastResult)
            SetSprite(sides[LastResult]);

        PlayLandSound();

        isFlipping = false;
        onComplete?.Invoke(LastResult);
    }
}
