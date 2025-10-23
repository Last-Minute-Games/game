using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonFX : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Audio")]
    public AudioSource audioSource;      // Drag your Audio Source here
    public AudioClip hoverClip;          // Sound played when hovering
    public AudioClip clickClip;          // Sound played when clicking

    [Header("Animation")]
    public float hoverScale = 1.06f;     // Scale when hovered
    public float pressScale = 0.98f;     // Scale when pressed
    public float tweenTime = 0.08f;      // Speed of transition

    // --- Hover sound cooldown ---
    private float lastHoverTime = 0f;
    public float hoverCooldown = 0.2f;   // Minimum time between hover sounds

    // --- Private variables ---
    private RectTransform rect;
    private Vector3 baseScale;
    private Coroutine tween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        rect.localScale = baseScale;
    }

    // When mouse enters the button
    public void OnPointerEnter(PointerEventData e)
    {
        // Prevents sound spam if cursor moves in/out quickly
        if (Time.unscaledTime - lastHoverTime > hoverCooldown)
        {
            lastHoverTime = Time.unscaledTime;
            if (hoverClip && audioSource)
                audioSource.PlayOneShot(hoverClip);
        }

        TweenTo(baseScale * hoverScale);
    }

    // When mouse leaves the button
    public void OnPointerExit(PointerEventData e)
    {
        TweenTo(baseScale);
    }

    // When mouse button is pressed down
    public void OnPointerDown(PointerEventData e)
    {
        TweenTo(baseScale * pressScale);
    }

    // When mouse button is released
    public void OnPointerUp(PointerEventData e)
    {
        // If still hovered, return to hover scale
        TweenTo(baseScale * hoverScale);
    }

    // When button is clicked
    public void OnPointerClick(PointerEventData e)
    {
        if (clickClip && audioSource)
            audioSource.PlayOneShot(clickClip);
    }

    // --- Helper methods ---
    void TweenTo(Vector3 target)
    {
        if (tween != null)
            StopCoroutine(tween);
        tween = StartCoroutine(ScaleRoutine(target));
    }

    IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = rect.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / tweenTime;
            rect.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        rect.localScale = target;
    }
}
